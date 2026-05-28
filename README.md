# Proyecto Metodos Numericos

Pagina educativa MVC en .NET para el proyecto final de Metodos Numericos 2026.

## Ejecutar localmente

```powershell
dotnet run --project ProFinalM.Web/ProFinalM.Web.csproj --urls http://localhost:5108
```

## Deploy en Render

El repositorio incluye `Dockerfile` y `render.yaml`. En Render se puede crear un nuevo Web Service desde GitHub y seleccionar este repositorio. Render construira la imagen Docker y publicara la aplicacion usando la variable `PORT` del entorno.
