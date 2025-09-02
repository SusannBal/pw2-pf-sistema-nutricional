# 🏥 Sistema de Gestión Nutricional - Backend


## 📋 Descripción del Proyecto
Sistema backend desarrollado en ASP.NET Core 8.0 para la gestión integral de pacientes, nutricionistas, consultas médicas y planes nutricionales.

## 🚀 Tecnologías Utilizadas

### Framework y Plataforma
- **ASP.NET Core 8.0.0**
- **Entity Framework Core 8.0**
- **SQL Server** (LocalDB o Express)
- **Swagger/OpenAPI** para documentación

### Herramientas de Desarrollo
- **Entity Framework Tools**
- **Microsoft.EntityFrameworkCore.Design**
- **Microsoft.EntityFrameworkCore.SqlServer**
- **Microsoft.EntityFrameworkCore.Tools**

## 🏗️ Arquitectura del Proyecto

### Estructura de Solución
BackendSolution/
├── Backend/ (API Principal)
│ ├── Controllers/ (Controladores API)
│ ├── Data/ (Contexto de Base de Datos)
│ ├── Models/ (Modelos de Entidades)
│ ├── Migrations/ (Migraciones de EF Core)
│ ├── appsettings.json (Configuración)
│ └── Program.cs (Configuración principal)
│
└── FrontEnd/ (Aplicación Web)
├── Pages/ (Razor Pages)
├── wwwroot/ (Archivos estáticos)
└── appsettings.json


## 📊 Modelos de Datos Principales

### Entidades Core
- **Persona** - Datos personales base
- **Paciente** - Información específica de pacientes
- **Nutricionista** - Profesionales de la nutrición
- **Consulta** - Citas y consultas médicas

### Entidades de Gestión
- **HistorialMedico** - Antecedentes médicos
- **HistorialPaciente** - Evolución de peso/IMC
- **PlanNutricional** - Planes alimenticios
- **Comida** - Comidas dentro de planes
- **ActividadFisica** - Tipos de actividad física
- **RegistroActividad** - Registros de actividad
- **Recordatorio** - Recordatorios para usuarios

## ⚙️ Configuración del Proyecto

### Requisitos Previos
- .NET 8.0 SDK
- SQL Server (LocalDB, Express o Full)
- Visual Studio 2022 o VS Code

### Configuración de Base de Datos

#### appsettings.json
```json
{
  "ConnectionStrings": {
    "BackendContext": "Server=SUSANN\\SQLEXPRESS;Database=PW2Backend;Trusted_Connection=True;TrustServerCertificate=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}

## Migraciones de Entity Framework
## Comandos esenciales

# Crear nueva migración
Add-Migration NombreMigracion -Project Backend

# Aplicar migraciones a la base de datos
Update-Database -Project Backend

# Revertir última migración
Remove-Migration -Project Backend