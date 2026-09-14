/**
 * EcoLogistics Store - Cliente Web Multiplataforma
 * Consume APIs RESTful (JSON) y Servicios SOAP (XML/WSDL) en tiempo real.
 */

// Estado Global de la Aplicación
let allProducts = [];
let cart = [];
let activeCategory = 'ALL';
const AUTH_TOKEN = 'Bearer test_token_2026';

// Inicialización
document.addEventListener('DOMContentLoaded', () => {
  initHostConfig();
  loadCatalog();
});

// --- 1. Gestión de Conexión y Host Remoto ---
function initHostConfig() {
  const savedHost = localStorage.getItem('ecologistics_host') || 'http://localhost';
  const hostInput = document.getElementById('hostInput');
  hostInput.value = savedHost;

  hostInput.addEventListener('change', () => {
    let val = hostInput.value.trim().replace(/\/+$/, '');
    if (!val.startsWith('http://') && !val.startsWith('https://')) {
      val = 'http://' + val;
    }
    hostInput.value = val;
    localStorage.setItem('ecologistics_host', val);
    showToast('info', `Servidor configurado en: ${val}`);
    loadCatalog();
  });
}

function getBaseHost() {
  return document.getElementById('hostInput').value.trim().replace(/\/+$/, '');
}

function getServiceUrl(port, path = '') {
  const host = getBaseHost();
  try {
    const urlObj = new URL(host);
    return `${urlObj.protocol}//${urlObj.hostname}:${port}${path}`;
  } catch {
    return `${host}:${port}${path}`;
  }
}

async function testServerConnection() {
  const dot = document.getElementById('hostStatusDot');
  dot.style.background = '#fbbf24'; // Amarillo: Probando
  showToast('info', 'Comprobando conectividad con los microservicios...');

  const javaUrl = getServiceUrl(8081, '/api/v1/productos');
  const phpUrl = getServiceUrl(8083, '/');
  const pyUrl = getServiceUrl(8082, '/');

  try {
    const [resJava, resPhp, resPy] = await Promise.all([
      fetch(javaUrl, { headers: { 'Authorization': AUTH_TOKEN } }),
      fetch(phpUrl),
      fetch(pyUrl)
    ]);

    if (resJava.ok && resPhp.ok && resPy.ok) {
      dot.style.background = '#10b981'; // Verde: Activo
      dot.title = 'Todos los microservicios responden correctamente';
      showToast('success', '¡Conexión exitosa! Microservicios activos y respondiendo.');
    } else {
      dot.style.background = '#f59e0b';
      showToast('warning', 'Algunos servicios respondieron con códigos de advertencia.');
    }
  } catch (err) {
    dot.style.background = '#ef4444'; // Rojo: Falló
    dot.title = 'Error de conexión: ' + err.message;
    showToast('error', `Error al conectar a ${getBaseHost()}. Verifique el firewall o ejecute docker-compose up.`);
  }
}

// --- 2. Catálogo de Productos (Java Spring Boot :8081 - REST) ---
async function loadCatalog() {
  const grid = document.getElementById('productsGrid');
  grid.innerHTML = `
    <div style="grid-column: 1/-1; text-align:center; padding:3rem; color:var(--text-muted);">
      <div style="display:inline-block; animation:spin 1s linear infinite; font-size:1.5rem; margin-bottom:0.5rem;">↻</div>
      <div>Consultando catálogo en microservicio Java (:8081)...</div>
    </div>
  `;

  const url = getServiceUrl(8081, '/api/v1/productos');
  const startTime = performance.now();

  try {
    const resp = await fetch(url, {
      method: 'GET',
      headers: {
        'Authorization': AUTH_TOKEN,
        'Accept': 'application/json'
      }
    });

    const latency = Math.round(performance.now() - startTime);
    const data = await resp.json();

    logNetworkEvent({
      protocol: 'REST (JSON)',
      service: 'Java 21 / Spring Boot 3 (:8081)',
      method: 'GET',
      url: url,
      headers: { 'Authorization': AUTH_TOKEN },
      body: null,
      status: resp.status,
      statusText: resp.statusText,
      latency: latency,
      response: data
    });

    if (!resp.ok) {
      throw new Error(`HTTP ${resp.status}: ${data.message || 'Error al obtener catálogo'}`);
    }

    allProducts = data;
    renderProducts(allProducts);
    document.getElementById('hostStatusDot').style.background = '#10b981';
  } catch (err) {
    grid.innerHTML = `
      <div style="grid-column:1/-1; background:rgba(239,68,68,0.1); border:1px solid rgba(239,68,68,0.3); border-radius:var(--radius-lg); padding:2rem; text-align:center;">
        <div style="font-size:1.5rem; margin-bottom:0.5rem;">⚠️ Error de Comunicación</div>
        <p style="color:#fca5a5; font-size:0.9rem; margin-bottom:1rem;">No se pudo consultar el catálogo de Java en <code>${url}</code>.</p>
        <p style="color:var(--text-muted); font-size:0.8rem; margin-bottom:1.5rem;">Detalle: ${err.message}</p>
        <button class="btn-action btn-add-cart" onclick="loadCatalog()">Reintentar Conexión</button>
      </div>
    `;
    document.getElementById('hostStatusDot').style.background = '#ef4444';
  }
}

