using Microsoft.EntityFrameworkCore;
using otelrezervation.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Controller (API) Zekasını ve Swagger Araçlarını Sisteme Ekliyoruz
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. PostgreSQL Veritabanı Köprümüz
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 3. Swagger Arayüzünü Tarayıcıya Yansıtma İzni (Sadece Geliştirme Ortamında)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Güvenlik yönlendirmesini test için kapalı tutuyoruz
// app.UseHttpsRedirection(); 

app.UseAuthorization();

//  Test 
app.MapGet("/test", () => "Sistem Kusursuz Calisiyor!");

//Controller klasöründeki dosyaları dış dünyaya aç!
app.MapControllers();

app.Run();