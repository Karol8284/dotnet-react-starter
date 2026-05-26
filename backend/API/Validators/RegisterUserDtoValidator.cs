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
            .EmailAddress().WithMessage("Email must be a valid email address");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must be at most 20 characters");

        RuleFor(x => x.Address)
            .MaximumLength(250).WithMessage("Address must be at most 250 characters");

        RuleFor(x => x.CreatedAt)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("CreatedAt cannot be in the future");
    }
}
