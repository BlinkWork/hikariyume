using Microsoft.AspNetCore.Mvc;

namespace Webclient.Controllers
{
    public class AccountController : Controller
    {
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
    }
}
