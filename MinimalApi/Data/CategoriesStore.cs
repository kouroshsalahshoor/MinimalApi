using MinimalApi.Models;

namespace MinimalApi.Data
{
    public static class CategoriesStore
    {
        public static List<Category> Categories = new List<Category>()
        {
            new Category(){Id = 1,Name = 1.ToString(),CreatedBy = 1.ToString(),CreatedOn = DateTime.Now,},
            new Category(){Id = 2,Name = 2.ToString(),CreatedBy = 2.ToString(),CreatedOn = DateTime.Now,},
            new Category(){Id = 3,Name = 3.ToString(),CreatedBy = 3.ToString(),CreatedOn = DateTime.Now,},
        };

        //public static List<Category> Categories()
        //{
        //    var now = DateTime.Now;
        //    var items = new List<Category>();
        //    for (int i = 1; i <= 10; i++)
        //    {
        //        items.Add(new Category()
        //        {
        //            Id = i,
        //            Name = i.ToString(),
        //            CreatedBy = i.ToString(),
        //            CreatedOn = now
        //        });
        //    }
        //    return items;
        //}
    }
}
