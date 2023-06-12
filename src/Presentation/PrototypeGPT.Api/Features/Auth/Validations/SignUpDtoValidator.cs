using PrototypeGPT.Api.Features.Auth.Dtos;
using FluentValidation;

namespace PrototypeGPT.Api.Features.Auth.Validations;

public class SignUpDtoValidator : AbstractValidator<SignUpDto>
{
    public SignUpDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty();
        RuleFor(x => x.UserName).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}
