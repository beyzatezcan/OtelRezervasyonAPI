namespace otelrezervation.Models;

public class Room
{
    public int Id { get; set; }
    public string OdaNumarasi { get; set; } = string.Empty;
    public decimal GecelikFiyat { get; set; }

    // entity frameworke iliskiyi anlatiyoruz 1:N
    // bir odanin birden fazla rezervasyonu olabilir
    public List<Reservation> Reservations { get; set; } = new();
}
