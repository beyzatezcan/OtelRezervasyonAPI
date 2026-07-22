namespace otelrezervation.DTOs;

// api'nin disariya dondurdugu oda bilgisi (cikis dto'su)
// reservations listesi yok disaridan biri odanin tum rezervasyonlarini gormemeli
public class RoomDto
{
    public int Id { get; set; }
    public string OdaNumarasi { get; set; } = string.Empty;
    public decimal GecelikFiyat { get; set; }
}
