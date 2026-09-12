package com.ecommerce.inventario.controller;

import com.ecommerce.inventario.model.Producto;
import com.ecommerce.inventario.repository.ProductoRepository;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.Parameter;
import io.swagger.v3.oas.annotations.responses.ApiResponse;
import io.swagger.v3.oas.annotations.responses.ApiResponses;
import io.swagger.v3.oas.annotations.security.SecurityRequirement;
import io.swagger.v3.oas.annotations.tags.Tag;
import jakarta.validation.Valid;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

@RestController
@RequestMapping("/api/v1/productos")
@Tag(name = "Productos", description = "Operaciones CRUD REST del Catálogo de Productos")
@SecurityRequirement(name = "BearerAuth")
public class ProductoRestController {

    private final ProductoRepository repository;

    public ProductoRestController(ProductoRepository repository) {
        this.repository = repository;
    }

    @GetMapping
    @Operation(summary = "Listar catálogo de productos", description = "Obtiene todos los productos activos con filtros opcionales de categoría o término de búsqueda.")
    @ApiResponses(value = {
        @ApiResponse(responseCode = "200", description = "Listado obtenido correctamente"),
        @ApiResponse(responseCode = "401", description = "No autorizado (Falta Bearer Token o API-Key)")
    })
    public ResponseEntity<List<Producto>> listar(
            @Parameter(description = "Filtrar por categoría (ej. 'Computadoras', 'Periféricos')")
            @RequestParam(required = false) String categoria,
            @Parameter(description = "Búsqueda textual por nombre o SKU")
            @RequestParam(required = false) String busqueda) {
        return ResponseEntity.ok(repository.listarTodos(categoria, busqueda));
    }

    @GetMapping("/{id}")
    @Operation(summary = "Obtener producto por ID", description = "Recupera los datos detallados de un producto específico.")
    @ApiResponses(value = {
        @ApiResponse(responseCode = "200", description = "Producto encontrado"),
        @ApiResponse(responseCode = "404", description = "Producto no encontrado"),
        @ApiResponse(responseCode = "401", description = "No autorizado")
    })
    public ResponseEntity<?> obtenerPorId(@PathVariable Long id) {
        return repository.buscarPorId(id)
                .<ResponseEntity<?>>map(ResponseEntity::ok)
                .orElseGet(() -> {
                    Map<String, Object> err = new HashMap<>();
                    err.put("error", "Not Found");
                    err.put("message", "Producto con ID " + id + " no existe.");
                    err.put("statusCode", 404);
                    return ResponseEntity.status(HttpStatus.NOT_FOUND).body(err);
                });
    }

    @PostMapping
    @Operation(summary = "Crear nuevo producto", description = "Registra un nuevo producto en el catálogo e inicializa su inventario.")
    @ApiResponses(value = {
        @ApiResponse(responseCode = "201", description = "Producto creado con éxito"),
        @ApiResponse(responseCode = "400", description = "Datos de entrada inválidos"),
        @ApiResponse(responseCode = "401", description = "No autorizado")
    })
    public ResponseEntity<?> crear(@Valid @RequestBody Producto producto) {
        if (repository.buscarPorSku(producto.getSku()).isPresent()) {
            Map<String, Object> err = new HashMap<>();
            err.put("error", "Conflict");
            err.put("message", "Ya existe un producto con el SKU " + producto.getSku());
            err.put("statusCode", 409);
            return ResponseEntity.status(HttpStatus.CONFLICT).body(err);
        }
        Producto guardado = repository.guardar(producto);
        return ResponseEntity.status(HttpStatus.CREATED).body(guardado);
    }

    @PutMapping("/{id}")
    @Operation(summary = "Actualizar producto existente", description = "Actualiza los atributos o stock de un producto.")
    @ApiResponses(value = {
        @ApiResponse(responseCode = "200", description = "Producto actualizado"),
        @ApiResponse(responseCode = "404", description = "Producto no encontrado"),
        @ApiResponse(responseCode = "401", description = "No autorizado")
    })
    public ResponseEntity<?> actualizar(@PathVariable Long id, @Valid @RequestBody Producto datos) {
        return repository.buscarPorId(id)
                .<ResponseEntity<?>>map(existente -> {
                    existente.setNombre(datos.getNombre());
                    existente.setDescripcion(datos.getDescripcion());
                    existente.setPrecio(datos.getPrecio());
                    existente.setStock(datos.getStock());
                    existente.setCategoria(datos.getCategoria());
                    repository.guardar(existente);
                    return ResponseEntity.ok(existente);
                })
                .orElseGet(() -> {
                    Map<String, Object> err = new HashMap<>();
                    err.put("error", "Not Found");
                    err.put("message", "Producto con ID " + id + " no encontrado.");
                    err.put("statusCode", 404);
                    return ResponseEntity.status(HttpStatus.NOT_FOUND).body(err);
                });
    }

    @DeleteMapping("/{id}")
    @Operation(summary = "Eliminar producto", description = "Realiza la baja lógica de un producto del catálogo.")
    @ApiResponses(value = {
        @ApiResponse(responseCode = "200", description = "Producto eliminado exitosamente"),
        @ApiResponse(responseCode = "404", description = "Producto no encontrado"),
        @ApiResponse(responseCode = "401", description = "No autorizado")
    })
    public ResponseEntity<?> eliminar(@PathVariable Long id) {
        boolean eliminado = repository.eliminar(id);
        if (eliminado) {
            Map<String, Object> resp = new HashMap<>();
            resp.put("message", "Producto con ID " + id + " eliminado correctamente.");
            resp.put("statusCode", 200);
            return ResponseEntity.ok(resp);
        } else {
            Map<String, Object> err = new HashMap<>();
            err.put("error", "Not Found");
            err.put("message", "Producto con ID " + id + " no encontrado.");
            err.put("statusCode", 404);
            return ResponseEntity.status(HttpStatus.NOT_FOUND).body(err);
        }
    }
}
