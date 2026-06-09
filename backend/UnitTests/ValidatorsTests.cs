using API.Validators;
using Application.DTOs.Auth;
using FluentValidation.TestHelper;
using Xunit;

namespace UnitTests;

public class ValidatorsTests
{
    [Fact]
    public void LoginUserDtoValidator_Should_HaveErrors_When_EmailOrPasswordIsMissing()
    {
        var validator = new LoginUserDtoValidator();
        var dto = new LoginUserDto
        {
            Email = string.Empty,
            Password = string.Empty
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void LoginUserDtoValidator_Should_NotHaveErrors_When_Valid()
    {
        var validator = new LoginUserDtoValidator();
        var dto = new LoginUserDto
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var result = validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void RegisterUserDtoValidator_Should_HaveErrors_When_RequiredFieldsAreMissing()
    {
        var validator = new RegisterUserDtoValidator();
        var dto = new RegisterUserDto
        {
            FirstName = string.Empty,
            LastName = string.Empty,
            Email = "invalid-email",
            PhoneNumber = new string('x', 25),
            Address = new string('x', 260),
            CreatedAt = System.DateTime.UtcNow.AddDays(1)
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.FirstName);
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
        result.ShouldHaveValidationErrorFor(x => x.Address);
        result.ShouldHaveValidationErrorFor(x => x.CreatedAt);
    }

    [Fact]
    public void RegisterUserDtoValidator_Should_NotHaveErrors_When_Valid()
    {
        var validator = new RegisterUserDtoValidator();
        var dto = new RegisterUserDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "123456789",
            Address = "123 Main St",
            CreatedAt = System.DateTime.UtcNow
        };

        var result = validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
        result.ShouldNotHaveValidationErrorFor(x => x.Address);
        result.ShouldNotHaveValidationErrorFor(x => x.CreatedAt);
    }
}
