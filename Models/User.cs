namespace otelrezervation.Models;

public class User
{
    public int Id { get; set; }
    
   
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer";

    // Sifre sifirlama (Forgot Password) icin token
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }

    // entity frameworke iliskiyi anlatiyoruz 1:N
    // bir kullanicinin birden fazla rezervasyonu olabilir
    public List<Reservation> Reservations { get; set; } = new();
}