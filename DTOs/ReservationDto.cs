namespace otelrezervation.DTOs;

// API'nin disariya dondugu rezervasyon bilgisi (Cikis DTO'su)
// UserId YOK — baska musterilerin kim oldugu gizli kalmalı
// RoomId YOK — onun yerine oda numarasini gosteriyoruz 
public class ReservationDto
{
    public int Id { get; set; }             // Rezervasyon numarasi (takip icin)
    public string OdaNumarasi { get; set; } = string.Empty;  // RoomId yerine oda numarasi
    public string MusteriAdi { get; set; } = string.Empty;   // YENI: UserId yerine musterinin Ad+Soyad bilgisi
    public DateTime GirisTarihi { get; set; }
    public DateTime CikisTarihi { get; set; }
}
