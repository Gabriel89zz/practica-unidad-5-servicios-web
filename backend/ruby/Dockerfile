FROM ruby:3.2-slim

WORKDIR /app

# Instalar dependencias de compilación para gems nativas (nokogiri, puma)
RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential \
    libxml2-dev \
    libxslt1-dev \
    && rm -rf /var/lib/apt/lists/*

COPY Gemfile ./
RUN bundle install

COPY . .

EXPOSE 8084

CMD ["bundle", "exec", "ruby", "app.rb"]
