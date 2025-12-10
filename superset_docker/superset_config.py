import os
from datetime import timedelta

# Environment variables'dan oku
def get_env(key, default=None):
    return os.environ.get(key, default)

# Secret Key
SECRET_KEY = get_env('SUPERSET_SECRET_KEY', 'YOUR_SUPER_SECRET_KEY_CHANGE_ME_PLEASE')

# Database - .env'den otomatik oluştur
DATABASE_DIALECT = get_env('DATABASE_DIALECT', 'postgresql')
DATABASE_USER = get_env('DATABASE_USER', 'postgres')
DATABASE_PASSWORD = get_env('DATABASE_PASSWORD', '123456')
DATABASE_HOST = get_env('DATABASE_HOST', 'host.docker.internal')
DATABASE_PORT = get_env('DATABASE_PORT', '5432')
DATABASE_DB = get_env('DATABASE_DB', 'Superset')

SQLALCHEMY_DATABASE_URI = f'{DATABASE_DIALECT}+psycopg2://{DATABASE_USER}:{DATABASE_PASSWORD}@{DATABASE_HOST}:{DATABASE_PORT}/{DATABASE_DB}'

# Session - Server-side
SESSION_TYPE = 'filesystem'
SESSION_FILE_DIR = '/tmp/superset_sessions'
SESSION_PERMANENT = True
SESSION_USE_SIGNER = True

# Session Cookie
SESSION_COOKIE_NAME = 'superset_session'
SESSION_COOKIE_SAMESITE = None
SESSION_COOKIE_SECURE = False
SESSION_COOKIE_HTTPONLY = False
SESSION_COOKIE_DOMAIN = None

# Session lifetime - .env'den oku
session_hours = int(get_env('SESSION_LIFETIME_HOURS', '24'))
PERMANENT_SESSION_LIFETIME = timedelta(hours=session_hours)

# CORS - .env'den origins listesini al
cors_origins_str = get_env('CORS_ORIGINS', 'http://localhost:44361,http://localhost:44308,http://localhost:8088')
cors_origins = [origin.strip() for origin in cors_origins_str.split(',')]

ENABLE_CORS = True
CORS_OPTIONS = {
    'supports_credentials': True,
    'allow_headers': ['*'],
    'resources': ['*'],
    'origins': cors_origins
}

# Security
TALISMAN_ENABLED = False
WTF_CSRF_ENABLED = False
WTF_CSRF_EXEMPT_LIST = []
WTF_CSRF_TIME_LIMIT = None

# Auth
AUTH_TYPE = 1  # AUTH_DB
AUTH_USER_REGISTRATION = False  # Disable self-registration

# Blueprint
def FLASK_APP_MUTATOR(app):
    from superset_auth import abp_auth_bp
    app.register_blueprint(abp_auth_bp)
    
    app.config['SESSION_COOKIE_SAMESITE'] = None
    app.config['SESSION_COOKIE_SECURE'] = False
    app.config['SESSION_COOKIE_HTTPONLY'] = False
    app.config['SESSION_TYPE'] = 'filesystem'
    app.config['SESSION_PERMANENT'] = True
