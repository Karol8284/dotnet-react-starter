using API.Validators;
using Application.DTOs.Auth;
using FluentValidation.TestHelper;
using Xunit;

namespace UnitTests.Validators;

public class LoginUserDtoValidatorTests
{
    private readonly LoginUserDtoValidator _validator = new();

    [Fact]
    public void Should_have_error_when_email_is_empty()
    {
        var dto = new LoginUserDto { Email = string.Empty, Password = "password123" };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_have_error_when_password_is_too_short()
    {
        var dto = new LoginUserDto { Email = "user@test.com", Password = "123" };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_have_error_when_password_is_too_long()
    {
        var dto = new LoginUserDto { Email = "user@test.com", Password = new string('a', 129) };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_have_error_when_email_is_too_long()
    {
        var dto = new LoginUserDto { Email = $"{new string('a', 249)}@test.com", Password = "password123" };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_not_have_error_when_dto_is_valid()
    {
        var dto = new LoginUserDto { Email = "user@test.com", Password = "password123" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}
