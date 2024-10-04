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
            string sessionToken = HttpContext.Session.GetString("EmailVerificationToken");
            string sessionTokenCreatedAt = HttpContext.Session.GetString("TokenCreatedAt");
            if (sessionToken != null || sessionTokenCreatedAt != null)
            {
                return;
            }
            string verificationLink = Url.Action("VerifyEmail", "Account",
                new { token = token }, Request.Scheme);

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("quannmhe171875@fpt.edu.vn");
            mail.To.Add(email);
            mail.Subject = "Xác thực tài khoản";
            mail.Body = $"Nhấn vào link để xác thực: <a href='{verificationLink}'>Xác thực</a>";
            mail.IsBodyHtml = true;

            SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
            smtpServer.Port = 587;
            smtpServer.Credentials = new NetworkCredential(
                                            "quannmhe171875@fpt.edu.vn",
                                            Environment.GetEnvironmentVariable("GMAIL_PASSWORD")
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
                ViewBag.Message = "chưa có yêu cầu";
                return View("Register");
            }

            DateTime tokenCreatedAt = DateTime.Parse(sessionTokenCreatedAt);
            if (DateTime.Now.Subtract(tokenCreatedAt).TotalMinutes > 15)
            {
                ViewBag.Message = "Token đã hết hạn. Vui lòng đăng ký lại.";
                Console.WriteLine(ViewBag.Message);
                return View("Register");
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
            return View("Register");
        }


        public IActionResult Cart()
        {
            try
            {
                int id = int.Parse(HttpContext.Session.GetString("UserId"));

                List<Cart> carts = context.Carts.Where(p => p.UserId == id).Include(c => c.Product).ToList();
                return View(carts);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Login");
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] Cart cart)
        {
            try
            {
                int id = int.Parse(HttpContext.Session.GetString("UserId"));
                cart.UserId = id;
                context.Carts.Add(cart);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (ArgumentNullException ale)
            {
                return BadRequest("Người dùng chưa đăng nhập!");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> RemoveItemFromCart(int id)
        {
            try
            {
                int userId = int.Parse(HttpContext.Session.GetString("UserId"));
                Cart find = context.Carts.FirstOrDefault(c => c.CartId == id);

                if (find == null) return NotFound();
                if (find.UserId != userId) return BadRequest();

                context.Carts.Remove(find);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (ArgumentNullException ale)
            {
                return BadRequest("Người dùng chưa đăng nhập!");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProceedCheckout()
        {
            try
            {
                int id = int.Parse(HttpContext.Session.GetString("UserId"));
                using (var context = new HikariYumeContext())
                {
                    // Get cart items for the user
                    var cartItems = context.Carts.Where(c => c.UserId == id).Include(p => p.Product).ToList();

                    if (cartItems.Count == 0)
                    {
                        throw new Exception("giỏ hàng đang trống.");
                    }
                    // Create a new Order
                    var order = new Order
                    {
                        UserId = id,
                        TotalPrice = cartItems.Sum(c => c.Quantity * c.Product.Price), // calculate total price
                        Status = "Đang chờ", // Default status
                        CreatedAt = DateTime.Now
                    };
                    context.Orders.Add(order);
                    context.SaveChanges();  

                    // Create OrderItems based on the cart items
                    foreach (var cartItem in cartItems)
                    {
                        var orderItem = new OrderItem
                        {
                            OrderId = order.OrderId,
                            ProductId = cartItem.ProductId,
                            Quantity = cartItem.Quantity,
                            Price = cartItem.Product.Price
                        };
                        context.OrderItems.Add(orderItem);
                    }

                    // Save the new OrderItems
                    context.SaveChanges();

                    // Remove the cart items for the user
                    context.Carts.RemoveRange(cartItems);
                    context.SaveChanges();
                }
                return Ok();
            }
            catch (ArgumentNullException ale)
            {
                return BadRequest("người dùng chưa đăng nhập!");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> History(int page = 1)
        {
            try
            {
                int userId = int.Parse(HttpContext.Session.GetString("UserId"));


                var list = context.Orders
                    .Where(o => o.UserId == userId)
                    .Include(o => o.OrderItems).ThenInclude(o => o.Product).Skip((page - 1) * 5).Take(5).ToList();

                ViewData["currentPage"] = page;
                int maxPage = context.Orders.Count() / 5 + (context.Orders.Count() % 5 == 0 ? 1 : 0);
                ViewData["maxPage"] = maxPage;

                return View(list);
            }
            catch (ArgumentNullException ale)
            {
                return RedirectToAction("Login");
            }

        }
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        [HttpGet]
        public IActionResult Information()
        {
            return View();
        }
    }
}
