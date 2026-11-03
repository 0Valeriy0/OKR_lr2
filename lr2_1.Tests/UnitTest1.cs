using System;
using Microsoft.AspNetCore.Mvc;
using OKR_lr2_1.Controllers;
using OKR_lr2_1.Models;
using Xunit;

namespace lr2_1.Tests
{
    public class UnitTest1
    {
        //тестування математичних операцій
        [Fact]
        public void Calculate_ValidExpression_ReturnsViewWithResult()
        {
            // Arrange
            var controller = new HomeController();

            // Act
            IActionResult result = controller.Calculate("2 + 2");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var expressionModel = Assert.IsType<Expression>(viewResult.ViewData["Result"]);
            Assert.Equal(4, expressionModel.Result);
        }

        [Theory]
        [InlineData("2 * 3", 6)]
        [InlineData("10 - 3 * 2", 4)]
        [InlineData("(5 + 5) / 2", 5)]
        public void Calculate_ValidExpression_ReturnsCorrectResult(string expression, double expectedResult)
        {
            // Arrange
            var controller = new HomeController();

            // Act
            var result = controller.Calculate(expression) as ViewResult;

            // Assert
            Assert.NotNull(result);

            var expressionModel = result.ViewData["Result"] as Expression;
            Assert.NotNull(expressionModel);
            Assert.False(expressionModel.IsError);
            Assert.Equal(expectedResult, expressionModel.Result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Calculate_EmptyOrNullExpression_ReturnsEmptyMessage(string expression)
        {
            // Arrange
            var controller = new HomeController();

            // Act
            var result = controller.Calculate(expression) as ViewResult;

            // Assert
            Assert.NotNull(result);

            var expressionModel = result.ViewData["Result"] as Expression;
            if (expressionModel != null)
            {
                Assert.True(expressionModel.IsError);
                Assert.Equal("Вираз не може бути порожнім", expressionModel.Error?.Trim());
            }
            else
            {
                var actualMessage = result.ViewData["Result"]?.ToString()?.Trim();
                Assert.Equal("Вираз не може бути порожнім", actualMessage);
            }
        }

        [Fact]
        public void Calculate_SyntaxError_ReturnsSyntaxErrorMessage()
        {
            // Arrange
            var controller = new HomeController();

            // Act
            var result = controller.Calculate("2 ++ 3 *") as ViewResult;

            // Assert
            Assert.NotNull(result);

            var expressionModel = result.ViewData["Result"] as Expression;
            Assert.NotNull(expressionModel);

            Assert.True(expressionModel.IsError || !string.IsNullOrEmpty(expressionModel.Error));
        }

        [Fact]
        public void Calculate_DivisionByZero_ReturnsDivisionByZeroMessage()
        {
            // Arrange
            var controller = new HomeController();

            // Act
            var result = controller.Calculate("10 / 0") as ViewResult;

            // Assert
            Assert.NotNull(result);

            var expressionModel = result.ViewData["Result"] as Expression;
            Assert.NotNull(expressionModel);

            Assert.True(expressionModel.IsError || !string.IsNullOrEmpty(expressionModel.Error));
        }

        [Theory]
        [InlineData("8 - -4 * (2 + -3 * 2)", -8)]
        [InlineData("26 - 3.5 * 2.5 * 4", -9)]
        [InlineData("2.5 * 4.2 - 1.1 / 0.5", 8.3)]
        [InlineData(" -3 + -3 + -1 * -2 / -1", -8)]
        public void Calculate_ComplexExpression_ReturnsCorrectResult(string expression, double expectedResult)
        {
            // Arrange
            var controller = new HomeController();

            // Act
            var result = controller.Calculate(expression) as ViewResult;

            // Assert
            Assert.NotNull(result);

            var expressionModel = result.ViewData["Result"] as Expression;
            Assert.NotNull(expressionModel);
            Assert.False(expressionModel.IsError);
            Assert.Equal(expectedResult, expressionModel.Result);
        }

        [Fact]
        private void Calculate_ModulusOperation_ReturnsCorrectResult()
        {
            // Arrange
            var controller = new HomeController();
            // Act
            var result = controller.Calculate("10 % 3") as ViewResult;
            // Assert
            Assert.NotNull(result);
            var expressionModel = result.ViewData["Result"] as Expression;
            Assert.NotNull(expressionModel);
            Assert.False(expressionModel.IsError);
            Assert.Equal(1, expressionModel.Result);
        }

        //тестування памяті
        private HomeController CreateController()
        {
            var controller = new HomeController();
            controller.ControllerContext = new ControllerContext();
            return controller;
        }

        [Fact]
        public void MemoryPlus_AddsValueToMemory_AndRecallReturnsCorrectValue()
        {
            // Arrange
            var controller = CreateController();
            controller.ClearAll();

            // Act
            controller.MemoryPlus(15.5);
            controller.MemoryPlus(4.5);

            var result = controller.RecallClearMemory() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(20.0, result.ViewData["MemoryValue"]);
        }

        [Fact]
        public void RecallClearMemory_FirstCallRecalls_SecondCallClearsMemory()
        {
            // Arrange
            var controller = CreateController();
            controller.ClearAll();
            controller.MemoryPlus(50);

            // Act 1
            var firstCallResult = controller.RecallClearMemory() as ViewResult;

            // Act 2
            var secondCallResult = controller.RecallClearMemory() as ViewResult;

            // Assert 1
            Assert.Equal(50.0, firstCallResult?.ViewData["MemoryValue"]);
            Assert.Equal("Cleared", secondCallResult?.ViewData["MemoryState"]);

            // Act 3
            var thirdCallResult = controller.RecallClearMemory() as ViewResult;

            // Assert 2
            Assert.Equal(0.0, thirdCallResult?.ViewData["MemoryValue"]);
        }

        [Fact]
        public void MemoryOperations_PlusAndMinus_CalculatesCorrectly()
        {
            // Arrange
            var controller = CreateController();
            controller.ClearAll();

            // Act
            controller.MemoryPlus(100);
            controller.MemoryMinus(30);

            var result = controller.RecallClearMemory() as ViewResult;

            // Assert
            Assert.Equal(70.0, result?.ViewData["MemoryValue"]);
        }

        [Fact]
        public void MemoryRecallClear_PressAfterOtherOperations_ShouldRecallNotClear()
        {
            // Arrange
            var controller = CreateController();
            controller.ClearAll();

            double memoryValue = 100;
            controller.MemoryPlus(memoryValue);

            // Act 1
            controller.RecallClearMemory();

            // Act 2
            controller.Calculate("5 * 5");

            // Act 3
            var result = controller.RecallClearMemory() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(memoryValue, result.ViewData["MemoryValue"]);
            Assert.NotEqual("Cleared", result.ViewData["MemoryState"]);
        }

        [Fact]
        public void RecallClearMemory_PressAfterOtherOperations_ShouldKeepMemoryAndAppendToExpression()
        {
            // Arrange
            var controller = CreateController();
            controller.ClearAll();

            // 1. Додаємо 9 у пам'ять (M+)
            controller.MemoryPlus(9);

            // 2. Викликаємо пам'ять уперше (R•CM) — у вираз вставляється "9"
            var view1 = controller.RecallClearMemory() as ViewResult;
            var expr1 = view1?.ViewData["Result"] as Expression;

            // 3. Користувач вводить операцію додавання "+ 5" і обчислює (або продовжує ввід)
            controller.Calculate("9 + 5");

            // 4. Користувач готує наступну операцію, наприклад вводить "+"
            // і тисне R•CM ще раз (очікуючи підставку збереженої 9 з пам'яті: "9 + 5 + 9")
            var view2 = controller.RecallClearMemory() as ViewResult;
            var expr2 = view2?.ViewData["Result"] as Expression;

            // Assert: Перевіряємо, що після проміжного Calculate пам'ять не очистилася
            Assert.NotNull(expr2);
            Assert.Contains("9", expr2.ExpressionText);
            Assert.False(expr2.IsError);
        }

        [Fact]
        public void MemoryPlus_ExpressionNotNumber_ReturnsErrorOnScreen()
        {
            // Arrange
            var controller = CreateController();
            controller.ClearAll();

            // Act: спроба додати складний вираз "2 + 2" у пам'ять
            var result = controller.MemoryPlus(expression: "2 + 2") as ViewResult;

            // Assert
            Assert.NotNull(result);
            var expr = result.ViewData["Result"] as Expression;
            Assert.NotNull(expr);
            Assert.True(expr.IsError);
            Assert.Equal("Для роботи з пам'яттю потрібне число без інших виразів", expr.Error);
        }

        [Fact]
        public void RecallClearMemory_AppendsToExistingExpression_Within16Chars()
        {
            // Arrange
            var controller = CreateController();
            controller.ClearAll();
            controller.MemoryPlus(99);

            // Act: викликаємо пам'ять, коли на екрані вже є "10+"
            var result = controller.RecallClearMemory("10+") as ViewResult;

            // Assert
            Assert.NotNull(result);
            var expr = result.ViewData["Result"] as Expression;
            Assert.NotNull(expr);
            Assert.Equal("10+99", expr.ExpressionText);
            Assert.True(expr.ExpressionText.Length <= 16);
        }

        [Fact]
        public void RecallClearMemory_SecondCallAfterOperator_RecallsInsteadOfClearing()
        {
            // Arrange
            var controller = CreateController();
            controller.ClearAll();
            controller.MemoryPlus(7);

            // 1. Перше натискання R•CM -> отримали "7"
            var view1 = controller.RecallClearMemory("0") as ViewResult;
            var expr1 = view1?.ViewData["Result"] as Expression;
            Assert.Equal("7", expr1?.ExpressionText);

            // 2. Користувач ввів оператор "+" -> поточний рядок "7+"
            // Друге натискання R•CM з виразом "7+" (оскільки рядок змінився, це не очищення, а повторна вставка)
            var view2 = controller.RecallClearMemory("7+") as ViewResult;
            var expr2 = view2?.ViewData["Result"] as Expression;

            // Assert: пам'ять не очистилася, а додалася до рядка -> "7+7"
            Assert.NotNull(expr2);
            Assert.Equal("7+7", expr2.ExpressionText);
            Assert.NotEqual("Cleared", view2?.ViewData["MemoryState"]);
        }
    }
}