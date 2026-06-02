using API.Controllers;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnitTests.TestHelpers;
using Xunit;

namespace UnitTests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<ILogger<UsersController>> _loggerMock = new();
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _controller = new UsersController(_userServiceMock.Object, _loggerMock.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = ControllerTestHelper.CreateHttpContext(ControllerTestHelper.CreateAuthenticatedUser(Guid.NewGuid().ToString(), "user@test.com"))
        };
    }

    [Fact]
    public async Task GetUserById_Returns_ok_with_user_data()
    {
        var userId = Guid.NewGuid();
        var userEntity = new User { Id = userId, Email = "user@test.com", DisplayName = "John Doe", IsActive = true };

        _userServiceMock.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(ApiResponse<User>.Success(userEntity));

        var actionResult = await _controller.GetUserById(userId);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var response = Assert.IsType<ApiResponse<User>>(okResult.Value);

        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(userEntity.Email, response.Data?.Email);
    }

    [Fact]
    public async Task GetAllUsers_Returns_paged_user_list()
    {
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Email = "user1@test.com", DisplayName = "User One", IsActive = true },
            new() { Id = Guid.NewGuid(), Email = "user2@test.com", DisplayName = "User Two", IsActive = true }
        };

        _userServiceMock.Setup(x => x.GetAllUsersPagedAsync(1, 10)).ReturnsAsync(ApiResponse<List<User>>.Success(users));

        var actionResult = await _controller.GetAllUsers();

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var response = Assert.IsType<ApiResponse<List<User>>>(okResult.Value);

        Assert.Equal(2, response.Data?.Count);
    }

    [Fact]
    public async Task GetCount_Returns_ok_with_total_user_count()
    {
        _userServiceMock.Setup(x => x.GetUserCountAsync()).ReturnsAsync(ApiResponse<int>.Success(42));

        var actionResult = await _controller.GetCount();

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var response = Assert.IsType<ApiResponse<int>>(okResult.Value);

        Assert.Equal(42, response.Data);
    }

    [Fact]
    public async Task Delete_Returns_ok_when_user_is_deleted()
    {
        var userId = Guid.NewGuid();
        _userServiceMock.Setup(x => x.DeleteUserAsync(userId)).ReturnsAsync(ApiResponse<bool>.Success(true));

        var actionResult = await _controller.Delete(userId);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);

        Assert.True(response.Data);
    }

    [Fact]
    public async Task UpdateDisplayName_Returns_ok_when_updated()
    {
        var userId = Guid.NewGuid();
        var expected = new User { Id = userId, Email = "user@test.com", DisplayName = "New Name", IsActive = true };

        _userServiceMock.Setup(x => x.UpdateDisplayNameAsync(userId, "NewName")).ReturnsAsync(ApiResponse<User>.Success(expected));

        var actionResult = await _controller.UpdateDisplayName(userId, "NewName");

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var response = Assert.IsType<ApiResponse<User>>(okResult.Value);

        Assert.Equal(expected.Id, response.Data?.Id);
        Assert.Equal("New Name", response.Data?.DisplayName);
    }

    [Fact]
    public async Task UpdateRole_Returns_ok_when_role_updated()
    {
        var userId = Guid.NewGuid();
        var expected = new User { Id = userId, Email = "user@test.com", DisplayName = "John Doe", IsActive = true, Role = Domain.Enums.UserRole.Admin };

        _userServiceMock.Setup(x => x.UpdateUserRoleAsync(userId, "Admin")).ReturnsAsync(ApiResponse<User>.Success(expected));

        var actionResult = await _controller.UpdateRole(userId, "Admin");

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var response = Assert.IsType<ApiResponse<User>>(okResult.Value);

        Assert.Equal(expected.Id, response.Data?.Id);
        Assert.Equal(Domain.Enums.UserRole.Admin, response.Data?.Role);
    }
}
