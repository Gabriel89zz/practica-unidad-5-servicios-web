package com.ecommerce.inventario;

import io.swagger.v3.oas.annotations.OpenAPIDefinition;
import io.swagger.v3.oas.annotations.info.Contact;
import io.swagger.v3.oas.annotations.info.Info;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;

@SpringBootApplication
@OpenAPIDefinition(
    info = @Info(
        title = "Microservicio de Catálogo e Inventario (Java)",
        version = "1.0.0",
        description = "Módulo de gestión de catálogo de productos (REST) y consulta de disponibilidad de stock (SOAP). Práctica de Programación en Ambiente Cliente-Servidor.",
        contact = @Contact(name = "Equipo de Arquitectura Distribuida")
    )
)
public class InventarioApplication {

    public static void main(String[] args) {
        SpringApplication.run(InventarioApplication.class, args);
    }
}
