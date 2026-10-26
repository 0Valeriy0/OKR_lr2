using System;
using Microsoft.AspNetCore.Mvc;
using OKR_lr2_1.Controllers;
using OKR_lr2_1.Models;
using Xunit;

namespace lr2_1.Tests
{
    public class UnitTest1
    {
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
    }
}