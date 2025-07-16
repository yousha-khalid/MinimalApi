using FluentValidation;
using MinimalApi.Model;

namespace MinimalApi.Validation
{
    public class UserValidation : AbstractValidator<UserModel>
    {
        public UserValidation() { 
            RuleFor(u=> u.UserName).NotEmpty().MaximumLength(10).Matches(@"^[a-zA-Z0-9_]+$").
                WithMessage("Can only contains Alphanumeric and be less than 10 characters");
            RuleFor(u=> u.Password).NotEmpty().MinimumLength(8);
        }
    }
}
