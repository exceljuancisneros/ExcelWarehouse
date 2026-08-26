#!/usr/bin/env python
"""Regenerate the ZPL ^GFA arrow bitmaps from the source arrow image.

ZPL ^GFA convention (verified against Zebra ZPL II Programming Guide +
multiple converters):
  - one bit per pixel, 8 pixels packed per byte, MSB first, left->right, top->bottom
  - a 1 bit PRINTS black (ink), a 0 bit is white (no ink)
  - ^GFA,totalBytes,totalBytes,bytesPerRow,,hexData
      totalBytes   = bytesPerRow * height
      bytesPerRow  = ceil(width / 8)
The 'A' suffix on ^GFA already means ASCII-hex; the 4th param is left empty.

Outputs:
  - arrow_down_hex.txt  : down-arrow hex (level 1)
  - arrow_up_hex.txt    : up-arrow hex   (level != 1) = vertical flip of down
  - arrow_preview_down.png / arrow_preview_up.png
Prints an ASCII rendering of each so you can eyeball-verify the shape.
"""
from PIL import Image

SRC = r"C:\Users\jc_x1\AppData\Roaming\Hermes\composer-images\composer_2026-08-25_17-50-13-391_1ff648.png"
OUT = r"C:\Users\jc_x1\OneDrive\Documentos\Visual Studio\Projects\source\repos\Android\PrintLabels"
W, H = 60, 100


def to_bits(img):
    """Return a flat list of ZPL bits (1=print black) for a mode-1 image."""
    px = list(img.getdata())
    # px value 0 == black pixel -> print ink (1); 255 == white -> no ink (0)
    return [1 if p == 0 else 0 for p in px]


def pack(bits, w, h):
    """Pack bits (1=black) into bytes MSB-first; return (hex, bytes_per_row)."""
    bpr = (w + 7) // 8
    out = []
    for y in range(h):
        row = bits[y * w:(y + 1) * w]
        for xs in range(0, w, 8):
            chunk = row[xs:xs + 8]
            while len(chunk) < 8:
                chunk.append(0)  # pad with white (no ink)
            byte = 0
            for i, b in enumerate(chunk):
                byte |= (b & 1) << (7 - i)
            out.append(f"{byte:02X}")
    return "".join(out), bpr


def preview(bits, w, h, label):
    print(f"--- {label} ({w}x{h}) ---")
    for y in range(h):
        line = "".join("#" if bits[y * w + x] else " " for x in range(w))
        print(line)
    print()


def main():
    img = Image.open(SRC).convert("L").point(lambda x: 0 if x < 128 else 255, "1")
    img = img.resize((W, H), Image.Resampling.LANCZOS)

    down_bits = to_bits(img)
    down_hex, bpr = pack(down_bits, W, H)
    total = bpr * H
    preview(down_bits, W, H, "DOWN (level 1)")

    # up arrow = vertical flip of the down arrow
    up_bits = list(reversed(down_bits))
    up_hex, _ = pack(up_bits, W, H)
    preview(up_bits, W, H, "UP (level != 1)")

    with open(OUT + r"\arrow_down_hex.txt", "w") as f:
        f.write(down_hex)
    with open(OUT + r"\arrow_up_hex.txt", "w") as f:
        f.write(up_hex)
    img.save(OUT + r"\arrow_preview_down.png")
    Image.frombytes("1", (W, H), bytes((255 if b else 0) for b in down_bits)).save(
        OUT + r"\arrow_preview_up.png")

    print("bytes_per_row =", bpr)
    print("total_bytes   =", total)
    print("down hex len  =", len(down_hex), "(expect", total * 2, ")")
    print("up   hex len  =", len(up_hex), "(expect", total * 2, ")")
    print("GFA header    = ^GFA,%d,%d,%d,," % (total, total, bpr))
    print("DOWN hex:")
    print(down_hex)
    print("UP hex:")
    print(up_hex)


if __name__ == "__main__":
    main()
