using Asu19.Database;
using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.AspNetCore.Mvc;

namespace Asu19.Areas.Account.Controllers
{
    [Area("Account")]
    public class HomeController : Controller
    {
        private ApplicationContext db;
        public HomeController(ApplicationContext db)
        {
            this.db = db;
        }

        [Route("/profile")]
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
