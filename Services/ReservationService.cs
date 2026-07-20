using Microsoft.EntityFrameworkCore;
using otelrezervation.DTOs;
using otelrezervation.Models;

namespace otelrezervation.Services;

public class ReservationService : IReservationService
{
    private readonly AppDbContext _context;

    public ReservationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReservationDto>> GetAllReservationsAsync()
    {
        // Reservation'a bağlı Room ve User'ı da getir ki DTO'ya isimlerini verebilelim
        var reservations = await _context.Reservations
            .Include(r => r.Room)
            .Include(r => r.User) // YENİ: Kullanıcı bilgisini de çekiyoruz
            .ToListAsync();

        return reservations.Select(r => new ReservationDto
        {
            Id = r.Id,
            OdaNumarasi = r.Room.OdaNumarasi,
            MusteriAdi = $"{r.User.Ad} {r.User.Soyad}", // YENİ: Ad ve Soyadı birleştirip veriyoruz
            GirisTarihi = r.GirisTarihi,
            CikisTarihi = r.CikisTarihi
        }).ToList();
    }

    public async Task<ReservationDto> CreateReservationAsync(CreateReservationDto dto)
    {
        // 1. GUVENLIK: Müşteri var mı?
        var user = await _context.Users.FindAsync(dto.UserId);
        if (user == null)
            throw new InvalidOperationException("Rezervasyon yapılmak istenen müşteri sistemde bulunamadı!");

        // 2. GUVENLIK: Oda var mı?
        var room = await _context.Rooms.FindAsync(dto.RoomId);
        if (room == null)
            throw new InvalidOperationException("Rezerve edilmek istenen oda sistemde bulunamadı!");

        // 3. GUVENLIK (YENI): Tarih Validasyonu
        // Giriş tarihi geçmiş bir tarih olamaz (bugünden önce olamaz)
        if (dto.GirisTarihi.Date < DateTime.Now.Date)
            throw new InvalidOperationException("Geçmiş bir tarihe rezervasyon yapılamaz!");

        // Çıkış tarihi giriş tarihinden önce veya aynı gün olamaz
        if (dto.CikisTarihi <= dto.GirisTarihi)
            throw new InvalidOperationException("Çıkış tarihi, giriş tarihinden sonra olmalıdır!");

        // 4. GUVENLIK: O tarihlerde oda dolu mu?
        bool isRoomTaken = await _context.Reservations.AnyAsync(r =>
            r.RoomId == dto.RoomId &&
            r.GirisTarihi < dto.CikisTarihi &&
            r.CikisTarihi > dto.GirisTarihi);

        if (isRoomTaken)
            throw new InvalidOperationException("Seçilen oda belirtilen tarihler arasında zaten dolu!");

        var yeniRezervasyon = new Reservation
        {
            UserId = dto.UserId,
            RoomId = dto.RoomId,
            GirisTarihi = dto.GirisTarihi,
            CikisTarihi = dto.CikisTarihi
        };

        _context.Reservations.Add(yeniRezervasyon);
        await _context.SaveChangesAsync();

        return new ReservationDto
        {
            Id = yeniRezervasyon.Id,
            OdaNumarasi = room.OdaNumarasi,
            MusteriAdi = $"{user.Ad} {user.Soyad}",
            GirisTarihi = yeniRezervasyon.GirisTarihi,
            CikisTarihi = yeniRezervasyon.CikisTarihi
        };
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
