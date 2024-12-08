namespace MinimalApi.Models;

public class Category :Audit
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
}
