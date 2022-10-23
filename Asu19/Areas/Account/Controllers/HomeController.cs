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
using System.Data;
using System.Security.Claims;
using System.Security.Policy;

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
        public IActionResult Profile()
        {
            var userInfo = db.Users.Where(u => u.Id == Convert.ToInt32(User.Claims.ElementAt(0).Value)).FirstOrDefault();

            return View(userInfo);
        }

        [HttpGet]
        [Route("/login")]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return new RedirectResult("/profile");
            return View();
        }

        [HttpPost]
        [Route("/login")]
        public async Task<IActionResult> Login([FromForm] UserLogInfo userLogInfo)
        {

            AddValidationRule(userLogInfo);

            if (!ModelState.IsValid)
                return new HtmlResult(AuthValidation(userLogInfo), "/login");

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
            if (User.Identity.IsAuthenticated)
                return new RedirectResult("/profile");
            return View();
        }

        [HttpPost]
        [Route("/registration")]
        public async Task<IActionResult> Registration([FromForm] UserRegInfo userRegInfo)
        {
            AddValidationRule(userRegInfo);

            if (!ModelState.IsValid)
                return new HtmlResult(AuthValidation(userRegInfo), "/registration");

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

        [Authorize(Roles = "admin")]
        [Route("/requests")]
        public async Task<IActionResult> Requests(int? id)
        {
            var userRequests = from requests in db.Requests
                               join users in db.Users on requests.UserId equals users.Id
                               join cars in db.Cars on requests.CarId equals cars.Id
                               join services in db.Services on requests.ServiceId equals services.Id
                               join employees in db.Employees on requests.EmployeeId equals employees.Id into em_join
                               from e in em_join.DefaultIfEmpty()
                               orderby requests.Id
                               select new UserRequestInfo
                               {
                                   Id = requests.Id,
                                   UserId = users.Id,
                                   UserName = users.FirstName + " " + users.LastName,
                                   Car = cars.Brand + " " + cars.Model,
                                   Service = services.Name,
                                   Price = services.Price.ToString(),
                                   Employee = e == null ? "None" : e.FirstName + " " + e.LastName,
                                   Status = requests.Status,
                                   StartTime = requests.StartTime,
                                   EndTime = requests.EndTime ?? DateTime.Now,
                               };

            return View(await userRequests.ToListAsync());
        }

        [Authorize(Roles = "admin")]
        [Route("/requests/{id?}")]
        public IActionResult Request(int? id)
        {
            ViewBag.Employees = db.Employees.ToList();
            
            var userRequests = (from requests in db.Requests
                                join users in db.Users on requests.UserId equals users.Id
                                join cars in db.Cars on requests.CarId equals cars.Id
                                join services in db.Services on requests.ServiceId equals services.Id
                                join employees in db.Employees on requests.EmployeeId equals employees.Id into em_join
                                from e in em_join.DefaultIfEmpty()
                                where requests.Id == id
                                select new UserRequestInfo
                                {
                                    Id = requests.Id,
                                    UserId = users.Id,
                                    UserName = users.FirstName + " " + users.LastName,
                                    Car = cars.Brand + " " + cars.Model,
                                    Service = services.Name,
                                    Price = services.Price.ToString(),
                                    Employee = e == null ? "None" : e.FirstName + " " + e.LastName,
                                    Status = requests.Status,
                                    StartTime = requests.StartTime,
                                    EndTime = requests.EndTime ?? DateTime.Now,
                                }).FirstOrDefault();

            return View(userRequests);
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
        public async Task<IActionResult> AddCar()
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

        [HttpPost]
        [Route("/addcar")]
        public async Task<IActionResult> AddCar([FromForm] UserCarInfo userCarInfo)
        {
            if (!ModelState.IsValid)
                return new HtmlResult("Неверный ввод", "/addcar");

            string brand = userCarInfo.Brand.Trim();
            string model = userCarInfo.Model.Trim();

            Cars? car = db.Cars.FirstOrDefault(c => c.Brand == brand && c.Model == model);

            if (car != null)
            {
                db.UserCar.Add(new UserCar
                {
                    Id = db.UserCar.Max(uc => uc.Id) + 1,
                    UserId = Convert.ToInt32(User.Claims.ElementAt(0).Value),
                    CarId = car.Id,
                });

                await db.SaveChangesAsync();
            }
            else
            {
                car = new Cars
                {
                    Id = db.Cars.Max(c => c.Id) + 1,
                    Brand = brand,
                    Model = model,
                };

                db.Cars.Add(car);

                db.UserCar.Add(new UserCar
                {
                    Id = db.UserCar.Max(uc => uc.Id) + 1,
                    UserId = Convert.ToInt32(User.Claims.ElementAt(0).Value),
                    CarId = car.Id,
                });

                await db.SaveChangesAsync();
            }

            return new RedirectResult("/addcar");
        }

        [HttpPost]
        [Route("/delcar")]
        public async Task DelCar([FromBody] UserCarInfo userCarInfo)
        {
            Cars? car = db.Cars.FirstOrDefault(c => c.Brand == userCarInfo.Brand && c.Model == userCarInfo.Model);

            UserCar? userCar = db.UserCar.FirstOrDefault(uc => uc.UserId == Convert.ToInt32(User.Claims.ElementAt(0).Value) && uc.CarId == car.Id);

            db.UserCar.Remove(userCar);

            await db.SaveChangesAsync();
        }

        [HttpGet]
        [Authorize]
        [Route("/addrequest")]
        public async Task<IActionResult> AddRequest()
        {
            var userCars = from userCar in db.UserCar
                           join cars in db.Cars on userCar.CarId equals cars.Id
                           where userCar.UserId == Convert.ToInt32(User.Claims.FirstOrDefault().Value)
                           select new UserCarInfo
                           {
                               Brand = cars.Brand,
                               Model = cars.Model,
                           };

            ViewBag.Cars = await userCars.ToListAsync();

            ViewBag.Services = db.Services;

            var userRequestInfo = from requests in db.Requests
                                  join users in db.Users on requests.UserId equals users.Id
                                  join cars in db.Cars on requests.CarId equals cars.Id
                                  join services in db.Services on requests.ServiceId equals services.Id
                                  join employees in db.Employees on requests.EmployeeId equals employees.Id into em_join
                                  from e in em_join.DefaultIfEmpty()
                                  where users.Id == Convert.ToInt32(User.Claims.ElementAt(0).Value)
                                  select new UserRequestInfo
                                  {
                                      Id = requests.Id,
                                      UserId = users.Id,
                                      UserName = users.FirstName + " " + users.LastName,
                                      Car = cars.Brand + " " + cars.Model,
                                      Service = services.Name,
                                      Price = services.Price.ToString(),
                                      Employee = e == null ? "None" : e.FirstName + " " + e.LastName,
                                      Status = requests.Status,
                                      StartTime = requests.StartTime,
                                      EndTime = requests.EndTime ?? DateTime.Now,
                                  };

            return View(await userRequestInfo.ToListAsync());
        }

        [HttpPost]
        [Route("/addrequest")]
        public async Task<IActionResult> AddRequest([FromForm] string? car, int? serviceId)
        {
            string brand = car.Split("_")[0];
            string model = car.Split("_")[1];

            db.Requests.Add(new Requests
            {
                Id = db.Requests.Max(r => r.Id) + 1 ?? 1,
                UserId = Convert.ToInt32(User.Claims.FirstOrDefault().Value),
                CarId = db.Cars.Where(c => c.Brand == brand && c.Model == model).FirstOrDefault().Id,
                ServiceId = serviceId,
                EmployeeId = null,
                StartTime = DateTime.Now,
                Status = "Обработка",
                EndTime = null,
            });
            await db.SaveChangesAsync();

            return new RedirectResult("/addrequest");
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
