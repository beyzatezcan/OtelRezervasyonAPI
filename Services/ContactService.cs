using Microsoft.EntityFrameworkCore;
using otelrezervation.Models;

namespace otelrezervation.Services;

public class ContactService : IContactService
{
    private readonly AppDbContext _context;

    public ContactService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ContactMessage>> GetAllMessagesAsync()
    {
        return await _context.ContactMessages
            .OrderByDescending(m => m.GonderilmeTarihi)
            .ToListAsync();
    }

    public async Task<ContactMessage?> GetMessageByIdAsync(int id)
    {
        return await _context.ContactMessages.FindAsync(id);
    }

    public async Task<ContactMessage> CreateMessageAsync(ContactMessage message)
    {
        message.GonderilmeTarihi = DateTime.Now;
        _context.ContactMessages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<bool> MarkAsReadAsync(int id)
    {
        var message = await _context.ContactMessages.FindAsync(id);
        if (message == null) return false;

        message.OkunduMu = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteMessageAsync(int id)
    {
        var message = await _context.ContactMessages.FindAsync(id);
        if (message == null) return false;

        _context.ContactMessages.Remove(message);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetTotalMessageCountAsync()
    {
        return await _context.ContactMessages.CountAsync();
    }
}
