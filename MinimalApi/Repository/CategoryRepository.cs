using Microsoft.EntityFrameworkCore;
using MinimalApi.Data;
using MinimalApi.Models;
using MinimalApi.Repository.IRepository;

namespace MinimalApi.Repository;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _db;

    public CategoryRepository(ApplicationDbContext db)
    {
        _db = db;
    }
    public async Task Create(Category model)
    {
        await _db.AddAsync(model);
        await _db.SaveChangesAsync();
    }

    public async Task Delete(Category model)
    {
        _db.Remove(model);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Category>> Get()
    {
        return await _db.Categories.AsNoTracking().ToListAsync();
    }

    public async Task<Category?> Get(int id)
    {
        return await _db.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Category?> Get(string name)
    {
        return await _db.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());
    }

    public async Task Update(Category model)
    {
        _db.Update(model);
        await _db.SaveChangesAsync();
    }
}
