# TransitAR

Plataforma web para la gestión integral de adopciones y tránsitos de animales.

Proyecto desarrollado como Práctica Profesional Supervisada (PPS)
de la Tecnicatura Universitaria en Programación — UTN.

## Descripción


## Tecnologías

- **Backend:** .NET 9 / ASP.NET Core Web API (C#)
- **ORM:** Entity Framework Core
- **Base de datos:** SQL Server
- **Frontend:** ASP.NET Core Razor Pages + Bootstrap
- **Autenticación:** JWT + BCrypt
- **Testing:** xUnit / Moq

## Estructura del proyecto

| Proyecto | Responsabilidad |
|---|---|
| `TransitAR.Api` | API REST: controllers, EF Core, autenticación |
| `TransitAR.Web` | Interfaz de usuario (Razor Pages) |
| `TransitAR.Structures` | DTOs y enums compartidos entre Api y Web |

## Estado del proyecto

En desarrollo — inicio septiembre 2026, entrega estimada noviembre 2026.

## Créditos

Basado conceptualmente en [AdoptAR](https://github.com/fedef1982/adoptar),
desarrollado por Federico Fresco y Paola Rodríguez.

## Licencia

MIT