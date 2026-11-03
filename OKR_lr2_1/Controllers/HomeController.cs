using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OKR_lr2_1.Models;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace OKR_lr2_1.Controllers
{
    public class HomeController : Controller
    {
        static double? savedResult;
        static double memory = 0;
        static bool isMemoryRecalled = false;
        static double taxRate = 0;
        static List<string> history = new List<string>();
        static int historyIndex = -1;

        [HttpGet("Index")]
        [HttpGet("~/")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("Calculate")]
        public IActionResult Calculate(string expression)
        {
            Expression exp = new Expression { ExpressionText = expression };

            try
            {
                double result = Evaluate(expression);

                if (double.IsInfinity(result) || double.IsNaN(result))
                {
                    throw new DivideByZeroException();
                }

                exp.Result = result;
                SaveResult(result);
                AddToHistory(expression);
            }
            catch (DivideByZeroException)
            {
                exp.Error = "Division by zero";
                exp.IsError = true;
            }
            catch (Exception ex)
            {
                exp.Error = ex.Message;
                exp.IsError = true;
            }

            ViewData["Result"] = exp;
            return View("Index");
        }

        private static double Evaluate(string? expression)
        {
            if (string.IsNullOrWhiteSpace(expression)) return 0;

            int pos = 0;
            string s = expression.Replace(" ", "").Replace(',', '.');

            double ParseExpression()
            {
                double val = ParseTerm();
                while (pos < s.Length)
                {
                    char op = s[pos];
                    if (op == '+' || op == '-')
                    {
                        pos++;
                        double next = ParseTerm();
                        val = op == '+' ? val + next : val - next;
                    }
                    else break;
                }
                return val;
            }

            double ParseTerm()
            {
                double val = ParseFactor();
                while (pos < s.Length)
                {
                    char op = s[pos];
                    if (op == '*' || op == '/' || op == '%')
                    {
                        pos++;
                        double next = ParseFactor();
                        if (op == '*')
                        {
                            val *= next;
                        }
                        else if (op == '/')
                        {
                            if (next == 0) throw new DivideByZeroException();
                            val /= next;
                        }
                        else
                        {
                            if (next == 0) throw new DivideByZeroException();
                            val %= next;
                        }
                    }
                    else break;
                }
                return val;
            }

            double ParseFactor()
            {
                if (pos < s.Length && s[pos] == '+') { pos++; return ParseFactor(); }
                if (pos < s.Length && s[pos] == '-') { pos++; return -ParseFactor(); }

                if (pos < s.Length && s[pos] == '(')
                {
                    pos++;
                    double val = ParseExpression();
                    if (pos < s.Length && s[pos] == ')')
                    {
                        pos++;
                    }
                    else
                    {
                        throw new FormatException("Missing closing parenthesis");
                    }
                    return val;
                }

                int start = pos;
                while (pos < s.Length && (char.IsDigit(s[pos]) || s[pos] == '.'))
                {
                    pos++;
                }

                if (start == pos) throw new FormatException("Invalid expression");

                return double.Parse(s.Substring(start, pos - start), CultureInfo.InvariantCulture);
            }

            double res = ParseExpression();
            if (pos < s.Length) throw new FormatException("Unexpected character: " + s[pos]);
            return res;
        }

        public void SaveResult(double? result) => savedResult = result;

        [HttpGet("GetSavedResult")]
        public IActionResult GetSavedResult()
        {
            ViewBag.Saved = savedResult;
            return View("Index");
        }

        private void AddToHistory(string expression)
        {
            history.Add(expression);
            historyIndex = history.Count - 1;
        }

        [HttpGet("PrevStep")]
        public IActionResult PrevStep()
        {
            if (historyIndex > 0) historyIndex--;
            ViewBag.HistoryExpression = history.Count > 0 ? history[historyIndex] : null;
            return View("Index");
        }

        [HttpGet("NextStep")]
        public IActionResult NextStep()
        {
            if (historyIndex < history.Count - 1) historyIndex++;
            ViewBag.HistoryExpression = history.Count > 0 ? history[historyIndex] : null;
            return View("Index");
        }

        [HttpGet("AutoReplay")]
        public IActionResult AutoReplay()
        {
            ViewBag.FullHistory = history;
            return View("Index");
        }

        [HttpPost("CorrectStep")]
        public IActionResult CorrectStep(int index, string newExpression)
        {
            if (index >= 0 && index < history.Count)
            {
                history[index] = newExpression;
            }
            return RedirectToAction("Index");
        }

        [HttpPost("MemoryPlus")]
        public IActionResult MemoryPlus(double value)
        {
            memory += value;
            isMemoryRecalled = false;
            return RedirectToAction("Index");
        }

        [HttpPost("MemoryMinus")]
        public IActionResult MemoryMinus(double value)
        {
            memory -= value;
            isMemoryRecalled = false;
            return RedirectToAction("Index");
        }

        [HttpGet("RecallClearMemory")]
        public IActionResult RecallClearMemory()
        {
            if (isMemoryRecalled)
            {
                memory = 0;
                isMemoryRecalled = false;
                ViewBag.MemoryState = "Cleared";
            }
            else
            {
                isMemoryRecalled = true;
                ViewBag.MemoryValue = memory;
            }
            return View("Index");
        }

        [HttpPost("SetTaxRate")]
        public IActionResult SetTaxRate(double rate)
        {
            taxRate = rate;
            return RedirectToAction("Index");
        }

        [HttpGet("RecallTaxRate")]
        public IActionResult RecallTaxRate()
        {
            ViewBag.TaxRate = taxRate;
            return View("Index");
        }

        [HttpPost("CalculateTax")]
        public IActionResult CalculateTax(double value, bool addTax)
        {
            double taxAmount = value * (taxRate / 100);
            ViewBag.TaxResult = addTax ? value + taxAmount : value - taxAmount;
            return View("Index");
        }

        [HttpPost("ClearAll")]
        public IActionResult ClearAll()
        {
            savedResult = null;
            history.Clear();
            historyIndex = -1;
            isMemoryRecalled = false;
            return RedirectToAction("Index");
        }
    }
}
