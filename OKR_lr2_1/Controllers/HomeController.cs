using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace OKR_lr2_1.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
