# High Noon — гайд по ассетам и интеграции

Документ описывает, что уже нарезано, как это класть в движок (Unity / Godot / свой Android-клиент) и как генерировать новых персонажей так, чтобы они вставали в те же слоты.

Игра: 2D пиксельная дуэль, ландшафт, Android → iOS / Web.  
Основной экран боя — тайминг (тап в «зелёную» зону / по сигналу часов).

---

## 1. Что лежит в архивах

| Архив | Назначение |
|---|---|
| `HighNoon_main_tab_kit.zip` | Главная вкладка Hero: фон города, герой, кнопки DUEL / STORY / LOADOUT, табы |
| `HighNoon_weapons_tab_kit.zip` | Вкладка оружия: стена салуна, револьвер, ящик, стрелки, категории |
| `HighNoon_events_tab_kit.zip` | Вкладка событий: пустыня, плашки ивентов, табы |
| `HighNoon_cemetery_2v2_kit.zip` | Экран дуэли 2v2 + HUD + слайдеры TEAM / RIVALS |
| `HighNoon_backgrounds.zip` | Фоны локаций (кладбище, прерия, город, каньон, станция, солончак) |
| `HighNoon_hero_sprites.zip` | Спрайт-лист героя: idle / shoot / death |

В каждом ките:

- `sprites_png/` или `sheets/` — PNG с прозрачностью. **Их и вставляй.**
- `crops/` — вырезки с макета, фон запечён. Только референс размеров.
- `bg/` — фоны без UI.
- `README.md` — коротко по папке.
- `atlas.json` (у героя) — координаты кадров.

Числа, имена, таймеры, XP **не запекай в картинку**. Рисуй шрифтом движка.

---

## 2. Общие правила картинок

1. Стиль: 16-bit pixel art, тёплая вестерн-палитра (охристые, дерево, закат) + фиолетовое кладбище.
2. Бой и меню — **ландшафт**. Под широкие Samsung есть папка `wide_2x1/` (кроп 2:1).
3. Зелёный `#00FF00` на исходниках спрайтов — хромакей. В PNG он уже вырезан.
4. Не растягивай пиксель фильтром. В Unity: `Filter Mode = Point (no filter)`, `Compression = None` / RGBA32. В Godot: texture filter **Nearest**.
5. Якорь персонажа — **ступни по центру нижней грани кадра**, не визуальный центр спрайта. Иначе смерть «уплывёт».
6. На ультрашироком экране ставь дуэлянтов не у краёв, а ближе к 30% и 70% ширины.

---

## 3. Сборка экранов

### 3.1 Главное меню (вкладка Hero)

Слои снизу вверх:

1. `bg_town_sunset.jpg` на весь экран.
2. `hero_idle.png` по центру (или кадр 0 спрайт-листа).
3. Шапка: `title_high_noon.png` + текст `Lv. X` + иконки валюты.
4. Полоска арен — UI-бар + текст `OUTLAW CAMP → DEADWOOD`. Прогресс из сейва.
5. `btn_duel_pvp` слева, `btn_story_pve` справа, `btn_loadout` под героем.
6. Нижние 5 табов. Активный — `tab_hero_selected`. Крайние два — заглушки `tab_locked`.

Хитбоксы кнопок бери с `crops/` (там реальные пропорции макета 1168×784).

### 3.2 Вкладка оружия

1. `bg_wood_wall.jpg`.
2. `crate.png` по центру, поверх `weapon_revolver.png`.
3. Стрелки влево/вправо — листают массив оружия.
4. `plaque_blank.png` + текст статов: DMG / RANGE / RELOAD / SPREAD.
5. Если ствол закрыт — `panel_locked.png` вместо превью.
6. Категории снизу: saloon / revolver / rifles / shotguns / gear.

Модель оружия в коде:

```
Weapon { id, name, rarity, dmg, range, reload, spread, unlocked, sprite }
```

### 3.3 Вкладка событий

