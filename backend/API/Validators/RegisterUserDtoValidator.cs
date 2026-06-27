using Application.DTOs.Auth;
using FluentValidation;
using System;

namespace API.Validators;

public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must be at most 100 characters");

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Last name must be at most 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(256).WithMessage("Email must be at most 256 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long")
            .MaximumLength(128).WithMessage("Password must be at most 128 characters");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must be at most 20 characters");

        RuleFor(x => x.Address)
            .MaximumLength(250).WithMessage("Address must be at most 250 characters");

        RuleFor(x => x.CreatedAt)
            .Must(createdAt => createdAt <= DateTime.UtcNow)
            .WithMessage("CreatedAt cannot be in the future");
    }
}
