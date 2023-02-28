using Microsoft.AspNetCore.Mvc;
using AIGuesserGame.Models;
using Google.Cloud.Firestore;
using DeepAI;
using Firebase.Storage;
using Firebase.Auth;
using Microsoft.AspNetCore.Session;
using Newtonsoft.Json;

namespace AIGuesserGame.Controllers
{
    // Helper Class to set and retrieve complex object as JSON:
    public static class SessionExtensions
    {
        
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }

    /// <summary>
    /// Responsible for routing controls of game, instructions, and results page.
    /// Author(s): Jasper Zhou
    /// Date: Nov 28, 2022
    /// </summary>
    public class GameController : Controller
    {
        // configure firestore 
        FirestoreDb db;

        // configure firebase auth and sotrage
        private static string ApiKey = "AIzaSyCR1cG3OxR1yKSG8TTbWKWwMKtK6hyhRfA";
        private static string Bucket = "gs://asp-ai-guesser.appspot.com";
        private static string AuthEmail = "jzhousynergy9@gmail.com";
        private static string AuthPass = "johnnytester";

        private readonly ILogger<GameController> _logger;
        public GameController(ILogger<GameController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Main method to load all elements for the game page
        /// </summary>
        /// Author(s): Jasper Zhou
        public IActionResult Index()
        {
            // connect firestore database to app
            string path = AppDomain.CurrentDomain.BaseDirectory + @"asp-ai-guesser-firebase.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

            // create ref to database
            db = FirestoreDb.Create("asp-ai-guesser");
            Console.WriteLine("successfully connected to db");

            // CreateGameData("ice cream shop closing at midnight").Wait();
            //CreateStaticGameData();
            GetRandomGameData().Wait();

            //GetGameData().Wait();

            /*
            CookieOptions options = new CookieOptions();
            Response.Cookies.Append("name", seshInfo[0], options);
            Response.Cookies.Append("IDasdf", seshInfo[1], options);
            */

            return View("Game");
        }

        public string GetSessionGame()
        {
            List<string> sessionInfo = new List<string>();

            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyUser)))
            {
                HttpContext.Session.SetString(SessionVariables.SessionKeyUser, "Current User");
                HttpContext.Session.SetString(SessionVariables.SessionKeyId, Guid.NewGuid().ToString());
            }

            var username = HttpContext.Session.GetString(SessionVariables.SessionKeyUser);
            var seshId = HttpContext.Session.GetString(SessionVariables.SessionKeyId);

            sessionInfo.Add(username);
            sessionInfo.Add(seshId);

            // HttpContext.Session.SetObjectAsJson(SessionVariables.SessionKeyGameData, game_data);
            var sessionGameData = HttpContext.Session.GetObjectFromJson<GameData>(SessionVariables.SessionKeyGameData);
            Console.WriteLine(sessionGameData.promptWords[0]);
            Console.WriteLine(sessionGameData.promptWords[1]);
            Console.WriteLine(sessionGameData.promptWords[2]);
            Console.WriteLine(sessionGameData.promptWords[3]);

