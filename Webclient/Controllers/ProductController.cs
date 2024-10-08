﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Webclient.Models;

namespace Webclient.Controllers
{
    public class ProductController : Controller
    {
        HikariYumeContext context = new HikariYumeContext();
        public IActionResult Index(string search, int? categoryId, int page = 1, string option = "default")
        {
            if (page <= 0)
            {
                return Index(search, categoryId, 1, "default");
            }
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
            List<Product> products = context.Products.OrderByDescending(m => m.StockQuantity).ToList();
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
            int maxPage = products.Count / 6;
            if (products.Count % 6 != 0)
            {
                maxPage++;
            }
            products = products.Skip((page - 1) * 6).Take(6).ToList();
            ViewData["maxDisplayProduct"] = products.Count;

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
            try
            {
                int userId = int.Parse(HttpContext.Session.GetString("UserId"));

                OrderItem orderItem = context.OrderItems.Include(oi => oi.Order).FirstOrDefault(oi => oi.ProductId == p.ProductId && oi.Order.UserId == userId);

                if ((orderItem != null) && (orderItem.Order.Status.Equals("Hoàn thành")) && (context.Reviews.FirstOrDefault(r => r.UserId == userId && r.ProductId == orderItem.ProductId) == null))
                {
                    ViewData["canReview"] = true;
                } else
                {
                    ViewData["canReview"] = false;
                }
            }
            catch
            {
                ViewData["canReview"] = false;
            }
            return View(p);

        }

        public IActionResult GetReview(int pId, int page = 1)
        {
            if (context.Products.FirstOrDefault(p => p.ProductId == pId) == null)
            {
                return NotFound();
            }
            List<Review> reviews = context.Reviews.Where(r => r.ProductId == pId).Include(r => r.User).Skip((page - 1) * 5).Take(5).ToList();
            List<ReviewDTO> reviewDTOs = reviews.Select(r => new ReviewDTO
            {
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                Rating = r.Rating,
                userFullname = r.User.FullName,
            }).OrderByDescending(p => p.CreatedAt).ToList();
            return Ok(reviewDTOs);
        }
        public IActionResult GetRemainReview(int pId, int page)
        {
            if (context.Products.FirstOrDefault(p => p.ProductId == pId) == null)
            {
                return NotFound();
            }
            int amount = context.Reviews.Where(r => r.ProductId == pId).Skip(page * 5).Count();
            return Ok(amount);
        }
        [HttpPost]
        public async Task<IActionResult> PostReview([FromBody] ReviewSubmit review)
        {
            try
            {
                Review newReview = new Review()
                {
                    UserId = review.UserId,
                    Comment = review.Comment,
                    ProductId = review.ProductId,
                    Rating = review.Rating
                };
                context.Reviews.Add(newReview);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
