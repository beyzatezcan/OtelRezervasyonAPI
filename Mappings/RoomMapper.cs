using Riok.Mapperly.Abstractions;
using otelrezervation.Models;
using otelrezervation.DTOs;

namespace otelrezervation.Mappings;

[Mapper]
public partial class RoomMapper
{
    public RoomDto RoomToRoomDto(Room room)
    {
        return new RoomDto
        {
            Id = room.Id,
            OdaNumarasi = room.OdaNumarasi,
            GecelikFiyat = room.GecelikFiyat,
            OdaTipi = room.OdaTipi,
            Kapasite = room.Kapasite,
            Aciklama = room.Aciklama,
            ImageUrls = room.ImageUrls,
            OrtalamaPuan = room.Reviews != null && room.Reviews.Any() ? Math.Round(room.Reviews.Average(r => r.Puan), 1) : 0,
            YorumSayisi = room.Reviews != null ? room.Reviews.Count : 0,
            EnSonYorum = room.Reviews != null && room.Reviews.Any() 
                ? room.Reviews.OrderByDescending(r => r.Tarih).First().Yorum 
                : string.Empty,
            Yorumlar = room.Reviews != null ? room.Reviews.Where(r => r.IsApproved).OrderByDescending(r => r.Tarih).Select(r => new ReviewDto
            {
                KullaniciAdSoyad = r.User != null ? r.User.Ad + " " + r.User.Soyad : "Gizli Kullanıcı",
                Puan = r.Puan,
                Yorum = r.Yorum,
                Tarih = r.Tarih
            }).ToList() : new List<ReviewDto>()
        };
    }
    
    public partial Room CreateRoomDtoToRoom(CreateRoomDto dto);
    public partial void UpdateRoomFromDto(UpdateRoomDto dto, Room room);
}
