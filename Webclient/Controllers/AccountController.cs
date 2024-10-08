using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using Webclient.Models;
using Microsoft.Win32;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Webclient.Controllers
{
    public class AccountController : Controller
    {
        HikariYumeContext context = new HikariYumeContext();
        public IActionResult Index()
        {
            try
            {
                int userId = int.Parse(HttpContext.Session.GetString("UserId"));
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }
                UserChangeInformation userChangeInformation = new UserChangeInformation()
                {
                    UserId = userId,
                    Address = user.Address,
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    Username = user.Username
                };
                return View("Infomration", userChangeInformation);
            }
            catch (ArgumentNullException)
            {
                return RedirectToAction("Login");
            }
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

                if (!BCrypt.Net.BCrypt.EnhancedVerify(loginModel.password, user.Password))
                {
                    ModelState.AddModelError("password", "Mật khẩu không chính xác");
                    return View();
                }

                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("UserName", user.Username);
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

                string emailVerificationToken = GenerateJwtToken(user);

                SendVerificationEmail(user.Email, emailVerificationToken);

                ViewBag.Message = "Vui lòng kiểm tra email để xác thực tài khoản.";
                return View();
            }
            return View();
        }
        private string GenerateJwtToken(RegisterModel user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("my_secret_key_hehehe_by_me_quan_32_ky_tu_cc");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim("Email", user.Email),
            new Claim("Password", BCrypt.Net.BCrypt.EnhancedHashPassword(user.Password,13)),
            new Claim("FullName", user.FullName),
            new Claim("Username", user.UserName),
            new Claim("PhoneNumber", user.PhoneNumber),
            new Claim("Address", user.Address)
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        private string GeneratePwdChange(string email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("my_secret_key_hehehe_by_me_quan_32_ky_tu_cc");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim("Email",email),
            new Claim("ChangePwd", "true"),
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        private void SendVerificationEmail(string email, string token)
        {
            string verificationLink = Url.Action("VerifyEmail", "Account", new { token = token }, Request.Scheme);

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
                                            Environment.GetEnvironmentVariable("GMAIL_PASSWORD"));
            smtpServer.EnableSsl = true;

            smtpServer.Send(mail);
        }
        private void SendChangePwdEmail(string email, string token)
        {
            string verificationLink = Url.Action("VerifyChangePwd", "Account", new { token = token }, Request.Scheme);

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("quannmhe171875@fpt.edu.vn");
            mail.To.Add(email);
            mail.Subject = "Xác thực thay đổi mật khẩu";
            mail.Body = $"Nhấn vào link để xác thực: <a href='{verificationLink}'>Thay đổi mật khẩu</a>";
            mail.IsBodyHtml = true;

            SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
            smtpServer.Port = 587;
            smtpServer.Credentials = new NetworkCredential(
                                            "quannmhe171875@fpt.edu.vn",
                                            Environment.GetEnvironmentVariable("GMAIL_PASSWORD"));
            smtpServer.EnableSsl = true;

            smtpServer.Send(mail);
        }
        [HttpGet]
        public IActionResult VerifyChangePwd(string token)
        {
            var userClaims = ValidateJwtToken(token);

            if (userClaims == null)
            {
                TempData["isPwdChange"] = false;

                return RedirectToAction("Index", "Home");
            }

            User newUser = context.Users.FirstOrDefault(u => u.Email.Equals(userClaims.FindFirst("Email").Value));
            if (newUser == null)
            {
                Console.WriteLine("87654321");
                TempData["isPwdChange"] = false;
                return RedirectToAction("Index", "Home");

            }

            ViewData["Email"] = userClaims.FindFirst("Email").Value.ToString();

            return View("ChangePasswordWT");
        }

        [HttpGet]
        public IActionResult VerifyEmail(string token)
        {
            var userClaims = ValidateJwtToken(token);

            if (userClaims == null)
            {
                ViewBag.Message = "Token không hợp lệ hoặc đã hết hạn.";
                return View("Register");
            }

            User newUser = new User
            {
                Email = userClaims.FindFirst("Email").Value,
                Password = userClaims.FindFirst("Password").Value,
                Username = userClaims.FindFirst("Username").Value,
                FullName = userClaims.FindFirst("FullName").Value,
                Address = userClaims.FindFirst("Address").Value,
                PhoneNumber = userClaims.FindFirst("PhoneNumber").Value,
                CreatedAt = DateTime.Now,
                Role = "user"
            };

            context.Users.Add(newUser);
            context.SaveChanges();

            ViewBag.Message = "Tài khoản của bạn đã được xác thực thành công! Vui lòng đăng nhập";
            return View("Login");
        }

        [HttpPost]
        public async Task<IActionResult> RequestForgetPassword(String email)
        {
            if (!context.Users.Any(u => u.Email == email))
            {
                ViewData["Error"] = "Email không gắn với bất kì tài khoản nào";
                return View("ForgetPassword");
            }

            string emailVerificationToken = GeneratePwdChange(email);

            SendChangePwdEmail(email, emailVerificationToken);

            ViewData["Information"] = "Đã gửi thành công vào email của bạn link thay đổi mật khẩu";
            return View("ForgetPassword");
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            try
            {
                ViewData["Email"] = HttpContext.Session.GetString("UserEmail");
                return View();
            }
            catch
            {
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel item)
        {
            try
            {
                User user = context.Users.FirstOrDefault(u => u.Email.Equals(item.Email));

                if (user == null)
                {
                    return NotFound();
                }
                if (user.Password.Length < 6)
                {
                    return BadRequest("mật khẩu chưa dài hơn 6 ký tự.");
                }
                user.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(item.Password, 13);
                context.Update(user);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
        [HttpPost]
        public async Task<IActionResult> ChangePasswordFromOld([FromBody] ChangeOldPasswordModel item)
        {
            try
            {
                User user = context.Users.FirstOrDefault(u => u.Email.Equals(item.Email));

                if (user == null)
                {
                    return NotFound();
                }

                if (!BCrypt.Net.BCrypt.EnhancedVerify(item.OldPassword, user.Password))
                {
                    return BadRequest("mật khẩu cũ không đúng.");
                }

                if (item.Password.Length < 6)
                {
                    return BadRequest("mật khẩu chưa dài hơn 6 ký tự.");
                }

                user.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(item.Password, 13);
                context.Update(user);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
        private ClaimsPrincipal ValidateJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("my_secret_key_hehehe_by_me_quan_32_ky_tu_cc");

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public IActionResult Cart()
        {
            try
            {
                int id = int.Parse(HttpContext.Session.GetString("UserId"));

                String address = context.Users.FirstOrDefault(u => u.UserId == id).Address;

                List<Cart> carts = context.Carts.Where(p => p.UserId == id).Include(c => c.Product).ToList();

                ViewData["Address"] = address;
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
                cart.Quantity = 1;
                Cart find = context.Carts.FirstOrDefault(c => c.ProductId == cart.ProductId);
                if (find != null)
                {
                    return BadRequest("Giỏ hàng đã tồn tại vật phẩm này");
                }
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

                    User user = context.Users.FirstOrDefault(u => u.UserId == id);

                    // Get cart items for the user
                    var cartItems = context.Carts.Where(c => c.UserId == id).Include(p => p.Product).ToList();

                    if (cartItems.Count == 0)
                    {
                        throw new Exception("giỏ hàng đang trống.");
                    }


                    // Check if product is still valid or not
                    foreach (Cart cart in cartItems)
                    {
                        var product = context.Products.FirstOrDefault(p => p.ProductId == cart.ProductId);
                        if (product != null && product.StockQuantity == 0)
                        {
                            throw new Exception($"mặt hàng {product.Name} đã hết.");
                        }
                    }

                    // Create a new Order
                    var order = new Order
                    {
                        UserId = id,
                        TotalPrice = cartItems.Sum(c => c.Quantity * c.Product.Price), // calculate total price
                        Status = "Đang chờ", // Default status
                        CreatedAt = DateTime.Now,
                        Address = user.Address
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
                        Product p = context.Products.FirstOrDefault(p => p.ProductId == cartItem.ProductId);
                        p.StockQuantity = 0;
                        context.Products.Update(p);
                    }


                    context.SaveChanges();
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
                if (page <= 0)
                {
                    return await History(1);
                }

                int userId = int.Parse(HttpContext.Session.GetString("UserId"));

                var list = context.Orders
                    .Where(o => o.UserId == userId).Include(o => o.OrderItems)
                    .Select(o => new OrderForHistory
                    {
                        OrderId = o.OrderId,
                        Address = o.Address,
                        CreatedAt = o.CreatedAt,
                        Status = o.Status,
                        TotalPrice = o.TotalPrice,
                        orderItemForHistories = context.OrderItems
                        .Where(oi => oi.OrderId == o.OrderId)
                        .Include(oi => oi.Product)
                        .Select(oi => new OrderItemForHistory
                        {
                            Product = new ProductForHistory
                            {
                                ProductId = oi.ProductId.GetValueOrDefault(),
                                Price = oi.Product.Price,
                                Age = oi.Product.Age,
                                Color = oi.Product.Color,
                                Image = oi.Product.Image,
                                Material = oi.Product.Material,
                                Name = oi.Product.Name,
                                Origin = oi.Product.Origin,
                                Size = oi.Product.Size,
                            },
                            Quantity = oi.Quantity,
                            HasReview = (o.Status.Equals("Hoàn thành")) && (context.Reviews.FirstOrDefault(r => r.ProductId == oi.ProductId && r.UserId == userId) != null)
                        }).ToList(),
                    }).Skip((page - 1) * 5).Take(5).ToList();

                ViewData["currentPage"] = page;
                int maxPage = context.Orders.Where(o => o.UserId == userId).Count() / 5 + (context.Orders.Where(o => o.UserId == userId).Count() % 5 == 0 ? 1 : 0);
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
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult Information()
        {
            try
            {
                int userId = int.Parse(HttpContext.Session.GetString("UserId"));
                User user = context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }
                UserChangeInformation userChangeInformation = new UserChangeInformation()
                {
                    UserId = userId,
                    Address = user.Address,
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    Username = user.Username
                };
                return View(userChangeInformation);
            }
            catch (ArgumentNullException)
            {
                return RedirectToAction("Login");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UserChangeInformation information)
        {
            User user = context.Users.FirstOrDefault(u => u.UserId == information.UserId);
            if (user != null)
            {
                user.Address = information.Address;
                user.FullName = information.FullName;
                user.PhoneNumber = information.PhoneNumber;
                user.Username = information.Username;
                try
                {
                    context.Update(user);
                    await context.SaveChangesAsync();
                    return Ok();
                }
                catch
                {
                    return BadRequest("Tên username đã có người dùng");
                }
            }
            else
            {
                return NotFound("Không tìm thấy người dùng");
            }
        }
        [HttpGet]
        public async Task<IActionResult> ForgetPassword()
        {
            return View();
        }
    }
}
