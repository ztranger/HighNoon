from __future__ import annotations

import json
from pathlib import Path

from PIL import Image


ROOT = Path(__file__).resolve().parents[2]
NAME = "CabaretDancer8"
SOURCE_DIR = ROOT / "ArtSource" / "Generation"
OUT_DIR = ROOT / "ArtSource" / "Characters" / NAME
ANIMATIONS = ("Idle", "Walk", "Shoot", "Death")
DEATH_HEIGHTS = (80, 73, 58, 49, 34, 29, 22, 22)
DEATH_WIDTHS = (32, 46, 41, 49, 60, 65, 66, 66)


def binary_rgba(image: Image.Image) -> Image.Image:
    image = image.convert("RGBA")
    pixels = image.load()
    for y in range(image.height):
        for x in range(image.width):
            r, g, b, a = pixels[x, y]
            pixels[x, y] = (r, g, b, 255) if a >= 128 else (0, 0, 0, 0)
    return image


def normalize_frame(raw: Image.Image, target_h: int, target_w: int | None = None) -> Image.Image:
    raw = binary_rgba(raw)
    bbox = raw.getchannel("A").getbbox()
    if bbox is None:
        raise ValueError("Generated frame is empty")

    sprite = raw.crop(bbox)
    width = target_w if target_w is not None else max(1, round(sprite.width * target_h / sprite.height))
    height = target_h
    if width > 92:
        height = max(1, round(height * 92 / width))
        width = 92

    sprite = binary_rgba(sprite.resize((width, height), Image.Resampling.NEAREST))
    cell = Image.new("RGBA", (96, 96), (0, 0, 0, 0))
    x = (96 - width) // 2
    y = 88 - height
    cell.alpha_composite(sprite, (x, y))
    return binary_rgba(cell)


def build(animation: str) -> Image.Image:
    source = Image.open(SOURCE_DIR / f"{NAME}_{animation}_source.png").convert("RGBA")
    sheet = Image.new("RGBA", (384, 192), (0, 0, 0, 0))

    for index in range(8):
        col, row = index % 4, index // 4
        box = (
            round(col * source.width / 4),
            round(row * source.height / 2),
            round((col + 1) * source.width / 4),
            round((row + 1) * source.height / 2),
        )
        raw = source.crop(box)
        if animation == "Death":
            frame = normalize_frame(raw, DEATH_HEIGHTS[index], DEATH_WIDTHS[index])
        else:
            frame = normalize_frame(raw, 80)
        sheet.alpha_composite(frame, (col * 96, row * 96))

    return binary_rgba(sheet)


def validate(sheet: Image.Image, animation: str) -> list[dict[str, int]]:
    assert sheet.size == (384, 192)
    assert set(sheet.getchannel("A").getdata()) <= {0, 255}
    frames: list[dict[str, int]] = []
    for index in range(8):
        col, row = index % 4, index // 4
        frame = sheet.crop((col * 96, row * 96, col * 96 + 96, row * 96 + 96))
        bbox = frame.getchannel("A").getbbox()
        if bbox is None:
            raise AssertionError(f"{animation} frame {index + 1} is empty")
        left, top, right, bottom = bbox
        assert bottom == 88, f"{animation} frame {index + 1}: bottom row is {bottom - 1}, expected 87"
        assert 0 < left and right < 96, f"{animation} frame {index + 1} touches a cell edge"
        frames.append(
            {
                "frame": index + 1,
                "left": left,
                "top": top,
                "right": right - 1,
                "bottom": bottom - 1,
                "width": right - left,
                "height": bottom - top,
            }
        )
    return frames


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    report: dict[str, object] = {
        "character": NAME,
        "sheet_size": [384, 192],
        "cell_size": [96, 96],
        "alpha_values": [0, 255],
        "bottom_occupied_row": 87,
        "animations": {},
    }

    built: dict[str, Image.Image] = {}
    for animation in ANIMATIONS:
        sheet = build(animation)
        report["animations"][animation] = validate(sheet, animation)
        sheet.save(OUT_DIR / f"{NAME}_{animation}.png", optimize=True)
        built[animation] = sheet

    side = built["Idle"].crop((0, 0, 96, 96))
    assert side.getchannel("A").getbbox() is not None
    side.save(OUT_DIR / f"{NAME}_Side.png", optimize=True)
    (OUT_DIR / "validation.json").write_text(
        json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8"
    )


if __name__ == "__main__":
    main()
