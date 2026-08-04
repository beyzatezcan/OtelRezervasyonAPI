using System;
using System.ComponentModel.DataAnnotations;

namespace otelrezervation.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int RoomId { get; set; }
        public Room Room { get; set; }

        [Required(ErrorMessage = "Lütfen 1 ile 5 arası bir puan veriniz.")]
        [Range(1, 5, ErrorMessage = "Puan 1 ile 5 arasında olmalıdır.")]
        public int Puan { get; set; }

        public string Yorum { get; set; }

        public DateTime Tarih { get; set; } = DateTime.Now;

        // admin onayından sonra sitede gosteriyoruz 
        public bool IsApproved { get; set; } = true;
    }
}
