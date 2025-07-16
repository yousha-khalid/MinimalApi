using FluentValidation;
using MinimalApi.Model;

namespace MinimalApi.Validation
{
    public class TodoValidation : AbstractValidator<TodoModel>
    {
        public TodoValidation()
        {
            RuleFor(u => u.Title).NotEmpty();
            RuleFor(u => u.IsCompleted).NotEmpty();
        }
    }
}
