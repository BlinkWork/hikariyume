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
                    Console.WriteLine("DCDCDCD");
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

                //int id = int.Parse(HttpContext.Session.GetString("UserId"));
                //if (cartItems == null || cartItems.Count == 0)
                //{
                //    throw new Exception("Giỏ hàng không có đồ");
                //}
                //decimal money = 0;
                //var list = new List<OrderItem>();
                //Order order = new Order()
                //{
                //    UserId = id,
                //    Status = "Đang chờ",
                //    TotalPrice = 0m,
                //};

                //context.Orders.Add(order);
                //await context.SaveChangesAsync();

                //Console.WriteLine(cartItems.Count());

                //int orderId = context.Orders.Last().OrderId;


                //foreach (Cart cart in cartItems)
                //{
                //    money += cart.Quantity * cart.Product.Price;
                //    OrderItem item = new OrderItem()
                //    {
                //        OrderId = orderId,
                //        ProductId = cart.ProductId,
                //        Price = cart.Product.Price,
                //        Quantity = cart.Quantity,
                //    };
                //    Product product = context.Products.FirstOrDefault(p => p.ProductId == cart.ProductId);
                //    if (product == null)
                //    {
                //        throw new Exception("Sản phẩm không tồn tại");
                //    }
                //    else
                //    {
                //        product.StockQuantity -= cart.Quantity;
                //        if (product.StockQuantity <= 0) throw new Exception("Số lượng sản phầm tồn kho không đúng");
                //    }
                //    context.Products.Update(product);
                //    await context.SaveChangesAsync();
                //    list.Add(item);
                //}

                //order.TotalPrice = money;
                //context.OrderItems.AddRange(list);
                //await context.SaveChangesAsync();
                //context.Carts.RemoveRange(cartItems);
                //await context.SaveChangesAsync();
                //context.Orders.Update(order);
                //await context.SaveChangesAsync();
                //return Ok("Đặt hàng thành công!");
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
                    //.Where(o => o.UserId == userId)
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
    }
}