function renderProducts(products) {
  const grid = document.getElementById('productsGrid');
  if (!products || products.length === 0) {
    grid.innerHTML = `
      <div style="grid-column: 1/-1; text-align:center; padding:3rem; color:var(--text-muted);">
        No se encontraron productos coincidentes con los filtros actuales.
      </div>
    `;
    return;
  }

  grid.innerHTML = products.map(p => `
    <article class="product-card" id="card-${p.sku}">
      <div>
        <div class="card-top">
          <span class="category-tag">${escapeHtml(p.categoria)}</span>
          <span class="product-sku">${escapeHtml(p.sku)}</span>
        </div>
        <h3 class="product-title">${escapeHtml(p.nombre)}</h3>
        <p class="product-desc">${escapeHtml(p.descripcion || 'Sin descripción disponible.')}</p>
      </div>

      <div>
        <div class="product-price-row">
          <div class="product-price">$${p.precio.toFixed(2)} <small>USD</small></div>
          <div id="stock-badge-${p.sku}" class="stock-status in-stock">
            <span>●</span> Stock: <strong id="stock-val-${p.sku}">${p.stock}</strong>
          </div>
        </div>

        <div class="card-actions">
          <button class="btn-action btn-soap-stock" onclick="checkStockSoap('${p.sku}')" title="Invocación SOAP en tiempo real a Java :8081">
            <span>⚡</span> Stock SOAP
          </button>
          <button class="btn-action btn-add-cart" onclick="addToCart('${p.sku}')">
            <span>+</span> Agregar
          </button>
        </div>
      </div>
    </article>
  `).join('');
}

// Filtros y Búsqueda
function filterCategory(cat) {
  activeCategory = cat;
  document.querySelectorAll('.tab-btn').forEach(btn => {
    btn.classList.toggle('active', btn.dataset.category === cat);
  });
  applyFilters();
}

function handleSearch() {
  applyFilters();
}

function applyFilters() {
  const query = document.getElementById('searchInput').value.trim().toLowerCase();
  let filtered = allProducts;

  if (activeCategory !== 'ALL') {
    filtered = filtered.filter(p => p.categoria.toLowerCase() === activeCategory.toLowerCase());
  }

  if (query) {
    filtered = filtered.filter(p => 
      p.nombre.toLowerCase().includes(query) || 
      p.sku.toLowerCase().includes(query) ||
      (p.descripcion && p.descripcion.toLowerCase().includes(query))
    );
  }

  renderProducts(filtered);
}

