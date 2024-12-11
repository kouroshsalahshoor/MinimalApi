using MinimalApi.Models;

namespace MinimalApi.Repository.IRepository;

public interface ICategoryRepository
{
    Task<List<Category>> Get();
    Task<Category?> Get(int id);
    Task<Category?> Get(string name);
    Task Create(Category model);
    Task Update(Category model);
    Task Delete(Category model);
}
