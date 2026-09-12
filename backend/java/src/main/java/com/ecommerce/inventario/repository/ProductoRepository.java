package com.ecommerce.inventario.repository;

import com.ecommerce.inventario.model.Producto;
import org.springframework.stereotype.Repository;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.*;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.atomic.AtomicLong;
import java.util.stream.Collectors;

@Repository
public class ProductoRepository {

    private final Map<Long, Producto> productos = new ConcurrentHashMap<>();
    private final AtomicLong idGenerator = new AtomicLong(1);

    public ProductoRepository() {
        // Inicializar datos semilla para pruebas
        guardar(new Producto(null, "LAP-001", "Laptop Dell XPS 15", "Intel i7, 16GB RAM, 512GB SSD", new BigDecimal("1899.99"), 15, "Computadoras"));
        guardar(new Producto(null, "MOU-002", "Mouse Inalámbrico Logitech MX Master 3S", "Sensor óptico 8000 DPI, silencioso", new BigDecimal("99.99"), 45, "Periféricos"));
        guardar(new Producto(null, "KEY-003", "Teclado Mecánico Keychron K2", "Switches Gateron Brown, inalámbrico Bluetooth", new BigDecimal("129.50"), 30, "Periféricos"));
        guardar(new Producto(null, "MON-004", "Monitor Dell UltraSharp 27 4K", "IPS, USB-C 90W Hub, 3840x2160", new BigDecimal("549.00"), 8, "Monitores"));
        guardar(new Producto(null, "AUD-005", "Auriculares Sony WH-1000XM5", "Cancelación de ruido activa, Bluetooth 5.2", new BigDecimal("349.99"), 22, "Audio"));
    }

    public List<Producto> listarTodos(String categoria, String busqueda) {
        return productos.values().stream()
                .filter(p -> Boolean.TRUE.equals(p.getActivo()))
                .filter(p -> categoria == null || categoria.isBlank() || p.getCategoria().equalsIgnoreCase(categoria))
                .filter(p -> busqueda == null || busqueda.isBlank() ||
                        p.getNombre().toLowerCase().contains(busqueda.toLowerCase()) ||
                        p.getSku().toLowerCase().contains(busqueda.toLowerCase()))
                .sorted(Comparator.comparing(Producto::getId))
                .collect(Collectors.toList());
    }

    public Optional<Producto> buscarPorId(Long id) {
        Producto p = productos.get(id);
        return (p != null && Boolean.TRUE.equals(p.getActivo())) ? Optional.of(p) : Optional.empty();
    }

    public Optional<Producto> buscarPorSku(String sku) {
        if (sku == null) return Optional.empty();
        return productos.values().stream()
                .filter(p -> Boolean.TRUE.equals(p.getActivo()) && p.getSku().equalsIgnoreCase(sku.trim()))
                .findFirst();
    }

    public synchronized Producto guardar(Producto producto) {
        if (producto.getId() == null) {
            producto.setId(idGenerator.getAndIncrement());
            producto.setFechaCreacion(LocalDateTime.now());
        }
        producto.setFechaActualizacion(LocalDateTime.now());
        productos.put(producto.getId(), producto);
        return producto;
    }

    public synchronized boolean eliminar(Long id) {
        Producto p = productos.get(id);
        if (p != null) {
            p.setActivo(false);
            p.setFechaActualizacion(LocalDateTime.now());
            return true;
        }
        return false;
    }

    public synchronized Optional<Producto> modificarStock(String sku, int delta) {
        Optional<Producto> opt = buscarPorSku(sku);
        if (opt.isPresent()) {
            Producto p = opt.get();
            int nuevo = Math.max(0, p.getStock() + delta);
            p.setStock(nuevo);
            p.setFechaActualizacion(LocalDateTime.now());
            return Optional.of(p);
        }
        return Optional.empty();
    }
}
