using Microsoft.EntityFrameworkCore;
using otelrezervation.DTOs;
using otelrezervation.Models;

namespace otelrezervation.Services;

// SERVICE = MUTFAK
// IUserService (menu) ne yapilacagini soyluyordu
// UserService (mutfak) ise NASIL yapilacagini burada anlatiyor
// ": IUserService" demek = "ben bu menudeki her seyi yapabilecegim" demek
public class UserService : IUserService
{
    // veritabani baglantisi (mutfagin malzemeleri)
    // eskiden bu controller'da idi, simdi service'te
    private readonly AppDbContext _context;

    // constructor: service olusturulunca veritabani baglantisini al
    // DI (Dependency Injection) sayesinde .NET bunu otomatik verecek
    public UserService(AppDbContext context)
    {
        _context = context;
    }

    // ============================================
    // 1. TUM KULLANICILARI LISTELE
    // ============================================
    // eskiden UsersController.GetUsers() icindeydi
    // simdi controller bu metodu cagiracak
    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _context.Users.ToListAsync();

        // entity -> dto cevirisi
        var userDtos = users.Select(u => new UserDto
        {
            Id = u.Id,
            Ad = u.Ad,
            Soyad = u.Soyad,
            Email = u.Email,
            Telefon = u.Telefon
        }).ToList();

        return userDtos;
    }

    // ============================================
    // 2. ID'YE GORE TEK KULLANICI GETIR
    // ============================================
    // bu yeni bir ozellik! eskiden controller'da yoktu
    // tek bir kullaniciyi bulmak icin kullanacagiz
    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);

        // kullanici bulunamazsa null don
        if (user == null)
            return null;

        // entity -> dto cevirisi
        return new UserDto
        {
            Id = user.Id,
            Ad = user.Ad,
            Soyad = user.Soyad,
            Email = user.Email,
            Telefon = user.Telefon
        };
    }

    // ============================================
    // 3. YENI KULLANICI OLUSTUR
    // ============================================
    // eskiden UsersController.AddUser() icindeydi
    public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
    {
        // guvenlik duvari: email zaten var mi?
        bool emailVarMi = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (emailVarMi)
        {
            // exception firlatiyoruz — controller bunu yakalayip 400 dondurecek
            throw new InvalidOperationException("Bu e-posta adresiyle kayitli bir kullanici zaten var.");
        }

        // dto -> entity cevirisi
        var yeniKullanici = new User
        {
            Ad = dto.Ad,
            Soyad = dto.Soyad,
            Email = dto.Email,
            Telefon = dto.Telefon
        };

        _context.Users.Add(yeniKullanici);
        await _context.SaveChangesAsync();

        // entity -> dto cevirisi
        return new UserDto
        {
            Id = yeniKullanici.Id,
            Ad = yeniKullanici.Ad,
            Soyad = yeniKullanici.Soyad,
            Email = yeniKullanici.Email,
            Telefon = yeniKullanici.Telefon
        };
    }

    // ============================================
    // 4. KULLANICI GUNCELLE
    // ============================================
    // eskiden UsersController.UpdateUser() icindeydi
    public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto dto)
    {
        // kullanici var mi?
        var kullanici = await _context.Users.FindAsync(id);
        if (kullanici == null)
            return null; // bulunamazsa null don — controller 404 dondurecek

        // guvenlik duvari: yeni email baska birinde var mi?
        bool emailKullanimdaMi = await _context.Users
            .AnyAsync(u => u.Email == dto.Email && u.Id != id);

        if (emailKullanimdaMi)
        {
            throw new InvalidOperationException("Bu e-posta adresi baska bir kullaniciya ait.");
        }

        // guncelle
        kullanici.Ad = dto.Ad;
        kullanici.Soyad = dto.Soyad;
        kullanici.Email = dto.Email;
        kullanici.Telefon = dto.Telefon;

        await _context.SaveChangesAsync();

        // entity -> dto cevirisi
        return new UserDto
        {
            Id = kullanici.Id,
            Ad = kullanici.Ad,
            Soyad = kullanici.Soyad,
            Email = kullanici.Email,
            Telefon = kullanici.Telefon
        };
    }

    // ============================================
    // 5. KULLANICI SIL
    // ============================================
    // eskiden UsersController.DeleteUser() icindeydi
    // YENI: artik aktif rezervasyon kontrolu de var!
    public async Task<bool> DeleteUserAsync(int id)
    {
        // kullanici var mi? .Include ile rezervasyonlarini da yukle
        var kullanici = await _context.Users
            .Include(u => u.Reservations) // user.Reservations'a erisebilmek icin
            .FirstOrDefaultAsync(u => u.Id == id);

        if (kullanici == null)
            return false; // bulunamazsa false don — controller 404 dondurecek

        // YENI GUVENLIK DUVARI: aktif rezervasyonu var mi?
        // eger varsa silmeye izin verme!
        if (kullanici.Reservations.Any())
        {
            throw new InvalidOperationException(
                "Bu kullanicinin aktif rezervasyonlari var. Once rezervasyonlari iptal edin.");
        }

        _context.Users.Remove(kullanici);
        await _context.SaveChangesAsync();

        return true; // basariyla silindi
    }
}
