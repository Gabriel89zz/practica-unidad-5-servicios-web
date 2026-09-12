<?php
namespace App;

use PDO;

class Database {
    private static ?PDO $pdo = null;

    public static function getConnection(): PDO {
        if (self::$pdo === null) {
            $dir = __DIR__ . '/../storage';
            if (!is_dir($dir)) {
                mkdir($dir, 0777, true);
            }
            $dbPath = $dir . '/ordenes.db';
            $isNew = !file_exists($dbPath);

            self::$pdo = new PDO("sqlite:$dbPath");
            self::$pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
            self::$pdo->setAttribute(PDO::ATTR_DEFAULT_FETCH_MODE, PDO::FETCH_ASSOC);

            if ($isNew) {
                self::initSchema(self::$pdo);
                self::seedData(self::$pdo);
            }
        }
        return self::$pdo;
    }

    private static function initSchema(PDO $pdo): void {
        $pdo->exec("
            CREATE TABLE IF NOT EXISTS orders (
                id TEXT PRIMARY KEY,
                cliente_id TEXT NOT NULL,
                cliente_nombre TEXT NOT NULL,
                cliente_email TEXT NOT NULL,
                direccion_envio TEXT NOT NULL,
                subtotal REAL NOT NULL,
                impuestos REAL NOT NULL,
                total REAL NOT NULL,
                estado TEXT NOT NULL, -- PENDIENTE, PAGADA, PROCESANDO, ENVIADA, CANCELADA
                pagado INTEGER NOT NULL DEFAULT 0,
                metodo_pago TEXT NOT NULL,
                fecha_creacion TEXT NOT NULL,
                fecha_actualizacion TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS order_items (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                order_id TEXT NOT NULL,
                sku TEXT NOT NULL,
                nombre TEXT NOT NULL,
                cantidad INTEGER NOT NULL,
                precio_unitario REAL NOT NULL,
                subtotal REAL NOT NULL,
                FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE
            );
        ");
    }

    private static function seedData(PDO $pdo): void {
        $ahora = date('c');
        $pdo->exec("
            INSERT INTO orders (id, cliente_id, cliente_nombre, cliente_email, direccion_envio, subtotal, impuestos, total, estado, pagado, metodo_pago, fecha_creacion, fecha_actualizacion)
            VALUES 
            ('ORD-2026-101', 'CLI-001', 'Laura Gómez Flores', 'laura.gomez@example.com', 'Av. Juárez 450, Puebla', 1899.99, 303.99, 2203.98, 'PAGADA', 1, 'TARJETA_CREDITO', '$ahora', '$ahora'),
            ('ORD-2026-102', 'CLI-002', 'Roberto Morales', 'roberto.m@example.com', 'Calle 60 #200, Mérida', 229.49, 36.71, 266.20, 'PENDIENTE', 0, 'TRANSFERENCIA', '$ahora', '$ahora');

            INSERT INTO order_items (order_id, sku, nombre, cantidad, precio_unitario, subtotal)
            VALUES 
            ('ORD-2026-101', 'LAP-001', 'Laptop Dell XPS 15', 1, 1899.99, 1899.99),
            ('ORD-2026-102', 'MOU-002', 'Mouse Inalámbrico Logitech MX Master 3S', 1, 99.99, 99.99),
            ('ORD-2026-102', 'KEY-003', 'Teclado Mecánico Keychron K2', 1, 129.50, 129.50);
        ");
    }
}
