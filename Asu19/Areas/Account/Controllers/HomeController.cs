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
using Microsoft.Extensions.FileProviders;
using System;
using System.Data;
using System.Security.Claims;
using System.Security.Policy;
using Xceed.Document.NET;
using Xceed.Words.NET;

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
            if (User.Claims.ElementAt(2).Value == "admin")
                return new RedirectResult("/requests");

            var userInfo = db.Users.Where(u => u.Id == Convert.ToInt32(User.Claims.ElementAt(0).Value)).FirstOrDefault();

            return View(userInfo);
        }

        [HttpPost]
        [Route("/profile/firstname")]
        public async Task UpdateFirstName([FromBody] UserUpdateInfo? userUpdateInfo)
        {
            db.Users.Where(u => u.Id == userUpdateInfo.Id).FirstOrDefault().FirstName = userUpdateInfo.NewValue;

            await db.SaveChangesAsync();
        }

        [HttpPost]
        [Route("/profile/lastname")]
        public async Task UpdateLastName([FromBody] UserUpdateInfo? userUpdateInfo)
        {
            db.Users.Where(u => u.Id == userUpdateInfo.Id).FirstOrDefault().LastName = userUpdateInfo.NewValue;

            await db.SaveChangesAsync();
        }

        [HttpPost]
        [Route("/profile/address")]
        public async Task UpdateAddress([FromBody] UserUpdateInfo? userUpdateInfo)
        {
            db.Users.Where(u => u.Id == userUpdateInfo.Id).FirstOrDefault().Address = userUpdateInfo.NewValue;

            await db.SaveChangesAsync();
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
                                   EndTime = requests.EndTime ?? null,
                               };

            return View(await userRequests.ToListAsync());
        }

        [Authorize(Roles = "admin")]
        [Route("/requests/{id?}")]
        public IActionResult Request(int? id)
        {
            if (id == null)
                return new NotFoundResult();

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
                                    ServiceId = services.Id,
                                    Service = services.Name,
                                    Price = services.Price.ToString(),
                                    Employee = e == null ? "None" : e.FirstName + " " + e.LastName,
                                    Status = requests.Status,
                                    StartTime = requests.StartTime,
                                    EndTime = requests.EndTime ?? null
                                }).FirstOrDefault();

            if (userRequests == null)
                return new NotFoundResult();

            var canEmployees = from es in db.EmployeeService
                               join e in db.Employees on es.EmployeeId equals e.Id
                               where es.ServiceId == userRequests.ServiceId
                               select new Employees
                               {
                                   Id = e.Id,
                                   FirstName = e.FirstName,
                                   LastName = e.LastName,
                                   Address = e.Address,
                               };

            ViewBag.Employees = canEmployees.ToList();

            return View(userRequests);
        }

        [Authorize]
        [Route("/requests/cheque/{id?}")]
        public async Task<IActionResult> Cheque(int? id)
        {
            if (id == null)
                return new NotFoundResult();

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
                                    ServiceId = services.Id,
                                    Service = services.Name,
                                    Price = services.Price.ToString(),
                                    Employee = e == null ? "None" : e.FirstName + " " + e.LastName,
                                    Status = requests.Status,
                                    StartTime = requests.StartTime,
                                    EndTime = requests.EndTime ?? null
                                }).FirstOrDefault();

            if (userRequests == null || userRequests.Status != "Выполнено")
                return new NotFoundResult();

            using (MemoryStream ms = new MemoryStream())
            {
                var doc = DocX.Create(ms);

                var image = doc.AddImage("wwwroot/img/AVTO_AV.png");
                var p = image.CreatePicture();
                p.Width = 100;
                p.Height = 100;

                var parImg = doc.InsertParagraph(" ");
                parImg.Alignment = Alignment.center;
                parImg.InsertPicture(p);

                var par = doc.InsertParagraph("AutoService");
                par.Alignment = Alignment.center;
                par.FontSize(30);
                par.Bold(true);

                par = doc.InsertParagraph("Заявка выполнена");
                par.Alignment = Alignment.center;
                par.FontSize(20);
                par.Bold(true);


                Table t = doc.AddTable(7, 2);
                t.Alignment = Alignment.center;
                t.Design = TableDesign.ColorfulListAccent3;

                t.Rows[0].Cells[0].Paragraphs.First().Append("Заявка");
                t.Rows[0].Cells[1].Paragraphs.First().Append(userRequests.Id.ToString());

                t.Rows[1].Cells[0].Paragraphs.First().Append("Имя клиента");
                t.Rows[1].Cells[1].Paragraphs.First().Append(userRequests.UserName);

                t.Rows[2].Cells[0].Paragraphs.First().Append("Авто");
                t.Rows[2].Cells[1].Paragraphs.First().Append(userRequests.Car);

                t.Rows[3].Cells[0].Paragraphs.First().Append("Услуга");
                t.Rows[3].Cells[1].Paragraphs.First().Append(userRequests.Service);

                t.Rows[4].Cells[0].Paragraphs.First().Append("Время прихода заявки");
                t.Rows[4].Cells[1].Paragraphs.First().Append(userRequests.StartTime.ToString());

                t.Rows[5].Cells[0].Paragraphs.First().Append("Время завершения");
                t.Rows[5].Cells[1].Paragraphs.First().Append(userRequests.EndTime.ToString());

                t.Rows[6].Cells[0].Paragraphs.First().Append("Стоимость");
                t.Rows[6].Cells[1].Paragraphs.First().Append(userRequests.Price.ToString() + " рублей");

                doc.InsertTable(t);

                doc.InsertParagraph($"\n{DateTime.Now}\t\t\t\t\t\t\t\t\t__________");

                doc.Save();

                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "Cheque.docx");
            }
        }

        [HttpPost]
        [Route("/updatestatus")]
        public async Task UpdateStatus([FromBody] RequestUpdateInfo requestUpdateInfo)
        {
            var request = db.Requests.Where(r => r.Id == requestUpdateInfo.RequestId).FirstOrDefault();

            request.EmployeeId = requestUpdateInfo.EmployeeId;
            request.Status = "В работе";

            await db.SaveChangesAsync();
        }

        [HttpPost]
        [Route("/confirmcompletion")]
        public async Task ConfirmCompletion([FromBody] RequestUpdateInfo requestUpdateInfo)
        {
            var request = db.Requests.Where(r => r.Id == requestUpdateInfo.RequestId).FirstOrDefault();

            request.Status = "Выполнено";
            request.EndTime = DateTime.Now;

            await db.SaveChangesAsync();
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
