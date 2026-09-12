<?php
namespace App;

use PDO;
use Exception;

class OrderService {
    private PDO $db;

    public function __construct() {
        $this->db = Database::getConnection();
    }

    public function getAllOrders(): array {
        $stmt = $this->db->query("SELECT * FROM orders ORDER BY fecha_creacion DESC");
        $orders = $stmt->fetchAll();

        foreach ($orders as &$order) {
            $order['pagado'] = (bool)$order['pagado'];
            $order['items'] = $this->getOrderItems($order['id']);
        }
        return $orders;
    }

    public function getOrderById(string $id): ?array {
        $stmt = $this->db->prepare("SELECT * FROM orders WHERE id = :id");
        $stmt->execute([':id' => $id]);
        $order = $stmt->fetch();

        if (!$order) {
            return null;
        }

        $order['pagado'] = (bool)$order['pagado'];
        $order['items'] = $this->getOrderItems($id);
        return $order;
    }

    public function getOrderItems(string $orderId): array {
        $stmt = $this->db->prepare("SELECT sku, nombre, cantidad, precio_unitario, subtotal FROM order_items WHERE order_id = :order_id");
        $stmt->execute([':order_id' => $orderId]);
        return $stmt->fetchAll();
    }

    public function createOrder(array $data): array {
        $orderId = 'ORD-2026-' . strtoupper(substr(bin2hex(random_bytes(3)), 0, 6));
        $ahora = date('c');

        $subtotal = 0.0;
        $items = $data['items'] ?? [];
        foreach ($items as $item) {
            $subtotal += floatval($item['precio_unitario']) * intval($item['cantidad']);
        }
        $impuestos = round($subtotal * 0.16, 2);
        $total = round($subtotal + $impuestos, 2);

        $this->db->beginTransaction();
        try {
            $stmt = $this->db->prepare("
                INSERT INTO orders (id, cliente_id, cliente_nombre, cliente_email, direccion_envio, subtotal, impuestos, total, estado, pagado, metodo_pago, fecha_creacion, fecha_actualizacion)
                VALUES (:id, :cliente_id, :cliente_nombre, :cliente_email, :direccion_envio, :subtotal, :impuestos, :total, :estado, :pagado, :metodo_pago, :fecha_creacion, :fecha_actualizacion)
            ");
            $stmt->execute([
                ':id' => $orderId,
                ':cliente_id' => $data['cliente_id'],
                ':cliente_nombre' => $data['cliente_nombre'],
                ':cliente_email' => $data['cliente_email'],
                ':direccion_envio' => $data['direccion_envio'],
                ':subtotal' => $subtotal,
                ':impuestos' => $impuestos,
                ':total' => $total,
                ':estado' => 'PENDIENTE',
                ':pagado' => 0,
                ':metodo_pago' => $data['metodo_pago'] ?? 'TARJETA',
                ':fecha_creacion' => $ahora,
                ':fecha_actualizacion' => $ahora
            ]);

            $itemStmt = $this->db->prepare("
                INSERT INTO order_items (order_id, sku, nombre, cantidad, precio_unitario, subtotal)
                VALUES (:order_id, :sku, :nombre, :cantidad, :precio_unitario, :subtotal)
            ");
            foreach ($items as $item) {
                $cant = intval($item['cantidad']);
                $pu = floatval($item['precio_unitario']);
                $itemStmt->execute([
                    ':order_id' => $orderId,
                    ':sku' => $item['sku'],
                    ':nombre' => $item['nombre'],
                    ':cantidad' => $cant,
                    ':precio_unitario' => $pu,
                    ':subtotal' => round($cant * $pu, 2)
                ]);
            }
            $this->db->commit();
        } catch (Exception $e) {
            $this->db->rollBack();
            throw $e;
        }

        return $this->getOrderById($orderId);
    }

    public function cancelOrder(string $id): bool {
        $order = $this->getOrderById($id);
        if (!$order) {
            return false;
        }

        $stmt = $this->db->prepare("UPDATE orders SET estado = 'CANCELADA', fecha_actualizacion = :act WHERE id = :id");
        $stmt->execute([
            ':act' => date('c'),
            ':id' => $id
        ]);
        return true;
    }
}
