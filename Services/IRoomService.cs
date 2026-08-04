using otelrezervation.DTOs;

namespace otelrezervation.Services;

public interface IRoomService
{
    // Tum odalari listele
    Task<List<RoomDto>> GetAllRoomsAsync();

    // Filtreli oda listesi
    Task<List<RoomDto>> GetFilteredRoomsAsync(string? odaTipi, int? minKapasite, decimal? maxFiyat);

    // Id'ye gore tek bir oda getir
    Task<RoomDto?> GetRoomByIdAsync(int id);

    // Yeni oda ekle
    Task<RoomDto> CreateRoomAsync(CreateRoomDto dto);

    // Oda guncelle
    Task<RoomDto?> UpdateRoomAsync(int id, UpdateRoomDto dto);

    // Oda sil
    Task<bool> DeleteRoomAsync(int id);

    // V4 Dashboard İstatistikleri için
    Task<int> GetTotalRoomCountAsync();
}
