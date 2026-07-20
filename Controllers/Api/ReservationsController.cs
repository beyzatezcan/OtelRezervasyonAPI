using Microsoft.AspNetCore.Mvc;
using otelrezervation.DTOs;
using otelrezervation.Services; // Service kullanımı için eklendi

namespace otelrezervation.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    // 1. Tüm Rezervasyonları Listele (GET)
    [HttpGet]
    public async Task<IActionResult> GetReservations()
    {
        var reservations = await _reservationService.GetAllReservationsAsync();
        return Ok(reservations);
    }

    // 2. Yeni Rezervasyon Yap (POST)
    [HttpPost]
    public async Task<IActionResult> MakeReservation(CreateReservationDto dto)
    {
        try
        {
            var reservation = await _reservationService.CreateReservationAsync(dto);
            return Ok(reservation);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // 3. Rezervasyon İptal Et (DELETE)
    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelReservation(int id)
    {
        var result = await _reservationService.DeleteReservationAsync(id);

        if (!result)
            return NotFound("İptal edilecek rezervasyon bulunamadı.");

        return Ok("Rezervasyon başarıyla iptal edildi.");
    }
}