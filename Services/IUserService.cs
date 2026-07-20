using otelrezervation.DTOs;

namespace otelrezervation.Services;

// INTERFACE = MENU
// "UserService su isleri yapabilir" diyor ama NASIL yapilacagini soylemiyor
// Asil is mantigi UserService.cs'te (mutfakta) olacak
public interface IUserService
{
    // tum kullanicilari listele
    Task<List<UserDto>> GetAllUsersAsync();

    // id'ye gore tek kullanici getir
    Task<UserDto?> GetUserByIdAsync(int id);

    // yeni kullanici olustur
    Task<UserDto> CreateUserAsync(CreateUserDto dto);

    // kullanici guncelle
    Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto dto);

    // kullanici sil
    Task<bool> DeleteUserAsync(int id);
}
