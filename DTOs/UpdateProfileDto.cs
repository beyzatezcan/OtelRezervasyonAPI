using System.ComponentModel.DataAnnotations; // data annotations sayesinde Required, Phone, Compare gibi özellikler kullanıldı
// asagidaki her bir property yukaridaki .net kütüphanesinden alinan bir attribute ile guncellenebilir

namespace otelrezervation.DTOs;

public class UpdateProfileDto
{
    [Required(ErrorMessage = "Ad alanı zorunludur.")] 
    public string Ad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad alanı zorunludur.")]
    public string Soyad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefon alanı zorunludur.")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    public string Telefon { get; set; } = string.Empty;

    public string? CurrentPassword { get; set; }
    
    [MinLength(6, ErrorMessage = "Yeni şifre en az 6 karakter olmalıdır.")]
    public string? NewPassword { get; set; }

    [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor.")]
    public string? ConfirmNewPassword { get; set; }
}
