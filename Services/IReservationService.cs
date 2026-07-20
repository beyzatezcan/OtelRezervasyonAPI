using otelrezervation.DTOs;

namespace otelrezervation.Services;

public interface IReservationService
{
    // Tüm rezervasyonları listele
    Task<List<ReservationDto>> GetAllReservationsAsync();

    // Yeni rezervasyon yap
    Task<ReservationDto> CreateReservationAsync(CreateReservationDto dto);

    // Rezervasyon iptal et (sil) - Eskiden bu özellik yoktu!
    Task<bool> DeleteReservationAsync(int id);
}
