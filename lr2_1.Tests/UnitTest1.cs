using Microsoft.AspNetCore.Mvc;
using OKR_lr2_1.Controllers;
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
            Assert.Equal(4, viewResult.ViewData["Result"]);
        }
        [Theory]
        [InlineData("2 * 3", 6)]
        [InlineData("10 - 3 * 2", 4)]
        [InlineData("(5 + 5) / 2", 5)]
        public void Calculate_ValidExpression_ReturnsCorrectResult(string expression, object expectedResult)
        {
            // Arrange
            var controller = new HomeController();

            // Act
            var result = controller.Calculate(expression) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult, result.ViewData["Result"]);
        }
    }
}