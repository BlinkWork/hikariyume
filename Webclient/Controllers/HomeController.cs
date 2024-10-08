using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Webclient.Models;

namespace Webclient.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HikariYumeContext context = new HikariYumeContext();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var list = context.Products.Where(p => p.StockQuantity > 0).OrderBy(p => p.CreatedAt).Take(3).ToList();
            bool? isPwdChange = TempData["isPwdChange"] != null ? (bool)TempData["isPwdChange"] : null;
            if (isPwdChange != null)
            {
                ViewData["isChangeSuccess"] = isPwdChange;
            }
            ViewData["list"] = list;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
