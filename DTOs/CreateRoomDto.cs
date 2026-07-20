using System.ComponentModel.DataAnnotations;

namespace otelrezervation.DTOs;

// Oda OLUŞTURMA formu
// Id YOK — veritabanı verir
// Reservations listesi YOK — oda yeni oluşuyor, henüz rezervasyon yok
public class CreateRoomDto
{
    [Required(ErrorMessage = "Oda numarası zorunludur.")]
    public string OdaNumarasi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gecelik fiyat zorunludur.")]
    [Range(1, 100000, ErrorMessage = "Gecelik fiyat 1-100000 arasında olmalıdır.")]
    public decimal GecelikFiyat { get; set; }
}