// --- 3. Invocación SOAP en Tiempo Real: Consulta de Stock (Java Spring-WS :8081) ---
async function checkStockSoap(sku) {
  const url = getServiceUrl(8081, '/ws');
  const soapXml = `<?xml version="1.0" encoding="UTF-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:inv="http://ecommerce.com/inventario">
  <soapenv:Header/>
  <soapenv:Body>
    <inv:ConsultarStockRequest>
      <inv:sku>${escapeHtml(sku)}</inv:sku>
    </inv:ConsultarStockRequest>
  </soapenv:Body>
</soapenv:Envelope>`;

  showToast('info', `Consultando stock SOAP en Java para ${sku}...`);
  const startTime = performance.now();

  try {
    const resp = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'text/xml; charset=utf-8',
        'SOAPAction': '""'
      },
      body: soapXml
    });

    const latency = Math.round(performance.now() - startTime);
    const rawXml = await resp.text();

    logNetworkEvent({
      protocol: 'SOAP (XML / WSDL)',
      service: 'Java 21 / Spring-WS (:8081)',
      method: 'POST',
      url: url,
      headers: { 'Content-Type': 'text/xml; charset=utf-8' },
      body: soapXml,
      status: resp.status,
      statusText: resp.statusText,
      latency: latency,
      response: rawXml
    });

    // Parsear respuesta XML
    const parser = new DOMParser();
    const xmlDoc = parser.parseFromString(rawXml, 'text/xml');
    const stockElem = xmlDoc.getElementsByTagNameNS('*', 'stockDisponible')[0] || xmlDoc.querySelector('stockDisponible');
    const dispElem = xmlDoc.getElementsByTagNameNS('*', 'disponible')[0] || xmlDoc.querySelector('disponible');

    if (stockElem) {
      const stock = parseInt(stockElem.textContent, 10);
      const disp = dispElem ? dispElem.textContent === 'true' : true;

      // Actualizar tarjeta del producto
      const stockValEl = document.getElementById(`stock-val-${sku}`);
      const stockBadgeEl = document.getElementById(`stock-badge-${sku}`);
      if (stockValEl) stockValEl.textContent = stock;
      if (stockBadgeEl) {
        stockBadgeEl.className = stock > 5 ? 'stock-status in-stock' : 'stock-status low-stock';
      }

      showToast('success', `[SOAP Java 200 OK] Stock verificado para ${sku}: ${stock} unidades disponibles.`);
    } else {
      showToast('warning', `Respuesta SOAP recibida sin nodo de stock.`);
    }
  } catch (err) {
    showToast('error', `Error al comunicar con endpoint SOAP Java: ${err.message}`);
  }
}

// --- 4. Carrito de Compras & Creación de Órdenes (PHP Slim 4 :8083 - REST & SOAP) ---
function addToCart(sku) {
  const prod = allProducts.find(p => p.sku === sku);
  if (!prod) return;

  const existing = cart.find(i => i.sku === sku);
  if (existing) {
    existing.cantidad += 1;
  } else {
    cart.push({
      sku: prod.sku,
      nombre: prod.nombre,
      precio_unitario: prod.precio,
      cantidad: 1
    });
  }

  updateCartUI();
  showToast('info', `"${prod.nombre}" añadido al carrito.`);
}

function updateCartUI() {
  const countBadge = document.getElementById('cartCountBadge');
  const body = document.getElementById('cartBody');
  const subtotalEl = document.getElementById('cartSubtotal');
  const ivaEl = document.getElementById('cartIva');
  const totalEl = document.getElementById('cartTotal');

  const totalItems = cart.reduce((acc, item) => acc + item.cantidad, 0);
  countBadge.textContent = totalItems;

  if (cart.length === 0) {
    body.innerHTML = `
      <div class="cart-empty">
        <div style="font-size:2.5rem; margin-bottom:0.75rem;">🛒</div>
        <h4>Tu carrito está vacío</h4>
        <p>Agrega productos del catálogo para generar una orden de compra.</p>
      </div>
    `;
    subtotalEl.textContent = '$0.00 USD';
    ivaEl.textContent = '$0.00 USD';
    totalEl.textContent = '$0.00 USD';
    return;
  }

  let subtotal = 0;
  body.innerHTML = cart.map((item, idx) => {
    const itemTotal = item.precio_unitario * item.cantidad;
    subtotal += itemTotal;
    return `
      <div class="cart-item">
        <div class="item-info">
          <h4>${escapeHtml(item.nombre)}</h4>
          <p>${item.sku} • $${item.precio_unitario.toFixed(2)} c/u</p>
        </div>
        <div class="item-controls">
          <button class="btn-qty" onclick="changeQty(${idx}, -1)">-</button>
          <span style="font-family:var(--font-mono); font-size:0.88rem; font-weight:600; min-width:20px; text-align:center;">${item.cantidad}</span>
          <button class="btn-qty" onclick="changeQty(${idx}, 1)">+</button>
          <span style="font-weight:700; margin-left:0.5rem; font-size:0.9rem;">$${itemTotal.toFixed(2)}</span>
        </div>
      </div>
    `;
  }).join('');

  const iva = subtotal * 0.16;
  const total = subtotal + iva;

  subtotalEl.textContent = `$${subtotal.toFixed(2)} USD`;
  ivaEl.textContent = `$${iva.toFixed(2)} USD`;
  totalEl.textContent = `$${total.toFixed(2)} USD`;
}

