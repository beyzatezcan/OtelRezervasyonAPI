using System;
using System.ComponentModel.DataAnnotations;

namespace otelrezervation.Models; 

// blog veritabani icin blog modeli olusturduk. 
public class Blog
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Başlık zorunludur.")]
    [MaxLength(200)]
    public string Baslik { get; set; }

    [Required(ErrorMessage = "İçerik zorunludur.")]
    public string Icerik { get; set; }

    public string ResimUrl { get; set; } // blog kapak görseli icin 

    public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;
}
