using Microsoft.EntityFrameworkCore;
using otelrezervation.Models;
using otelrezervation.Services;  // ← Service'leri tanıması için eklendi
// PostgreSQL'in tarih formatı (UTC vs Local) hatasını önlemek için eski tip tarih kullanımını açıyoruz
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);
// 1. Controller (API) ve Arayüz (MVC) Zekasını Sisteme Ekliyoruz
builder.Services.AddControllersWithViews(); // YENİ: Views desteği eklendi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. PostgreSQL Veritabanı Köprümüz
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Service Katmanı Kayıtları (DI)
// "Birisi IUserService isterse, ona UserService ver"
builder.Services.AddScoped<IUserService, UserService>();
// "Birisi IRoomService isterse, ona RoomService ver"
builder.Services.AddScoped<IRoomService, RoomService>();
// "Birisi IReservationService isterse, ona ReservationService ver"
builder.Services.AddScoped<IReservationService, ReservationService>();

var app = builder.Build();

// 3. Swagger Arayüzünü Tarayıcıya Yansıtma İzni (Sadece Geliştirme Ortamında)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles(); // YENİ: HTML içinde CSS/JS dosyalarını kullanabilmek için eklendi

// Güvenlik yönlendirmesini test için kapalı tutuyoruz
// app.UseHttpsRedirection(); 

app.UseAuthorization();

//  Test 
app.MapGet("/test", () => "Sistem Kusursuz Calisiyor!");

// API rotaları için (Eski sistemimiz çalışmaya devam etsin diye)
app.MapControllers();

// YENİ: MVC Arayüz Rotaları (Web sayfalarımız için)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();