require 'sinatra'
require 'json'
require 'securerandom'
require 'time'
require 'nokogiri'
require 'base64'

set :port, 8084
set :bind, '0.0.0.0'
set :default_content_type, 'application/json'

# --- Almacenamiento en Memoria Thread-Safe ---
class NotificationStore
  def initialize
    @mutex = Mutex.new
    @destinatarios = [
      { id: 'DEST-001', nombre: 'Laura Gómez Flores', email: 'laura.gomez@example.com', telefono: '+525512345678', canal_preferido: 'EMAIL', activo: true },
      { id: 'DEST-002', nombre: 'Roberto Morales', email: 'roberto.m@example.com', telefono: '+529998765432', canal_preferido: 'SMS', activo: true },
      { id: 'DEST-003', nombre: 'Administrador de Almacén', email: 'almacen@ecommerce.com', telefono: '+525598765432', canal_preferido: 'WEBHOOK', activo: true }
    ]
    @plantillas = [
      { codigo: 'PEDIDO_CONFIRMADO', titulo: 'Tu orden fue recibida', cuerpo: 'Hola {nombre}, tu orden #{orden_id} ha sido confirmada.', canal: 'EMAIL' },
      { codigo: 'ENVIO_EN_TRANSITO', titulo: 'Tu paquete va en camino', cuerpo: 'El envío con guía {guia} se encuentra en tránsito a {destino}.', canal: 'SMS' },
      { codigo: 'ALERTA_STOCK_BAJO', titulo: 'Stock Crítico Detectado', cuerpo: 'Atención: El SKU {sku} tiene solo {stock} piezas disponibles.', canal: 'WEBHOOK' }
    ]
    @alertas_despachadas = []
  end

  def listar_destinatarios
    @mutex.synchronize { @destinatarios.dup }
  end

  def agregar_destinatario(datos)
    @mutex.synchronize do
      nuevo = {
        id: "DEST-#{SecureRandom.hex(3).upcase}",
        nombre: datos['nombre'],
        email: datos['email'],
        telefono: datos['telefono'] || '',
        canal_preferido: (datos['canal_preferido'] || 'EMAIL').upcase,
        activo: true,
        fecha_registro: Time.now.iso8601
      }
      @destinatarios << nuevo
      nuevo
    end
  end

  def actualizar_destinatario(id, datos)
    @mutex.synchronize do
      dest = @destinatarios.find { |d| d[:id] == id }
      return nil unless dest
      dest[:nombre] = datos['nombre'] if datos['nombre']
      dest[:email] = datos['email'] if datos['email']
      dest[:telefono] = datos['telefono'] if datos['telefono']
      dest[:canal_preferido] = datos['canal_preferido'].upcase if datos['canal_preferido']
      dest
    end
  end

  def eliminar_destinatario(id)
    @mutex.synchronize do
      idx = @destinatarios.find_index { |d| d[:id] == id }
      return false unless idx
      @destinatarios.delete_at(idx)
      true
    end
  end

  def listar_plantillas
    @mutex.synchronize { @plantillas.dup }
  end

  def agregar_plantilla(datos)
    @mutex.synchronize do
      nueva = {
        codigo: datos['codigo'].to_s.upcase,
        titulo: datos['titulo'],
        cuerpo: datos['cuerpo'],
        canal: (datos['canal'] || 'EMAIL').upcase
      }
      @plantillas.delete_if { |p| p[:codigo] == nueva[:codigo] }
      @plantillas << nueva
      nueva
    end
  end

  def registrar_alerta(alerta)
    @mutex.synchronize do
      @alertas_despachadas << alerta
      alerta
    end
  end

  def listar_alertas
    @mutex.synchronize { @alertas_despachadas.dup }
  end
end

STORE = NotificationStore.new

# --- Manejo de CORS ---
before do
  headers 'Access-Control-Allow-Origin' => '*',
          'Access-Control-Allow-Methods' => 'GET, POST, PUT, DELETE, OPTIONS',
          'Access-Control-Allow-Headers' => 'Content-Type, Authorization, X-API-Key, SOAPAction'
