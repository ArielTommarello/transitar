<a name="readme-top"></a>
# PPS UTN – Tecnicatura Universitaria en Programación 2026

<div align="center">
  <h3 align="center">TransitAR</h3>
  <p align="center">
    Plataforma web para la gestión integral de adopciones y tránsitos de animales.
  </p>
</div>

Proyecto desarrollado como Práctica Profesional Supervisada (PPS)
de la Tecnicatura Universitaria en Programación — UTN.

## Sobre el proyecto

TransitAR es una aplicación web que facilita la conexión entre los refugios de animales y las personas que quieren adoptar o dar un hogar de tránsito.
Permite a los refugios publicar sus mascotas, recibir postulaciones, elegir a quién entregarlas y hacer el seguimiento de cada tenencia, además de conversar con el candidato por un chat interno.


## Alcance del sistema

<ul>
  <li>Permitir el registro e inicio de sesión de postulantes y refugios.</li>
  <li>Permitir a los refugios la creación, actualización y borrado de las mascotas, con sus fotos.</li>
  <li>Permitir a los refugios publicar sus mascotas para adopción o para tránsito.</li>
  <li>Permitir a los postulantes armar su perfil y postularse a las publicaciones.</li>
  <li>Permitir a los refugios aceptar o rechazar postulaciones y confirmar la entrega de la mascota.</li>
  <li>Permitir a los refugios hacer el seguimiento de cada tenencia con controles programados, y convertir un tránsito en adopción.</li>
  <li>Permitir a refugios y postulantes comunicarse por un chat dentro de la plataforma, con archivos adjuntos.</li>
  <li>Permitir a los administradores aprobar refugios, gestionar usuarios y mantener los catálogos de especies y condiciones.</li>
</ul>


## Tecnologías

<ul>
  <li>Frontend: ASP.NET Core Razor Pages, Bootstrap.</li>
  <li>Backend: .NET 9, ASP.NET Core Web API (C#).</li>
  <li>Base de datos: SQL Server.</li>
  <li>ORM: Entity Framework Core.</li>
  <li>Autenticación: JWT, BCrypt.</li>
  <li>Testing: xUnit, SQLite en memoria.</li>
</ul>


## Colaboradores

<ul>
  <li>Tommarello, Ariel. - ariel.tommarello@gmail.com</li>
</ul>


## Estructura del proyecto

| Proyecto | Responsabilidad |
|---|---|
| `TransitAR.Api` | API REST: controllers, EF Core, autenticación |
| `TransitAR.Web` | Interfaz de usuario (Razor Pages) |
| `TransitAR.Structures` | Entidades, 'DbContext', Migrations, DTOs y enums compartidos entre Api y Web |
| `TransitAR.Tests` | Pruebas unitarias de los services de la API |

## Project setup

<p>Acceda al <a href="instalacion.md">manual de instalación</a></p>


## Compilar y levantar el proyecto

```bash
# crear la base de datos con los datos de demo
dotnet ef database update --project TransitAR.Structures --startup-project TransitAR.Api

# levantar solo Backend
dotnet run --project TransitAR.Api --launch-profile https

# levantar solo Frontend
dotnet run --project TransitAR.Web --launch-profile https
```

## Run tests

```bash
# unit tests
dotnet test

# test coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Créditos

Basado conceptualmente en [AdoptAR](https://github.com/fedef1982/adoptar),
desarrollado por Federico Fresco y Paola Rodríguez.

## Licencia

MIT

<p align="right">(<a href="#readme-top">back to top</a>)</p>