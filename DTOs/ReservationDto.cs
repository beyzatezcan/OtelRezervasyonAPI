namespace otelrezervation.DTOs;

// API'nin dışarıya döndüğü rezervasyon bilgisi (Çıkış DTO'su)
// UserId YOK — başka müşterilerin kim olduğu gizli kalmalı
// RoomId YOK — onun yerine oda numarasını gösteriyoruz (daha anlamlı)
public class ReservationDto
{
    public int Id { get; set; }             // Rezervasyon numarası (takip için)
    public string OdaNumarasi { get; set; } = string.Empty;  // RoomId yerine oda numarası
    public string MusteriAdi { get; set; } = string.Empty;   // YENİ: UserId yerine müşterinin Ad+Soyad bilgisi
    public DateTime GirisTarihi { get; set; }
    public DateTime CikisTarihi { get; set; }
}