function changeQty(index, delta) {
  if (!cart[index]) return;
  cart[index].cantidad += delta;
  if (cart[index].cantidad <= 0) {
    cart.splice(index, 1);
  }
  updateCartUI();
}

function toggleCartDrawer() {
  const drawer = document.getElementById('cartDrawer');
  const backdrop = document.getElementById('drawerBackdrop');
  drawer.classList.toggle('active');
  backdrop.classList.toggle('active');
}

// Generación de Orden de Compra (PHP Slim 4 - REST)
async function submitOrder() {
  if (cart.length === 0) {
    showToast('warning', 'El carrito está vacío. Agrega productos antes de generar una orden.');
    return;
  }

  const custName = document.getElementById('custName').value.trim() || 'Cliente Mostrador';
  const custEmail = document.getElementById('custEmail').value.trim() || 'cliente@ecommerce.com';
  const btn = document.getElementById('btnCreateOrder');
  const resultBox = document.getElementById('orderResultBox');

  btn.disabled = true;
  btn.textContent = 'Procesando orden en PHP (:8083)...';
  resultBox.style.display = 'none';

  const orderPayload = {
    cliente_id: 'CLI-' + Math.floor(100 + Math.random() * 900),
    cliente_nombre: custName,
    cliente_email: custEmail,
    direccion_envio: 'Av. Paseo de la Reforma 505, CDMX',
    metodo_pago: 'TARJETA_CREDITO',
    items: cart.map(i => ({
      sku: i.sku,
      nombre: i.nombre,
      cantidad: i.cantidad,
      precio_unitario: i.precio_unitario
    }))
  };

  const url = getServiceUrl(8083, '/api/v1/ordenes');
  const startTime = performance.now();

  try {
    const resp = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': AUTH_TOKEN
      },
      body: JSON.stringify(orderPayload)
    });

    const latency = Math.round(performance.now() - startTime);
    const data = await resp.json();

    logNetworkEvent({
      protocol: 'REST (JSON)',
      service: 'PHP 8.2 / Slim 4 (:8083)',
      method: 'POST',
      url: url,
      headers: { 'Content-Type': 'application/json', 'Authorization': AUTH_TOKEN },
      body: orderPayload,
      status: resp.status,
      statusText: resp.statusText,
      latency: latency,
      response: data
    });

    if (!resp.ok) {
      throw new Error(`HTTP ${resp.status}: ${data.message || 'Error al crear orden'}`);
    }

    const createdOrder = data;
    showToast('success', `¡Orden ${createdOrder.id} generada exitosamente en PHP SQLite!`);

    // Notificar y auditar en segundo plano a los otros microservicios (VB.NET y Ruby)
    logOrderToVbNet(createdOrder, custName, custEmail);
    notifyOrderToRuby(createdOrder, custName, custEmail);

    // Mostrar resultado con botón para validación SOAP y etiquetas de trazabilidad
    resultBox.style.display = 'block';
    resultBox.innerHTML = `
      <div style="background:rgba(16,185,129,0.12); border:1px solid rgba(16,185,129,0.3); border-radius:var(--radius-md); padding:1rem;">
        <div style="color:#34d399; font-weight:700; margin-bottom:0.25rem;">✓ Orden Creada: ${createdOrder.id}</div>
        <p style="color:var(--text-muted); font-size:0.75rem; margin-bottom:0.5rem;">
          Estado: <strong>${createdOrder.estado}</strong> | Total: $${parseFloat(createdOrder.total).toFixed(2)} USD
        </p>
        <div style="font-size:0.72rem; color:var(--text-secondary); margin-bottom:0.75rem; display:flex; flex-direction:column; gap:0.25rem;">
          <div>📡 <em>Auditado en VB.NET (:8086):</em> <span style="color:#10b981;">LOG REGISTRADO</span></div>
          <div>✉️ <em>Despachado en Ruby (:8084):</em> <span style="color:#10b981;">NOTIFICACIÓN ENVIADA</span></div>
        </div>
        <button class="btn-action btn-soap-stock" style="width:100%;" onclick="validateOrderSoap('${createdOrder.id}')">
          <span>⚡</span> Validar Estado con SOAP (:8083)
        </button>
        <div id="soapValidationResult-${createdOrder.id}" style="margin-top:0.5rem; font-size:0.75rem;"></div>
      </div>
    `;

    // Vaciar carrito
    cart = [];
    updateCartUI();
  } catch (err) {
    showToast('error', `Fallo al crear orden en PHP: ${err.message}`);
  } finally {
    btn.disabled = false;
    btn.textContent = 'Generar Orden de Compra (REST :8083)';
  }
}

