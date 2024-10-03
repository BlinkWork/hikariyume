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
            if (categoryId != null)
            {
                ViewData["categoryId"] = categoryId;
            }
            List<Product> products = context.Products.ToList();
            if (categoryId != null)
            {
                products = products.Where(p => p.CategoryId == categoryId).ToList();
            }
            if (!String.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.Name.Contains(search)).ToList();
                ViewData["search"] = search;
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
            ViewData["option"] = option;
            ViewData["minProduct"] = products.Count > 0 ? 1 : 0;
            ViewData["maxProduct"] = products.Count;
            products = products.Skip((page - 1) * 6).Take(6).ToList();
            ViewData["maxDisplayProduct"] = products.Count;
            int maxPage = products.Count / 6 ;
            if (products.Count % 6 != 0)
            {
                maxPage++;
            }
            ViewData["currentPage"] = page;
            ViewData["maxPage"] = maxPage;
            return View(products);
        }

        public IActionResult Details(int id)
        {
            Product p = context.Products.FirstOrDefault(p => p.ProductId == id);
            if (p == null)
            {
                return View("Index", "Home");
            }
            List<Product> relatedProduct = context.Products.Where(t => t.CategoryId == p.CategoryId).ToList();
            if (relatedProduct.Contains(p))
            {
                relatedProduct.Remove(p);
            }
            ViewData["relatedProducts"] = relatedProduct.Take(4).ToList();
            return View(p);
        }
    }
}
