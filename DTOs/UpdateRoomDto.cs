using System.ComponentModel.DataAnnotations;

namespace otelrezervation.DTOs;

// oda guncelleme formu
public class UpdateRoomDto
{
    [Required(ErrorMessage = "Oda numarası zorunludur.")]
    public string OdaNumarasi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gecelik fiyat zorunludur.")]
    [Range(1, 100000, ErrorMessage = "Gecelik fiyat 1-100000 arasında olmalıdır.")]
    public decimal GecelikFiyat { get; set; }

    [Required(ErrorMessage = "Oda tipi seçilmelidir.")]
    public string OdaTipi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kapasite belirtilmelidir.")]
    [Range(1, 10, ErrorMessage = "Kapasite 1-10 arasında olmalıdır.")]
    public int Kapasite { get; set; }

    public string? Aciklama { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public List<Microsoft.AspNetCore.Http.IFormFile>? ImageFiles { get; set; }
}
