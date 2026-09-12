<?php
use Psr\Http\Message\ResponseInterface as Response;
use Psr\Http\Message\ServerRequestInterface as Request;
use Slim\Factory\AppFactory;
use App\OrderService;
use App\SoapHandler;

require __DIR__ . '/../vendor/autoload.php';

$uri = parse_url($_SERVER['REQUEST_URI'], PHP_URL_PATH);

// Si es un archivo físico estático existente que no sea la raíz, permitir que PHP lo sirva directamente
if ($uri !== '/' && is_file(__DIR__ . $uri)) {
    return false;
}

// Manejo previo de CORS para todas las rutas
if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    header("Access-Control-Allow-Origin: *");
    header("Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS");
    header("Access-Control-Allow-Headers: Content-Type, Authorization, X-API-Key, SOAPAction");
    header("Access-Control-Max-Age: 3600");
    http_response_code(200);
    exit;
}
header("Access-Control-Allow-Origin: *");
header("Access-Control-Allow-Headers: Content-Type, Authorization, X-API-Key, SOAPAction");

// 1. Manejo del servicio SOAP
if ($uri === '/soap/ordenes.wsdl' || (str_starts_with($uri, '/soap/ordenes') && isset($_GET['wsdl']))) {
    header('Content-Type: text/xml; charset=utf-8');
    readfile(__DIR__ . '/soap/ordenes.wsdl');
    exit;
}

if ($uri === '/soap/ordenes' && $_SERVER['REQUEST_METHOD'] === 'POST') {
    header("Access-Control-Allow-Origin: *");
    $wsdl = __DIR__ . '/soap/ordenes.wsdl';
    
    // Si la extensión soap está instalada, usamos el SoapServer nativo de PHP
    if (class_exists('SoapServer')) {
        ini_set("soap.wsdl_cache_enabled", "0");
        $server = new SoapServer($wsdl, ['uri' => 'http://ecommerce.com/ordenes']);
        $server->setClass(SoapHandler::class);
        $server->handle();
    } else {
        // Fallback robusto en caso de entorno sin extensión soap activa
        $rawXml = file_get_contents('php://input');
        preg_match('/<ordenId[^>]*>([^<]+)<\/ordenId>/i', $rawXml, $matches);
        $ordenId = $matches[1] ?? '';
        
        $handler = new SoapHandler();
        try {
            $res = $handler->ValidarEstadoOrden($ordenId);
            header('Content-Type: text/xml; charset=utf-8');
            echo '<?xml version="1.0" encoding="UTF-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tns="http://ecommerce.com/ordenes">
  <soap:Body>
    <tns:ValidarEstadoOrdenResponse>
      <resultado>
        <id>' . htmlspecialchars($res['id']) . '</id>
        <clienteId>' . htmlspecialchars($res['clienteId']) . '</clienteId>
        <clienteNombre>' . htmlspecialchars($res['clienteNombre']) . '</clienteNombre>
        <estado>' . htmlspecialchars($res['estado']) . '</estado>
        <total>' . $res['total'] . '</total>
        <pagado>' . ($res['pagado'] ? 'true' : 'false') . '</pagado>
        <fechaCreacion>' . htmlspecialchars($res['fechaCreacion']) . '</fechaCreacion>
        <valido>' . ($res['valido'] ? 'true' : 'false') . '</valido>
        <mensaje>' . htmlspecialchars($res['mensaje']) . '</mensaje>
      </resultado>
    </tns:ValidarEstadoOrdenResponse>
  </soap:Body>
</soap:Envelope>';
        } catch (Exception $e) {
            http_response_code(500);
            header('Content-Type: text/xml; charset=utf-8');
            echo '<?xml version="1.0" encoding="UTF-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <soap:Fault>
      <faultcode>soap:Client.AuthenticationFailed</faultcode>
      <faultstring>' . htmlspecialchars($e->getMessage()) . '</faultstring>
    </soap:Fault>
  </soap:Body>
</soap:Envelope>';
        }
    }
    exit;
}

// 2. Inicializar Slim 4 para endpoints REST
$app = AppFactory::create();
$app->addBodyParsingMiddleware();
$app->addRoutingMiddleware();

// Middleware de Autenticación REST para rutas /api/**
$authMiddleware = function (Request $request, $handler) {
    $authHeader = $request->getHeaderLine('Authorization');
    $apiKeyHeader = $request->getHeaderLine('X-API-Key');

    $authorized = ($authHeader === 'Bearer test_token_2026') || ($apiKeyHeader === 'secret_key_cs');

    if (!$authorized) {
        $response = new \Slim\Psr7\Response();
        $payload = json_encode([
            'error' => 'Unauthorized',
            'message' => "Token Bearer o API-Key inválida o ausente. Use 'Authorization: Bearer test_token_2026' o 'X-API-Key: secret_key_cs'",
            'statusCode' => 401,
            'timestamp' => date('c')
        ]);
        $response->getBody()->write($payload);
        return $response->withHeader('Content-Type', 'application/json')->withStatus(401);
    }

    return $handler->handle($request);
};

