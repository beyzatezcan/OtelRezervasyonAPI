namespace otelrezervation.Models;

public class User
{
    public int Id { get; set; }
    
   
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;

    // Navigation Property (1:N) — Bir müşterinin birden çok rezervasyonu olabilir
    public List<Reservation> Reservations { get; set; } = new();
}