namespace otelrezervation.Models;

public class Room
{
    public int Id { get; set; }
    public string OdaNumarasi { get; set; } = string.Empty;
    public decimal GecelikFiyat { get; set; }
    public string OdaTipi { get; set; } = "Standart";
    public int Kapasite { get; set; } = 2;
    public string? Aciklama { get; set; }
    public List<string>? ImageUrls { get; set; } = new(); // nullable yaptik cunku eger resim eklenmezse null olacak

    // entity frameworke iliskiyi anlatiyoruz 1:N
    // bir odanin birden fazla rezervasyonu olabilir
    public List<Reservation> Reservations { get; set; } = new();

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
