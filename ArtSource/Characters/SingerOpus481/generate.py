# -*- coding: utf-8 -*-
"""
SingerOpus481 procedural pixel-art sprite renderer.
Cabaret singer with a sawed-off shotgun, side view, facing RIGHT.
Spec: ArtSource/Generation/sprite_sheet_spec.md
"""
import math
from PIL import Image, ImageDraw, ImageChops, ImageFilter

CELL = 144
GROUND = 131
CX = 72

# ---------------------------------------------------------------- palette
OUT   = (24, 12, 16, 255)
SKIN  = (226, 172, 134, 255)
SKIN_S= (186, 128, 96, 255)
SKIN_H= (243, 200, 164, 255)
LIP   = (172, 46, 60, 255)
HAIR  = (98, 44, 30, 255)
HAIR_S= (62, 25, 18, 255)
HAIR_H= (150, 74, 46, 255)
FEATH = (162, 36, 54, 255)
FEATH_H=(210, 70, 92, 255)
DRESS = (140, 28, 46, 255)
DRESS_S=(92, 16, 30, 255)
DRESS_H=(186, 52, 74, 255)
LACE  = (240, 222, 178, 255)
CHEST = (233, 183, 146, 255)
GLOVE = (44, 32, 40, 255)
GLOVE_S=(26, 18, 24, 255)
GLOVE_H=(72, 56, 66, 255)
STK   = (40, 34, 44, 255)
STK_S = (24, 20, 28, 255)
STK_H = (84, 76, 92, 255)
BOOT  = (60, 38, 28, 255)
BOOT_S= (40, 24, 17, 255)
BOOT_H= (96, 62, 44, 255)
HEEL  = (26, 17, 13, 255)
GUNM  = (78, 82, 90, 255)
GUNM_H= (132, 136, 146, 255)
GUNM_S= (48, 50, 56, 255)
WOOD  = (122, 76, 42, 255)
WOOD_S= (86, 52, 28, 255)
GOLD  = (226, 180, 72, 255)
WHITE = (250, 250, 250, 255)

def L():
    return Image.new("RGBA", (CELL, CELL), (0, 0, 0, 0))

def disc(dr, c, r, col):
    x, y = c
    dr.ellipse([x - r, y - r, x + r, y + r], fill=col)

def capsule(dr, p0, p1, r0, r1, col):
    (x0, y0), (x1, y1) = p0, p1
    d = math.hypot(x1 - x0, y1 - y0)
    n = max(1, int(d * 1.5))
    for i in range(n + 1):
        t = i / n
        x = x0 + (x1 - x0) * t
        y = y0 + (y1 - y0) * t
        r = r0 + (r1 - r0) * t
        dr.ellipse([x - r, y - r, x + r, y + r], fill=col)

def poly(dr, pts, col):
    dr.polygon(pts, fill=col)

def outline_paste(base, layer, ring=OUT, grow=1):
    a = layer.split()[3]
    dil = a.filter(ImageFilter.MaxFilter(1 + 2 * grow))
    r = ImageChops.subtract(dil, a)
    ol = Image.new("RGBA", (CELL, CELL), (0, 0, 0, 0))
    ol.paste(ring, (0, 0), r)
    ol.alpha_composite(layer)
    base.alpha_composite(ol)

# ---------------------------------------------------------------- parts
def leg_layer(hip, knee, ankle, toe, near):
    lay = L(); dr = ImageDraw.Draw(lay)
    c = STK if near else STK_S
    capsule(dr, hip, knee, 5.6, 3.8, c)
    capsule(dr, knee, ankle, 3.8, 2.7, c)
    if near:
        mid = ((knee[0]+ankle[0])*.5, (knee[1]+ankle[1])*.5)
        capsule(dr, mid, ankle, 0.9, 0.7, STK_H)
    # heeled ankle boot
    bcol  = BOOT if near else BOOT_S
    bcolh = BOOT_H if near else BOOT_S
    ax, ay = ankle; tx, ty = toe
    capsule(dr, (ax, ay-1), (ax, ay+5), 3.2, 3.3, bcol)          # boot upper
    poly(dr, [(ax-3, ay+2), (tx, ty-3), (tx+1, ty), (ax-3, ty)], bcol)  # foot
    poly(dr, [(ax-3, ty-3), (ax-1, ty-3), (ax-1, ty+1), (ax-4, ty+1)], HEEL)  # heel
    dr.line([(ax-1, ay), (tx-1, ty-3)], fill=bcolh, width=1)     # shine
    return lay

