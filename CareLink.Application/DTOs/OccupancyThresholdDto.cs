using System.ComponentModel.DataAnnotations;

namespace CareLink.Application.DTOs;

public class OccupancyThresholdDto
{
    [Range(1, 100)]
    public double Warning { get; set; } = 80;

    [Range(1, 100)]
    public double Critical { get; set; } = 95;
}
