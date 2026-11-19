# Plug-Bee

Plugin avanzado para MusicBee 3.7, con una interfaz de escritorio inspirada en la experiencia de Spotify y una identidad visual propia.

## Objetivo

Ampliar MusicBee sin reemplazar su motor de reproducción. El plugin se comunica con MusicBee mediante la API oficial y presenta una ventana independiente para la experiencia mejorada.

## Funcionalidades planeadas

- Panel de reproducción y controles.
- Biblioteca musical con búsqueda y filtros.
- Visualizador de audio.
- Letras sincronizadas.
- Herramientas de ecualización.
- Estadísticas locales de escucha.
- Recomendaciones basadas en la biblioteca local.
- Edición de portadas y metadatos.

## Estado actual

**Fase 0 — Base del proyecto.** Se está preparando la estructura del plugin, la conexión con MusicBee y la primera ventana de reproducción. Las funciones que requieren acceso especial al flujo de audio se validarán por separado antes de prometer compatibilidad.

## Requisitos de desarrollo

- Windows 10/11
- Visual Studio 2022 o posterior
- .NET Framework 4.8 Developer Pack
- MusicBee 3.7 o posterior

## Compilación

Abre `PlugBee.sln` en Visual Studio, selecciona `Release | x86` y compila el proyecto `PlugBee.Plugin`.

El ensamblado debe llamarse `MB_PlugBee.dll` para que MusicBee lo detecte como plugin.

## Instalación

1. Compila el proyecto.
2. Empaqueta `MB_PlugBee.dll` en un ZIP.
3. En MusicBee abre **Edit → Preferences → Plugins → Add Plugin**.
4. Selecciona el ZIP y reinicia MusicBee si lo solicita.
5. Abre las preferencias del plugin y pulsa **Configure** para abrir Plug-Bee.

## API

El proyecto incluye `MusicBeeInterface.cs`, basado en la interfaz C# publicada por MusicBee. Consulta la [documentación oficial de la API](https://getmusicbee.com/help/api/).

## Licencia

La licencia del proyecto se definirá antes de publicar una versión distribuible. La interfaz de MusicBee se conserva separada y con su atribución correspondiente.
