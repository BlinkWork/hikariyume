using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webclient.Models;

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
        public IActionResult Register(User uesr)
        {
            return View();
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
