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
        static string? lastRecalledExpression = null;
        static double taxRate = 0;
        static List<string> history = new List<string>();
        static int historyIndex = -1;

        public static bool HasMemory => memory != 0;

        [HttpGet("Index")]
        [HttpGet("~/")]
        public IActionResult Index()
        {
            ViewBag.HasMemory = memory != 0;
            return View();
        }

        [HttpGet("Calculate")]
        public IActionResult Calculate(string expression)
        {
            isMemoryRecalled = false;
            lastRecalledExpression = null;

            if (string.IsNullOrWhiteSpace(expression))
            {
                Expression emptyExp = new Expression
                {
                    ExpressionText = expression ?? "",
                    IsError = true,
                    Error = "Вираз не може бути порожнім"
                };
                ViewData["Result"] = emptyExp;
                ViewBag.HasMemory = memory != 0;
                return View("Index");
            }

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
            ViewBag.HasMemory = memory != 0;
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
            ViewBag.HasMemory = memory != 0;
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
            ViewBag.HasMemory = memory != 0;
            return View("Index");
        }

        [HttpGet("NextStep")]
        public IActionResult NextStep()
        {
            if (historyIndex < history.Count - 1) historyIndex++;
            ViewBag.HistoryExpression = history.Count > 0 ? history[historyIndex] : null;
            ViewBag.HasMemory = memory != 0;
            return View("Index");
        }

        [HttpGet("AutoReplay")]
        public IActionResult AutoReplay()
        {
            ViewBag.FullHistory = history;
            ViewBag.HasMemory = memory != 0;
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
        public IActionResult MemoryPlus(double? value = null, string? expression = null)
        {
            return HandleMemoryChange(value, expression, isPlus: true);
        }

        [HttpPost("MemoryMinus")]
        public IActionResult MemoryMinus(double? value = null, string? expression = null)
        {
            return HandleMemoryChange(value, expression, isPlus: false);
        }

        private IActionResult HandleMemoryChange(double? value, string? expression, bool isPlus)
        {
            // 1. Прямий виклик з юніт-тестів, якщо передано лише числовий аргумент value
            if (value.HasValue && string.IsNullOrEmpty(expression))
            {
                if (isPlus) memory += value.Value;
                else memory -= value.Value;

                isMemoryRecalled = false;
                lastRecalledExpression = null;
                return RedirectToAction("Index");
            }

            // 2. Виклик форми з інтерфейсу (передається expression з екрана)
            string raw = (expression ?? "").Trim().Replace(',', '.');

            // Перевіряємо, чи є вираз чистим одиночним числом без операторів
            if (Regex.IsMatch(raw, @"^[+-]?\d+(\.\d+)?$") && double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double num))
            {
                if (isPlus) memory += num;
                else memory -= num;

                isMemoryRecalled = false;
                lastRecalledExpression = null;

                ViewBag.MemoryValue = memory;
                ViewBag.HasMemory = memory != 0;
                ViewData["Result"] = new Expression
                {
                    ExpressionText = expression ?? "",
                    Result = num,
                    IsError = false
                };
                return View("Index");
            }
            else
            {
                // Не є одиночним числом: повідомляємо про це на екрані
                string errorMsg = "Для роботи з пам'яттю потрібне число без інших виразів";
                Expression exp = new Expression
                {
                    ExpressionText = expression ?? "",
                    IsError = true,
                    Error = errorMsg
                };
                ViewData["Result"] = exp;
                ViewBag.ErrorMessage = errorMsg;
                ViewBag.MemoryValue = memory;
                ViewBag.HasMemory = memory != 0;
                return View("Index");
            }
        }

        [HttpGet("RecallClearMemory")]
        public IActionResult RecallClearMemory(string? currentExpression = null)
        {
            // Друге поспіль натискання, якщо стан пам'яті вже викликаний і рядок між цим не редагувався
            bool isConsecutiveRecall = isMemoryRecalled && (currentExpression == null || currentExpression == lastRecalledExpression);

            if (isConsecutiveRecall)
            {
                // Очищення пам'яті
                memory = 0;
                isMemoryRecalled = false;
                lastRecalledExpression = null;

                ViewBag.MemoryState = "Cleared";
                ViewBag.HasMemory = false;

                Expression exp = new Expression
                {
                    ExpressionText = currentExpression ?? "0",
                    Result = 0,
                    IsError = false
                };
                ViewData["Result"] = exp;
                ViewBag.CurrentExpression = currentExpression ?? "0";
            }
            else
            {
                // Перший виклик пам'яті (або після інших операцій)
                isMemoryRecalled = true;
                string memStr = memory.ToString(CultureInfo.InvariantCulture);

                string newExpr;
                if (string.IsNullOrWhiteSpace(currentExpression)
                    || currentExpression == "0"
                    || currentExpression == "Помилка"
                    || currentExpression.StartsWith("Для роботи з пам"))
                {
                    newExpr = memStr;
                }
                else
                {
                    // Дописуємо значення пам'яті в рядок, не перевищуючи 16 символів
                    if ((currentExpression + memStr).Length <= 16)
                    {
                        newExpr = currentExpression + memStr;
                    }
                    else
                    {
                        int fit = 16 - currentExpression.Length;
                        newExpr = fit > 0 ? currentExpression + memStr.Substring(0, fit) : currentExpression;
                    }
                }

                lastRecalledExpression = newExpr;

                ViewBag.MemoryValue = memory;
                ViewBag.HasMemory = memory != 0;
                ViewBag.CurrentExpression = newExpr;

                Expression exp = new Expression
                {
                    ExpressionText = newExpr,
                    Result = memory,
                    IsError = false
                };
                ViewData["Result"] = exp;
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
            ViewBag.HasMemory = memory != 0;
            return View("Index");
        }

        [HttpPost("CalculateTax")]
        public IActionResult CalculateTax(double value, bool addTax)
        {
            double taxAmount = value * (taxRate / 100);
            ViewBag.TaxResult = addTax ? value + taxAmount : value - taxAmount;
            ViewBag.HasMemory = memory != 0;
            return View("Index");
        }

        [HttpPost("ClearAll")]
        public IActionResult ClearAll()
        {
            savedResult = null;
            history.Clear();
            historyIndex = -1;
            isMemoryRecalled = false;
            lastRecalledExpression = null;
            memory = 0;
            return RedirectToAction("Index");
        }
    }
}