end

options '*' do
  response.headers['Access-Control-Allow-Origin'] = '*'
  response.headers['Access-Control-Allow-Methods'] = 'GET, POST, PUT, DELETE, OPTIONS'
  response.headers['Access-Control-Allow-Headers'] = 'Content-Type, Authorization, X-API-Key, SOAPAction'
  200
end

# --- Seguridad REST ---
VALID_BEARER = 'Bearer test_token_2026'
VALID_API_KEY = 'secret_key_cs'

before '/api/*' do
  return if request.request_method == 'OPTIONS'

  auth_header = request.env['HTTP_AUTHORIZATION']
  api_key_header = request.env['HTTP_X_API_KEY']

  authorized = (auth_header == VALID_BEARER) || (api_key_header == VALID_API_KEY)

  unless authorized
    halt 401, { 'Content-Type' => 'application/json' }, {
      error: 'Unauthorized',
      message: "Token Bearer o API-Key inválida o ausente. Use 'Authorization: Bearer test_token_2026' o 'X-API-Key: secret_key_cs'",
      statusCode: 401,
      timestamp: Time.now.iso8601
    }.to_json
  end
end

# --- Documentación OpenAPI / Swagger UI ---
get '/docs' do
  content_type 'text/html; charset=utf-8'
  File.read(File.join(__dir__, 'docs.html'))
end

get '/swagger' do
  redirect to('/docs')
end

get '/openapi.json' do
  content_type 'application/json; charset=utf-8'
  File.read(File.join(__dir__, 'openapi.json'))
end

# --- Rutas Informativas ---
get '/' do
  {
    servicio: 'Módulo de Notificaciones y Webhooks (Ruby Sinatra)',
    version: '1.0.0',
    puerto: 8084,
    swagger_ui: '/docs',
    openapi_spec: '/openapi.json',
    rutas_rest: [
      'GET /api/v1/destinatarios',
      'POST /api/v1/destinatarios',
      'GET /api/v1/plantillas',
      'POST /api/v1/plantillas',
      'GET /api/v1/alertas'
    ],
    servicio_soap: {
      wsdl: '/soap/notificaciones/wsdl',
      endpoint: '/soap/notificaciones'
    }
  }.to_json
end

# --- Rutas REST ---
get '/api/v1/destinatarios' do
  STORE.listar_destinatarios.to_json
end

post '/api/v1/destinatarios' do
  payload = JSON.parse(request.body.read) rescue {}
  if payload['nombre'].to_s.strip.empty? || payload['email'].to_s.strip.empty?
    halt 400, { error: 'Bad Request', message: 'Campos nombre y email son obligatorios', statusCode: 400 }.to_json
  end
  nuevo = STORE.agregar_destinatario(payload)
  status 201
  nuevo.to_json
end

put '/api/v1/destinatarios/:id' do
  payload = JSON.parse(request.body.read) rescue {}
  actualizado = STORE.actualizar_destinatario(params[:id], payload)
  unless actualizado
    halt 404, { error: 'Not Found', message: "Destinatario #{params[:id]} no encontrado", statusCode: 404 }.to_json
  end
  actualizado.to_json
end

delete '/api/v1/destinatarios/:id' do
  eliminado = STORE.eliminar_destinatario(params[:id])
  unless eliminado
    halt 404, { error: 'Not Found', message: "Destinatario #{params[:id]} no encontrado", statusCode: 404 }.to_json
  end
  { message: "Destinatario #{params[:id]} eliminado correctamente", statusCode: 200 }.to_json
end

get '/api/v1/plantillas' do
  STORE.listar_plantillas.to_json
end

post '/api/v1/plantillas' do
  payload = JSON.parse(request.body.read) rescue {}
  if payload['codigo'].to_s.strip.empty? || payload['cuerpo'].to_s.strip.empty?
    halt 400, { error: 'Bad Request', message: 'Campos codigo y cuerpo son obligatorios', statusCode: 400 }.to_json
  end
  nueva = STORE.agregar_plantilla(payload)
  status 201
  nueva.to_json
