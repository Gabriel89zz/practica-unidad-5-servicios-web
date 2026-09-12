FROM php:8.2-cli

WORKDIR /var/www/html

# Instalar dependencias de sistema y extensiones PHP requeridas (soap, sqlite, libxml)
RUN apt-get update && apt-get install -y --no-install-recommends \
    libxml2-dev \
    sqlite3 \
    libsqlite3-dev \
    unzip \
    git \
    && docker-php-ext-install soap pdo pdo_sqlite \
    && rm -rf /var/lib/apt/lists/*

# Instalar Composer
COPY --from=composer:2 /usr/bin/composer /usr/bin/composer

COPY composer.json .
RUN composer install --no-dev --optimize-autoloader

COPY src ./src
COPY public ./public

EXPOSE 8083

CMD ["php", "-S", "0.0.0.0:8083", "-t", "public", "public/index.php"]
