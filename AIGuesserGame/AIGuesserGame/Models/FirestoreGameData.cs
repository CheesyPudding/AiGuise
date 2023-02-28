using Google.Cloud.Firestore;

namespace AIGuesserGame.Models
{
    [FirestoreData]
    public class FirestoreGameData
    {
        [FirestoreProperty]
        public string imageData1 { get; set; }
        [FirestoreProperty]
        public string imageData2 { get; set; }
        [FirestoreProperty]
        public string imageData3 { get; set; }
        [FirestoreProperty]
        public string imageData4 { get; set; }
        [FirestoreProperty]
        public List<String> prompt { get; set; }
    }
}