def arm_layer(sh, elb, hand, near):
    lay = L(); dr = ImageDraw.Draw(lay)
    c = GLOVE if near else GLOVE_S
    ru = 2.7 if near else 3.0
    capsule(dr, sh, elb, ru, 2.1, c)
    capsule(dr, elb, hand, 2.1, 1.7, c)
    if near:
        capsule(dr, elb, hand, 0.7, 0.6, GLOVE_H)
    disc(dr, hand, 1.9, c)   # gloved hand
    return lay

def gun_layer(hand, ang):
    lay = L(); dr = ImageDraw.Draw(lay)
    hx, hy = hand
    a = math.radians(ang)
    ux, uy = math.cos(a), math.sin(a)
    px, py = -uy, ux
    # wood grip behind hand
    capsule(dr, (hx - ux*2, hy - uy*2), (hx - ux*6 - px*2, hy - uy*6 - py*2), 2.2, 1.7, WOOD)
    dr.line([(hx - ux*2, hy - uy*2), (hx - ux*6 - px*2, hy - uy*6 - py*2)], fill=WOOD_S, width=1)
    # double barrels
    bl = 16
    for off in (-1.4, 1.4):
        sx, sy = hx + px*off, hy + py*off
        ex, ey = sx + ux*bl, sy + uy*bl
        capsule(dr, (sx, sy), (ex, ey), 1.5, 1.4, GUNM)
    dr.line([(hx + px*1.4, hy + py*1.4), (hx + px*1.4 + ux*bl, hy + py*1.4 + uy*bl)], fill=GUNM_H, width=1)
    disc(dr, (hx, hy), 2.3, GUNM_S)  # receiver
    return lay

def torso_layer(J):
    lay = L(); dr = ImageDraw.Draw(lay)
    sx, sy = J['shoulder']; bx, by = J['bust']; wx, wy = J['waist']; hx, hy = J['hip']
    # corset body: back edge (left) + front edge (right) polygon, curvy hourglass
    pts = [
        (sx-4, sy), (sx+4, sy-1),          # shoulders
        (bx+9, by-2), (bx+10, by+2),       # bust front (protrudes)
        (wx+3, wy),                         # waist front (pinched)
        (hx+8, hy),                         # hip front (wide)
        (hx-7, hy),                         # hip back (wide)
        (wx-5, wy),                         # waist back
        (bx-6, by),                         # rib back
    ]
    poly(dr, pts, DRESS)
    # bust front lit panel
    poly(dr, [(bx+3, by-2), (bx+10, by+1), (bx+8, by+4), (bx+3, by+3)], DRESS_H)
    # decolletage skin (subtle)
    disc(dr, (bx+6, by-3), 1.7, CHEST)
    dr.point((bx+8, by-3), fill=SKIN)
    # corset cream lacing (vertical rungs on front)
    for i in range(5):
        ly = by + 2 + i*3
        dr.line([(bx+4, ly), (bx+8, ly)], fill=LACE, width=1)
    # boning shade line + pinched-waist shadow
    dr.line([(wx+2, wy-6), (wx+1, wy)], fill=DRESS_S, width=1)
    dr.line([(sx-3, sy+1), (wx-4, wy-1)], fill=DRESS_S, width=1)  # back shade
    # thin black choker at neck base
    dr.line([(sx, sy-3), (sx+5, sy-4)], fill=(20,16,20,255), width=1)
    return lay

