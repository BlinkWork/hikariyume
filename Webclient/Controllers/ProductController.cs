using Microsoft.AspNetCore.Mvc;

namespace Webclient.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index(string search, string categoryId, string page = "1", string option = "default")
        {
            switch (categoryId)
            {
                case "1":
                    {
                        ViewData["categoryName"] = "Bát";
                        break;
                    }
                case "2":
                    {
                        ViewData["categoryName"] = "Đĩa";
                        break;
                    }
                case "3":
                    {
                        ViewData["categoryName"] = "Cốc";
                        break;
                    }
                default:
                    {
                        ViewData["categoryName"] = "Sản phẩm";
                        break;
                    }

            }
            return View();
        }

        public IActionResult Detail()
        {
            return View();
        }
    }
}
