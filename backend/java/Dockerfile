# Stage 1: Build JAR with Maven
FROM maven:3.9.6-eclipse-temurin-21 AS build
WORKDIR /app
COPY pom.xml .
RUN mvn dependency:go-offline -B || true
COPY src ./src
RUN mvn clean package -DskipTests -B

# Stage 2: Minimal Runtime JRE
FROM eclipse-temurin:21-jre-jammy
WORKDIR /app
COPY --from=build /app/target/*.jar app.jar

ENV SERVER_PORT=8081
EXPOSE 8081

ENTRYPOINT ["java", "-jar", "app.jar"]
