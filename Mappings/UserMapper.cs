using Riok.Mapperly.Abstractions;
using otelrezervation.Models;
using otelrezervation.DTOs;

namespace otelrezervation.Mappings;

[Mapper]
public partial class UserMapper
{
    // Veritabani modelini DTO'ya cevirir
    public partial UserDto UserToUserDto(User user);

    // Yeni olusturulan DTO'yu veritabani modeline cevirir
    public partial User CreateUserDtoToUser(CreateUserDto dto);

    // Guncelleme islemi icin DTO'daki verileri varolan User nesnesinin icine aktarir
    public partial void UpdateUserFromDto(UpdateUserDto dto, User user);
}