// Rutas de Documentación OpenAPI / Swagger UI
$app->get('/docs', function (Request $request, Response $response) {
    $html = file_get_contents(__DIR__ . '/docs.html');
    $response->getBody()->write($html);
    return $response->withHeader('Content-Type', 'text/html; charset=utf-8');
});

$app->get('/swagger', function (Request $request, Response $response) {
    return $response->withHeader('Location', '/docs')->withStatus(302);
});

$app->get('/openapi.json', function (Request $request, Response $response) {
    $json = file_get_contents(__DIR__ . '/openapi.json');
    $response->getBody()->write($json);
    return $response->withHeader('Content-Type', 'application/json; charset=utf-8');
});

// Rutas REST
$app->get('/', function (Request $request, Response $response) {
    $data = [
        'servicio' => 'Gestión de Órdenes de Compra (PHP Slim 4)',
        'estado' => 'Activo',
        'puerto' => 8083,
        'swagger_ui' => '/docs',
        'openapi_spec' => '/openapi.json',
        'rutas_rest' => [
            'GET /api/v1/ordenes' => 'Listar órdenes',
            'POST /api/v1/ordenes' => 'Crear orden',
            'GET /api/v1/ordenes/{id}' => 'Detalle de orden',
            'DELETE /api/v1/ordenes/{id}' => 'Cancelar orden'
        ],
        'servicio_soap' => [
            'wsdl' => '/soap/ordenes.wsdl',
            'endpoint' => '/soap/ordenes'
        ]
    ];
    $response->getBody()->write(json_encode($data, JSON_PRETTY_PRINT | JSON_UNESCAPED_SLASHES));
    return $response->withHeader('Content-Type', 'application/json');
});

// Grupo de API con middleware de autenticación
$app->group('/api/v1/ordenes', function ($group) {
    $group->get('', function (Request $request, Response $response) {
        $service = new OrderService();
        $orders = $service->getAllOrders();
        $response->getBody()->write(json_encode($orders));
        return $response->withHeader('Content-Type', 'application/json');
    });

    $group->post('', function (Request $request, Response $response) {
        $data = (array)$request->getParsedBody();
        if (empty($data['cliente_id']) || empty($data['items'])) {
            $err = [
                'error' => 'Bad Request',
                'message' => 'Los campos cliente_id e items son obligatorios.',
                'statusCode' => 400
            ];
            $response->getBody()->write(json_encode($err));
            return $response->withHeader('Content-Type', 'application/json')->withStatus(400);
        }

        $service = new OrderService();
        try {
            $created = $service->createOrder($data);
            $response->getBody()->write(json_encode($created));
            return $response->withHeader('Content-Type', 'application/json')->withStatus(201);
        } catch (Exception $e) {
            $err = ['error' => 'Internal Error', 'message' => $e->getMessage(), 'statusCode' => 500];
            $response->getBody()->write(json_encode($err));
            return $response->withHeader('Content-Type', 'application/json')->withStatus(500);
        }
    });

    $group->get('/{id}', function (Request $request, Response $response, array $args) {
        $service = new OrderService();
        $order = $service->getOrderById($args['id']);
        if (!$order) {
            $err = ['error' => 'Not Found', 'message' => "Orden {$args['id']} no encontrada.", 'statusCode' => 404];
            $response->getBody()->write(json_encode($err));
            return $response->withHeader('Content-Type', 'application/json')->withStatus(404);
        }
        $response->getBody()->write(json_encode($order));
        return $response->withHeader('Content-Type', 'application/json');
    });

    $group->delete('/{id}', function (Request $request, Response $response, array $args) {
        $service = new OrderService();
        $cancelled = $service->cancelOrder($args['id']);
        if (!$cancelled) {
            $err = ['error' => 'Not Found', 'message' => "Orden {$args['id']} no encontrada.", 'statusCode' => 404];
            $response->getBody()->write(json_encode($err));
            return $response->withHeader('Content-Type', 'application/json')->withStatus(404);
        }
        $resp = ['message' => "Orden {$args['id']} cancelada correctamente.", 'statusCode' => 200];
        $response->getBody()->write(json_encode($resp));
        return $response->withHeader('Content-Type', 'application/json');
    });
})->add($authMiddleware);

$app->run();
