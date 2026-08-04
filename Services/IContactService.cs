using otelrezervation.Models;

namespace otelrezervation.Services;

public interface IContactService
{
    Task<List<ContactMessage>> GetAllMessagesAsync();
    Task<ContactMessage?> GetMessageByIdAsync(int id);
    Task<ContactMessage> CreateMessageAsync(ContactMessage message);
    Task<bool> MarkAsReadAsync(int id);
    Task<bool> DeleteMessageAsync(int id);
    Task<int> GetTotalMessageCountAsync();
}
