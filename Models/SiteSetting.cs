using System.ComponentModel.DataAnnotations;

namespace otelrezervation.Models;

public class SiteSetting
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Key { get; set; } = string.Empty;
    
    public string? Value { get; set; }

    [StringLength(50)]
    public string? PageName { get; set; } // Hangi sayfaya ait olduğunu belirler (Örn: Home, About)
}
