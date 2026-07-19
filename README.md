# LyXel


**⚡ PREVIEW DISPONIBLE: LyXel v1.5.6** — [📥 Descargar v1.5.6 Preview](https://github.com/sudoNks/Lyxel/releases/tag/v1.5.6) *(pre-release — puede contener errores)*

**🧪 Novedades en v1.5.6:**
- 🖥️ **Pantalla virtual (modo DeX)**: escritorio virtual independiente del teléfono, con su propia resolución, al estilo Samsung DeX
- 🎮 **Mapeador en pantalla virtual**: mapea los controles jugando sobre la pantalla virtual, con mejor precisión de cámara (hasta 2560×1440)
- ⚙️ **Resolución y DPI configurables**: de 720p a 4K y DPI 160 / 240 / 320 o personalizado, como en un emulador
- 🔊 **Audio en la PC**: al usar la pantalla virtual, el sonido del juego se reproduce en la computadora
- ⛶ **Pantalla completa y multitarea**: espejo del Mapeador a pantalla completa, cursor oculto al jugar y uso de otras ventanas del PC sin interferencias

⚠️ *Esta es una pre-release. La versión estable sigue siendo la **v1.4.5**.*

---

---


<div align="center">

GUI para Scrcpy — Control total de tu Android desde PC

[![Release](https://img.shields.io/github/v/release/sudoNks/Lyxel?label=versión&color=6d1a36)](https://github.com/sudoNks/Lyxel/releases/latest)

[![Downloads](https://img.shields.io/github/downloads/sudoNks/Lyxel/total?color=6d1a36&cacheSeconds=3600)](https://github.com/sudoNks/Lyxel/releases)
[![License](https://img.shields.io/github/license/sudoNks/Lyxel?color=6d1a36)](LICENSE)

[🌐 Sitio web](https://sudonks.github.io/Lyxel) • [💬 Discord](https://discord.gg/CU5quVNyun) • [☕ Ko-fi](https://ko-fi.com/nks_array) • [📥 Descargar](https://github.com/sudoNks/Lyxel/releases/latest)

</div>

---

## 🤔 ¿Qué es LyXel?

LyXel es una interfaz gráfica (GUI) para [Scrcpy](https://github.com/Genymobile/scrcpy) que simplifica el control de dispositivos Android desde Windows, sin necesidad de usar comandos ni herramientas externas.

> LyXel NO es un fork de Scrcpy. Es un proyecto independiente que usa Scrcpy como herramienta subyacente. Scrcpy es desarrollado por [Genymobile](https://github.com/Genymobile) bajo licencia Apache 2.0.

Antes conocido como MobiladorSteX.

---

## ✨ ¿Por qué LyXel?

🎮 Pensado para jugadores

Perfiles preconfigurados para gama baja, media y alta. Optimizaciones ADB para Free Fire, Samsung, Xiaomi, Pixel y Huawei con un solo toggle.

🖥️ Sin comandos

Todo lo que normalmente harías en una terminal — cambiar DPI, resolución, modo de entrada, codificador — lo haces desde la interfaz.

⚡ Scrcpy y ADB incluidos

No necesitas instalar nada adicional. LyXel incluye scrcpy 4.0 y ADB 37.0.0 (platform-tools).

🔧 Compatible con 32 y 64 bits

Desde v1.4.0 incluye soporte para sistemas x86 con Modo Compatibilidad integrado.

---

## 🚀 Descarga

| Plataforma | Enlace |

|---|---|

| Windows 10/11 x64 (recomendado) | [⬇️ LyXel_Setup_v1.4.5.exe](https://github.com/sudoNks/Lyxel/releases/download/v1.4.5/LyXel_Setup_v1.4.5.exe) |

| Windows 10 x86 (32 bits) | [⬇️ LyXel_Setup_v1.4.5.exe](https://github.com/sudoNks/Lyxel/releases/download/v1.4.5/LyXel_Setup_v1.4.5.exe) |

Todas las versiones: [Releases](https://github.com/sudoNks/Lyxel/releases)

---

## 🐧 LyXel para Linux

Edición Linux de LyXel: mira y maneja tu Android directamente desde el PC, con interfaz gráfica. Paquete autocontenido — incluye todo lo necesario, no hay que instalar nada aparte. Versionado propio, independiente del de Windows.

**⚠️ Nota:** esta edición no incluye el Mapeador, disponible solo en LyXel para Windows.

| Plataforma | Enlace |
|---|---|
| Linux x64 (64 bits) | [⬇️ lyxel-v1.0.3-linux-x64.tar.gz](https://github.com/sudoNks/Lyxel/releases/download/linux-v1.0.3/lyxel-v1.0.3-linux-x64.tar.gz) |

Release: [linux-v1.0.3](https://github.com/sudoNks/Lyxel/releases/tag/linux-v1.0.3)

### Guía de instalación (Linux)

**1.** Descarga el paquete `lyxel-v1.0.3-linux-x64.tar.gz`

**2.** Extráelo con clic derecho → "Extraer aquí", o en terminal:
```
tar -xzf lyxel-v1.0.3-linux-x64.tar.gz
```

**3.** Entra a la carpeta y ejecuta el instalador:
```
cd lyxel-v1.0.3-linux-x64
bash instalar.sh
```

El instalador hace todo por ti y abre la app al terminar; después la encuentras buscando "LyXel" en tu menú de aplicaciones.

**4.** En tu teléfono, activa la Depuración USB: Ajustes → Acerca del teléfono → toca 7 veces "Número de compilación"; luego en Opciones de desarrollador activa "Depuración USB".

**5.** Conecta el cable, acepta "¿Permitir depuración USB?" en el teléfono (marca "Permitir siempre"), elige un perfil en LyXel y pulsa Iniciar.

---

## 📋 Requisitos

PC

- Windows 10 o Windows 11 (x86 o x64)

- Scrcpy y ADB incluidos — no requieren instalación adicional

Dispositivo Android

- Android 13 o superior (mínimo Android 11)

- Depuración USB habilitada en Opciones de desarrollador

- En Xiaomi: activar "Depuración USB (modo seguridad)" en ajustes adicionales

- Cable USB para la configuración inicial (WiFi opcional después)

---

## 📦 Historial de versiones

### 🆕 Versiones actuales — C# / .NET 8

| Versión | Descripción | Descargar |
|---|---|---|
| v1.4.5 ⭐ | Alineamiento de renderizado: nuevos modos (DirectX 11/12, GPU, Direct3D 9, OpenGL, OpenGL ES 2, Software) según arquitectura, y mejoras de estabilidad | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.4.5) |
| v1.4.4 | scrcpy 4.0, ADB 37.0.0, Keep Active, redimensionado libre, atajos y mejoras de estabilidad | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.4.4) |
| v1.4.3 | Hotfix de persistencia: perfiles y config se guardan en %LocalAppData% para usuarios sin admin | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.4.3) |
| v1.4.2 | Hotfix: ventana de debug permanece abierta al cerrar scrcpy | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.4.2) |
| v1.4.1 | Modo de renderizado, validación ADB, diálogos unificados, perfiles actualizados | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.4.1) |

| v1.4.0 | Soporte x86, sección Controles, Modo Debug, preview de comando | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.4.0) |

| v1.3.0 | Primera versión oficial como LyXel. Módulo de optimización ADB | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.3.0) |

| v1.2.3 | MobiladorSteX MORRIGAN Dreadnought — versión estable de la serie | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.2.3) |

| v1.2.2 | MobiladorSteX MORRIGAN Dreadnought — mejoras sobre v1.2.1 | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.2.2) |

| v1.2.1 | MobiladorSteX MORRIGAN Dreadnought — correcciones sobre v1.2.0 | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.2.1) |

| v1.2.0 | MobiladorSteX MORRIGAN — inicio de la serie | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.2.0) |

| v1.1.3 | MobiladorSteX — versión estable de la serie 1.1.x | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.1.3) |

| v1.1.2 | MobiladorSteX — mejoras sobre v1.1.1 | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.1.2) |

| v1.1.1 | MobiladorSteX — correcciones menores | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.1.1) |

| v1.1.0 | MobiladorSteX — segunda versión pública en .NET | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.1.0) |

