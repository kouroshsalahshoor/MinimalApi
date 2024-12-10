using FluentValidation;
using MinimalApi.Models.Dtos;

namespace MinimalApi.Validations
{
    public class CategoryCreateValidation : AbstractValidator<CategoryCreateDto>
    {
        public CategoryCreateValidation() {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}
