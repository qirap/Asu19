using Asu19.Areas.Account.Models;
using Asu19.Areas.Account.MyResult;
using Asu19.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
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
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userCarInfo = from userCar in db.UserCar
                              join cars in db.Cars on userCar.CarId equals cars.Id
                              where userCar.UserId == Convert.ToInt32(User.Claims.FirstOrDefault().Value)
                              select new UserCarInfo
                              {
                                  Car = cars.Brand + " " + cars.Model,
                              };
            return View(await userCarInfo.ToListAsync());
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

            if (userAuthInfo.Login?.Length < 4 || userAuthInfo.Login?.Length > 20)
            {
                ModelState.AddModelError("Login", "Длина логина должна быть от 4 до 20 символов");
            }
            if (userAuthInfo.Password?.Length < 4 || userAuthInfo.Password?.Length > 20)
            {
                ModelState.AddModelError("Password", "Длина пароля должна быть от 4 до 20 символов");
            }

            string errorMessages = "";

            if (!ModelState.IsValid)
            {
                foreach (var item in ModelState)
                {
                    if (item.Value.ValidationState == ModelValidationState.Invalid)
                    {
                        foreach (var error in item.Value.Errors)
                        {
                            errorMessages = $"{errorMessages}{error.ErrorMessage};";
                        }
                    }
                }
                return new HtmlResult(errorMessages);
            }

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
            if (userRegInfo.Login?.Length < 4 || userRegInfo.Login?.Length > 20)
            {
                ModelState.AddModelError("Login", "Длина логина должна быть от 4 до 20 символов");
            }
            if (userRegInfo.Password?.Length < 4 || userRegInfo.Password?.Length > 20)
            {
                ModelState.AddModelError("Password", "Длина пароля должна быть от 4 до 20 символов");
            }

            string errorMessages = "";

            if (!ModelState.IsValid)
            {
                foreach (var item in ModelState)
                {
                    if (item.Value.ValidationState == ModelValidationState.Invalid)
                    {
                        foreach (var error in item.Value.Errors)
                        {
                            errorMessages = $"{errorMessages}{error.ErrorMessage};";
                        }
                    }
                }
                return new HtmlResult(errorMessages);
            }

            Users? user = db.Users.FirstOrDefault(u => u.Login == userRegInfo.Login);

            if (user != null)
                return new UnauthorizedResult();

            user = new Users
            {
                Id = db.Users.Max(u => u.Id) + 1,
                Login = userRegInfo.Login,
                Password = userRegInfo.Password,
                FirstName = userRegInfo.FirstName,
                LastName = userRegInfo.LastName,
                Address = userRegInfo.Address,
                Role = "user"
            };

            db.Users.Add(user);

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
            return new RedirectResult("/");
        }

        /*[HttpPost]
        [Route("/addcar")]
        public async Task<IActionResult> AddCar()
        {
            
        }*/
    }
}
