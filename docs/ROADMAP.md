# Hoja de ruta

## Etapa 1 — Conexión y reproducción

- [x] Incorporar la interfaz oficial de la API de MusicBee.
- [x] Registrar el plugin y sus notificaciones.
- [x] Abrir una ventana WPF desde Configure.
- [x] Consultar título, artista y álbum actuales.
- [x] Controles de anterior, reproducir/pausar y siguiente.
- [ ] Compilar y probar dentro de MusicBee 3.7.9796 Store.

## Etapa 2 — Biblioteca

- [ ] Consultar canciones de la biblioteca mediante la API.
- [ ] Búsqueda, ordenamiento y filtros.
- [ ] Vistas de álbumes, artistas y listas.
- [ ] Portadas desde la API de MusicBee.

## Etapa 3 — Experiencia de escucha

- [ ] Letras y sincronización LRC.
- [ ] Visualizador con datos de espectro/sonido expuestos por la API.
- [ ] Estadísticas locales con SQLite.

## Etapa 4 — Herramientas

- [ ] Panel de ecualizador y perfiles.
- [ ] Edición de metadatos y portadas.
- [ ] Recomendaciones locales basadas en historial y biblioteca.

## Restricciones técnicas conocidas

- MusicBee ejecuta plugins en proceso de 32 bits; el plugin se compila como x86.
- La API oficial requiere compatibilidad con .NET Framework; el proyecto usa .NET Framework 4.8.
- La API expone algunas funciones de audio (por ejemplo, espectro y nivel pico), pero el acceso y la frecuencia disponibles se deben validar en ejecución.
- El plugin no reemplaza el motor de audio ni la interfaz interna de MusicBee; Plug-Bee presenta una ventana propia y controla MusicBee mediante su API.