            return "";
        }

        /// <summary>
        /// Unused method to store images in Firebase Storage, doesn't work.
        /// </summary>
        /// <returns></returns>
        private async Task<IActionResult> upload() {

            //upload to filebase
            string foldername = "firebaseFiles";
            string path = "Images/game_image_1.jpg";

            byte[] bytes = System.IO.File.ReadAllBytes(path);

            // authenticate firebase
            var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
            var authUser = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPass);

            // cancellation token 
            var cancellation = new CancellationTokenSource();

            // upload to firebase storage
            MemoryStream filestream = new MemoryStream(bytes);
            var upload = new FirebaseStorage(
                Bucket,
                new FirebaseStorageOptions {
                    AuthTokenAsyncFactory = () => Task.FromResult(authUser.FirebaseToken),
                    ThrowOnCancel = true // throw exception when canelling upload
                }) 
                .Child("images") // name of storage folder
                .Child("test_image.jpg")
                .PutAsync(filestream, cancellation.Token);

            try {
                ViewBag.link = await upload;
                return Ok();
            } catch (Exception ex){
                Console.WriteLine(ex);
                throw;
            }      
        }

        /// <summary>
        /// Create GameData object to store in the Firestore database using API call
        /// </summary>
        /// Author(s): Jasper Zhou
        /// <param name="imagePrompt">String</param>
        async private Task CreateGameData(String imagePrompt) {
            // load the DeepAI API
            DeepAI_API api = new DeepAI_API(apiKey: "b54db3ef-f339-4e47-99e5-0aae2dfb3ef4");

            StandardApiResponse resp = api.callStandardApi("text2img", new
            {
                text = imagePrompt,
            });
            Console.Write(api.objectAsJsonString(resp));

            string imageUrl = resp.output_url;  
            string imgSrc1 = "";
            string imgSrc2 = "";
            string imgSrc3 = "";
            string imgSrc4 = "";

            using (var client = new HttpClient()) {
                using (var response = await client.GetAsync(imageUrl)) {
                    byte[] imageBytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    string imgSrc = Convert.ToBase64String(imageBytes);

                    // have to split img string in half to fit in firestore string limit
                    int substringCount = 4;
                    int substringLength = imgSrc.Length / substringCount;
                    int remainder = imgSrc.Length % substringCount;

                    string[] substrings = new string[substringCount];
                    for (int i = 0; i < substringCount; i++)
                    {
                        if (i < remainder)
                        {
                            substrings[i] = imgSrc.Substring(i * (substringLength + 1), substringLength + 1);
                        }
                        else
                        {
                            substrings[i] = imgSrc.Substring(remainder + (i * substringLength), substringLength);
                        }
                    }

                    imgSrc1 = substrings[0];
                    imgSrc2 = substrings[1];
                    imgSrc3 = substrings[2];
                    imgSrc4 = substrings[3];

                }  
            }

            // convert prompt into list of words
            List<String> listPromptWords = imagePrompt.Split(" ").ToList();

            // create user collection and add example
            CollectionReference imageCollection = db.Collection("gameData");
            Dictionary<string, object> game1 = new Dictionary<string, object>()
            {
                {"imageData1", imgSrc1},
                {"imageData2", imgSrc2},
                {"imageData3", imgSrc3},
                {"imageData4", imgSrc4},
                {"prompt", listPromptWords }
            };
            
            // set game id to play it in session variable
            var doc = await imageCollection.AddAsync(game1); // this returns link to doc with doc id
            // ex. projects / asp - ai - guesser / databases / (default) / documents / gameData / UwOV8uCLEJhla6YjZMVh
            string docId = doc.ToString();
            string currentDocId = docId.Substring(docId.LastIndexOf('/') + 1);
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionVariables.SessionKeyId)))
            {
                HttpContext.Session.SetString(SessionVariables.SessionKeyId, currentDocId);
            }
            Console.WriteLine(currentDocId);
        }

        /// <summary>
        /// Create static GameData object to store in the Firestore database using API call
        /// </summary>
        /// Author(s): Jasper Zhou
        async private void CreateStaticGameData()
        {
            /* create user collection and add example
            CollectionReference userCollection = db.Collection("gameData");
            Dictionary<string, object> user1 = new Dictionary<string, object>()
            {
                {"FirstName", "John"},
                {"LastName", "Testing"},
                {"Password", 1234}
            };
            await userCollection.AddAsync(user1);
            */

            // create static game data images
            string staticFileLink1 = "Images/game_image_1.jpg";
            string staticFileLink2 = "Images/game_image_2.jpg";

            string imgSrc1 = Convert.ToBase64String(System.IO.File.ReadAllBytes(staticFileLink1));
            string imgSrc2 = Convert.ToBase64String(System.IO.File.ReadAllBytes(staticFileLink2));

            // uploading static prompt data 
            List<String> staticPromptWords = new List<String>(new string[] { "pigs", "eating", "ramen", "at", "a", "restaurant" });

            // create user collection and add example
            CollectionReference imageCollection = db.Collection("gameData");
            Dictionary<string, object> game1 = new Dictionary<string, object>()
            {
                {"imageData1", imgSrc1},
                {"imageData2", imgSrc2},
                {"imageData3", ""},
                {"imageData4", ""},
                {"prompt", staticPromptWords }
            };
            await imageCollection.AddAsync(game1);
        }

        async private Task GetRandomGameData() {
            Query Qref = db.Collection("gameData");
            QuerySnapshot Qsnap = await Qref.GetSnapshotAsync();

            List<string> randomIdList = new List<string>();
            int docCount = 0;
            foreach (DocumentSnapshot snap in Qsnap) { // loop through all available games, add all game IDs to list
                randomIdList.Add(snap.Id);
            }

            Random rnd = new Random();
            int gameNum = rnd.Next(0, (randomIdList.Count)); // select a random game within the firebase

            DocumentReference docref = db.Collection("gameData").Document(randomIdList.ElementAt(gameNum));
            // DocumentReference docref = db.Collection("gameData").Document(randomIdList.ElementAt((int)(randomIdList.Count) - 2));
            DocumentSnapshot docsnap = await docref.GetSnapshotAsync(); // get the snapshot for the specific game

            FirestoreGameData FSgameData = docsnap.ConvertTo<FirestoreGameData>();

            // get all database game data
            string image1_database = FSgameData.imageData1;
            string image2_database = FSgameData.imageData2;
            string image3_database = FSgameData.imageData3;
            string image4_database = FSgameData.imageData4;
            List<String>promptWords = FSgameData.prompt; // This will the prompt list

            // create  game data for one game
            GameData game_data = new GameData();
            game_data.lifelines = 5;
            game_data.promptWords = promptWords;
            game_data.promptWordsGuessed = new List<String>(new string[] { });
            game_data.guessedWords = new List<String>(new string[] { });
            game_data.wordsLeft = promptWords.Count; // TODO: check for special words (at, a, from, to, hard to guess words, etc.) and subtract them in wordsLeft
            game_data.promptCount = promptWords.Count;
            foreach (var word in promptWords) { // add a list where each word in prompt is blank for user to fill
                game_data.promptWordsGuessed.Add(" ");
            }
            game_data.imageData = String.Format("data:image/gif;base64,{0}", (image1_database + image2_database + image3_database + image4_database));

            // add game data to view bag
            ViewBag.GameData = game_data;
            // add game data to session variable 
            HttpContext.Session.SetObjectAsJson(SessionVariables.SessionKeyGameData, game_data);
        }

        /// <summary>
        /// get static GameData object from Firestore database and convert to GameData model
        /// </summary>
        /// Author(s): Jasper Zhou
        async private Task GetStaticGameData() 
        {

            DocumentReference docref = db.Collection("gameData").Document("pig1"); 
            DocumentSnapshot snap = await docref.GetSnapshotAsync();

            if (snap.Exists) {
                Dictionary<string, object> image = snap.ToDictionary();

                string image_database = "";
                Console.WriteLine(image["imageData"]);
                foreach (var item in image)
                {
                    image_database = String.Format("data:image/gif;base64,{0}", item.Value);
                }
                // create static game data for one game
                GameData game_data = new GameData();

                game_data.lifelines = 5;
                game_data.promptWords = new List<String>(new string[] { "pigs", "eating", "ramen", "at", "a", "restaurant" });
                game_data.promptWordsGuessed = new List<String>(new string[] { });
                game_data.guessedWords = new List<String>(new string[] { });
                game_data.wordsLeft = 4;
                game_data.promptCount = 6;
                game_data.promptWordsGuessed.Add(" ");
                game_data.promptWordsGuessed.Add(" ");
                game_data.promptWordsGuessed.Add(" ");
                game_data.promptWordsGuessed.Add("at");
                game_data.promptWordsGuessed.Add("a");
                game_data.promptWordsGuessed.Add(" ");
                // game_data.imageData1 = image_database;
                // static_game_data = game_data;
                // ViewBag.GameData = static_game_data;
            }
        }

        [HttpPost]
        /// <summary>
        /// using user input from submit form button, check whether guessed word is in the prompt, and eiher win, lose, or continue the game depending on the user guess.
        /// </summary>
        /// Author(s): Jasper Zhou
        /// <param name="userInput">String</param>
        public IActionResult guessWord(string userInput)
        {
            // get session game data
            var static_game_data = HttpContext.Session.GetObjectFromJson<GameData>(SessionVariables.SessionKeyGameData);

            if (userInput == null) {
                userInput = "";
            }
            userInput = userInput.ToLower();

            int promptWordNum = 0;
            bool wordMatch = false;

            // checks if user word has already been entered TODO: add alert
            if (static_game_data.guessedWords.Contains(userInput))
            {
                //Console.WriteLine(userInput + " word has already been entered...");
            }
            else {
                foreach (string word in static_game_data.promptWords)
                {
                    if (userInput == word)
                    {
                        static_game_data.promptWordsGuessed[promptWordNum] = userInput; // assign to prompt words to show
                        wordMatch = true;
                        static_game_data.wordsLeft--; // subtract one from remaining prompt words
                    }
                    promptWordNum++; // increment current prompt word
                }
                if (!wordMatch)
                {
                    static_game_data.lifelines--;
                }
                static_game_data.guessedWords.Add(userInput);
            }
            // update game data to view bag
            ViewBag.GameData = static_game_data;
            // update game data to session variable 
            HttpContext.Session.SetObjectAsJson(SessionVariables.SessionKeyGameData, static_game_data);

            // win condition
            if (static_game_data.wordsLeft <= 0)
            {
                ViewBag.GameWon = true;
                return View("Results");
            }
            // lose condition
            else if (static_game_data.lifelines <= 0)
            {
                ViewBag.GameWon = false;
                return View("Results");
            }
            else { 
            }
            //static_game_data.guessedWords.ToList().ForEach(Console.WriteLine);
            return View("Game");
        }

        /// <summary>
        /// 
        /// </summary>
        /// Author: Xiang Zhu
        /// <returns></returns>
        public IActionResult Play()
        {
            ViewBag.Easy = GameDifficulty.Easy;
            ViewBag.Normal = GameDifficulty.Normal;
            ViewBag.Expert = GameDifficulty.Expert;

            return View("Instructions");
        }

        /// <summary>
        /// Check the button is clicked or not.
        /// </summary>
        /// Author: Xiang Zhu
        /// Source: https://www.youtube.com/watch?v=2nOieuwix2A,
        /// https://www.youtube.com/watch?v=FN4F6_saTn4
        /// <param name="button"> a button</param>
        /// <returns></returns>
        public JsonResult Check(string button)
        {

            if (button == "easy")
            {
                TempData["buttonval"] = "Easy";
            }
            else if (button == "normal")
            {
                TempData["buttonval"] = "Normal";
            }
            else if (button == "expert")
            {
                TempData["buttonval"] = "Expert";
            }
            else
            {
                TempData["buttonval"] = null;
            }

            return Json(TempData["buttonval"]);
        }
    }
}
