# Documento de Instalación - Aplicación TransitAR

## Tabla de Contenidos
1. [Requisitos Previos](#1-requisitos-previos)
2. [Instalación de SQL Server](#2-instalación-de-sql-server)
3. [Instalación de .NET](#3-instalación-de-net)
4. [Clonación del Repositorio](#4-clonación-del-repositorio)
5. [Configuración de las Claves](#5-configuración-de-las-claves)
6. [Configuración de la Base de Datos](#6-configuración-de-la-base-de-datos)
7. [Ejecución del Backend y del Frontend](#7-ejecución-del-backend-y-del-frontend)

Cada paso se puede hacer de dos formas: **con Visual Studio** o **con la terminal**. Elegí una y seguila hasta el final.

## 1. Requisitos Previos
- Windows 10 u 11
- Acceso a internet
- Permisos de administrador
- **Con Visual Studio:** Visual Studio 2022 con la carga de trabajo **Desarrollo de ASP.NET y web**
- **Con la terminal:** Git


## 2. Instalación de SQL Server


### 2.1 Instalación
Descargá e instalá **SQL Server Developer** desde la [página oficial de Microsoft](https://www.microsoft.com/es-es/sql-server/sql-server-downloads), con la instalación básica.
Este paso es igual para los dos caminos.

> **Nota:** si instalás **SQL Server Express**, el servidor se llama `localhost\SQLEXPRESS` y no `localhost`. En ese caso, cambiá la cadena de conexión en el paso 6.1.

### 2.2 Verificar Instalación
```bash
# Verificar que el servicio de SQL Server esté corriendo
sc query MSSQLSERVER
```

> **Nota:** si no está activo, abrí **Servicios** de Windows y arrancá "SQL Server (MSSQLSERVER)" o reinicialo.


## 3. Instalación de .NET

**Con Visual Studio:** no hace falta hacer nada. El SDK de .NET 9 viene con la carga de trabajo **Desarrollo de ASP.NET y web**.

**Con la terminal:** descargá e instalá el **SDK de .NET 9** desde la [página oficial](https://dotnet.microsoft.com/download/dotnet/9.0), y después la herramienta de Entity Framework Core:
```bash
# Verificar la versión de .NET (debería mostrar "9.x.x")
dotnet --version

# Instalar la herramienta que crea la base de datos
dotnet tool install --global dotnet-ef

# Confiar en el certificado HTTPS de desarrollo
dotnet dev-certs https --trust
```

## 4. Clonación del Repositorio

**Con Visual Studio:** en la ventana de inicio elegí **Clonar un repositorio**, pegá la URL `https://github.com/ArielTommarello/transitar.git` y hacé clic en **Clonar**. Se abre la solución `TransitAR.sln`.

**Con la terminal:**
```bash
# Clonar repositorio desde GitHub
git clone https://github.com/ArielTommarello/transitar.git transitar

# Navegar al directorio del proyecto
cd transitar
```


> **Importante:** con la terminal, todos los comandos que siguen se corren desde esta carpeta, donde está `TransitAR.sln`.


## 5. Configuración de las Claves

Las claves no se guardan en el código, en desarrollo van en los **user-secrets** de .NET, que quedan en tu máquina y no se suben al repositorio.
Hacen falta dos:
- `Jwt:Key`: la clave para firmar los tokens de sesión, de al menos 32 caracteres.
- `AdminInicial:Clave`: la contraseña del usuario administrador.

**Con Visual Studio:** en el Explorador de soluciones, hacé clic derecho en `TransitAR.Api` → **Administrar secretos de usuario**. Se abre el archivo `secrets.json`. Reemplazá su contenido por esto y guardá:
```json
{
  "Jwt:Key": "una-clave-larga-de-al-menos-32-caracteres",
  "AdminInicial:Clave": "la-clave-que-quieras-para-el-admin"
}
```

**Con la terminal:**
```bash
dotnet user-secrets set "Jwt:Key" "una-clave-larga-de-al-menos-32-caracteres" --project TransitAR.Api
dotnet user-secrets set "AdminInicial:Clave" "la-clave-que-quieras-para-el-admin" --project TransitAR.Api
```


## 6. Configuración de la Base de Datos

### 6.1 Cadena de conexión
La conexión está en `TransitAR.Api/appsettings.json`:
```json
"ConnectionStrings": {
  "TransitAR": "Server=localhost;Database=TransitARDev;Integrated Security=True;Encrypt=False;"
}
```

> **Nota:** con SQL Server Express, cambiá `Server=localhost` por `Server=localhost\\SQLEXPRESS`, como dijimos arriba.


### 6.2 Crear la base de datos
La base no viene en el repositorio, se crea a partir de las migraciones de Entity Framework Core.

**Con Visual Studio:** abrí **Herramientas → Administrador de paquetes NuGet → Consola del Administrador de paquetes**. Arriba, en **Proyecto predeterminado**, elegí `TransitAR.Structures` y ejecutá:
```powershell
Update-Database -StartupProject TransitAR.Api
```

**Con la terminal:**
```bash
dotnet ef database update --project TransitAR.Structures --startup-project TransitAR.Api
```

Este paso crea la base `TransitARDev` con todas sus tablas y carga:
- las especies y condiciones;
- el usuario administrador, con la contraseña del paso 5;
- los datos de demo: 3 refugios, 5 postulantes y 12 mascotas con fotos, publicaciones, postulaciones, tenencias, controles y mensajes.

> **Nota:** los datos de demo se cargan solo en el entorno `Development`, que es el que se usa en desarrollo. 
En `Production` la base arranca únicamente con el administrador.


## 7. Ejecución del Backend y del Frontend

**Con Visual Studio:** hacé clic derecho en la solución → **Configurar proyectos de inicio** → **Varios proyectos de inicio**. Marcá `TransitAR.Api` y `TransitAR.Web` como **Iniciar**, aceptá y ejecutá con **F5**.

> **Nota:** la primera vez, Visual Studio pregunta si confiás en el certificado SSL de desarrollo. Respondé **Sí**.

**Con la terminal:** en dos terminales distintas:
```bash
# Terminal 1: ejecutar la API
dotnet run --project TransitAR.Api --launch-profile https
```
```bash
# Terminal 2: ejecutar la Web
dotnet run --project TransitAR.Web --launch-profile https
```

> **Nota:** el backend se ejecuta en `https://localhost:7026` y el frontend en `https://localhost:7187`. Mantené las dos terminales abiertas.


## Verificación Final

1. **Backend**: abrí `https://localhost:7026/swagger` en tu navegador.

2. **Frontend**: abrí `https://localhost:7187` en tu navegador. Tendrían que aparecer las publicaciones de los datos de demo que comentamos antes.

3. **Base de datos**: en Visual Studio, abrí **Ver → Explorador de objetos de SQL Server**, conectate a `localhost` y fijate que exista la base `TransitARDev` con la tabla `Usuarios`. 

O desde la terminal:
   ```bash
   sqlcmd -S localhost -d TransitARDev -E -C -Q "SELECT COUNT(*) FROM Usuarios;"
   ```
Debería mostrar `9`.


## Credenciales de Acceso

- **URL de la aplicación**: `https://localhost:7187`
- **Usuario administrador**: `admin@transitar.org`
- **Contraseña**: la que pusiste en el paso 5

Usuarios de demo (datos inventados, solo para probar):

| Rol | Email | Contraseña |
|---|---|---|
| Refugio — Patitas al Rescate | `contacto@patitasalrescate.org` | `patitas2026` |
| Refugio — Refugio del Sur | `info@refugiodelsur.org` | `refugiosur2026` |
| Refugio — Huellitas Quilmes | `hola@huellitasquilmes.org` | `huellitas2026` |
| Postulante | `ana.perez@gmail.com` | `postulante2026` |
| Postulante | `sofia.ramirez@gmail.com` | `sofia2026` |
| Postulante | `carlos.diaz@gmail.com` | `carlos2026` |
| Postulante | `lucia.fernandez@gmail.com` | `lucia2026` |
| Postulante | `diego.morales@gmail.com` | `diego2026` |

Las fotos de animales son de [Pexels](https://www.pexels.com/license/) y [Unsplash](https://unsplash.com/license). Los logos de los refugios, los íconos de los postulantes y la foto del patio se generaron con IA.


## Solución de Problemas Comunes

Errores que pueden aparecer al instalarlo, algunos de los cuales surgieron durante el desarrollo.

### No conecta con SQL Server
Fijate en **Servicios** de Windows que "SQL Server (MSSQLSERVER)" esté en ejecución. Si usás SQL Server Express, revisá la cadena de conexión del paso 6.1.

### La API no arranca: "Falta la configuracion Jwt:Key"
Falta el secreto del paso 5. En Visual Studio, abrí de nuevo **Administrar secretos de usuario** y revisá el archivo. O desde la terminal:
```bash
dotnet user-secrets list --project TransitAR.Api
```

### No se creó el usuario administrador
Al crear la base aparece el aviso "AdminInicial: falta Email o Clave". Cargá la contraseña del paso 5 y volvé a hacer el paso 6.2.


### "Update-Database" da error por el proyecto de inicio
Revisá que en **Proyecto predeterminado** esté elegido `TransitAR.Structures` y que el comando tenga `-StartupProject TransitAR.Api`

### "dotnet ef" no se reconoce (terminal)
Cerrá y volvé a abrir la terminal después de instalar la herramienta. Si sigue sin andar:
```bash
dotnet tool update --global dotnet-ef
```

### El navegador avisa que el sitio no es seguro
Falta confiar en el certificado de desarrollo. En Visual Studio, aceptalo cuando lo pregunte al ejecutar. Desde la terminal:
```bash
dotnet dev-certs https --trust
```

### Problemas con puertos ocupados
```bash
netstat -ano | findstr :7026
netstat -ano | findstr :7187
```

### Volver a los datos de demo originales
Borra la base completa (pide confirmación) y la vuelve a crear con los datos de demo.

**Con Visual Studio**, en la Consola del Administrador de paquetes, con `TransitAR.Structures` como proyecto predeterminado:
```powershell
Drop-Database -StartupProject TransitAR.Api
Update-Database -StartupProject TransitAR.Api
```

**Con la terminal:**
```bash
dotnet ef database drop --project TransitAR.Structures --startup-project TransitAR.Api
dotnet ef database update --project TransitAR.Structures --startup-project TransitAR.Api
```

## Soporte
Para problemas adicionales, revisá la salida de la consola de la API y de la Web, o consultá la documentación del proyecto.