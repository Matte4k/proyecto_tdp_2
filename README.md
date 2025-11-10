# MiReclamo

**MiReclamo** es una aplicación de escritorio desarrollada en **WPF (.NET)** con patrón **MVVM (Model-View-ViewModel)**, diseñada para la **gestión de reclamos de clientes** en organizaciones que manejan soporte, mantenimiento o atención ciudadana.

Permite a los usuarios registrar, asignar, supervisar y resolver reclamos, manteniendo trazabilidad completa del proceso.

---

## Características principales

-**Roles de usuario**:

- **SuperAdmin**: puede gestionar usuarios, reclamos y asignaciones globales.
- **Supervisor**: administra reclamos y asigna tareas a sus operadores supervisados.
- **Operador**: gestiona y resuelve los reclamos asignados.

- **Gestión de Reclamos**:

  - Alta, edición, cierre y filtrado de reclamos.
  - Asignación dinámica a operadores (según jerarquía y rol).
  - Filtros por tipo, prioridad, estado y búsqueda por DNI del cliente.

- **Base de datos SQL Server** con relaciones normalizadas:

  - Tablas principales: `Usuarios`, `Roles`, `Reclamos`, `Ubicaciones`, `SupervisorOperador`, `Estados`, `Prioridades`.

- **Dashboard interactivo**:

  - Muestra estadísticas de reclamos por estado, tiempos de resolución y desempeño por operador.

- **Control de acceso por roles**:
  - Cada usuario solo accede a las funciones correspondientes a su perfil.

---

## Arquitectura MVVM

El proyecto está estructurado bajo el patrón **MVVM**, garantizando separación de responsabilidades y facilidad de mantenimiento:

```
proyecto_tdp_2/
│
├── MVVM/
│   ├── Model/
│   │   ├── Usuario.cs
│   │   ├── Reclamo.cs
│   │   ├── Ubicacion.cs
│   │   └── Estado.cs
│   │
│   ├── View/
│   │   ├── LoginView.xaml
│   │   ├── DashboardView.xaml
│   │   ├── MisReclamosView.xaml
│   │   ├── DetalleReclamo.xaml
│   │   ├── ClaimView.xaml
│   │   └── AsignarOperadorView.xaml
│   │
│   └── ViewModel/
│       ├── LoginViewModel.cs
│       ├── DashboardViewModel.cs
│       ├── ReclamoViewModel.cs
│       ├── AsignarOperadorViewModel.cs
│       └── MainViewModel.cs
│
├── Services/
│   ├── DatabaseService.cs
│   ├── ReclamoService.cs
│   └── UsuarioService.cs
│
├── App.xaml
└── MainWindow.xaml
```

---

## Requisitos del sistema

- **.NET 8.0** o superior
- **Microsoft SQL Server 2019+**
- **Visual Studio 2022** (recomendado)
- **Windows 10 / 11**

---

## Configuración de la base de datos

1. Crear una base de datos llamada `mireclamo`.
2. Ejecutar el script de creación de tablas y relaciones (`mireclamo.sql`).
3. Verificar la cadena de conexión en `App.config`:

```xml
<connectionStrings>
    <add name="MiReclamoDB"
         connectionString="Data Source=.;Initial Catalog=mireclamo;Integrated Security=True;"
         providerName="System.Data.SqlClient"/>
</connectionStrings>
```

---

## Lógica de negocio

- **SuperAdmin** → puede asignar reclamos a cualquier operador.
- **Supervisor** → solo puede asignar reclamos a los operadores bajo su supervisión (`SupervisorOperador`).
- **Operador** → solo puede ver los reclamos que le fueron asignados.

---

## Instalación y ejecución

1. Clonar el repositorio:
   ```bash
   git clone https://github.com/usuario/MiReclamo.git
   ```
2. Abrir el proyecto en **Visual Studio**.
3. Configurar la cadena de conexión (ver arriba).
4. Compilar y ejecutar (F5).

---

## Mejoras futuras

- Notificaciones por correo al asignar reclamos.
- API REST para integraciones externas.
- Panel web para clientes.
- Integración de un sistema de monetización

---

## Autores

- **Benjamin Delfor Sánchez Morales**
- **Mateo Nicolás Gálvez Díaz Colodrero**
- Colaboradores y testers internos

---

## Licencia

Este proyecto es de uso académico y/o empresarial interno.  
Distribución restringida sin autorización del autor.
