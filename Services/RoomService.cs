using Microsoft.EntityFrameworkCore;
using otelrezervation.DTOs;
using otelrezervation.Models;

namespace otelrezervation.Services;

public class RoomService : IRoomService
{
    private readonly AppDbContext _context;
    private readonly Mappings.RoomMapper _mapper = new();

    public RoomService(AppDbContext context)
    {
        _context = context;
    }

    // 1. TUM ODALARI LISTELE (V3: Puan sırasına göre)
    public async Task<List<RoomDto>> GetAllRoomsAsync()
    {
        var rooms = await _context.Rooms
            .Include(r => r.Reviews)
            .ToListAsync();

        return rooms.Select(r => _mapper.RoomToRoomDto(r))
                    .OrderByDescending(r => r.OrtalamaPuan)
                    .ToList();
    }

    public async Task<List<RoomDto>> GetFilteredRoomsAsync(string? odaTipi, int? minKapasite, decimal? maxFiyat)
    {
        var query = _context.Rooms.Include(r => r.Reviews).AsQueryable();

        if (!string.IsNullOrEmpty(odaTipi))
        {
            query = query.Where(r => r.OdaTipi == odaTipi);
        }

        if (minKapasite.HasValue)
        {
            query = query.Where(r => r.Kapasite >= minKapasite.Value);
        }

        if (maxFiyat.HasValue)
        {
            query = query.Where(r => r.GecelikFiyat <= maxFiyat.Value);
        }

        var rooms = await query.ToListAsync();

        return rooms.Select(r => _mapper.RoomToRoomDto(r))
                    .OrderByDescending(r => r.OrtalamaPuan)
                    .ToList();
    }

    // 2. ID'YE GORE TEK ODA GETIR
    public async Task<RoomDto?> GetRoomByIdAsync(int id)
    {
        var room = await _context.Rooms
            .Include(r => r.Reviews)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null) return null;

        return _mapper.RoomToRoomDto(room);
    }

    // 3. YENI ODA EKLE
    public async Task<RoomDto> CreateRoomAsync(CreateRoomDto dto)
    {
        // GUVENLIK DUVARI: Bu oda numarasi var mi?
        bool numaraVarMi = await _context.Rooms.AnyAsync(r => r.OdaNumarasi == dto.OdaNumarasi);

        if (numaraVarMi)
        {
            throw new InvalidOperationException($"{dto.OdaNumarasi} numarali oda zaten sistemde kayitli!");
        }

        var yeniOda = _mapper.CreateRoomDtoToRoom(dto);

        _context.Rooms.Add(yeniOda);
        await _context.SaveChangesAsync();

        return _mapper.RoomToRoomDto(yeniOda);
    }

    // 4. ODA GUNCELLE
    public async Task<RoomDto?> UpdateRoomAsync(int id, UpdateRoomDto dto)
    {
        var oda = await _context.Rooms.FindAsync(id);

        if (oda == null) return null;

        // GUVENLIK DUVARI: Yeni oda numarasi baska odaya ait mi?
        bool numaraKullanimdaMi = await _context.Rooms
            .AnyAsync(r => r.OdaNumarasi == dto.OdaNumarasi && r.Id != id);

        if (numaraKullanimdaMi)
        {
            throw new InvalidOperationException($"{dto.OdaNumarasi} numarali oda zaten baska bir kayit tarafindan kullaniliyor!");
        }

        _mapper.UpdateRoomFromDto(dto, oda);

        await _context.SaveChangesAsync();

        return _mapper.RoomToRoomDto(oda);
    }

    // 5. ODA SIL
    public async Task<bool> DeleteRoomAsync(int id)
    {
        // Odayi bul ve rezervasyonlarini da yukle (Include)
        var oda = await _context.Rooms
            .Include(r => r.Reservations)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (oda == null) return false;

        // YENI GUVENLIK DUVARI: Odanin aktif rezervasyonu var mi?
        if (oda.Reservations.Any())
        {
            throw new InvalidOperationException("Bu odanin aktif rezervasyonlari var. Once rezervasyonlari iptal edin, sonra odayi silebilirsiniz.");
        }

        _context.Rooms.Remove(oda);
        await _context.SaveChangesAsync();
        return true;
    }

    // V4: Dashboard İstatistikleri
    public async Task<int> GetTotalRoomCountAsync()
    {
        return await _context.Rooms.CountAsync();
    }
}
