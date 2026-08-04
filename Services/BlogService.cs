using Microsoft.EntityFrameworkCore;
using otelrezervation.Models;

namespace otelrezervation.Services;

public class BlogService : IBlogService
{
    private readonly AppDbContext _context;

    public BlogService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Blog>> GetAllBlogsAsync()
    {
        return await _context.Blogs
            .OrderByDescending(b => b.OlusturulmaTarihi)
            .ToListAsync();
    }

    public async Task<Blog?> GetBlogByIdAsync(int id)
    {
        return await _context.Blogs.FindAsync(id);
    }

    public async Task<Blog> CreateBlogAsync(Blog blog)
    {
        blog.OlusturulmaTarihi = DateTime.Now;
        _context.Blogs.Add(blog);
        await _context.SaveChangesAsync();
        return blog;
    }

    public async Task<Blog?> UpdateBlogAsync(int id, Blog updatedBlog)
    {
        var blog = await _context.Blogs.FindAsync(id);
        if (blog == null) return null;

        blog.Baslik = updatedBlog.Baslik;
        blog.Icerik = updatedBlog.Icerik;
        blog.ResimUrl = updatedBlog.ResimUrl;

        await _context.SaveChangesAsync();
        return blog;
    }

    public async Task<bool> DeleteBlogAsync(int id)
    {
        var blog = await _context.Blogs.FindAsync(id);
        if (blog == null) return false;

        _context.Blogs.Remove(blog);
        await _context.SaveChangesAsync();
        return true;
    }
}
