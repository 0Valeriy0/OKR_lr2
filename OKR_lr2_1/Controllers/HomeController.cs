using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;

namespace OKR_lr2_1.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Calculate(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                ViewBag.Result = "Вираз не може бути порожнім.";
                return View();
            }
            try
            {
                DataTable table = new DataTable();
                var result = table.Compute(expression, string.Empty);
                ViewBag.Result = result;
            }
            catch (DivideByZeroException)
            {
                ViewBag.Result = "Помилка: Ділення на нуль.";
            }
            catch (SyntaxErrorException)
            {
                ViewBag.Result = "Помилка: Неправильний синтаксис виразу.";
            }
            catch (Exception)
            {
                ViewBag.Result = "Помилка: Некоректний вираз.";
            }
            return View();
        }
    }
}
