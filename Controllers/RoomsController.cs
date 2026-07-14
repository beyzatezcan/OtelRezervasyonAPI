using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otelrezervation.Models;

namespace otelrezervation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoomsController : ControllerBase
{
    private readonly AppDbContext _context;

    public RoomsController(AppDbContext context)
    {
        _context = context;
    }
    // icerdekini oku
    [HttpGet]
    public async Task<IActionResult> GetRooms()
    {
        var rooms = await _context.Rooms.ToListAsync();
        return Ok(rooms);
    }

    // Odaya Yeni Kayıt Ekleme İşlemi (Create - POST)
    [HttpPost]
public async Task<IActionResult> AddRoom(Room yeniOda)
{
    // 1. GÜVENLİK DUVARI: Bu oda numarası içeride var mı?
    bool numaraVarMi = await _context.Rooms.AnyAsync(r => r.OdaNumarasi == yeniOda.OdaNumarasi);

    if (numaraVarMi)
    {
        // Eğer varsa veritabanına gitmeden işlemi durdur ve hata fırlat!
        return BadRequest($"Hata: {yeniOda.OdaNumarasi} numaralı oda zaten sistemde kayıtlı!");
    }

    // 2. Eğer numara benzersizse, normal kayıt işlemine devam et
    _context.Rooms.Add(yeniOda);
    await _context.SaveChangesAsync();
    
    return Ok(yeniOda);
}

    // Odayı Güncelleme İşlemi (Update - PUT)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoom(int id, Room guncelOda)
    {
        // 1. Önce güncellenmek istenen oda veritabanında var mı diye bakıyoruz
        var oda = await _context.Rooms.FindAsync(id);
        
        if (oda == null)
        {
            return NotFound("Güncellenecek oda bulunamadı.");
        }

        // 2. GÜVENLİK DUVARI: Girilen yeni oda numarası, sistemdeki BAŞKA bir oda tarafından kullanılıyor mu?
        // (r.Id != id kısmı: "Kendisi hariç diğer odaları kontrol et" anlamına gelir)
        bool numaraKullanimdaMi = await _context.Rooms
            .AnyAsync(r => r.OdaNumarasi == guncelOda.OdaNumarasi && r.Id != id);

        if (numaraKullanimdaMi)
        {
            return BadRequest($"Hata: {guncelOda.OdaNumarasi} numaralı oda zaten başka bir kayıt tarafından kullanılıyor!");
        }

        // 3. Her şey temizse verileri güncelliyoruz
        oda.OdaNumarasi = guncelOda.OdaNumarasi;
        oda.GecelikFiyat = guncelOda.GecelikFiyat;

        // 4. Veritabanına kaydediyoruz
        await _context.SaveChangesAsync();

        return Ok(oda);
    }
    
    // Odayı Silme İşlemi (Delete - DELETE)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        // 1. Önce silinecek odayı veritabanında buluyoruz
        var oda = await _context.Rooms.FindAsync(id);
        
        if (oda == null)
        {
            return NotFound("Silinecek oda bulunamadı.");
        }

        // 2. Entity Framework'e "Bu odayı tablodan çıkar" talimatını veriyoruz
        _context.Rooms.Remove(oda);
        
        // 3. Değişikliği kaydedip işlemi bitiriyoruz
        await _context.SaveChangesAsync();

        return Ok("Oda başarıyla silindi.");
    }
}