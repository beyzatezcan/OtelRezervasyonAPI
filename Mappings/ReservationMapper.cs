using Riok.Mapperly.Abstractions;
using otelrezervation.Models;
using otelrezervation.DTOs;

namespace otelrezervation.Mappings;

[Mapper]
public partial class ReservationMapper
{
    public partial Reservation CreateReservationDtoToReservation(CreateReservationDto dto);

    // Riok.Mapperly bazi ozel birlestirme (Ad+Soyad) islemlerinde bizim yazdigimiz metotlari kullanabilir.
    public ReservationDto ReservationToReservationDto(Reservation r)
    {
        return new ReservationDto
        {
            Id = r.Id,
            OdaNumarasi = r.Room?.OdaNumarasi ?? string.Empty,
            MusteriAdi = r.User != null ? $"{r.User.Ad} {r.User.Soyad}" : string.Empty,
            GirisTarihi = r.GirisTarihi,
            CikisTarihi = r.CikisTarihi,
            OzelIstekler = r.OzelIstekler,
            Status = r.Status
        };
    }
}