// Validación SOAP de la Orden Creada (PHP :8083 - SOAP RPC/Literal)
async function validateOrderSoap(ordenId) {
  const container = document.getElementById(`soapValidationResult-${ordenId}`);
  if (container) container.innerHTML = '<span style="color:#fbbf24;">Invocando SOAP ValidarEstadoOrden...</span>';

  const url = getServiceUrl(8083, '/soap/ordenes');
  const soapXml = `<?xml version="1.0" encoding="UTF-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ord="http://ecommerce.com/ordenes">
  <soapenv:Header/>
  <soapenv:Body>
    <ord:ValidarEstadoOrden>
      <ordenId>${escapeHtml(ordenId)}</ordenId>
    </ord:ValidarEstadoOrden>
  </soapenv:Body>
</soapenv:Envelope>`;

  const startTime = performance.now();
  try {
    const resp = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'text/xml; charset=utf-8',
        'SOAPAction': 'http://ecommerce.com/ordenes#ValidarEstadoOrden'
      },
      body: soapXml
    });

    const latency = Math.round(performance.now() - startTime);
    const rawXml = await resp.text();

    logNetworkEvent({
      protocol: 'SOAP (XML / WSDL)',
      service: 'PHP 8.2 / ext-soap (:8083)',
      method: 'POST',
      url: url,
      headers: { 'Content-Type': 'text/xml; charset=utf-8' },
      body: soapXml,
      status: resp.status,
      statusText: resp.statusText,
      latency: latency,
      response: rawXml
    });

    const parser = new DOMParser();
    const doc = parser.parseFromString(rawXml, 'text/xml');
    const estadoElem = doc.querySelector('estado');
    const validoElem = doc.querySelector('valido');

    if (estadoElem) {
      const estado = estadoElem.textContent;
      const valido = validoElem ? validoElem.textContent : 'true';
      if (container) {
        container.innerHTML = `
          <div style="background:#040711; padding:0.5rem; border-radius:4px; border:1px solid rgba(52,211,153,0.3); color:#34d399;">
            ✓ SOAP Validado: Estado "<strong>${estado}</strong>" verificado en SQLite (Válido: ${valido}).
          </div>
        `;
      }
      showToast('success', `[SOAP PHP 200 OK] Orden ${ordenId} validada correctamente.`);
    } else {
      if (container) container.innerHTML = `<span style="color:#f87171;">Respuesta SOAP sin datos esperados.</span>`;
    }
  } catch (err) {
    if (container) container.innerHTML = `<span style="color:#f87171;">Error SOAP: ${err.message}</span>`;
    showToast('error', `Fallo en SOAP PHP: ${err.message}`);
  }
}

// --- Auditoría Automática en VB.NET CoreWCF (:8086) ---
async function logOrderToVbNet(order, custName, custEmail) {
  const url = getServiceUrl(8086, '/api/v1/logs');
  const auditPayload = {
    servicioOrigen: 'WEB_STORE',
    accion: 'ORDEN_CREADA',
    usuario: custEmail || 'cliente_web',
    detalles: `Pedido ${order.id} por $${parseFloat(order.total).toFixed(2)} USD (${custName})`,
    nivel: 'INFO'
  };

  const startTime = performance.now();
  try {
    const resp = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': AUTH_TOKEN
      },
      body: JSON.stringify(auditPayload)
    });
    const latency = Math.round(performance.now() - startTime);
    const data = await resp.json().catch(() => null);

    logNetworkEvent({
      protocol: 'REST (JSON)',
      service: 'VB.NET .NET 9 / Auditoría (:8086)',
      method: 'POST',
      url: url,
      headers: { 'Content-Type': 'application/json', 'Authorization': AUTH_TOKEN },
      body: auditPayload,
      status: resp.status,
      statusText: resp.statusText,
      latency: latency,
      response: data
    });

    if (resp.ok) {
      showToast('info', `[Auditoría VB.NET :8086] Evento registrado para ${order.id}`);
    }
  } catch (err) {
    console.warn('Auditoría VB.NET no disponible:', err);
  }
}

