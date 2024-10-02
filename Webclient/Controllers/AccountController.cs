using Microsoft.AspNetCore.Mvc;

namespace Webclient.Controllers
{
    public class AccountController : Controller
    {
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

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Cart()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> RemoveItemFromCart(int id)
        {
            return Ok();
        }

        [HttpGet] 
        public async Task<IActionResult> ProceedCheckout()
        {
            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> History(string page = "1")
        {
            return View();
        }
    }
}