def skirt_layer(J):
    lay = L(); dr = ImageDraw.Draw(lay)
    hx, hy = J['hip']
    top = hy - 3
    hem = hy + 13
    pts = [(hx-6, top), (hx+7, top), (hx+13, hem-2), (hx-12, hem-2)]
    poly(dr, pts, DRESS)
    # front lit panel
    poly(dr, [(hx+2, top), (hx+7, top), (hx+13, hem-2), (hx+6, hem-2)], DRESS_H)
    # ruffle hem (little scallops) + shade
    x = hx-12
    prev = (x, hem-2)
    while x <= hx+13:
        nx = min(x+3, hx+13)
        dr.line([prev, (nx, hem)], fill=DRESS_S, width=1)
        dr.line([(nx, hem), (nx, hem-2)], fill=DRESS_S, width=1)
        prev = (nx, hem-2); x += 3
    # fold shadows
    dr.line([(hx-2, top+1), (hx-4, hem-3)], fill=DRESS_S, width=1)
    dr.line([(hx+5, top+1), (hx+9, hem-3)], fill=DRESS_S, width=1)
    return lay

def head_layer(J):
    lay = L(); dr = ImageDraw.Draw(lay)
    hcx, hcy = J['head']
    R = 6.4
    # back hair mass
    disc(dr, (hcx-3, hcy+1), 7.6, HAIR)
    # updo bun top-back
    disc(dr, (hcx-2, hcy-6), 4.4, HAIR)
    disc(dr, (hcx-3, hcy-6), 2.0, HAIR_H)
    # neck
    capsule(dr, (hcx+1, hcy+R-1), J['neck'], 2.2, 2.7, SKIN)
    dr.line([(hcx-1, hcy+R), (J['neck'][0]-2, J['neck'][1])], fill=SKIN_S, width=1)
    # face (soft oval)
    disc(dr, (hcx+1, hcy), R, SKIN)
    # small nose + rounded chin (front = right)
    poly(dr, [(hcx+R, hcy), (hcx+R+1, hcy+1), (hcx+R, hcy+2)], SKIN)
    disc(dr, (hcx+2, hcy+4), 2.6, SKIN)          # rounded chin/cheek
    disc(dr, (hcx, hcy+3), 2.0, SKIN_S)          # jaw shade
    # lips (fuller)
    dr.line([(hcx+R-1, hcy+3), (hcx+R+1, hcy+3)], fill=LIP, width=1)
    dr.point((hcx+R, hcy+4), fill=(210,120,120,255))
    # eye + lashes
    dr.point((hcx+4, hcy+1), fill=OUT)
    dr.point((hcx+3, hcy+1), fill=WHITE)
    dr.line([(hcx+3, hcy-1), (hcx+6, hcy-1)], fill=HAIR_S, width=1)   # upper lash line
    dr.point((hcx+6, hcy), fill=HAIR_S)                               # outer lash
    dr.point((hcx+4, hcy+2), fill=(180,90,90,255))                    # blush
    # fringe sweeping over forehead (front)
    poly(dr, [(hcx-2, hcy-6), (hcx+5, hcy-5), (hcx+6, hcy-2), (hcx+2, hcy-3), (hcx-2, hcy-2)], HAIR)
    # side-swept curl by the cheek
    capsule(dr, (hcx-4, hcy-2), (hcx-5, hcy+6), 2.0, 1.4, HAIR)
    # loose wavy strand / low tail down back of neck
    capsule(dr, (hcx-6, hcy+3), (hcx-6, hcy+12), 2.2, 1.2, HAIR)
    dr.point((hcx-6, hcy+8), fill=HAIR_H)
    # earring
    dr.point((hcx+1, hcy+5), fill=GOLD)
    # feather: leaf-shaped burgundy plume rising up & back from bun
    base = (hcx-3, hcy-9)
    tip  = (hcx-9, hcy-21)
    midl = (hcx-8, hcy-14)
    midr = (hcx-3, hcy-15)
    poly(dr, [base, midl, tip, midr], FEATH)
    dr.line([base, tip], fill=FEATH_H, width=1)   # rib highlight
    # barbs
    for t in (0.35, 0.6, 0.82):
        bx = base[0] + (tip[0]-base[0])*t
        by = base[1] + (tip[1]-base[1])*t
        dr.line([(bx, by), (bx-2, by-1)], fill=FEATH_H, width=1)
    return lay

