using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OKR_lr2_1.Models;
using System.Data;
using System.Diagnostics;

namespace OKR_lr2_1.Controllers
{
    public class HomeController : Controller
    {
        static double? savedResult;
        [HttpGet("Index")]
        [HttpGet("~/")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet("Calculate/{expression}")]
        public IActionResult Calculate(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                ViewBag.Result = "Вираз не може бути порожнім";
                return View();
            }
            Expression exp = new Expression { ExpressionText = expression };
            try
            {
                DataTable table = new DataTable();
                var result = (double?)table.Compute(expression, string.Empty);
                if (result == double.PositiveInfinity || result == double.NegativeInfinity || result == null) throw new DivideByZeroException();
                exp.Result = result;
                SaveResult(result);
            }
            catch (Exception ex)
            {
                exp.Error = ex.Message;
                exp.IsError = true;
            }
            ViewBag.Result = exp;
            return View("Index");
        }
        public void SaveResult(double? result) => savedResult = result;
        [HttpGet("GetSavedResult")]
        public IActionResult GetSavedResult()
        {
            ViewBag.Saved = savedResult;
            return View("Index");
        }
    }
}
