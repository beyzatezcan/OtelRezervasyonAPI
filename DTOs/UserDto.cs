namespace otelrezervation.DTOs;

// API'nin dışarıya DÖNDÜĞÜ kullanıcı bilgisi (Çıkış DTO'su)
// Şu an entity ile aynı görünüyor ama ileride User modeline
// PasswordHash, TCKimlik gibi alanlar eklendiğinde
// bu DTO sayesinde o hassas bilgiler dışarıya sızmaz
public class UserDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
}
