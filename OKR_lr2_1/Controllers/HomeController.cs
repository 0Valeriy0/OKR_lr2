using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace OKR_lr2_1.Controllers
{
    [Route("")]
    [Route("Home")]
    public class HomeController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
