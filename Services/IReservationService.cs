using otelrezervation.DTOs;

namespace otelrezervation.Services;

public interface IReservationService
{
    // tum rezervasyonlari listele
    Task<List<ReservationDto>> GetAllReservationsAsync();

    // yeni rezervasyon yap
    Task<ReservationDto> CreateReservationAsync(CreateReservationDto dto);

    // rezervsyon iptal et (sil) 
    Task<bool> DeleteReservationAsync(int id);

    // Dashboard İstatistikleri icin
    Task<int> GetTotalReservationCountAsync();
}