1. `bg_desert_sunset.jpg`.
2. Три плашки столбиком. Для ландшафта удобнее `crops/banner_*.jpg`.
3. Таймер `ENDS IN` — текст поверх, тикает с сервера.
4. Заблокированный ивент не прячь: серая плашка + замок.

```
Event { id, title, art, endsAt, locked, reward }
```

### 3.4 Карта главы PvE

Не нарезана отдельным китом, макеты есть в чате (пергамент / тропа / вертикаль / развилка).

Минимальная схема:

```
Chapter { id, locationBg, nodes: [Node] }
Node { id, type: duel|boss, locked, cleared, next[] }
```

Точка → тот же экран дуэли, меняется только фон и набор противников. Последняя точка — босс.

### 3.5 Экран дуэли

1. Фон локации на весь экран (`HighNoon_backgrounds`).
2. 1v1: два спрайта лицом друг к другу.  
   2v2: четыре спрайта, свои слева, враги справа.
3. HUD сверху: портрет + имя + сердца. В 2v2 — две колонки.
4. Снизу одна или две длинные полоски реакции (`slider_bar.png`).
   - Своя команда: зелёные маркеры.
   - Враги: красные маркеры.
5. Тап — **на весь экран**, не на полоску. Полоска только показывает попадание.

Состояния персонажа на экране:

```
idle → (сигнал DRAW) hold
     → (тап) shoot
     → если проиграл: death (стоп на последнем кадре)
     → если выиграл: остаётся в кадре выстрела / короткий hold
```

---

## 4. Спрайты героя

Файл: `HighNoon_hero_sprites.zip` → `sheets/hero_sheet_4x6.png`

| Ряд | Анимация | Кадры | Цикл |
|---|---|---|---|
| 0 | idle вправо | 6 | да |
| 1 | shoot вправо | 6 | нет, вспышка на кадре 3 |
| 2 | death вправо | 6 | нет, стоп на 5 |
| 3 | idle влево | 6 | да |

Отдельные ленты по 8 кадров: `idle_strip`, `shoot_strip`, `death_strip`.  
Для продакшена бери **один 4×6 лист** — персонаж там стабильнее.

Рекомендуемый FPS: idle 6–8, shoot 12, death 10.

Влево для shoot/death: `flipX = true`. Не генерируй второй лист, пока не нужен уникальный силуэт.

Якорь: ноги. Смерть рисуется в том же прямоугольнике, что idle — не сдвигай root.

Пример логики:

```
play("idle")
on DrawSignal:  // можно оставить idle или короткий hold
on Tap:
    playOnce("shoot")
    if lost: playOnce("death")
```

---

## 5. Как генерировать нового персонажа, чтобы он вставал в игру

Цель: любой новый ковбой занимает **тот же атлас**, те же ряды и тот же размер клетки.

### 5.1 Жёсткий контракт спрайт-листа

- Холст: **6 колонок × 4 ряда**.
- Фон кадра: плоский `#00FF00` (chroma). Никакой сцены.
- Один персонаж на клетку, один масштаб на весь лист.
- Камера / «пиксельная высота» героя одинаковая во всех кадрах (примерно 2/3 высоты клетки).
- Ноги всегда на одной линии (baseline).
- Ряд 0: idle вправо, оружие в кобуре, лёгкое дыхание / пончо.
- Ряд 1: draw → aim → **muzzle flash** → recoil → hold.
- Ряд 2: попадание → спотыкание → колени → падение → труп ×2.
- Ряд 3: idle влево (зеркало ряда 0).

Имя файла: `char_{id}_sheet.png`.  
Атлас тот же, что у героя: `cellW = width/6`, `cellH = height/4`.

### 5.2 Промпт-шаблон

Копируй и меняй только блок APPEARANCE.

