namespace CareLink.Application.DTOs;

public class RoomDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Shelter { get; set; } = string.Empty;
    public string ShelterId { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<BedDto> Beds { get; set; } = new();
}
