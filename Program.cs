using Microsoft.EntityFrameworkCore;
using otelrezervation.Models;
using otelrezervation.Services;  // ← Service'leri tanıması için eklendi

using Microsoft.AspNetCore.Authentication.Cookies;

// PostgreSQL'in tarih formatı (UTC vs Local) hatasını önlemek için eski tip tarih kullanımını açıyoruz
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// 1. Controller (API) ve Arayüz (MVC) Zekasını Sisteme Ekliyoruz
builder.Services.AddControllersWithViews(); // YENİ: Views desteği eklendi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Kimlik Doğrulama (Authentication) Servisi Ekle
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";
    });

// 2. PostgreSQL veritabani komprumuz 
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Service katmani kayitlari (DI)
// IUserService istenirse, ona UserService ver
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// 3. Swagger arayuzunu tarayicida yansitma izni (Sadece gelistirme ortaminda)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles(); // HTML içinde CSS/JS dosyalarını kullanabilmek için eklendi

app.UseAuthentication();
app.UseAuthorization();

//  Test 
app.MapGet("/test", () => "Sistem Kusursuz Calisiyor!");

// api ile mvc arayuz ayni anda calisabilir
// API rotalari 
app.MapControllers();

// MVC arayuz rotalari 

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); 

app.Run();