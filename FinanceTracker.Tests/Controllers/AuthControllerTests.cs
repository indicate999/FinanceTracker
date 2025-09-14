using Xunit;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // Needed for IdentityResult
using Microsoft.AspNetCore.Mvc;
using FinanceTracker.BusinessLogic.DTOs.Auth;
using FinanceTracker.BusinessLogic.Interfaces.Services;
using FinanceTracker.Controllers;

namespace FinanceTracker.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AuthController _authController;
    private readonly string _testUserId = "test-user-id";

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _authController = new AuthController(_mockAuthService.Object);

        // Setup a mock HttpContext.User for controller actions that need user claims
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId),
        }, "mock"));

        _authController.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task Register_ReturnsOkResult_OnSuccess()
    {
        // Arrange
        var registerDto = new RegisterDto { Username = "testuser", Password = "Password123!", DisplayName = "Test User" };
        var successMessage = "User created successfully.";
        _mockAuthService.Setup(s => s.RegisterAsync(registerDto))
            .ReturnsAsync((IdentityResult.Success, successMessage)); // Ensure message is returned

        // Act
        var result = await _authController.Register(registerDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value); // Value should not be null now

        var returnedData = okResult.Value as IDictionary<string, object>;
        Assert.NotNull(returnedData); // Ensure cast was successful

        Assert.True(returnedData.ContainsKey("message"));
        Assert.Equal(successMessage, returnedData["message"]);

        _mockAuthService.Verify(s => s.RegisterAsync(registerDto), Times.Once);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_OnServiceFailure()
    {
        // Arrange
        var registerDto = new RegisterDto { Username = "testuser", Password = "Password123!", DisplayName = "Test User" };
        var identityErrors = new List<IdentityError> { new IdentityError { Code = "DuplicateUserName", Description = "Username already taken." } };
        _mockAuthService.Setup(s => s.RegisterAsync(registerDto))
                        .ReturnsAsync((IdentityResult.Failed(identityErrors.ToArray()), null));

        // Act
        var result = await _authController.Register(registerDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);
        Assert.True(modelState.ContainsKey("DuplicateUserName"));
        _mockAuthService.Verify(s => s.RegisterAsync(registerDto), Times.Once);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_OnInvalidModelState()
    {
        // Arrange
        var registerDto = new RegisterDto { Username = "tu", Password = "", DisplayName = "" }; // Invalid DTO
        _authController.ModelState.AddModelError("Username", "Username too short.");
        _authController.ModelState.AddModelError("Password", "Password is required.");

        // Act
        var result = await _authController.Register(registerDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _mockAuthService.Verify(s => s.RegisterAsync(It.IsAny<RegisterDto>()), Times.Never); // Service should not be called
    }

    [Fact]
    public async Task Login_ReturnsOkResultWithToken_OnSuccess()
    {
        // Arrange
        var loginDto = new LoginDto { Username = "testuser", Password = "Password123!" };
        var token = "mock.jwt.token";
        _mockAuthService.Setup(s => s.LoginAsync(loginDto))
            .ReturnsAsync((token, null)); // Ensure token is returned

        // Act
        var result = await _authController.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value); // Value should not be null now

        var returnedData = okResult.Value as IDictionary<string, object>;
        Assert.NotNull(returnedData); // Ensure cast was successful

        Assert.True(returnedData.ContainsKey("token"));
        Assert.Equal(token, returnedData["token"]);

        _mockAuthService.Verify(s => s.LoginAsync(loginDto), Times.Once);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_OnServiceFailure()
    {
        // Arrange
        var loginDto = new LoginDto { Username = "testuser", Password = "wrongpassword" };
        var errorMessage = "Invalid login or password";
        _mockAuthService.Setup(s => s.LoginAsync(loginDto))
                        .ReturnsAsync((null, errorMessage));

        // Act
        var result = await _authController.Login(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(errorMessage, unauthorizedResult.Value);
        _mockAuthService.Verify(s => s.LoginAsync(loginDto), Times.Once);
    }

    [Fact]
    public async Task DeleteAccount_ReturnsNoContent_OnSuccess()
    {
        // Arrange
        _mockAuthService.Setup(s => s.DeleteAccountAsync(_testUserId))
                        .ReturnsAsync((IdentityResult.Success, null));

        // Act
        var result = await _authController.DeleteAccount();

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockAuthService.Verify(s => s.DeleteAccountAsync(_testUserId), Times.Once);
    }

    [Fact]
    public async Task DeleteAccount_ReturnsBadRequest_OnServiceFailure()
    {
        // Arrange
        var identityErrors = new List<IdentityError> { new IdentityError { Code = "Failed", Description = "Deletion failed." } };
        _mockAuthService.Setup(s => s.DeleteAccountAsync(_testUserId))
                        .ReturnsAsync((IdentityResult.Failed(identityErrors.ToArray()), "Deletion failed."));

        // Act
        var result = await _authController.DeleteAccount();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Deletion failed.", badRequestResult.Value);
        _mockAuthService.Verify(s => s.DeleteAccountAsync(_testUserId), Times.Once);
    }

    [Fact]
    public async Task DeleteAccount_ReturnsUnauthorized_WhenUserIdIsMissing()
    {
        // Arrange - Remove the user ID claim
        _authController.ControllerContext.HttpContext!.User = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = await _authController.DeleteAccount();

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("User ID not found in token.", (result as UnauthorizedObjectResult)?.Value);
        _mockAuthService.Verify(s => s.DeleteAccountAsync(It.IsAny<string>()), Times.Never); // Service should not be called
    }

    // Helper class to represent anonymous types for assertion
    private class AnonType
    {
        public string? message { get; set; }
        public string? token { get; set; }
    }
}
