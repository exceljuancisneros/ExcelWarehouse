# ZPL Printer for Zebra ZQ530

## Overview
This skill covers ZPL generation and transmission for Zebra ZQ530 Bluetooth printers from .NET MAUI Android apps.

## ZPL Generation Rules

### Encoding
- **Use default ASCII encoding** — never use `^CI28` (UTF-16BE) as it breaks all ZPL command parsing
- All ZPL commands are sent via `Encoding.ASCII.GetBytes(zpl)` from C#
- Backslash in ZPL: `^FH\` (single backslash) — in C# strings use a variable: `string fh = "^FH\\";` (produces single `\`)

### Command Structure
Every data field line follows: `^FTx,y^Afont,size,rotation^FH\^FD[value]^FS`
- `^FH\` enables escape sequence handling (hex values)
- No `^CI` directive needed
- Arrow: use `^GFA` bitmap graphics instead of font characters

### Arrow Rendering
**ASCII arrows (`u`/`v`) do not work in Block font (`^A0B`).** Use `^GFA` bitmap instead:
```
^FO240,32^GFA,630,630,9,,[hex_bitmap]^FS
```
- Parameters: `^GFA,total_bytes,total_bytes,bytes_per_row,,hex_data^FS`
- For 70×70 arrow: total_bytes = 630, bytes_per_row = 9
- Arrow determined by level value: level="1" = down arrow, else = up arrow

### Layout (4×6 inch label, 203 DPI)
- Width: ^PW812 (4 inches × 203 DPI = 812 dots)
- Length: ^LL1218 (6 inches × 203 DPI = 1218 dots)
- Day labels left side, location labels right side
- Product info at bottom right
- QR code at bottom center

## Bluetooth Transmission
- Use RFCOMM socket with SPP UUID: `00001101-0000-1000-8000-00805F9B34FB`
- 5-second connection timeout recommended
- Send raw ZPL bytes, no additional formatting
- Close socket after each copy
- Handle bonding verification before connecting

## Common Pitfalls
1. **Double backslash** — C# `"^FH\\\\"` produces `^FH\\` (two backslashes), printer expects `^FH\` (single)
2. **`^CI28`** — UTF-16BE encoding breaks all command parsing; working ZPL has no `^CI` directive
3. **`Encoding.BigEndianUnicode`** — only use when `^CI28` is active; default is ASCII
4. **Arrow characters** — `u`/`v`/`<`/`>` in Block font produce incorrect output; use `^GFA` bitmap
