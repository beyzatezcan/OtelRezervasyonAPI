namespace otelrezervation.Models;

public class Reservation
{
    public int Id { get; set; }
    
    // Bağlantı Noktaları (Foreign Keys)
    public int UserId { get; set; } // Rezervasyonu HANGİ MÜŞTERİ yaptı?
    public int RoomId { get; set; } // HANGİ ODA rezerve edildi?
    
    // Tarih Bilgileri
    public DateTime GirisTarihi { get; set; }
    public DateTime CikisTarihi { get; set; }

    // Navigation Property'ler (İlişkisel Bağlantılar)
    // Bu sayede Reservation üzerinden Room ve User bilgilerine erişebiliriz
    // Örnek: reservation.Room.OdaNumarasi
    public Room Room { get; set; } = null!;
    public User User { get; set; } = null!;
}