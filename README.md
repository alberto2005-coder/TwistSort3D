# Twist & Sort 3D

[Español](#español) · [English](#english)

---

## Español

### Índice

- [Descripción](#descripción)
- [Características](#características)
- [Cómo jugar](#cómo-jugar)
  - [Controles](#controles)
- [Requisitos](#requisitos)
- [Instalación y ejecución](#instalación-y-ejecución)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Tecnologías](#tecnologías)
- [Estado del proyecto](#estado-del-proyecto)
- [Licencia](#licencia)
- [Autor](#autor)

### Descripción

**Twist & Sort 3D** es un juego de puzle y lógica en 3D basado en mecánicas de giro y clasificación espacial (*sorting*), desarrollado de forma independiente con un enfoque interactivo, visual y dinámico.

Desatornilla las piezas de cada estructura, gira las placas para encontrar el ángulo correcto y agrupa tres tornillos del mismo color en la bandeja antes de que se llene.

### Características

- **Puzles 3D por niveles** con dificultad progresiva y generación procedural de tableros.
- **Mecánica principal**: retira tornillos, completa tríos del mismo color y vacía cada estructura.
- **Cinco herramientas**: martillo, intercambio, ampliar bandeja, pista y congelar.
- **Obstáculos variados**: tornillos dobles, atascados, helados, enlazados, candado, placas giratorias, pintura y bombas.
- **8 biomas visuales** que cambian la paleta y la ambientación cada 25 niveles.
- **Reto diario** y modo maratón.
- **Personalización** de skins y monedas para desbloquear contenido.
- **Ajustes de accesibilidad**: reducción de movimiento, símbolos de color, calidad gráfica, música y efectos.
- **Localización completa en español e inglés**.
- **Sin anuncios, sin compras integradas y sin conexión a internet**.

### Cómo jugar

1. **Desatornilla**: toca un tornillo libre para quitarlo de la placa.
2. **Junta tres**: tres tornillos del mismo color en la bandeja desaparecen.
3. **Vacía la estructura**: cuando una placa se queda sin tornillos, cae.
4. **No llenes la bandeja**: si se llena sin formar un trío, pierdes. Usa las herramientas cuando la situación se complique.

#### Controles

| Acción | Ratón / táctil | Teclado |
|---|---|---|
| Quitar un tornillo | Clic / toque sobre el tornillo | — |
| Girar la estructura | Arrastrar sobre el tablero | — |
| Usar herramienta 1-5 | Clic en su botón | `1` … `5` |
| Pausa / volver | Botón de pausa | `Esc` |
| Confirmar | Botón de la interfaz | `Enter` o `Espacio` |

### Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) o superior.
- Windows, Linux o macOS con soporte para OpenGL (MonoGame DesktopGL).
- No se necesitan dependencias externas: los paquetes NuGet se restauran automáticamente.

### Instalación y ejecución

```bash
# 1. Clona el repositorio
git clone https://github.com/alberto2005-coder/TwistSort3D.git
cd TwistSort3D

# 2. Restaura los paquetes NuGet
dotnet restore

# 3. Ejecuta el juego
dotnet run
```

Para generar una compilación de release:

```bash
dotnet publish -c Release -r win-x64 --self-contained false
```

El ejecutable se genera en `bin/Release/net8.0/win-x64/publish/`.

### Estructura del proyecto

```text
TwistSort3D/
├── Assets/          Fuentes y recursos del juego
├── Core/            Configuración, audio, guardado y localización
├── Game/            Lógica del tablero, sesiones y generación de niveles
├── Render/          Renderizado 3D, mallas e interfaz
├── Screens/         Pantallas: menú, juego, ayuda, ajustes y skins
├── Program.cs       Punto de entrada
├── TwistSortGame.cs Ciclo principal y gestión de entrada
└── TwistSort3D.csproj
```

### Tecnologías

- **C# / .NET 8**
- **MonoGame Framework DesktopGL 3.8**
- **FontStashSharp** para texto dinámico
- Arquitectura por pantallas y separación entre lógica, render y persistencia

### Estado del proyecto

El juego se encuentra en desarrollo activo. Las contribuciones, sugerencias y reportes de errores son bienvenidos a través de *Issues* y *Pull Requests*.

### Licencia

Este proyecto se distribuye bajo la **GNU General Public License v3.0 (GPL-3.0)**. Consulta el archivo [LICENSE](LICENSE) para más detalles.

### Autor

Desarrollado por **Alberto Ortiz Sánchez** ([@alberto2005-coder](https://github.com/alberto2005-coder)) bajo el sello **PixelBolt Games**.

---

## English

### Table of contents

- [Overview](#overview)
- [Features](#features)
- [How to play](#how-to-play)
  - [Controls](#controls)
- [Requirements](#requirements)
- [Installation and usage](#installation-and-usage)
- [Project structure](#project-structure)
- [Technologies](#technologies)
- [Project status](#project-status)
- [License](#license)
- [Author](#author)

### Overview

**Twist & Sort 3D** is a 3D puzzle and logic game built around twisting and spatial sorting mechanics, independently developed with an interactive, visual and dynamic approach.

Unscrew the bolts from each structure, rotate the plates to find the right angle, and match three bolts of the same color in the tray before it fills up.

### Features

- **Level-based 3D puzzles** with progressive difficulty and procedurally generated boards.
- **Core mechanic**: remove bolts, complete same-color trios and clear every structure.
- **Five tools**: hammer, swap, expand tray, hint and freeze.
- **Varied obstacles**: double, stuck, iced, linked, locked, swivel, paint and bomb bolts.
- **8 visual biomes** that change the palette and atmosphere every 25 levels.
- **Daily challenge** and marathon mode.
- **Customization** with skins and coins to unlock content.
- **Accessibility settings**: reduced motion, color symbols, graphics quality, music and sound effects.
- **Full Spanish and English localization**.
- **No ads, no in-app purchases and no internet connection required**.

### How to play

1. **Unscrew**: tap a free bolt to remove it from the plate.
2. **Match three**: three bolts of the same color in the tray disappear.
3. **Clear the structure**: when a plate runs out of bolts, it falls.
4. **Don't fill the tray**: if it fills without a match, you lose. Use your tools when things get tough.

#### Controls

| Action | Mouse / touch | Keyboard |
|---|---|---|
| Remove a bolt | Click / tap the bolt | — |
| Rotate the structure | Drag on the board | — |
| Use tool 1-5 | Click its button | `1` … `5` |
| Pause / back | Pause button | `Esc` |
| Confirm | UI button | `Enter` or `Space` |

### Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later.
- Windows, Linux or macOS with OpenGL support (MonoGame DesktopGL).
- No external dependencies: NuGet packages are restored automatically.

### Installation and usage

```bash
# 1. Clone the repository
git clone https://github.com/alberto2005-coder/TwistSort3D.git
cd TwistSort3D

# 2. Restore NuGet packages
dotnet restore

# 3. Run the game
dotnet run
```

To create a release build:

```bash
dotnet publish -c Release -r win-x64 --self-contained false
```

The executable is generated in `bin/Release/net8.0/win-x64/publish/`.

### Project structure

```text
TwistSort3D/
├── Assets/          Game fonts and resources
├── Core/            Configuration, audio, save data and localization
├── Game/            Board logic, sessions and level generation
├── Render/          3D rendering, meshes and UI
├── Screens/         Menu, game, help, settings and skins screens
├── Program.cs       Entry point
├── TwistSortGame.cs Main loop and input handling
└── TwistSort3D.csproj
```

### Technologies

- **C# / .NET 8**
- **MonoGame Framework DesktopGL 3.8**
- **FontStashSharp** for dynamic text rendering
- Screen-based architecture with separated logic, rendering and persistence

### Project status

The game is under active development. Contributions, suggestions and bug reports are welcome through *Issues* and *Pull Requests*.

### License

This project is distributed under the **GNU General Public License v3.0 (GPL-3.0)**. See the [LICENSE](LICENSE) file for details.

### Author

Developed by **Alberto Ortiz Sánchez** ([@alberto2005-coder](https://github.com/alberto2005-coder)) under the **PixelBolt Games** label.
