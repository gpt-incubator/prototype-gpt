using PrototypeGPT.Api.Features.Auth.Dtos;
using FluentValidation;

namespace PrototypeGPT.Api.Features.Auth.Validations;

public class SignInDtoValidator : AbstractValidator<SignInDto>
{
    public SignInDtoValidator()
    {
        RuleFor(x => x.UserName).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}
