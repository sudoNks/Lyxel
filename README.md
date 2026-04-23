# LyXel

<div align="center">

GUI para Scrcpy — Control total de tu Android desde PC

[![Release](https://img.shields.io/github/v/release/sudoNks/Lyxel?label=versión&color=6d1a36)](https://github.com/sudoNks/Lyxel/releases/latest)

[![Downloads](https://img.shields.io/github/downloads/sudoNks/Lyxel/total?color=6d1a36)](https://github.com/sudoNks/Lyxel/releases)

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

No necesitas instalar nada adicional. LyXel trae todo lo necesario.

🔧 Compatible con 32 y 64 bits

Desde v1.4.0 incluye soporte para sistemas x86 con Modo Compatibilidad integrado.

---

## 🚀 Descarga

| Plataforma | Enlace |

|---|---|

| Windows 10/11 x64 (recomendado) | [⬇️ LyXel_Setup_v1.4.1.exe](https://github.com/sudoNks/Lyxel/releases/download/v1.4.1/LyXel_Setup_v1.4.1.exe) |

| Windows 10 x86 (32 bits) | [⬇️ LyXel_Setup_v1.4.1.exe](https://github.com/sudoNks/Lyxel/releases/download/v1.4.1/LyXel_Setup_v1.4.1.exe) |

Todas las versiones: [Releases](https://github.com/sudoNks/Lyxel/releases)

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

| v1.4.1 ⭐ | Modo de renderizado, validación ADB, diálogos unificados, perfiles actualizados | [Download](https://github.com/sudoNks/Lyxel/releases/tag/v1.4.1) |

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

## 📄 Créditos

- [Scrcpy](https://github.com/Genymobile/scrcpy) por Genymobile — licencia Apache 2.0

- LyXel es un proyecto independiente creado por [@sudoNks](https://github.com/sudoNks)
