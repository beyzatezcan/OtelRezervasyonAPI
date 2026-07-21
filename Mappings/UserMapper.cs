using Riok.Mapperly.Abstractions;
using otelrezervation.Models;
using otelrezervation.DTOs;

namespace otelrezervation.Mappings;

[Mapper]
public partial class UserMapper
{
    // Veritabanı modelini DTO'ya çevirir
    public partial UserDto UserToUserDto(User user);

    // Yeni oluşturulan DTO'yu veritabanı modeline çevirir
    public partial User CreateUserDtoToUser(CreateUserDto dto);

    // Güncelleme işlemi için DTO'daki verileri varolan User nesnesinin içine aktarır
    public partial void UpdateUserFromDto(UpdateUserDto dto, User user);
}
