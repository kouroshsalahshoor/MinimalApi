using MinimalApi.Models;

namespace MinimalApi.Data
{
    public static class CategoriesStore
    {
        public static List<Category> Categories()
        {
            var now = DateTime.Now;
            var items = new List<Category>();
            for (int i = 1; i <= 10; i++)
            {
                items.Add(new Category()
                {
                    Id = i,
                    Name = i.ToString(),
                    CreatedBy = i.ToString(),
                    CreatedOn = now
                });
            }
            return items;
        }
    }
}
