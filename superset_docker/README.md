# Superset SSO Configuration - Parametrik Yapılandırma


### HOSTA Kaydet
```
C:\Windows\System32\drivers\etc\hosts git

0.0.1    local.authserver.com
127.0.0.1    local.webapp.com

ekle.
```

### DB EKLE
```
POSTGRESQL İçin boş DB Ekle Örn; Superset
```


## 📁 Dosya Yapısı

```
superset-new/
├── .env                          # ✅ TÜM AYARLAR BURADA
├── docker-compose.yml            # Docker yapılandırması
├── superset_config.py            # Superset ana config (.env'den okur)
├── superset_auth.py              # SSO endpoints
└── README.md                     # Bu dosya
```

## 🔧 Kurulum

### 1. `.env` Dosyasını Düzenle

Tek yapman gereken `.env` dosyasındaki değerleri kendi ortamına göre düzenlemek:

```bash
# Database Configuration
DATABASE_DIALECT=postgresql
DATABASE_HOST=host.docker.internal    # ⬅️ Kendi DB host'unu yaz
DATABASE_PORT=5432
DATABASE_DB=Superset                  # ⬅️ Kendi DB ismini yaz
DATABASE_USER=postgres                # ⬅️ Kendi DB kullanıcını yaz
DATABASE_PASSWORD=123456              # ⬅️ Kendi DB şifreni yaz

# Superset Configuration
SUPERSET_SECRET_KEY=YOUR_SUPER_SECRET_KEY_CHANGE_ME_PLEASE_32_CHARS_OR_MORE_1234567890
SUPERSET_PORT=8088                    # ⬅️ Farklı port istersen değiştir

# Session Configuration
SESSION_LIFETIME_HOURS=24             # ⬅️ Session süresini ayarla

# CORS Origins (virgülle ayır)
CORS_ORIGINS=http://localhost:44361,http://localhost:44308,http://localhost:8088

# Docker Configuration
SUPERSET_VERSION=3.1.0                # ⬅️ Superset versiyonunu değiştir
GUNICORN_WORKERS=4                    # ⬅️ Worker sayısını ayarla
GUNICORN_TIMEOUT=120                  # ⬅️ Timeout süresini ayarla
```

### 2. Docker'ı Başlat

```bash
docker-compose up -d
```

### Database Değiştirmek
Sadece `.env` dosyasında ilgili satırları değiştir:
```bash
DATABASE_HOST=yeni-host.com
DATABASE_PORT=5433
DATABASE_USER=yeni-kullanici
```

Sonra restart et:
```bash
docker-compose down
docker-compose up -d
```

### Port Değiştirmek
`.env` dosyasında:
```bash
SUPERSET_PORT=9999
```

### CORS Origin Eklemek
`.env` dosyasında:
```bash
CORS_ORIGINS=http://localhost:44361,http://localhost:44308,http://yeni-origin:3000
```

### Session Süresini Değiştirmek
`.env` dosyasında:
```bash
SESSION_LIFETIME_HOURS=48  # 2 gün
```

## 🚀 Kullanım Senaryoları

### Senaryo 1: Development'tan Production'a Geçiş
Sadece `.env` dosyasını kopyala ve değerleri değiştir:

```bash
# Development .env
DATABASE_HOST=localhost
DATABASE_DB=SupersetDev

# Production .env
DATABASE_HOST=prod-db.example.com
DATABASE_DB=SupersetProd
```

### Senaryo 2: Farklı Ortamlar
Farklı `.env` dosyaları oluştur:

```bash
.env.development
.env.staging
.env.production
```

Kullanırken:
```bash
# Development
cp .env.development .env
docker-compose up -d

# Production
cp .env.production .env
docker-compose up -d
```

### Senaryo 3: CI/CD Pipeline
`.env` dosyasını GitLab/Azure DevOps variables'dan oluştur:

```yaml
# Azure DevOps Pipeline
- task: Bash@3
  inputs:
    targetType: 'inline'
    script: |
      cat > .env << EOF
      DATABASE_HOST=$(DB_HOST)
      DATABASE_USER=$(DB_USER)
      DATABASE_PASSWORD=$(DB_PASSWORD)
      EOF
```

## 🔒 Güvenlik

### .gitignore'a Ekle
```
.env
.env.local
.env.*.local
```

### Örnek .env Dosyası Oluştur
```bash
# .env.example dosyası oluştur (şifreler olmadan)
cp .env .env.example

# .env.example'daki şifreleri temizle
sed -i 's/DATABASE_PASSWORD=.*/DATABASE_PASSWORD=your-password-here/' .env.example
```

## 📝 Notlar

- **Varsayılan Değerler**: `.env` dosyası yoksa bile çalışır (hardcoded defaults var)
- **Override**: `.env` dosyasındaki değerler her zaman önceliklidir
- **Validation**: Yanlış değerler girilirse Docker hata verir
- **Hot Reload**: Değişikliklerden sonra `docker-compose restart` yeterli

## 🐛 Sorun Giderme

### Database bağlanamıyor
`.env` dosyasını kontrol et:
```bash
cat .env | grep DATABASE
```

### Port zaten kullanımda
`.env` dosyasında portu değiştir:
```bash
SUPERSET_PORT=8089
```

### Değişiklikler yansımıyor
Container'ı yeniden başlat:
```bash
docker-compose down
docker-compose up -d --force-recreate
```

## 📚 Ek Bilgiler

### Tüm Environment Variables
```bash
# Container içindeki değerleri görmek için
docker exec superset_app env | grep DATABASE
```

### Loglara Bakmak
```bash
# Hangi değerlerin kullanıldığını görmek için
docker-compose logs superset
```
