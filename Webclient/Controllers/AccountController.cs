using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using Webclient.Models;
using Microsoft.Win32;

namespace Webclient.Controllers
{
    public class AccountController : Controller
    {
        HikariYumeContext context = new HikariYumeContext();
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Edit()
        {
            return Ok();
        }
        public IActionResult Login()
        {

            return View();
        }
        [HttpPost]
        public IActionResult Login(LoginModel loginModel)
        {
            if (ModelState.IsValid)
            {
                var user = context.Users.SingleOrDefault(u => u.Email == loginModel.email);
                if (user == null)
                {
                    ModelState.AddModelError("email", "Email chưa đăng kí");
                    return View();
                }

                if (loginModel.password != user.Password)
                {
                    ModelState.AddModelError("password", "Mật khẩu không chính xác");
                    return View();
                }

                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserRole", user.Role);

                ViewData["id"] = HttpContext.Session.GetString("UserId");

                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Register(RegisterModel user)
        {
            if (ModelState.IsValid)
            {
                if (context.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng");
                    return View(user);
                }
                string emailVerificationToken = Guid.NewGuid().ToString();

                SendVerificationEmail(user.Email, emailVerificationToken);

                HttpContext.Session.SetString("PendingUserEmail", user.Email);
                HttpContext.Session.SetString("PendingUserPassword", user.Password);
                HttpContext.Session.SetString("PendingUserFullName", user.FullName);
                HttpContext.Session.SetString("PendingUserUserName", user.UserName);
                HttpContext.Session.SetString("PendingUserPhoneNumber", user.PhoneNumber);
                HttpContext.Session.SetString("PendingUserAddress", user.Address);
                HttpContext.Session.SetString("EmailVerificationToken", emailVerificationToken);
                HttpContext.Session.SetString("TokenCreatedAt", DateTime.Now.ToString());

                ViewBag.Message = "Vui lòng kiểm tra email để xác thực tài khoản.";

                return View();
            }
            return View();
        }

        private void SendVerificationEmail(string email, string token)
        {
            string verificationLink = Url.Action("VerifyEmail", "Account",
                new { token = token }, Request.Scheme);

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(Environment.GetEnvironmentVariable("GMAIL_EMAIL"));
            mail.To.Add(email);
            mail.Subject = "Xác thực tài khoản";
            mail.Body = $"Nhấn vào link để xác thực: <a href='{verificationLink}'>Xác thực</a>";
            mail.IsBodyHtml = true;

            SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
            smtpServer.Port = 587;
            smtpServer.Credentials = new NetworkCredential(
                                            "quannmhe171875@fpt.edu.vn",
                                            "ckvt sxdv ypmr pjls"
                                            );
            smtpServer.EnableSsl = true;

            smtpServer.Send(mail);
        }

        [HttpGet]
        public IActionResult VerifyEmail(string token)
        {
            string sessionToken = HttpContext.Session.GetString("EmailVerificationToken");
            string sessionTokenCreatedAt = HttpContext.Session.GetString("TokenCreatedAt");

            if (sessionToken == null || sessionTokenCreatedAt == null)
            {
                return View("Error");
            }

            DateTime tokenCreatedAt = DateTime.Parse(sessionTokenCreatedAt);
            if (DateTime.Now.Subtract(tokenCreatedAt).TotalMinutes > 15)
            {
                ViewBag.Message = "Token đã hết hạn. Vui lòng đăng ký lại.";
                return View("Error");
            }

            if (sessionToken == token)
            {
                User newUser = new User
                {
                    Email = HttpContext.Session.GetString("PendingUserEmail"),
                    Password = HttpContext.Session.GetString("PendingUserPassword"),
                    Username = HttpContext.Session.GetString("PendingUserUserName"),
                    FullName = HttpContext.Session.GetString("PendingUserFullName"),
                    Address = HttpContext.Session.GetString("PendingUserAddress"),
                    PhoneNumber = HttpContext.Session.GetString("PendingUserPhoneNumber"),
                    CreatedAt = DateTime.Now,
                    Role = "user"
                };

                context.Users.Add(newUser);
                context.SaveChanges();

                HttpContext.Session.Remove("PendingUserEmail");
                HttpContext.Session.Remove("PendingUserPassword");
                HttpContext.Session.Remove("PendingUserFullName");
                HttpContext.Session.Remove("EmailVerificationToken");
                HttpContext.Session.Remove("PendingUserUserName");
                HttpContext.Session.Remove("PendingUserAddress");
                HttpContext.Session.Remove("PendingUserPhoneNumber");


                ViewBag.Message = "Tài khoản của bạn đã được xác thực thành công! Vui lòng đăng nhập";
                return View("Login");
            }
            ViewBag.Message = "Mã xác thực không hợp lệ.";
            return View("Error");
        }


        public IActionResult Cart()
        {

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> RemoveItemFromCart(int id)
        {
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> ProceedCheckout()
        {
            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> History(string page = "1")
        {
            return View();
        }
    }
}
