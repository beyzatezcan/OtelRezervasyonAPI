using System.ComponentModel.DataAnnotations;

namespace otelrezervation.DTOs;

// Rezervasyon OLUŞTURMA formu
// Id YOK — veritabanı verir
// UserId ve RoomId VAR — sistem kimin hangi odayı istediğini bilmeli
// (İleride Authentication eklenince UserId buradan kalkacak,
//  sistem kullanıcıyı token'dan tanıyacak)
public class CreateReservationDto
{
    [Required(ErrorMessage = "Kullanıcı ID zorunludur.")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Oda ID zorunludur.")]
    public int RoomId { get; set; }

    [Required(ErrorMessage = "Giriş tarihi zorunludur.")]
    public DateTime GirisTarihi { get; set; }

    [Required(ErrorMessage = "Çıkış tarihi zorunludur.")]
    public DateTime CikisTarihi { get; set; }
}
