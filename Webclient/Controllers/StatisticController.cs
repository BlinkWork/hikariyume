using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Webclient.Models;

namespace Webclient.Controllers
{
    public class StatisticController : Controller
    {
        private readonly HikariYumeContext _context;

        public StatisticController(HikariYumeContext context)
        {
            _context = context;
        }
        public bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "admin";
        }

        public IActionResult Dashboard()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            decimal totalMonth = _context.Orders
                .Where(o => o.Status == "Hoàn thành" && o.CreatedAt.HasValue &&
                    o.CreatedAt.Value.Month == DateTime.Now.Month &&
                    o.CreatedAt.Value.Year == DateTime.Now.Year)
                .Sum(o => o.TotalPrice);

            decimal totalLast7Days = _context.Orders
                .Where(o => o.Status == "Hoàn thành" &&
                    o.CreatedAt.HasValue &&
                    o.CreatedAt >= DateTime.Now.AddDays(-7))
                .Sum(o => o.TotalPrice);

            ViewBag.TotalMonth = totalMonth;
            ViewBag.TotalLast7Days = totalLast7Days;

            return View();
        }

        public IActionResult Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            var products = _context.Products.ToList();
            return View(products);
        }

        public IActionResult Create()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["categoryList"] = _context.Categories.Select(c => new Category
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
            }).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile ImageFile)
        {
            ModelState.Remove("Image");
            if (ImageFile == null || !(ImageFile.ContentType == "image/jpeg" || ImageFile.ContentType == "image/png"))
            {
                ModelState.AddModelError("Image", "Please upload a valid image (JPG or PNG).");
            }

            if (ModelState.IsValid)
            {
                if (ImageFile != null && (ImageFile.ContentType == "image/jpeg" || ImageFile.ContentType == "image/png"))
                {
                    var fileName = Path.GetFileNameWithoutExtension(ImageFile.FileName);
                    var extension = Path.GetExtension(ImageFile.FileName);
                    var newFileName = $"{fileName}_{DateTime.Now.Ticks}{extension}";

                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", newFileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    product.Image = newFileName;
                }

                product.CreatedAt = DateTime.Now;
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        public IActionResult Edit(int? id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            if (id == null)
            {
                return NotFound();
            }

            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    product.CreatedAt = DateTime.Now;
                    _context.Update(product);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.ProductId == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        public IActionResult Delete(int? id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            if (id == null)
            {
                return NotFound();
            }

            var product = _context.Products
                .Include(p => p.Reviews)
                .Include(p => p.Carts)
                .Include(p => p.OrderItems)
                .Include(p => p.Wishlists)
                .FirstOrDefault(m => m.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var product = _context.Products
                .Include(p => p.Reviews)
                .Include(p => p.Carts)
                .Include(p => p.OrderItems)
                .Include(p => p.Wishlists)
                .FirstOrDefault(m => m.ProductId == id);

            if (product != null)
            {
                _context.Reviews.RemoveRange(product.Reviews);
                _context.Carts.RemoveRange(product.Carts);
                _context.OrderItems.RemoveRange(product.OrderItems);
                _context.Wishlists.RemoveRange(product.Wishlists);
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = _context.Products.FirstOrDefault(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
}
