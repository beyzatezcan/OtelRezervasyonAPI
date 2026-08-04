namespace otelrezervation.DTOs;

// api'nin disariya dondurdugu oda bilgisi (cikis dto'su)
// reservations listesi yok disaridan biri odanin tum rezervasyonlarini gormemeli
public class RoomDto
{
    public int Id { get; set; }
    public string OdaNumarasi { get; set; } = string.Empty;
    public decimal GecelikFiyat { get; set; }
    public string OdaTipi { get; set; } = string.Empty;
    public int Kapasite { get; set; }
    public string? Aciklama { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    
    // Puanlama
    public double OrtalamaPuan { get; set; }
    public int YorumSayisi { get; set; }
    public string EnSonYorum { get; set; } = string.Empty;
    public List<ReviewDto> Yorumlar { get; set; } = new();
}
