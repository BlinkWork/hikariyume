using Microsoft.AspNetCore.Mvc;
using Webclient.Models;

namespace Webclient.Controllers
{
    public class ProductController : Controller
    {
        HikariYumeContext context = new HikariYumeContext();
        public IActionResult Index(string search, int? categoryId, int page = 1, string option = "default")
        {
            switch (categoryId)
            {
                case 1:
                    {
                        ViewData["categoryName"] = "Bát";
                        break;
                    }
                case 2:
                    {
                        ViewData["categoryName"] = "Đĩa";
                        break;
                    }
                case 3:
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
            List<Product> products = context.Products.ToList();
            if (categoryId != null)
            {
                products = products.Where(p => p.CategoryId == categoryId).ToList();
            }
            if (String.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.Name.Contains(search)).ToList();
            }
            switch (option)
            {
                case "asc":
                    products = products.OrderBy(p => p.Price).ToList();
                    break;
                case "desc":
                    products = products.OrderByDescending(p => p.Price).ToList();
                    break;
                case "new":
                    products = products.OrderBy(p => p.CreatedAt).ToList();
                    break;
                case "old":
                    products = products.OrderByDescending(p => p.CreatedAt).ToList();
                    break;
            }
            products = products.Skip((page - 1) * 6).Take(6).ToList();
            int maxPage = products.Count / 6 ;
            if (products.Count % 6 != 0)
            {
                maxPage++;
            }
            ViewData["maxPage"] = maxPage;
            return View(products);
        }

        public IActionResult Detail(int id)
        {
            Product p = context.Products.FirstOrDefault(p => p.ProductId == id);
            if (p == null)
            {
                return View("Index", "Home");
            }
            return View(p);
        }
    }
}
