from pathlib import Path

from PIL import Image


ROOT = Path(r"G:\Projects\HighNoon")
GENERATED = Path(
    r"C:\Users\kirik\.codex\generated_images\01a0d887-0f2c-7353-b869-f825eb8f572f"
)
SOURCES = {
    "Idle": GENERATED / "exec-5e4cbb2d-3886-418c-ac28-45b4f31143c0.png",
    "Walk": GENERATED / "exec-24f36ebc-41e8-4a58-9b2a-1be9df2068fc.png",
    "Shoot": GENERATED / "exec-4d529b39-a636-4e13-a58d-5e4f51393861.png",
    "Death": GENERATED / "exec-fae4ea06-a28a-454f-97ca-39cf73110f1d.png",
}
OUTPUT = ROOT / "ArtSource" / "Characters" / "Undertaker"

SOURCE_CELL = (384, 512)
FRAME_SIZE = (443, 443)
SHEET_SIZE = (1774, 887)
BASELINE_Y = 405
ALPHA_THRESHOLD = 250
DEATH_HEIGHTS = (369, 337, 268, 226, 157, 134, 101, 101)
DEATH_WIDTHS = (147, 213, 189, 226, 277, 300, 305, 305)


def binary_cutout(source: Image.Image, index: int, animation: str) -> Image.Image:
    col, row = index % 4, index // 4
    x0, y0 = col * SOURCE_CELL[0], row * SOURCE_CELL[1]
    # The edited fifth death pose extends 16 px into the next logical source cell.
    # Recover that connected shovel tip here, then remove the same spill from frame 6.
    source_width = 400 if animation == "Death" and index == 4 else SOURCE_CELL[0]
    frame = source.crop((x0, y0, x0 + source_width, y0 + SOURCE_CELL[1]))
    alpha = frame.getchannel("A").point(
        lambda value: 255 if value >= ALPHA_THRESHOLD else 0
    )

    # The generated death sheet's first row touches the horizontal split.
    # Clear only the narrow carry-over band before extracting bottom-row poses.
    if animation == "Death" and row == 1:
        alpha.paste(0, (0, 0, frame.width, 100))
    if animation == "Death" and index == 5:
        alpha.paste(0, (0, 0, 16, frame.height))

    frame.putalpha(alpha)
    bbox = alpha.getbbox()
    if bbox is None:
        raise RuntimeError(f"No opaque pixels in {animation} frame {index + 1}")
    return frame.crop(bbox)


def normalize_frame(
    cutout: Image.Image, target_height: int, exact_width: int | None = None
) -> Image.Image:
    width, height = cutout.size
    target_width = exact_width or max(1, round(width * target_height / height))

    # Leave a safety margin for wide equipment and muzzle flash.
    if target_width > 419:
        scale = 419 / target_width
        target_width = 419
        target_height = max(1, round(target_height * scale))

    resized = cutout.resize((target_width, target_height), Image.Resampling.NEAREST)
    frame = Image.new("RGBA", FRAME_SIZE, (0, 0, 0, 0))
    occupied = resized.getchannel("A").getbbox()
    if occupied is None:
        raise RuntimeError("Normalized frame unexpectedly became transparent")
    left = round(FRAME_SIZE[0] / 2 - (occupied[0] + occupied[2]) / 2)
    top = BASELINE_Y + 1 - occupied[3]
    frame.alpha_composite(resized, (left, top))

    # Reassert binary alpha after all transforms.
    alpha = frame.getchannel("A").point(lambda value: 255 if value else 0)
    frame.putalpha(alpha)
    return frame


def shared_palette(frames: dict[str, list[Image.Image]]) -> Image.Image:
    samples: list[tuple[int, int, int]] = []
    for animation_frames in frames.values():
        for frame in animation_frames:
            pixels = list(frame.getdata())
            opaque = [(r, g, b) for r, g, b, a in pixels if a == 255]
            samples.extend(opaque[::12])

    sample_image = Image.new("RGB", (len(samples), 1))
    sample_image.putdata(samples)
    reduced = sample_image.quantize(
        colors=24, method=Image.Quantize.MEDIANCUT, dither=Image.Dither.NONE
    )
    colors = reduced.getpalette()[: 24 * 3]

    palette = Image.new("P", (1, 1))
    palette.putpalette(colors + colors[:3] * (256 - 24))
    return palette


def quantize_frame(frame: Image.Image, palette: Image.Image) -> Image.Image:
    alpha = frame.getchannel("A")
    indexed = frame.convert("RGB").quantize(palette=palette, dither=Image.Dither.NONE)
    result = indexed.convert("RGBA")
    result.putalpha(alpha)
    return result


def assemble_sheet(frames: list[Image.Image]) -> Image.Image:
    sheet = Image.new("RGBA", SHEET_SIZE, (0, 0, 0, 0))
    for index, frame in enumerate(frames):
        sheet.alpha_composite(frame, ((index % 4) * 443, (index // 4) * 443))
    return sheet


def main() -> None:
    OUTPUT.mkdir(parents=True, exist_ok=True)
    normalized: dict[str, list[Image.Image]] = {}

    for animation, source_path in SOURCES.items():
        source = Image.open(source_path).convert("RGBA")
        heights = DEATH_HEIGHTS if animation == "Death" else (369,) * 8
        widths = DEATH_WIDTHS if animation == "Death" else (None,) * 8
        normalized[animation] = [
            normalize_frame(
                binary_cutout(source, index, animation), heights[index], widths[index]
            )
            for index in range(8)
        ]

    palette = shared_palette(normalized)
    quantized = {
        animation: [quantize_frame(frame, palette) for frame in frames]
        for animation, frames in normalized.items()
    }

    for animation, frames in quantized.items():
        assemble_sheet(frames).save(OUTPUT / f"Undertaker_{animation}.png")

    quantized["Idle"][0].save(OUTPUT / "Undertaker_Side.png")


if __name__ == "__main__":
    main()
