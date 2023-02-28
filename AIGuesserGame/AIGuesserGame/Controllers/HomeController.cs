using AIGuesserGame.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Google.Cloud.Firestore;
using System.Drawing;

/// <summary>
/// Responsible for routing controls of home page.
/// Author(s): Lukasz Bednarek, Xiang Zhu, Jasper Zhou
/// Date: Nov 28, 2022
/// </summary>
namespace AIGuesserGame.Controllers
{
    /// <summary>
    /// Controller of Home page functionality.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// The logger of the controller.
        /// </summary>
        private readonly ILogger<HomeController> _logger;


        /// <summary>
        /// 
        /// </summary>
        /// Author(s): Jasper Zhou, Lukasz Bednarek
        /// <param name="logger">A Logger.</param>
        /// <param name="context"></param>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Returns the homepage of the app.
        /// </summary>
        /// Author: Jasper Zhou
        /// <returns>Returns the homepage of the app.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Redirects to the contact page.
        /// </summary>
        /// <returns>Contact view</returns>
        public IActionResult Contact()
        {
            return View("Contact");
        }

        /// <summary>
        /// Returns an error view.
        /// </summary>
        /// Author: Xiang Zhu
        /// <returns>Returns a view of the error.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}