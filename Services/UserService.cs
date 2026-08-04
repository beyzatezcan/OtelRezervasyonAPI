using Microsoft.EntityFrameworkCore;
using otelrezervation.DTOs;
using otelrezervation.Models;

namespace otelrezervation.Services;

// SERVICE = MUTFAK
// IUserService (menu) ne yapilacagini soyluyordu
// UserService (mutfak) ise nasil yapilacagini burada anlatiyor
// "IUserService demek = ben bu menudeki her seyi yapabilecegim demek
public class UserService : IUserService
{
    // veritabani baglantisi (mutfagin malzemeleri)
    private readonly AppDbContext _context;
    private readonly Mappings.UserMapper _mapper = new();

    // constructor: service olusturulunca veritabani baglantisini al
    public UserService(AppDbContext context)
    {
        _context = context;
    }


    // 1. TUM KULLANICILARI LISTELE
    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _context.Users.ToListAsync();

        // entity - dto cevirisi 
        // veritabanindan gelen ham veriyi kullaniciya gostermek icin DTO'ya ceviriyoruz
        return users.Select(u => _mapper.UserToUserDto(u)).ToList();
    }

    // 2. ID'YE GORE TEK KULLANICI GETIR

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return null;

        // entity- dto cevirisi
        return _mapper.UserToUserDto(user);
    }

    // 3. YENI KULLANICI OLUSTUR
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
        // buradaki yapi veritabanina eklenecek veriyi hazirliyor 
        var yeniKullanici = _mapper.CreateUserDtoToUser(dto);

        // V4: İlk kayıt olan kullanıcıyı otomatik olarak Admin yap
        if (!await _context.Users.AnyAsync())
        {
            yeniKullanici.Role = "Admin";
        }

        _context.Users.Add(yeniKullanici);
        await _context.SaveChangesAsync();

        // entity -> dto cevirisi
        // buradaki yapi veritabanina eklenen yeni kaydi kullaniciya gostermek icin duzenliyor
        return _mapper.UserToUserDto(yeniKullanici);
    }

  
    // 4. KULLANICI GUNCELLE
    public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto dto)
    {
        // kullanici var mi?
        var kullanici = await _context.Users.FindAsync(id);
        if (kullanici == null)
            return null; 

        // guvenlik duvari: yeni email baska birinde var mi?
        bool emailKullanimdaMi = await _context.Users
            .AnyAsync(u => u.Email == dto.Email && u.Id != id);

        if (emailKullanimdaMi)
        {
            throw new InvalidOperationException("Bu e-posta adresi baska bir kullaniciya ait.");
        }

        // guncelle
        _mapper.UpdateUserFromDto(dto, kullanici);

        await _context.SaveChangesAsync();

        // entity -> dto cevirisi
        return _mapper.UserToUserDto(kullanici);
    }

    // 5. KULLANICI SIL
    public async Task<bool> DeleteUserAsync(int id)
    {
        // kullanici var mi? .Include ile rezervasyonlarini da yukle
        var kullanici = await _context.Users
            .Include(u => u.Reservations) // user.Reservations'a erisebilmek icin
            .FirstOrDefaultAsync(u => u.Id == id);

        if (kullanici == null)
            return false; 

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

    // V4: Dashboard İstatistikleri
    public async Task<int> GetTotalUserCountAsync()
    {
        return await _context.Users.CountAsync();
    }
}