| v1.0.0 | MobiladorSteX — primera versión en C#/.NET | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.0.0) |

### 🐍 Versiones históricas — Python (pre-release)

Estas versiones son las raíces del proyecto, desarrolladas en Python antes de la migración a C#/.NET. Se publican como pre-releases solo para preservar el historial.

| Versión | Descripción | Descargar |

|---|---|---|

| v0.3.8-debugfix | ⚠️ Error conocido: muestra consola de Pygame. Se conserva por historial | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v0.3.8-debugfix) |

| v0.3.7-insiderdebug | Versión de depuración interna | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v0.3.7-insiderdebug) |

| v0.3.6-insiderfix | Corrección de bugs del Insider | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v0.3.6-insiderfix) |

| v0.3.5-insider | Versión Insider con mejoras adicionales | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v0.3.5-insider) |

| v0.3.2-insider | Versión Insider estable | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v0.3.2-insider) |

| v0.3.1-insider | Segunda versión Insider | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v0.3.1-insider) |

| v0.3.0-insider | Primera versión Insider | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v0.3.0-insider) |

| v0.2.0-experimental | Versión experimental, base para las versiones Insider | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v0.2.0-experimental) |

| v0.1.0-beta | Primera versión pública del proyecto en Python | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v0.1.0-beta) |

---

## 🛠️ Tecnología

- v1.0.0 – presente: C# / .NET 8, WinForms, Windows 10/11 x86/x64

- v0.1.0-beta – v0.3.8: Python (versiones históricas)

---

## 🚀 Sobre el desarrollo de LyXel

LyXel es desarrollado por sudoNks (@nks_array) en C# y .NET 8, con un enfoque directo en la experiencia del usuario y la integración nativa con scrcpy.

El módulo de optimización Android está potenciado por investigación asistida con IA, una herramienta que acelera la exploración de documentación oficial, configuraciones por fabricante y comandos ADB especializados. Cada optimización es validada en entorno real bajo criterios de estabilidad, seguridad y rendimiento tangible antes de su integración.

Este enfoque combina velocidad de iteración con supervisión humana en cada decisión.

---

## 📄 Créditos

- [Scrcpy](https://github.com/Genymobile/scrcpy) por Genymobile — licencia Apache 2.0

- LyXel es un proyecto independiente creado por [@sudoNks](https://github.com/sudoNks)
