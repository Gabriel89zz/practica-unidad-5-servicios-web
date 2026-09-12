package com.ecommerce.inventario.config;

import jakarta.servlet.*;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import org.springframework.boot.web.servlet.FilterRegistrationBean;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.core.Ordered;
import org.springframework.http.HttpMethod;
import org.springframework.web.servlet.HandlerInterceptor;
import org.springframework.web.servlet.config.annotation.CorsRegistry;
import org.springframework.web.servlet.config.annotation.InterceptorRegistry;
import org.springframework.web.servlet.config.annotation.WebMvcConfigurer;

import java.io.IOException;
import java.io.PrintWriter;
import java.time.Instant;

@Configuration
public class CorsAndSecurityConfig implements WebMvcConfigurer {

    public static final String VALID_BEARER_TOKEN = "Bearer test_token_2026";
    public static final String VALID_API_KEY = "secret_key_cs";

    @Bean
    public FilterRegistrationBean<Filter> globalCorsFilter() {
        FilterRegistrationBean<Filter> bean = new FilterRegistrationBean<>(new Filter() {
            @Override
            public void doFilter(ServletRequest req, ServletResponse res, FilterChain chain)
                    throws IOException, ServletException {
                HttpServletRequest request = (HttpServletRequest) req;
                HttpServletResponse response = (HttpServletResponse) res;

                response.setHeader("Access-Control-Allow-Origin", "*");
                response.setHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                response.setHeader("Access-Control-Allow-Headers", "*");
                response.setHeader("Access-Control-Max-Age", "3600");

                if ("OPTIONS".equalsIgnoreCase(request.getMethod())) {
                    response.setStatus(HttpServletResponse.SC_OK);
                    return;
                }

                chain.doFilter(req, res);
            }
        });
        bean.setOrder(Ordered.HIGHEST_PRECEDENCE);
        return bean;
    }

    @Override
    public void addCorsMappings(CorsRegistry registry) {
        registry.addMapping("/**")
                .allowedOrigins("*")
                .allowedMethods("GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH")
                .allowedHeaders("*")
                .maxAge(3600);
    }

    @Override
    public void addInterceptors(InterceptorRegistry registry) {
        registry.addInterceptor(new RestAuthInterceptor())
                .addPathPatterns("/api/**")
                .excludePathPatterns("/swagger-ui/**", "/v3/api-docs/**", "/swagger-ui.html");
    }

    public static class RestAuthInterceptor implements HandlerInterceptor {
        @Override
        public boolean preHandle(HttpServletRequest request, HttpServletResponse response, Object handler) throws Exception {
            // Permitir preflight OPTIONS para CORS
            if (HttpMethod.OPTIONS.matches(request.getMethod())) {
                return true;
            }

            String authHeader = request.getHeader("Authorization");
            String apiKeyHeader = request.getHeader("X-API-Key");

            boolean authorized = false;
            if (authHeader != null && authHeader.trim().equals(VALID_BEARER_TOKEN)) {
                authorized = true;
            } else if (apiKeyHeader != null && apiKeyHeader.trim().equals(VALID_API_KEY)) {
                authorized = true;
            }

            if (!authorized) {
                response.setStatus(HttpServletResponse.SC_UNAUTHORIZED);
                response.setContentType("application/json;charset=UTF-8");
                try (PrintWriter writer = response.getWriter()) {
                    writer.write(String.format(
                        "{\"error\":\"Unauthorized\",\"message\":\"Token Bearer o API-Key inválida o ausente. Use 'Authorization: Bearer test_token_2026' o 'X-API-Key: secret_key_cs'\",\"statusCode\":401,\"timestamp\":\"%s\"}",
                        Instant.now().toString()
                    ));
                    writer.flush();
                }
                return false;
            }

            return true;
        }
    }
}
