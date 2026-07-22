namespace otelrezervation.DTOs;

// api'nin disariya dondurdugu kullanici bilgisi (cikis dto'su)
// su an entity ile ayni gorunuyor ama ileride user modeline
// passwordhash, tckimlik gibi alanlar eklendiginde
// bu dto sayesinde o hassas bilgiler disariya sizmaz
public class UserDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
}
