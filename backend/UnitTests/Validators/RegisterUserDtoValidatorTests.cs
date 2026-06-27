using API.Validators;
using Application.DTOs.Auth;
using FluentValidation.TestHelper;
using Xunit;

namespace UnitTests.Validators;

public class RegisterUserDtoValidatorTests
{
    private readonly RegisterUserDtoValidator _validator = new();

    [Fact]
    public void Should_have_error_when_required_fields_are_missing()
    {
        var dto = new RegisterUserDto
        {
            FirstName = string.Empty,
            LastName = string.Empty,
            Email = "invalid-email",
            Password = string.Empty,
            PhoneNumber = new string('9', 21),
            Address = new string('a', 251),
            CreatedAt = DateTime.UtcNow.AddDays(1)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.FirstName);
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
        result.ShouldHaveValidationErrorFor(x => x.Address);
        result.ShouldHaveValidationErrorFor(x => x.CreatedAt);
    }

    [Fact]
    public void Should_not_have_error_when_dto_is_valid()
    {
        var dto = new RegisterUserDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "password123",
            PhoneNumber = "1234567890",
            Address = "123 Main St",
            CreatedAt = DateTime.UtcNow
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
        result.ShouldNotHaveValidationErrorFor(x => x.Address);
        result.ShouldNotHaveValidationErrorFor(x => x.CreatedAt);
    }

    [Fact]
    public void Should_have_error_when_password_is_too_long()
    {
        var dto = new RegisterUserDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = new string('a', 129),
            CreatedAt = DateTime.UtcNow
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
