using Microsoft.EntityFrameworkCore;
using otelrezervation.DTOs;
using otelrezervation.Models;

namespace otelrezervation.Services;

public class ReservationService : IReservationService
{
    private readonly AppDbContext _context;
    private readonly Mappings.ReservationMapper _mapper = new();

    public ReservationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReservationDto>> GetAllReservationsAsync()
    {
        // Reservation'a bagli Room ve User'i da getir ki DTO'ya isimlerini verebilelim
        var reservations = await _context.Reservations
            .Include(r => r.Room)
            .Include(r => r.User) // YENI: Kullanici bilgisini de cekiyoruz
            .ToListAsync();

        return reservations.Select(r => _mapper.ReservationToReservationDto(r)).ToList();
    }

    public async Task<ReservationDto> CreateReservationAsync(CreateReservationDto dto)
    {
        // 1. GUVENLIK: musteri var mi?
        var user = await _context.Users.FindAsync(dto.UserId);
        if (user == null)
            throw new InvalidOperationException("Rezervasyon yapılmak istenen müşteri sistemde bulunamadı!");

        // 2. GUVENLIK: Oda var mi?
        var room = await _context.Rooms.FindAsync(dto.RoomId);
        if (room == null)
            throw new InvalidOperationException("Rezerve edilmek istenen oda sistemde bulunamadı!");

        // 3. GUVENLIK : Tarih Validasyonu
        // Giris tarihi gecmis bir tarih olamaz (bugunden once olamaz)
        if (dto.GirisTarihi.Date < DateTime.Now.Date)
            throw new InvalidOperationException("Gecmis bir tarihe rezervasyon yapilamaz!");

        // Cikis tarihi giris tarihinden once veya ayni gun olamaz
        if (dto.CikisTarihi <= dto.GirisTarihi)
            throw new InvalidOperationException("Çıkış tarihi, giriş tarihinden sonra olmalıdır!");

        // 4. GUVENLIK: O tarihlerde oda dolu mu?
        bool isRoomTaken = await _context.Reservations.AnyAsync(r =>
            r.RoomId == dto.RoomId &&
            r.GirisTarihi < dto.CikisTarihi &&
            r.CikisTarihi > dto.GirisTarihi);

        if (isRoomTaken)
            throw new InvalidOperationException("Seçilen oda belirtilen tarihler arasında zaten dolu!");

        // HERSEY OKSA: Rezervasyonu Ekle
        var yeniRezervasyon = _mapper.CreateReservationDtoToReservation(dto);
        //VERITABANINA KAYDET
        _context.Reservations.Add(yeniRezervasyon);
        await _context.SaveChangesAsync();

        yeniRezervasyon.User = user;
        yeniRezervasyon.Room = room;

        return _mapper.ReservationToReservationDto(yeniRezervasyon);
    }
    public async Task<bool> DeleteReservationAsync(int id)
    {
        var rezervasyon = await _context.Reservations.FindAsync(id);

        if (rezervasyon == null) return false;

        _context.Reservations.Remove(rezervasyon);
        await _context.SaveChangesAsync();

        return true;
    }
}
