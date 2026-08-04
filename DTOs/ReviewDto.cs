namespace otelrezervation.DTOs;

public class ReviewDto 
{
    public string KullaniciAdSoyad { get; set; } = string.Empty;
    public int Puan { get; set; }
    public string Yorum { get; set; } = string.Empty;
    public DateTime Tarih { get; set; }
}
