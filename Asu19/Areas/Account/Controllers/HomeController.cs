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
                                  Brand = cars.Brand,
                                  Model = cars.Model,
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
        public async Task<IActionResult> Login([FromForm] UserLogInfo userLogInfo)
        {

            AddValidationRule(userLogInfo);

            if (!ModelState.IsValid)
                return new HtmlResult(AuthValidation(userLogInfo));

            Users? user = db.Users.FirstOrDefault(u => u.Login == userLogInfo.Login && u.Password == userLogInfo.Password);

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
            AddValidationRule(userRegInfo);

            if (!ModelState.IsValid)
                return new HtmlResult(AuthValidation(userRegInfo));

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

        [HttpGet]
        [Authorize]
        [Route("/addcar")]
        public IActionResult AddCar()
        {
            return View();
        }

        [HttpPost]
        [Route("/addcar")]
        public async Task<IActionResult> AddCar([FromForm] UserCarInfo userCarInfo)
        {
            return new HtmlResult(userCarInfo.Brand + " " + userCarInfo.Model);
        }

        public void AddValidationRule(IUserAuthInfo userAuthInfo)
        {
            foreach (var item in userAuthInfo.GetType().GetProperties())
            {
                if (item.GetValue(userAuthInfo, null)?.ToString()?.Length < 4)
                {
                    ModelState.AddModelError(item.Name, $"Длина поля {item.Name} должна быть от 4 до 20 символов");
                }
            }
        }

        public string AuthValidation(IUserAuthInfo userAuthInfo)
        {
            string errorMessages = "";

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
            return errorMessages;
        }
    }
}