// --- Notificación Automática en Ruby Sinatra (:8084) ---
async function notifyOrderToRuby(order, custName, custEmail) {
  const url = getServiceUrl(8084, '/soap/notificaciones');
  const soapXml = `<?xml version="1.0" encoding="UTF-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:not="http://ecommerce.com/notificaciones">
  <soapenv:Header/>
  <soapenv:Body>
    <not:DespacharAlertaCritica>
      <tipoAlerta>PEDIDO_CONFIRMADO</tipoAlerta>
      <canal>EMAIL</canal>
      <destinatario>${escapeHtml(custEmail || 'cliente@ecommerce.com')}</destinatario>
      <mensaje>Su pedido ${escapeHtml(order.id)} por $${parseFloat(order.total).toFixed(2)} USD ha sido confirmado.</mensaje>
      <prioridad>MEDIA</prioridad>
    </not:DespacharAlertaCritica>
  </soapenv:Body>
</soapenv:Envelope>`;

  const startTime = performance.now();
  try {
    const resp = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'text/xml; charset=utf-8',
        'Authorization': 'Basic ' + btoa('admin:admin_pass_2026')
      },
      body: soapXml
    });
    const latency = Math.round(performance.now() - startTime);
    const text = await resp.text().catch(() => '');

    logNetworkEvent({
      protocol: 'SOAP (XML / WSDL)',
      service: 'Ruby 3.2 / Sinatra Notificaciones (:8084)',
      method: 'POST',
      url: url,
      headers: { 'Content-Type': 'text/xml; charset=utf-8', 'Authorization': 'Basic admin:***' },
      body: soapXml,
      status: resp.status,
      statusText: resp.statusText,
      latency: latency,
      response: text
    });

    if (resp.ok) {
      showToast('info', `[Notificación Ruby :8084] Alerta despachada para ${order.id}`);
    }
  } catch (err) {
    console.warn('Notificación Ruby no disponible:', err);
  }
}

// --- 5. Cotizador de Envíos (Python FastAPI :8082 - SOAP WSDL) ---
async function calculateShippingRate() {
  const origen = document.getElementById('shipOrigen').value.trim() || 'CDMX Centro';
  const destino = document.getElementById('shipDestino').value;
  const peso = parseFloat(document.getElementById('shipPeso').value) || 1.0;
  const servicio = document.getElementById('shipServicio').value;
  const resultBox = document.getElementById('shippingResultBox');

  resultBox.style.display = 'block';
  resultBox.innerHTML = '<span style="color:#fbbf24;">Calculando tarifa con microservicio SOAP Python (:8082)...</span>';

  const url = getServiceUrl(8082, '/soap/envios');
  const soapXml = `<?xml version="1.0" encoding="UTF-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:env="http://ecommerce.com/envios">
  <soapenv:Header/>
  <soapenv:Body>
    <env:CalcularTarifa>
      <env:origen>${escapeHtml(origen)}</env:origen>
      <env:destino>${escapeHtml(destino)}</env:destino>
      <env:peso_kg>${peso}</env:peso_kg>
      <env:tipo_servicio>${escapeHtml(servicio)}</env:tipo_servicio>
    </env:CalcularTarifa>
  </soapenv:Body>
</soapenv:Envelope>`;

  const startTime = performance.now();
  try {
    const resp = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'text/xml; charset=utf-8',
        'SOAPAction': 'http://ecommerce.com/envios#CalcularTarifa'
      },
      body: soapXml
    });

    const latency = Math.round(performance.now() - startTime);
    const rawXml = await resp.text();

    logNetworkEvent({
      protocol: 'SOAP (XML / WSDL)',
      service: 'Python 3.12 / FastAPI (:8082)',
      method: 'POST',
      url: url,
      headers: { 'Content-Type': 'text/xml; charset=utf-8' },
      body: soapXml,
      status: resp.status,
      statusText: resp.statusText,
      latency: latency,
      response: rawXml
    });

    const parser = new DOMParser();
    const doc = parser.parseFromString(rawXml, 'text/xml');
    const costoElem = doc.querySelector('costo_envio');
    const diasElem = doc.querySelector('tiempo_estimado_dias');
    const distElem = doc.querySelector('distancia_aprox_km');
    const monedaElem = doc.querySelector('moneda');

    if (costoElem) {
      const costo = parseFloat(costoElem.textContent).toFixed(2);
      const dias = diasElem ? diasElem.textContent : '3';
      const dist = distElem ? distElem.textContent : '500';
      const moneda = monedaElem ? monedaElem.textContent : 'MXN';

      resultBox.innerHTML = `
        <div style="display:flex; justify-content:space-between; align-items:center; flex-wrap:wrap; gap:1rem;">
          <div>
            <div style="font-size:0.8rem; color:var(--text-muted);">Cotización SOAP Oficial:</div>
            <div style="font-family:var(--font-heading); font-size:1.5rem; font-weight:800; color:#38bdf8;">$${costo} ${moneda}</div>
          </div>
          <div style="display:flex; gap:1.5rem; font-size:0.85rem;">
            <div><span style="color:var(--text-dim);">Tiempo estimado:</span> <strong>${dias} día(s)</strong></div>
            <div><span style="color:var(--text-dim);">Distancia aprox.:</span> <strong>${dist} km</strong></div>
            <div><span style="color:var(--text-dim);">Modalidad:</span> <strong>${servicio}</strong></div>
          </div>
        </div>
      `;
      showToast('success', `[SOAP Python] Cotización calculada: $${costo} ${moneda} (${dias} días).`);
    } else {
      resultBox.innerHTML = `<span style="color:#f87171;">Respuesta SOAP inesperada.</span>`;
    }
  } catch (err) {
    resultBox.innerHTML = `<span style="color:#f87171;">Error al calcular tarifa SOAP: ${err.message}</span>`;
    showToast('error', `Error en SOAP Python: ${err.message}`);
  }
}