def flash_layer(hand, ang):
    lay = L(); dr = ImageDraw.Draw(lay)
    hx, hy = hand
    a = math.radians(ang)
    ux, uy = math.cos(a), math.sin(a)
    px, py = -uy, ux
    bl = 17
    tip = (hx + ux*bl, hy + uy*bl)
    # burst
    disc(dr, tip, 3.2, (255, 236, 150, 255))
    disc(dr, tip, 2.0, (255, 255, 220, 255))
    for k in (-1, 0, 1):
        ex = tip[0] + ux*5 + px*k*2.4
        ey = tip[1] + uy*5 + py*k*2.4
        dr.line([tip, (ex, ey)], fill=(255, 210, 90, 255), width=1)
    dr.line([tip, (tip[0]+ux*7, tip[1]+uy*7)], fill=(255, 244, 200, 255), width=1)
    return lay

STAND_ORDER = ['far_arm', 'far_leg', 'near_leg', 'torso', 'skirt', 'head', 'near_arm', 'gun', 'flash']

def build_parts(J):
    parts = {
        'far_leg':  leg_layer(*J['bl'], near=False),
        'near_leg': leg_layer(*J['fl'], near=True),
        'far_arm':  arm_layer(*J['ba'], near=False),
        'near_arm': arm_layer(*J['fa'], near=True),
        'gun':      gun_layer(J['fa'][2], J['gun_ang']),
        'torso':    torso_layer(J),
        'skirt':    skirt_layer(J),
        'head':     head_layer(J),
    }
    if J.get('flash'):
        parts['flash'] = flash_layer(J['fa'][2], J['gun_ang'])
    return parts

def character(J):
    img = L()
    parts = build_parts(J)
    for name in J['order']:
        if name in parts:
            outline_paste(img, parts[name])
    return img

