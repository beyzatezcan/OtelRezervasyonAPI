using System.ComponentModel.DataAnnotations;

namespace otelrezervation.DTOs;

// Kullanıcı GÜNCELLEME formu
// CreateUserDto ile aynı görünüyor ama ayrı tutuyoruz
// Çünkü ileride "güncelleme sırasında email değiştirilemez" gibi
// bir kural koymak istersen, buradan Email alanını çıkarırsın
public class UpdateUserDto
{
    [Required(ErrorMessage = "Ad alanı zorunludur.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Ad 2-50 karakter arasında olmalıdır.")]
    public string Ad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad alanı zorunludur.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Soyad 2-50 karakter arasında olmalıdır.")]
    public string Soyad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email alanı zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefon alanı zorunludur.")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası girin.")]
    public string Telefon { get; set; } = string.Empty;
}