```
16-bit pixel art game sprite sheet on flat solid #00FF00 chroma green background,
no scene, no UI, no text, no numbers, no labels.

One western character only.
APPEARANCE: {шляпа}, {пончо/плащ/жилет}, {цвет рубахи}, {штаны}, {обувь}, {борода да/нет}.
Same character and same proportions in every cell.

Sheet layout exactly 4 rows x 6 columns of equal frames, character centered in each cell,
feet on the same baseline.

Row 1 IDLE facing right: 6 frames subtle breathing, guns holstered.
Row 2 DRAW AND SHOOT facing right: 6 frames hand to holster, raise revolver,
      fire with small muzzle flash, recoil, hold aim.
Row 3 DEATH facing right: 6 frames hit, stumble, kneel, fall, lie still, last frame corpse.
Row 4 IDLE facing left: 6 frames, mirror of row 1.

Clean chunky 16-bit pixels, limited western palette, no blur.
```

Примеры APPEARANCE:

- герой: `brown hat, red poncho with gold trim, blue shirt, jeans, brown boots`
- Rio: `sombrero, colorful serape, jeans, boots`
- Calhoun: `dark cowboy hat, red bandana, brown vest, gray shirt`
- Doc Graves: `black top hat, long dark frock coat, beard, two revolvers`

### 5.3 После генерации

1. Снять зелёный: `G > 170 и G > R+30 и G > B+30` → альфа 0.
2. Порезать сеткой 6×4. Не кропай каждую клетку по bbox — **размер клетки общий**, иначе анимация прыгает.
3. Проверить baseline ног на кадрах idle и shoot. Если плывёт — сдвинь спрайт внутри клетки, не меняй pivot в рантайме.
4. Положить в `characters/{id}/sheet.png` + `characters/{id}/meta.json`.

`meta.json`:

```json
{
  "id": "doc_graves",
  "displayName": "Doc Graves",
  "sheet": "sheet.png",
  "cols": 6,
  "rows": 4,
  "anims": {
    "idle":   { "row": 0, "frames": 6, "fps": 7, "loop": true },
    "shoot":  { "row": 1, "frames": 6, "fps": 12, "loop": false, "flashFrame": 3 },
    "death":  { "row": 2, "frames": 6, "fps": 10, "loop": false },
    "idle_l": { "row": 3, "frames": 6, "fps": 7, "loop": true }
  },
  "flipShoot": true,
  "role": "rival"
}
```

Портрет для HUD: отдельный квадрат 128×128 (голова + плечи + рамка). Не вырезай из боевого кадра — будет мелкий и кривой.

### 5.4 Что не делать

- Не мешать стили (аниме / реализм) с пикселем боя.
- Не писать подписи рядов на самом листе.
- Не менять число кадров у одного персонажа — сломаешь общий проигрыватель.
- Не рисовать кровь огромным пятном: на ландшафте телефона это каша. 3–6 пикселей достаточно.
- Не генерировать walk/run, пока не появится карта-тропа с бегущим героем.

### 5.5 Новый фон локации

Промпт:

```
16-bit pixel art game background only, no characters, no UI, no text.
{локация и время суток}.
Empty foreground path in the center for two duelists.
Crisp chunky pixels.
```

Для Samsung сохрани оригинал и кроп 2:1 (высота = ширина / 2, центр).

---

## 6. Папки в проекте (рекомендуется)

```
assets/
  ui/main/
  ui/weapons/
  ui/events/
  ui/hud/          # сердца, рамки, маркеры, slider_bar
  bg/cemetery.png
  bg/prairie.png
  bg/town.png
  bg/canyon.png
  bg/station.png
  bg/saltflat.png
  characters/hero/sheet.png
  characters/hero/portrait.png
  characters/hero/meta.json
```

Один проигрыватель анимаций на всех `meta.json`. Новый персонаж = новая папка, без правки кода, если ряды совпали.

---

## 7. Минимальный чеклист перед вставкой

- [ ] PNG, не JPG (кроме фонов)
- [ ] Point filter
- [ ] 6×4, одинаковый cell
- [ ] Ноги на одной линии
- [ ] Idle зациклен, shoot/death нет
- [ ] Вспышка выстрела на фиксированном кадре
- [ ] Портрет отдельным файлом
- [ ] id совпадает с сейвом / лобби

Если лист «поплыл» по стилю — перегенерируй **весь** 4×4/4×6 одним промптом, не склеивай ряды из разных картинок.
