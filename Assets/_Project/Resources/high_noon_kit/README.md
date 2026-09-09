# High Noon — cemetery 2v2 kit

Исходный макет: reference/mockup_2v2_cemetery.jpg (1712x1152)

## Что класть в игру

Используй в первую очередь sprites_png/ — персонажи и UI с прозрачностью.

- sprites_png/char_rio_vela.png
- sprites_png/char_dusty_hart.png
- sprites_png/char_black_calhoun.png
- sprites_png/char_doc_graves.png
- sprites_png/slider_bar.png
- sprites_png/heart_full.png
- sprites_png/heart_empty.png
- sprites_png/marker_green.png
- sprites_png/marker_red.png
- sprites_png/portrait_frame.png
- bg/bg_cemetery.jpg  (фон без персонажей)

Вырезы прямо с макета (фон запечён, для референса):
- portraits/*.jpg
- characters/*_crop.jpg
- ui/slider_team.jpg, slider_rivals.jpg
- hud/*

## Рекомендуемая сборка экрана

1. Растянуть bg_cemetery на весь ландшафт.
2. Поставить 4 спрайта персонажей в ряд (свои слева лицом вправо, враги справа лицом влево).
3. HUD сверху: frame + портрет + имя текстом движка + 5 heart_full/empty.
4. Снизу две slider_bar друг над другом.
5. На своей полоске два marker_green, на вражеской два marker_red. Двигать по X от 0..1.

Тап — по всему экрану, не по полоске. Полоска только показывает тайминг.

## Имена

Team: Dusty B. Hart, Rio Vela
Rivals: Black J. Calhoun, Doc Graves
