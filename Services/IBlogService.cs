using otelrezervation.Models;

namespace otelrezervation.Services;

public interface IBlogService
{
    Task<List<Blog>> GetAllBlogsAsync();
    Task<Blog?> GetBlogByIdAsync(int id);
    Task<Blog> CreateBlogAsync(Blog blog);
    Task<Blog?> UpdateBlogAsync(int id, Blog updatedBlog);
    Task<bool> DeleteBlogAsync(int id);
}
