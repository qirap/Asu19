using Asu19.Areas.Account.Models;
using Asu19.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        [HttpGet]
        [Route("/login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [Route("/login")]
        public async Task<IActionResult> Login([FromForm] UserAuthInfo userAuthInfo)
        {
            Users? user = db.Users.FirstOrDefault(u => u.Login == userAuthInfo.Login && u.Password == userAuthInfo.Password);

            if (user == null)
                return new UnauthorizedResult();

            var claims = new List<Claim>
            { 
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName),
                new Claim(ClaimTypes.Role, user.Role),
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            return new RedirectResult("/");
        }

        [HttpGet]
        [Route("/registration")]
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        [Route("/registration")]
        public async Task<IActionResult> Registration([FromForm] UserRegInfo userRegInfo)
        {
            Users? user = db.Users.FirstOrDefault(u => u.Login == userRegInfo.Login);

            if (user != null)
                return new UnauthorizedResult();

            db.Users.Add(new Users
            {
                Id = db.Users.Max(u => u.Id) + 1,
                Login = userRegInfo.Login,
                Password = userRegInfo.Password,
                FirstName = userRegInfo.FirstName,
                LastName = userRegInfo.LastName,
                Address = userRegInfo.Address,
                Role = "user"
            });

            await db.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName),
                new Claim(ClaimTypes.Role, user.Role),
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            return new RedirectResult("/");
        }

        [Route("/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return new RedirectResult("/login");
        }
    }
}
