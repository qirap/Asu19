using Microsoft.AspNetCore.Mvc;

namespace Asu19.Areas.Account.Controllers
{
    [Area("Account")]
    public class HomeController : Controller
    {
        [Route("/personal")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("/login")]
        public IActionResult Login()
        {
            return View();
        }

        [Route("/registration")]
        public IActionResult Registration()
        {
            return View();
        }
    }
}
