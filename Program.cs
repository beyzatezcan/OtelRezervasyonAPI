using Microsoft.EntityFrameworkCore;
using otelrezervation.Models;
using otelrezervation.Services;

// PostgreSQL'in tarih formati (UTC vs Local) hatasini onlemek icin eski tip tarih kullanimini aciyoruz
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// 1. Controller (API) ve arayuz (MVC) zekasini sisteme ekliyoruz
builder.Services.AddControllersWithViews(); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. PostgreSQL veritabani komprumuz 
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Service katmani kayitlari (DI)
// IUserService istenirse, ona UserService ver
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IReservationService, ReservationService>();

var app = builder.Build();

// 3. Swagger arayuzunu tarayicida yansitma izni (Sadece gelistirme ortaminda)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles(); // HTML içinde CSS/JS dosyalarını kullanabilmek için eklendi


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