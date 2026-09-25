from pathlib import Path
from PIL import Image


ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "ArtSource" / "Characters" / "CabaretSinger5"
SOURCES = {
    name: ROOT / "ArtSource" / "Generation" / f"CabaretSinger5_{name}_source.png"
    for name in ("Idle", "Walk", "Shoot", "Death")
}


def hard_alpha(img: Image.Image) -> Image.Image:
    img = img.convert("RGBA")
    alpha = img.getchannel("A").point(lambda a: 255 if a >= 128 else 0)
    img.putalpha(alpha)
    return img


def normalize_frame(frame: Image.Image, target_height: int, target_width: int | None = None) -> Image.Image:
    frame = hard_alpha(frame)
    bbox = frame.getchannel("A").getbbox()
    if bbox is None:
        raise ValueError("Empty generated frame")
    sprite = frame.crop(bbox)
    scale = target_height / sprite.height
    scaled_width = max(1, round(sprite.width * scale))
    if target_width is None:
        target_width = scaled_width
    if target_width > 352:
        scale = 352 / sprite.width
        target_width = 352
        target_height = max(1, round(sprite.height * scale))
    sprite = sprite.resize((target_width, target_height), Image.Resampling.NEAREST)
    sprite = hard_alpha(sprite)
    cell = Image.new("RGBA", (384, 384), (0, 0, 0, 0))
    x = (384 - target_width) // 2
    y = 352 - target_height
    cell.alpha_composite(sprite, (x, y))
    return cell


def build_sheet(name: str, source: Path) -> Image.Image:
    image = Image.open(source).convert("RGBA")
    sheet = Image.new("RGBA", (1536, 768), (0, 0, 0, 0))
    death_heights = [320, 292, 232, 196, 136, 116, 88, 88]
    death_widths = [128, 184, 164, 196, 240, 260, 264, 264]
    # The generator uses a visually regular grid, but the death poses extend
    # below the mathematical half-height. Split that source at its actual
    # inter-row gap so the first-row boots do not contaminate row two.
    row_split = 560 if name == "Death" else round(image.height / 2)
    for index in range(8):
        col, row = index % 4, index // 4
        left = round(col * image.width / 4)
        right = round((col + 1) * image.width / 4)
        top = 0 if row == 0 else row_split
        bottom = row_split if row == 0 else image.height
        raw = image.crop((left, top, right, bottom))
        height = death_heights[index] if name == "Death" else 320
        width = death_widths[index] if name == "Death" else None
        frame = normalize_frame(raw, height, width)
        sheet.alpha_composite(frame, (col * 384, row * 384))
    return hard_alpha(sheet)


def main() -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    built = {}
    for animation, source in SOURCES.items():
        sheet = build_sheet(animation, source)
        target = OUT / f"CabaretSinger5_{animation}.png"
        sheet.save(target, optimize=True)
        built[animation] = sheet
    side = built["Idle"].crop((0, 0, 384, 384))
    side.save(OUT / "CabaretSinger5_Side.png", optimize=True)


if __name__ == "__main__":
    main()
