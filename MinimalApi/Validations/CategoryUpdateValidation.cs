using FluentValidation;
using MinimalApi.Models.Dtos;

namespace MinimalApi.Validations
{
    public class CategoryUpdateValidation : AbstractValidator<CategoryUpdateDto>
    {
        public CategoryUpdateValidation() {
            RuleFor(x => x.Id).NotEmpty().GreaterThan(0);
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}
