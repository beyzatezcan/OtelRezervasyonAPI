using Microsoft.EntityFrameworkCore;
using otelrezervation.DTOs;
using otelrezervation.Models;

namespace otelrezervation.Services;

public class RoomService : IRoomService
{
    private readonly AppDbContext _context;

    public RoomService(AppDbContext context)
    {
        _context = context;
    }

    // 1. TUM ODALARI LISTELE
    public async Task<List<RoomDto>> GetAllRoomsAsync()
    {
        var rooms = await _context.Rooms.ToListAsync();

        return rooms.Select(r => new RoomDto
        {
            Id = r.Id,
            OdaNumarasi = r.OdaNumarasi,
            GecelikFiyat = r.GecelikFiyat
        }).ToList();
    }

    // 2. ID'YE GORE TEK ODA GETIR
    public async Task<RoomDto?> GetRoomByIdAsync(int id)
    {
        var room = await _context.Rooms.FindAsync(id);

        if (room == null) return null;

        return new RoomDto
        {
            Id = room.Id,
            OdaNumarasi = room.OdaNumarasi,
            GecelikFiyat = room.GecelikFiyat
        };
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

        var yeniOda = new Room
        {
            OdaNumarasi = dto.OdaNumarasi,
            GecelikFiyat = dto.GecelikFiyat
        };

        _context.Rooms.Add(yeniOda);
        await _context.SaveChangesAsync();

        return new RoomDto
        {
            Id = yeniOda.Id,
            OdaNumarasi = yeniOda.OdaNumarasi,
            GecelikFiyat = yeniOda.GecelikFiyat
        };
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

        oda.OdaNumarasi = dto.OdaNumarasi;
        oda.GecelikFiyat = dto.GecelikFiyat;

        await _context.SaveChangesAsync();

        return new RoomDto
        {
            Id = oda.Id,
            OdaNumarasi = oda.OdaNumarasi,
            GecelikFiyat = oda.GecelikFiyat
        };
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
}