# ---------------------------------------------------------------- baseline
def finalize(img, center_x=True):
    # binary alpha first (rotation may have produced partial edges)
    r, g, b, al = img.split()
    al = al.point(lambda v: 255 if v >= 128 else 0)
    img = Image.merge("RGBA", (r, g, b, al))
    a = img.split()[3]
    bbox = a.getbbox()
    if bbox is None:
        return img
    dy = GROUND - (bbox[3] - 1)
    dx = (CX - (bbox[0] + bbox[2] - 1) // 2) if center_x else 0
    if dx or dy:
        img = ImageChops.offset(img, dx, dy)
        px = img.load()
        for y in range(CELL):
            for x in range(CELL):
                # clear wrapped rows/cols
                if (dy > 0 and y < dy) or (dy < 0 and y >= CELL + dy) or \
                   (dx > 0 and x < dx) or (dx < 0 and x >= CELL + dx):
                    px[x, y] = (0, 0, 0, 0)
    # zero rgb where transparent
    px = img.load()
    for y in range(CELL):
        for x in range(CELL):
            if px[x, y][3] == 0:
                px[x, y] = (0, 0, 0, 0)
    return img

def standing(breath=0.0, gun_ang=62, arm_reach=0.0):
    cx = CX
    b = breath
    J = {
        'order': STAND_ORDER,
        'shoulder': (cx-1, 43 - b),
        'bust':     (cx,   52 - b),
        'waist':    (cx,   65),
        'hip':      (cx,   74),
        'neck':     (cx,   41 - b),
        'head':     (cx+1, 33 - b),
        'gun_ang':  gun_ang,
        'fl': ((cx+1, 75), (cx+2, 99), (cx+1, 121), (cx+9, 130)),
        'bl': ((cx-3, 75), (cx-4, 99), (cx-5, 121), (cx+2, 130)),
        'fa': ((cx+1, 46 - b), (cx+3, 63), (cx+10, 81)),
        'ba': ((cx-3, 46 - b), (cx-6, 63), (cx-7, 78)),
    }
    return J

# ================================================================ ANIMATIONS
def lerp(a, b, t):
    return a + (b - a) * t

def bent_leg(hipp, footp, bend, calf_ratio=0.52):
    """Return (hip, knee, ankle, toe). Knee pushed forward (+x) by `bend`."""
    hx, hy = hipp; fx, fy = footp
    kx = lerp(hx, fx, 1 - calf_ratio) + bend
    ky = lerp(hy, fy, 1 - calf_ratio)
    ankle = (fx, fy - 3)
    toe = (fx + 8, fy)
    return ((hx, hy), (kx, ky), ankle, toe)

# ---- IDLE -----------------------------------------------------------------
def idle_frames():
    breaths = [0, 0, 1, 1, 2, 1, 1, 0]
    sway    = [0, 1, 1, 2, 1, 1, 0, 0]
    frames = []
    for i in range(8):
        b = breaths[i]
        J = standing(breath=b, gun_ang=60 + sway[i])
        # tiny gun/hand sway
        sh, el, hd = J['fa']
        J['fa'] = (sh, el, (hd[0], hd[1] + sway[i]*0.4))
        frames.append(finalize(character(J)))
    return frames

# ---- WALK -----------------------------------------------------------------
def walk_leg(hipp, phase, stride=6, lift=6, ground=121):
    hx, hy = hipp
    # stance phase 0..0.5 foot moves back; swing 0.5..1 lifts & returns front
    if phase < 0.5:
        t = phase / 0.5
        fx = hx + stride - (2 * stride) * t
        fy = ground
    else:
        t = (phase - 0.5) / 0.5
        fx = hx - stride + (2 * stride) * t
        fy = ground - lift * math.sin(math.pi * t)
    bend = 3 + 3 * (1 if phase >= 0.5 else 0.3)
    return bent_leg((hx, hy), (fx, fy), bend)

def walk_frames():
    frames = []
    for i in range(8):
        p = i / 8.0
        # body bob: up at passing (0.25,0.75), down at contact
        bob = -1.4 * abs(math.sin(2 * math.pi * p))
        b = bob  # feed as 'breath' style vertical lift (raises upper body)
        cx = CX
        J = standing(breath=0, gun_ang=64)
        hipY = 74 - bob*0  # keep hips; move whole via legs
        # override torso/head slightly bob + forward lean for walk
        lean = 1
        for k in ['shoulder', 'bust', 'neck', 'head']:
            x, y = J[k]; J[k] = (x + lean, y + bob)
        # legs
        J['fl'] = walk_leg((cx + 1, 75 + bob), p)
        J['bl'] = walk_leg((cx - 2, 75 + bob), (p + 0.5) % 1.0)
        # far arm swings opposite the near leg; near arm holds gun (slight swing)
        swing = math.sin(2 * math.pi * p)
        J['ba'] = ((cx - 3, 46 + bob), (cx - 5 - swing*2, 60 + bob), (cx - 6 - swing*4, 76 + bob))
        sh = (cx + 1, 46 + bob)
        J['fa'] = (sh, (cx + 3 + swing*1, 63 + bob), (cx + 10 + swing*1.5, 81 + bob))
        frames.append(finalize(character(J)))
    return frames

# ---- SHOOT ----------------------------------------------------------------
def shoot_frames():
    # aim right, fire w/ flash + recoil, return
    # per-frame: (arm_raise 0..1 to horizontal, recoil px back+up, gun_ang, flash)
    key = [
        (0.15, 0, 30, False),  # 1 ready (gun a bit up)
        (0.7,  0,  8, False),  # 2 raising
        (1.0,  0,  0, False),  # 3 steady aim (horizontal)
        (1.0, -3, -14, True),  # 4 FIRE, kick up/back
        (1.0, -4, -18, True),  # 5 peak recoil, lean back
        (1.0, -1, -6, False),  # 6 recovering
        (1.0,  0,  0, False),  # 7 back to aim
        (0.6,  0, 12, False),  # 8 settling toward ready
    ]
    frames = []
    for i, (raise_, recoil, gang, flash) in enumerate(key):
        cx = CX
        leanback = 1 if recoil < -3 else 0
        J = standing(breath=0, gun_ang=gang)
        # lean torso/head back on recoil
        for k in ['shoulder', 'bust', 'neck', 'head']:
            x, y = J[k]; J[k] = (x - leanback, y)
        # front arm rises toward horizontal aim; hand moves forward+up
        sh = (cx + 1, 46)
        # low hand (ready) -> high forward hand (aim)
        hx = lerp(cx + 10, cx + 16, raise_) + recoil * 0.6
        hy = lerp(80, 49, raise_) + (-recoil)  # recoil kicks up (recoil negative -> up)
        ex = lerp(cx + 4, cx + 9, raise_) + recoil * 0.4
        ey = lerp(64, 49, raise_)
        J['fa'] = (sh, (ex, ey), (hx, hy))
        # far/support arm reaches toward gun when aiming
        J['ba'] = ((cx - 3, 46), (cx + 2, 55), (cx + 9, lerp(70, 52, raise_)))
        J['gun_ang'] = gang
        if flash:
            J['flash'] = True
            J['order'] = STAND_ORDER
        frames.append(finalize(character(J), center_x=False))
    return frames

# ---- DEATH ----------------------------------------------------------------
def rotate_about(layer, angle_ccw, pivot):
    return layer.rotate(angle_ccw, resample=Image.NEAREST, center=pivot)

def death_frames():
    cx = CX
    frames = []
    # per frame: (crouch dy for hips, upper-body forward-fold degrees, leg spec fn)
    # forward fold: positive = fold forward (head swings toward +x then down then -x)
    specs = [
        dict(fold=0,   hipy=74,  legs='stand'),
        dict(fold=-14, hipy=80,  legs='bend1'),   # lean back reaction, knees soften
        dict(fold=34,  hipy=92,  legs='bend2'),   # sink, hunch fwd, knees deep
        dict(fold=66,  hipy=101, legs='kneel'),   # to one knee, bowed over
        dict(fold=96,  hipy=110, legs='sprawl1'), # pitch forward, torso near horizontal
        dict(fold=124, hipy=119, legs='sprawl2'), # torso lowers, legs extend right
        dict(fold=150, hipy=126, legs='prone'),   # head-left, boots-right
        dict(fold=155, hipy=127, legs='prone2'),
    ]
    for s in specs:
        hipy = s['hipy']
        hip = (cx, hipy)
        # ---- lower body (legs + skirt), not folded ----
        J = standing(breath=0, gun_ang=60)
        # place hips/waist for this crouch (upper body will be rotated separately)
        J['hip'] = hip
        J['waist'] = (cx, hipy - 9)
        lg = s['legs']
        if lg == 'stand':
            J['fl'] = ((cx+1, hipy), (cx+2, 100), (cx+1, 121), (cx+9, 130))
            J['bl'] = ((cx-3, hipy), (cx-4, 100), (cx-5, 121), (cx+2, 130))
        elif lg == 'bend1':
            J['fl'] = bent_leg((cx+2, hipy), (cx+7, 121), 6)
            J['bl'] = bent_leg((cx-3, hipy), (cx-2, 121), 6)
        elif lg == 'bend2':
            J['fl'] = bent_leg((cx+4, hipy), (cx+12, 122), 9)
            J['bl'] = bent_leg((cx-2, hipy), (cx+2, 122), 9)
        elif lg == 'kneel':
            # back leg knee down, front foot planted forward
            J['bl'] = ((cx-2, hipy), (cx-6, 122), (cx+2, 130), (cx+9, 131))  # shin toward ground
            J['fl'] = bent_leg((cx+4, hipy), (cx+16, 124), 7)
        elif lg == 'sprawl1':
            J['bl'] = ((cx-1, hipy), (cx+7, 124), (cx+15, 130), (cx+22, 131))
            J['fl'] = ((cx+3, hipy), (cx+13, 123), (cx+23, 129), (cx+30, 131))
        elif lg == 'sprawl2':
            J['bl'] = ((cx+0, hipy), (cx+11, 127), (cx+21, 130), (cx+28, 131))
            J['fl'] = ((cx+3, hipy), (cx+15, 126), (cx+25, 130), (cx+33, 131))
        else:  # prone
            J['bl'] = ((cx+1, hipy), (cx+13, 128), (cx+23, 130), (cx+30, 131))
            J['fl'] = ((cx+3, hipy), (cx+16, 127), (cx+27, 130), (cx+35, 131))

        lower = L()
        for name in ['far_leg', 'near_leg']:
            p = {'far_leg': leg_layer(*J['bl'], near=False),
                 'near_leg': leg_layer(*J['fl'], near=True)}[name]
            outline_paste(lower, p)
        skirt = skirt_layer(J)
        outline_paste(lower, skirt)

        # ---- upper body (torso+arms+head+gun), folded forward about hip ----
        # build upright about the hip
        JU = standing(breath=0, gun_ang=60)
        JU['hip'] = hip
        JU['waist'] = (cx, hipy - 9)
        JU['bust'] = (cx, hipy - 22)
        JU['shoulder'] = (cx - 1, hipy - 31)
        JU['neck'] = (cx, hipy - 34)
        JU['head'] = (cx + 1, hipy - 42)
        # arms hang / gun drifts down
        JU['fa'] = ((cx+1, hipy-29), (cx+3, hipy-12), (cx+10, hipy+2))
        JU['ba'] = ((cx-3, hipy-29), (cx-6, hipy-13), (cx-7, hipy+0))
        JU['gun_ang'] = 70
        upper = L()
        for name in ['far_arm', 'torso', 'head', 'near_arm', 'gun']:
            parts = build_parts(JU)
            outline_paste(upper, parts[name])
        # fold forward about hip: forward (top->+x then down) = clockwise = negative CCW
        upper = rotate_about(upper, -s['fold'], hip)

        img = L()
        # when folded a lot (prone), upper body lies over legs at the head end
        img.alpha_composite(lower)
        img.alpha_composite(upper)
        frames.append(finalize(img))
    return frames

# ================================================================ BUILD SHEET
def build_sheet(frames):
    sheet = Image.new("RGBA", (CELL * 8, CELL), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        sheet.paste(f, (i * CELL, 0))
    return sheet

def verify(sheet, name):
    assert sheet.size == (1152, 144), sheet.size
    px = sheet.load()
    issues = []
    for i in range(8):
        low = -1; top = 145; l = 999; r = -1
        for y in range(CELL):
            for x in range(i*CELL, (i+1)*CELL):
                a = px[x, y][3]
                if a not in (0, 255):
                    issues.append(f'f{i} nonbinary alpha {a}')
                if a == 255:
                    low = max(low, y); top = min(top, y)
                    l = min(l, x - i*CELL); r = max(r, x - i*CELL)
        if low != GROUND:
            issues.append(f'{name} f{i+1}: bottom y={low} (want {GROUND})')
        if l < 0 or r > 143:
            issues.append(f'{name} f{i+1}: x out of cell {l}..{r}')
    return issues

if __name__ == '__main__':
    import os
    prev = os.path.dirname(os.path.abspath(__file__))
    out = r'G:/Projects/HighNoon/ArtSource/Characters/SingerOpus481'
    os.makedirs(out, exist_ok=True)
    anims = {
        'Idle': idle_frames(),
        'Walk': walk_frames(),
        'Shoot': shoot_frames(),
        'Death': death_frames(),
    }
    for name, frames in anims.items():
        sheet = build_sheet(frames)
        sheet.save(os.path.join(out, f'SingerOpus481_{name}.png'))
        sheet.resize((CELL*8*4, CELL*4), Image.NEAREST).save(os.path.join(prev, f'prev_{name}.png'))
        iss = verify(sheet, name)
        print(name, 'OK' if not iss else 'ISSUES:')
        for x in iss:
            print('  ', x)
