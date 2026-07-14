namespace otelrezervation.Models;

public class Room
{
    public int Id { get; set; }
    public string OdaNumarasi { get; set; } = string.Empty;
    public decimal GecelikFiyat { get; set; }

    // Entity Framework'e İlişkiyi Anlatıyoruz (1:N)
    public List<Reservation> Reservations { get; set; } = new();
}