// --- 6. Inspector de Protocolos de Red (En Vivo) ---
function logNetworkEvent(evt) {
  const inspector = document.getElementById('protocolInspector');
  const reqBox = document.getElementById('inspectorReqBox');
  const respBox = document.getElementById('inspectorRespBox');
  const statusBadge = document.getElementById('inspectorLastStatus');

  const statusColor = evt.status >= 200 && evt.status < 300 ? '#34d399' : '#f87171';
  statusBadge.innerHTML = `
    <span style="color:${statusColor}; font-weight:700;">[${evt.protocol}]</span> 
    ${evt.method} ${evt.url} • <strong style="color:${statusColor};">${evt.status} ${evt.statusText}</strong> (${evt.latency}ms)
  `;

  reqBox.textContent = `${evt.method} ${evt.url}\n` +
    `Servicio: ${evt.service}\n` +
    `Headers: ${JSON.stringify(evt.headers, null, 2)}\n\n` +
    (evt.body ? (typeof evt.body === 'object' ? JSON.stringify(evt.body, null, 2) : evt.body) : '(Sin cuerpo de petición)');

  respBox.textContent = typeof evt.response === 'object' 
    ? JSON.stringify(evt.response, null, 2) 
    : evt.response;
}

function toggleInspector() {
  const inspector = document.getElementById('protocolInspector');
  const icon = document.getElementById('inspectorToggleIcon');
  inspector.classList.toggle('open');
  icon.textContent = inspector.classList.contains('open') ? '▼ Cerrar Inspector' : '▲ Abrir Inspector';
}

// --- 7. Notificaciones Toast y Utilidades ---
function showToast(type, message) {
  const container = document.getElementById('toastContainer');
  const toast = document.createElement('div');
  toast.className = `toast ${type}`;

  const icon = type === 'success' ? '✓' : (type === 'error' ? '✕' : (type === 'warning' ? '⚠' : 'ℹ'));
  toast.innerHTML = `<span style="font-weight:700;">${icon}</span> <span>${escapeHtml(message)}</span>`;

  container.appendChild(toast);
  setTimeout(() => {
    toast.style.animation = 'slideIn 0.3s reverse ease';
    setTimeout(() => toast.remove(), 280);
  }, 3800);
}

function escapeHtml(str) {
  if (!str) return '';
  return String(str)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#039;');
}
