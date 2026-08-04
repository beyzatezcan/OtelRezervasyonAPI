using System.ComponentModel.DataAnnotations;

namespace otelrezervation.DTOs;

// rezervasyon olusturma formu
// userid ve roomid var sistem kimin hangi odayı istediğini bilmeli
// ileride Authentication eklenince userid buradan kalkacak, sistem kullaniciyi token'dan taniyacak
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

    public string? OzelIstekler { get; set; }
}
