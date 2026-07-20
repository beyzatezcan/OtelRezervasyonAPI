namespace otelrezervation.DTOs;

// API'nin dışarıya döndüğü oda bilgisi (Çıkış DTO'su)
// Reservations listesi YOK — dışarıdan biri odanın
// tüm rezervasyonlarını görmemeli
public class RoomDto
{
    public int Id { get; set; }
    public string OdaNumarasi { get; set; } = string.Empty;
    public decimal GecelikFiyat { get; set; }
}
