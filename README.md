# LyXel

**Version estable actual: LyXel v1.6.4**: [Descargar v1.6.4](https://github.com/sudoNks/Lyxel/releases/download/v1.6.4/LyXel_Setup_v1.6.4.exe)

**Novedades destacadas en v1.6.4:** ahora puedes personalizar la tecla Mod (elige la tecla de atajos del espejo que prefieras) y mejora la compatibilidad del teclado en el Mapeador. Se mantienen las novedades de la v1.6.0: aceleracion por hardware (el video se procesa en la tarjeta grafica, con hasta 83% menos uso del procesador), Mapeador rediseñado (camara continua y precisa, editor de controles renovado y perfiles importables/exportables) y Pantalla dedicada (juega en una pantalla propia con la resolucion y el DPI que elijas, de 720p a 4K).

---

<div align="center">

GUI para Scrcpy: control total de tu Android desde PC

[![Release](https://img.shields.io/github/v/release/sudoNks/Lyxel?label=version&color=6d1a36)](https://github.com/sudoNks/Lyxel/releases/latest)

[![Downloads](https://img.shields.io/github/downloads/sudoNks/Lyxel/total?color=6d1a36&cacheSeconds=3600)](https://github.com/sudoNks/Lyxel/releases)
[![License](https://img.shields.io/github/license/sudoNks/Lyxel?color=6d1a36)](LICENSE)

[Sitio web](https://sudonks.github.io/Lyxel) - [Discord](https://discord.gg/CU5quVNyun) - [Ko-fi](https://ko-fi.com/nks_array) - [Descargar](https://github.com/sudoNks/Lyxel/releases/latest)

</div>

---

## Que es LyXel?

LyXel es una interfaz grafica (GUI) para [Scrcpy](https://github.com/Genymobile/scrcpy) que simplifica el control de dispositivos Android desde Windows, sin necesidad de usar comandos ni herramientas externas.

Antes conocido como MobiladorSteX.

---

## Por que LyXel?

**Pensado para jugadores.** Perfiles preconfigurados para gama baja, media y alta. Optimizaciones ADB para Free Fire, Samsung, Xiaomi, Pixel y Huawei con un solo toggle.

**Sin comandos.** Todo lo que normalmente harias en una terminal (cambiar DPI, resolucion, modo de entrada, codificador) lo haces desde la interfaz.

**Scrcpy y ADB incluidos.** No necesitas instalar nada adicional. LyXel incluye scrcpy 4.0 y ADB 37.0.0 (platform-tools).

**Compatible con 32 y 64 bits.** Desde v1.4.0 incluye soporte para sistemas x86 con Modo Compatibilidad integrado.


---

## Descarga

| Plataforma | Enlace |
|---|---|
| Windows 10/11 x64 (recomendado) | [LyXel_Setup_v1.6.4.exe](https://github.com/sudoNks/Lyxel/releases/download/v1.6.4/LyXel_Setup_v1.6.4.exe) |
| Windows 10 x86 (32 bits) | [LyXel_Setup_v1.6.4.exe](https://github.com/sudoNks/Lyxel/releases/download/v1.6.4/LyXel_Setup_v1.6.4.exe) |

Todas las versiones: [Releases](https://github.com/sudoNks/Lyxel/releases)

---

## LyXel para Linux

Edicion Linux de LyXel: mira y maneja tu Android directamente desde el PC, con interfaz grafica. Paquete autocontenido: incluye todo lo necesario, no hay que instalar nada aparte. Versionado propio, independiente del de Windows.

**Nota:** esta edicion no incluye el Mapeador, disponible solo en LyXel para Windows.

| Plataforma | Enlace |
|---|---|
| Linux x64 (64 bits) | [lyxel-v1.0.3-linux-x64.tar.gz](https://github.com/sudoNks/Lyxel/releases/download/linux-v1.0.3/lyxel-v1.0.3-linux-x64.tar.gz) |

Release: [linux-v1.0.3](https://github.com/sudoNks/Lyxel/releases/tag/linux-v1.0.3)


### Guia de instalacion (Linux)

**1.** Descarga el paquete lyxel-v1.0.3-linux-x64.tar.gz

**2.** Extraelo con clic derecho "Extraer aqui", o en terminal con: tar -xzf lyxel-v1.0.3-linux-x64.tar.gz

**3.** Entra a la carpeta (cd lyxel-v1.0.3-linux-x64) y ejecuta el instalador con: bash instalar.sh

El instalador hace todo por ti y abre la app al terminar; despues la encuentras buscando "LyXel" en tu menu de aplicaciones.

**4.** En tu telefono, activa la Depuracion USB: Ajustes, Acerca del telefono, toca 7 veces "Numero de compilacion"; luego en Opciones de desarrollador activa "Depuracion USB".

**5.** Conecta el cable, acepta "Permitir depuracion USB?" en el telefono (marca "Permitir siempre"), elige un perfil en LyXel y pulsa Iniciar.

---

## Requisitos

**PC:** Windows 10 o Windows 11 (x86 o x64) o Linux x64. Scrcpy y ADB incluidos, no requieren instalacion adicional.

**Dispositivo Android:** Android 13 o superior (minimo Android 11), con Depuracion USB habilitada en Opciones de desarrollador. En Xiaomi: activar "Depuracion USB (modo seguridad)" en ajustes adicionales. Cable USB para la configuracion inicial (WiFi opcional despues).

---

## Historial de versiones

| Version | Descripcion | Descargar |
|---|---|---|
| v1.6.4 | Tecla Mod personalizable y mejor compatibilidad de teclado en el Mapeador; conserva todo lo de la v1.6.0 | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.6.4) |
| v1.6.0 | Aceleracion por hardware (hasta 83% menos CPU), Mapeador rediseñado y Pantalla dedicada | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.6.0) |
| v1.5.6 | Preview: Pantalla virtual (modo DeX) con resolucion y DPI configurables, Mapeador sobre pantalla virtual, audio en la PC y pantalla completa | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.5.6) |
| v1.5.3 | Preview: Mapeador corregido con scrcpy 4.1, mejor deteccion del dispositivo, modo dual mas estable y contador de FPS renovado | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.5.3) |
| v1.5.1 | Preview: mejoras del Mapeador Beta, mouse sobre la ventana del juego y botones del mouse como tecla de captura | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.5.1) |
| v1.5.0 | Preview: nueva interfaz WPF, Modo Dual experimental, Mapeador Beta y modulo de Optimizacion Android | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.5.0) |
| v1.4.5 | Alineamiento de renderizado: nuevos modos segun arquitectura y mejoras de estabilidad | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.4.5) |
| v1.4.4 | scrcpy 4.0, ADB 37.0.0, Keep Active, redimensionado libre, atajos y mejoras de estabilidad | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.4.4) |
| v1.4.3 | Hotfix de persistencia: perfiles y config se guardan en %LocalAppData% para usuarios sin admin | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.4.3) |
| v1.4.2 | Hotfix: ventana de debug permanece abierta al cerrar scrcpy | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.4.2) |
| v1.4.1 | Modo de renderizado, validacion ADB, dialogos unificados, perfiles actualizados | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.4.1) |
| v1.4.0 | Soporte x86, seccion Controles, Modo Debug, preview de comando | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.4.0) |
| v1.3.0 | Primera version oficial como LyXel. Modulo de optimizacion ADB | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.3.0) |
| v1.2.3 | MobiladorSteX MORRIGAN Dreadnought, version estable de la serie | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.2.3) |
| v1.2.2 | MobiladorSteX MORRIGAN Dreadnought, mejoras sobre v1.2.1 | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.2.2) |
| v1.2.1 | MobiladorSteX MORRIGAN Dreadnought, correcciones sobre v1.2.0 | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.2.1) |
| v1.2.0 | MobiladorSteX MORRIGAN, inicio de la serie | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.2.0) |
| v1.1.3 | MobiladorSteX, version estable de la serie 1.1.x | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.1.3) |
| v1.1.2 | MobiladorSteX, mejoras sobre v1.1.1 | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.1.2) |
| v1.1.1 | MobiladorSteX, correcciones menores | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.1.1) |
| v1.1.0 | MobiladorSteX, segunda version publica | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.1.0) |
| v1.0.0 | MobiladorSteX, primera version | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.0.0) |

---

## Creditos

[Scrcpy](https://github.com/Genymobile/scrcpy) por Genymobile, licencia Apache 2.0. LyXel es un proyecto independiente creado por [@sudoNks](https://github.com/sudoNks).

---

## Licencia

LyXel - Freeware License

Copyright (c) 2026 sudoNks (@nks_array)

LyXel is free to use for personal, non-commercial purposes. Redistribution, modification, or commercial use of this software or any of its components is not permitted without explicit written permission from the author. The source code of this project is proprietary and not publicly available.

Scrcpy is developed by Genymobile and is not part of this license.
