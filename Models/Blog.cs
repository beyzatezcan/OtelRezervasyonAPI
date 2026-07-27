using System;
using System.ComponentModel.DataAnnotations;

namespace otelrezervation.Models;

public class Blog
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Başlık zorunludur.")]
    [MaxLength(200)]
    public string Baslik { get; set; }

    [Required(ErrorMessage = "İçerik zorunludur.")]
    public string Icerik { get; set; }

    public string ResimUrl { get; set; } // Blog kapak görseli için (opsiyonel)

    public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;
}
