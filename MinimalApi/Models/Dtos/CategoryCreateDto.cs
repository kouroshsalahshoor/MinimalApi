using System.ComponentModel.DataAnnotations;

namespace MinimalApi.Models.Dtos
{
    public class CategoryCreateDto
    {
        //[Required]
        public string Name { get; set; } = default!;
    }
}
