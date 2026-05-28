# Proyecto Métodos Numéricos

Página educativa MVC en .NET para el proyecto final de Métodos Numéricos 2026.

## Ejecutar localmente

```powershell
dotnet run --project ProFinalM.Web/ProFinalM.Web.csproj --urls http://localhost:5108
```

## Deploy en Render

El repositorio incluye `Dockerfile` y `render.yaml`. En Render se puede crear un nuevo Web Service desde GitHub y seleccionar este repositorio. Render construirá la imagen Docker y publicará la aplicación usando la variable `PORT` del entorno.