end

get '/api/v1/alertas' do
  STORE.listar_alertas.to_json
end

# --- Rutas SOAP ---
get '/soap/notificaciones/wsdl' do
  content_type 'text/xml; charset=utf-8'
  wsdl_path = File.join(__dir__, 'wsdl', 'notificaciones.wsdl')
  File.read(wsdl_path)
end

get '/soap/notificaciones' do
  if params.key?('wsdl') || params.key?('WSDL')
    content_type 'text/xml; charset=utf-8'
    File.read(File.join(__dir__, 'wsdl', 'notificaciones.wsdl'))
  else
    content_type 'text/plain'
    "Servicio SOAP Notificaciones activo. Consulte WSDL en /soap/notificaciones/wsdl"
  end
end

post '/soap/notificaciones' do
  content_type 'text/xml; charset=utf-8'
  raw_body = request.body.read

  # Validación de credenciales SOAP (Basic Auth o SOAP Header)
  auth_header = request.env['HTTP_AUTHORIZATION']
  if auth_header && auth_header.start_with?('Basic ')
    cred = Base64.decode64(auth_header.sub('Basic ', '')).split(':', 2) rescue []
    if cred[0] != 'admin' || cred[1] != 'admin_pass_2026'
      status 500
      return <<~XML
        <?xml version="1.0" encoding="UTF-8"?>
        <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">
          <soapenv:Body>
            <soapenv:Fault>
              <faultcode>soapenv:Client.AuthenticationFailed</faultcode>
              <faultstring>Credenciales SOAP inválidas o no autorizadas.</faultstring>
            </soapenv:Fault>
          </soapenv:Body>
        </soapenv:Envelope>
      XML
    end
  end

  # Parsear XML con Nokogiri
  doc = Nokogiri::XML(raw_body)
  doc.remove_namespaces!

  tipo_alerta = doc.xpath('//tipoAlerta').text.strip
  canal = doc.xpath('//canal').text.strip
  destinatario = doc.xpath('//destinatario').text.strip
  mensaje = doc.xpath('//mensaje').text.strip
  prioridad = doc.xpath('//prioridad').text.strip

  tipo_alerta = 'ALERTA_GENERAL' if tipo_alerta.empty?
  canal = 'EMAIL' if canal.empty?
  destinatario = 'sistema@ecommerce.com' if destinatario.empty?
  mensaje = 'Notificación del sistema' if mensaje.empty?
  prioridad = 'ALTA' if prioridad.empty?

  mensaje_id = "MSG-2026-#{SecureRandom.hex(4).upcase}"
  timestamp = Time.now.iso8601

  alerta = {
    mensaje_id: mensaje_id,
    tipo_alerta: tipo_alerta,
    canal: canal,
    destinatario: destinatario,
    mensaje: mensaje,
    prioridad: prioridad,
    entregado: true,
    timestamp: timestamp
  }
  STORE.registrar_alerta(alerta)

  <<~XML
    <?xml version="1.0" encoding="UTF-8"?>
    <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tns="http://ecommerce.com/notificaciones">
      <soapenv:Body>
        <tns:DespacharAlertaCriticaResponse>
          <resultado>
            <mensajeId>#{mensaje_id}</mensajeId>
            <tipoAlerta>#{tipo_alerta}</tipoAlerta>
            <canal>#{canal}</canal>
            <destinatario>#{destinatario}</destinatario>
            <entregado>true</entregado>
            <timestamp>#{timestamp}</timestamp>
            <estado>DESPACHADO_EXITOSAMENTE</estado>
            <detalle>Alerta de prioridad #{prioridad} encolada y entregada al canal #{canal}</detalle>
          </resultado>
        </tns:DespacharAlertaCriticaResponse>
      </soapenv:Body>
    </soapenv:Envelope>
  XML
end
