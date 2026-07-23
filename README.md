# 🏨 Otel Rezervasyon Yönetim Sistemi

ASP.NET Core MVC ile geliştirilmiş, rol tabanlı yetkilendirme ve akıllı takvim entegrasyonuna sahip full-stack bir otel rezervasyon yönetim sistemidir.

## 🛠️ Teknoloji Yığını

| Teknoloji | Kullanım Amacı |
|-----------|---------------|
| **ASP.NET Core 10** | Web sunucusu ve MVC çatısı |
| **Entity Framework Core** | ORM (Veritabanı işlemleri) |
| **PostgreSQL** | İlişkisel veritabanı |
| **Riok.Mapperly** | Compile-time DTO ↔ Entity dönüşümü |
| **Bootstrap 5** | Responsive arayüz tasarımı |
| **Flatpickr** | Akıllı tarih seçici takvim |
| **Cookie Authentication** | Oturum ve rol yönetimi |
| **Swagger (OpenAPI)** | API test arayüzü |

## 📐 Mimari

Proje **Katmanlı Mimari (Layered Architecture)** prensibiyle tasarlanmıştır:

```
Controller (İstek Yönetimi)
    ↓
Service (İş Kuralları)
    ↓
Mapper (DTO ↔ Entity Dönüşümü)
    ↓
DbContext → PostgreSQL
```

- **Models/** → Veritabanı tabloları (Entity)
- **DTOs/** → Veri Aktarım Nesneleri (Dış dünyaya güvenli veri)
- **Mappings/** → Riok.Mapperly ile otomatik dönüşüm kuralları
- **Services/** → Interface + implementasyon (DI ile enjekte edilir)
- **Controllers/Api/** → RESTful API endpoint'leri (JSON)
- **Controllers/Web/** → MVC Controller'ları (HTML View döndürür)
- **Views/** → Razor (.cshtml) sayfaları

## 👥 Roller ve Yetkiler

### Müşteri (Customer)
- Ana sayfadan oda vitrinini görüntüleme
- Akıllı takvim ile oda rezervasyonu yapma (dolu günler kırmızı ve kilitli)
- Kendi rezervasyonlarını listeleme ve iptal etme

### Yönetici (Admin)
- Müşteri yönetimi (CRUD)
- Oda yönetimi (CRUD)
- Tüm rezervasyonları görüntüleme ve silme
- Herhangi bir müşteri adına manuel rezervasyon oluşturma
- Swagger API test paneline erişim

## 🔒 Güvenlik Katmanları

- Rol bazlı sayfa erişim kontrolü (`[Authorize(Roles = "...")]`)
- Tarih çakışması kontrolü (aynı odaya çift rezervasyon engeli)
- Geçmiş tarihe rezervasyon engeli
- Email ve oda numarası benzersizlik kontrolü
- Aktif rezervasyonu olan oda/müşteri silme engeli
- Müşterinin yalnızca kendi rezervasyonlarını iptal edebilmesi

## 📅 Akıllı Takvim (Flatpickr)

Rezervasyon formlarında **Flatpickr** kütüphanesi entegre edilmiştir:
- Seçilen odaya ait dolu tarihler veritabanından çekilir
- Dolu günler takvimde **kırmızı, üstü çizili ve tıklanamaz** olarak gösterilir
- Yönetici panelinde oda değiştirildiğinde takvim anlık olarak güncellenir

## 🗄️ Veritabanı Şeması

```
User (1) ──────── (N) Reservation
Room (1) ──────── (N) Reservation
```

| Tablo | Alanlar |
|-------|---------|
| **Users** | Id, Ad, Soyad, Email, Telefon, Password, Role |
| **Rooms** | Id, OdaNumarasi, GecelikFiyat |
| **Reservations** | Id, UserId (FK), RoomId (FK), GirisTarihi, CikisTarihi |

## 🚀 Kurulum

### Gereksinimler
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/)

### Adımlar

```bash
# 1. Projeyi klonla
git clone https://github.com/beyzatezcan/OtelRezervasyonAPI.git
cd OtelRezervasyonAPI

# 2. appsettings.json içindeki bağlantı bilgilerini kendi PostgreSQL ayarlarına göre düzenle

# 3. Veritabanını oluştur (Migration uygula)
dotnet ef database update

# 4. Uygulamayı çalıştır
dotnet run
```

Uygulama varsayılan olarak `http://localhost:5171` adresinde açılır.

### İlk Giriş
- **Yönetici:** `admin@otel.com` / `123456`
- Müşteri hesabı oluşturmak için Kayıt Ol sayfasını kullanın.

## 📁 Proje Yapısı

```
otelrezervation/
├── Models/          → Entity sınıfları + DbContext
├── DTOs/            → Veri aktarım nesneleri
├── Mappings/        → Riok.Mapperly dönüşüm kuralları
├── Services/        → İş mantığı (Interface + Implementasyon)
├── Controllers/
│   ├── Api/         → RESTful API (JSON)
│   └── Web/         → MVC (HTML View)
├── Views/           → Razor sayfaları (.cshtml)
├── Migrations/      → EF Core migration geçmişi
└── Program.cs       → Uygulama başlangıç noktası
```
