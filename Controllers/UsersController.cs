using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otelrezervation.Models;

namespace otelrezervation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    // 1. LİSTELEME (Read - GET)
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users.ToListAsync();
        return Ok(users);
    }

    // 2. EKLEME (Create - POST)
    [HttpPost]
    public async Task<IActionResult> AddUser(User yeniKullanici)
    {
        // GÜVENLİK DUVARI: Bu e-posta adresi veritabanında zaten var mı?
        bool emailVarMi = await _context.Users.AnyAsync(u => u.Email == yeniKullanici.Email);

        if (emailVarMi)
        {
            return BadRequest($"Hata: {yeniKullanici.Email} adresiyle kayıtlı bir kullanıcı zaten var!");
        }

        _context.Users.Add(yeniKullanici);
        await _context.SaveChangesAsync();
        
        return Ok(yeniKullanici);
    }

    // 3. GÜNCELLEME (Update - PUT)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, User guncelKullanici)
    {
        // 1. Güncellenecek kullanıcı var mı?
        var kullanici = await _context.Users.FindAsync(id);
        if (kullanici == null)
        {
            return NotFound("Güncellenecek kullanıcı bulunamadı.");
        }

        // 2. GÜVENLİK DUVARI: Girilen yeni e-posta, BAŞKA bir kullanıcı tarafından kullanılıyor mu?
        // (u.Id != id -> "Kendisi dışındaki kullanıcıları kontrol et" demektir)
        bool emailKullanimdaMi = await _context.Users
            .AnyAsync(u => u.Email == guncelKullanici.Email && u.Id != id);

        if (emailKullanimdaMi)
        {
            return BadRequest($"Hata: {guncelKullanici.Email} adresi başka bir kullanıcıya ait!");
        }

        // 3. Bilgileri güncelle
        kullanici.Ad = guncelKullanici.Ad;
        kullanici.Soyad = guncelKullanici.Soyad;
        kullanici.Email = guncelKullanici.Email;
        kullanici.Telefon = guncelKullanici.Telefon;

        await _context.SaveChangesAsync();
        return Ok(kullanici);
    }

    // 4. SİLME (Delete - DELETE)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var kullanici = await _context.Users.FindAsync(id);
        if (kullanici == null)
        {
            return NotFound("Silinecek kullanıcı bulunamadı.");
        }

        _context.Users.Remove(kullanici);
        await _context.SaveChangesAsync();

        return Ok("Kullanıcı başarıyla silindi.");
    }
}