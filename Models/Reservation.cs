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
}