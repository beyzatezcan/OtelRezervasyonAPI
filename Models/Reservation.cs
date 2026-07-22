namespace otelrezervation.Models;

public class Reservation
{
    public int Id { get; set; }
    
    // baglanti noktalar (foreign keys)
    public int UserId { get; set; } // rezervasyonu hangi musteriyi yapti?
    public int RoomId { get; set; } // hangi oda rezerve edildi?
    
    // tarih bilgileri
    public DateTime GirisTarihi { get; set; }
    public DateTime CikisTarihi { get; set; }

    // navigation property'ler (iliskisel baglantilar)
    // bu sayede reservation uzerinden room ve user bilgilerine erisebiliriz
    // ornek: reservation.Room.OdaNumarasi
    public Room Room { get; set; } = null!;
    public User User { get; set; } = null!;
}