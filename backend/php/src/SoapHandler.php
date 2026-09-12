<?php
namespace App;

use SoapFault;

class SoapHandler {
    private OrderService $orderService;

    public function __construct() {
        $this->orderService = new OrderService();
    }

    public function ValidarEstadoOrden($ordenId): array {
        // Validación de autenticación Basic Auth si se proporciona
        if (isset($_SERVER['PHP_AUTH_USER'])) {
            if ($_SERVER['PHP_AUTH_USER'] !== 'admin' || ($_SERVER['PHP_AUTH_PW'] ?? '') !== 'admin_pass_2026') {
                throw new SoapFault('Client.AuthenticationFailed', 'Credenciales SOAP (Basic Auth) inválidas o no autorizadas.');
            }
        }

        if (is_object($ordenId) && isset($ordenId->ordenId)) {
            $idStr = (string)$ordenId->ordenId;
        } elseif (is_array($ordenId) && isset($ordenId['ordenId'])) {
            $idStr = (string)$ordenId['ordenId'];
        } else {
            $idStr = (string)$ordenId;
        }

        $order = $this->orderService->getOrderById($idStr);
        if (!$order) {
            return [
                'id' => $idStr,
                'clienteId' => 'N/A',
                'clienteNombre' => 'N/A',
                'estado' => 'NO_EXISTE',
                'total' => 0.0,
                'pagado' => false,
                'fechaCreacion' => 'N/A',
                'valido' => false,
                'mensaje' => "La orden con ID '$idStr' no existe en el sistema."
            ];
        }

        return [
            'id' => $order['id'],
            'clienteId' => $order['cliente_id'],
            'clienteNombre' => $order['cliente_nombre'],
            'estado' => $order['estado'],
            'total' => (float)$order['total'],
            'pagado' => (bool)$order['pagado'],
            'fechaCreacion' => $order['fecha_creacion'],
            'valido' => true,
            'mensaje' => "Orden encontrada y validada con estado: " . $order['estado']
        ];
    }
}
