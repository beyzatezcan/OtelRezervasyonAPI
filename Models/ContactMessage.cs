using System;
using System.ComponentModel.DataAnnotations;

namespace otelrezervation.Models;

public class ContactMessage
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Ad ve Soyad zorunludur.")]
    [MaxLength(100)]
    public string AdSoyad { get; set; }

    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Mesaj alanı boş bırakılamaz.")]
    public string Mesaj { get; set; }

    public DateTime GonderilmeTarihi { get; set; } = DateTime.Now;

    public bool OkunduMu { get; set; } = false; // Admin panelinde kontrol etmek için
}
