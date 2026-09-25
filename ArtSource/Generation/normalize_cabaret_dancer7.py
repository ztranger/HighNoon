from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
NAME = "CabaretDancer7"
OUT = ROOT / "ArtSource" / "Characters" / NAME
SOURCE_DIR = ROOT / "ArtSource" / "Generation"
ANIMATIONS = ("Idle", "Walk", "Shoot", "Death")


def binary_rgba(image: Image.Image) -> Image.Image:
    image = image.convert("RGBA")
    pixels = image.load()
    for y in range(image.height):
        for x in range(image.width):
            r, g, b, a = pixels[x, y]
            pixels[x, y] = (r, g, b, 255) if a >= 128 else (0, 0, 0, 0)
    return image


def frame_from_source(raw: Image.Image, target_h: int, target_w: int | None = None) -> Image.Image:
    raw = binary_rgba(raw)
    bbox = raw.getchannel("A").getbbox()
    if bbox is None:
        raise ValueError("Generated frame is empty")
    sprite = raw.crop(bbox)
    base_h = target_h // 4
    base_w = target_w // 4 if target_w else max(1, round(sprite.width * base_h / sprite.height))
    if base_w > 88:
        base_w = 88
        base_h = max(1, round(sprite.height * base_w / sprite.width))
    base = sprite.resize((base_w, base_h), Image.Resampling.NEAREST)
    base = binary_rgba(base)
    enlarged = base.resize((base_w * 4, base_h * 4), Image.Resampling.NEAREST)
    cell = Image.new("RGBA", (384, 384), (0, 0, 0, 0))
    x = ((96 - base_w) // 2) * 4
    y = (88 - base_h) * 4
    cell.alpha_composite(enlarged, (x, y))
    return binary_rgba(cell)


def build(animation: str) -> Image.Image:
    src = Image.open(SOURCE_DIR / f"{NAME}_{animation}_source.png").convert("RGBA")
    sheet = Image.new("RGBA", (1536, 768), (0, 0, 0, 0))
    heights = [320, 292, 232, 196, 136, 116, 88, 88]
    widths = [128, 184, 164, 196, 240, 260, 264, 264]
    for i in range(8):
        col, row = i % 4, i // 4
        box = (round(col * src.width / 4), round(row * src.height / 2),
               round((col + 1) * src.width / 4), round((row + 1) * src.height / 2))
        raw = src.crop(box)
        cell = frame_from_source(raw, heights[i], widths[i]) if animation == "Death" else frame_from_source(raw, 320)
        sheet.alpha_composite(cell, (col * 384, row * 384))
    return binary_rgba(sheet)


def validate(image: Image.Image) -> None:
    assert image.size == (1536, 768)
    assert set(image.getchannel("A").getdata()) <= {0, 255}
    for frame in range(8):
        x0, y0 = (frame % 4) * 384, (frame // 4) * 384
        cell = image.crop((x0, y0, x0 + 384, y0 + 384))
        assert cell.getchannel("A").getbbox() is not None
        assert cell.getchannel("A").getbbox()[3] == 352
        base = cell.resize((96, 96), Image.Resampling.NEAREST)
        restored = base.resize((384, 384), Image.Resampling.NEAREST)
        assert restored.tobytes() == cell.tobytes()


def main() -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    built = {}
    for animation in ANIMATIONS:
        sheet = build(animation)
        validate(sheet)
        sheet.save(OUT / f"{NAME}_{animation}.png", optimize=True)
        built[animation] = sheet
    side = built["Idle"].crop((0, 0, 384, 384))
    side.save(OUT / f"{NAME}_Side.png", optimize=True)


if __name__ == "__main__":
    main()
