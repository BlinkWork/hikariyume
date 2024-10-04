using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
