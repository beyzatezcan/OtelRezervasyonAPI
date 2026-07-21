using Riok.Mapperly.Abstractions;
using otelrezervation.Models;
using otelrezervation.DTOs;

namespace otelrezervation.Mappings;

[Mapper]
public partial class RoomMapper
{
    public partial RoomDto RoomToRoomDto(Room room);
    public partial Room CreateRoomDtoToRoom(CreateRoomDto dto);
    public partial void UpdateRoomFromDto(UpdateRoomDto dto, Room room);
}
