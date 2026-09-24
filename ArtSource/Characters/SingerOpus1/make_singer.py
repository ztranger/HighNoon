"""SingerOpus1 — procedural pixel-art sprite sheets (Idle / Walk / Shoot / Death).

Spec: ArtSource/Generation/sprite_sheet_spec.md
Each sheet: 1152x144 RGBA, 8 cells of 144x144, binary alpha, bottom occupied row y=131,
character always faces right, standing height ~120 px.

Run:  python make_singer.py            -> writes the four PNGs next to this script
      python make_singer.py --preview  -> also writes a 4x preview contact sheet
"""
import math
import os
import sys

import numpy as np
from PIL import Image, ImageDraw

CELL = 144
FRAMES = 8
GROUND_Y = 131          # last occupied row in each cell
WORK = 224              # working canvas (larger than a cell so nothing clips)
ROOT = (112.0, 120.0)   # hip centre on the working canvas (frames are re-aligned afterwards)

HERE = os.path.dirname(os.path.abspath(__file__))
NAME = "SingerOpus1"

# ----------------------------------------------------------------------------- palette
OUT = (30, 16, 22)
R_SKIN = [(118, 58, 52), (166, 94, 78), (212, 142, 112), (234, 180, 146), (248, 210, 180)]
R_HAIR = [(40, 12, 14), (78, 24, 22), (120, 42, 30), (160, 66, 40), (192, 98, 58)]
R_CRIM = [(54, 10, 22), (92, 18, 34), (138, 28, 44), (180, 48, 58), (214, 88, 84)]
R_BLACK = [(16, 12, 20), (28, 22, 32), (44, 36, 50), (70, 60, 80), (106, 96, 118)]
R_STOCK = [(56, 30, 36), (86, 50, 54), (118, 72, 70), (150, 98, 90), (180, 128, 114)]
R_GOLD = [(92, 58, 20), (146, 100, 32), (198, 150, 54), (232, 194, 94), (252, 228, 150)]
R_LACE = [(120, 108, 104), (164, 150, 138), (206, 192, 172), (232, 222, 202), (248, 242, 228)]
R_WOOD = [(56, 28, 16), (88, 48, 26), (124, 72, 38), (158, 98, 54), (186, 128, 76)]
R_STEEL = [(36, 38, 48), (60, 62, 74), (92, 94, 106), (130, 132, 144), (172, 174, 184)]
R_FEATH = [(44, 8, 18), (84, 16, 30), (130, 26, 42), (176, 50, 60), (214, 96, 96)]
LIPS = (170, 30, 44)
LIPS_HI = (210, 70, 80)
EYE = (26, 14, 20)
EYE_WHITE = (232, 222, 214)
SHADOW_LID = (126, 70, 86)
BLUSH = (222, 128, 112)
FLASH = [(255, 250, 210), (255, 220, 96), (240, 140, 40)]

LIGHT = (1, -1)  # light comes from upper-right (in front of the character)


# ----------------------------------------------------------------------------- math
def v_add(a, b): return (a[0] + b[0], a[1] + b[1])
def v_sub(a, b): return (a[0] - b[0], a[1] - b[1])
def v_mul(a, s): return (a[0] * s, a[1] * s)
def v_len(a): return math.hypot(a[0], a[1])


def v_norm(a):
    l = v_len(a) or 1.0
    return (a[0] / l, a[1] / l)


def dir_down(deg):
    """Direction for an angle measured from straight down, + = towards +x (forward)."""
    r = math.radians(deg)
    return (math.sin(r), math.cos(r))


def dir_x(deg):
    """Direction for an angle measured from +x, + = rotating downwards (screen y)."""
    r = math.radians(deg)
    return (math.cos(r), math.sin(r))


class Frame:
    """Local frame: x = forward axis, y = up axis (both unit vectors in world space)."""

    def __init__(self, origin, fwd, up):
        self.o, self.f, self.u = origin, fwd, up

    def w(self, x, y):
        return (self.o[0] + self.f[0] * x + self.u[0] * y, self.o[1] + self.f[1] * x + self.u[1] * y)

    def pts(self, pts):
        return [self.w(x, y) for x, y in pts]

    def local(self, X, Y):
        dx, dy = X - self.o[0], Y - self.o[1]
        return dx * self.f[0] + dy * self.f[1], dx * self.u[0] + dy * self.u[1]


def frame_from_up(origin, up_deg):
    """up_deg: 0 = upright, + = leaning forward (top towards +x)."""
    r = math.radians(up_deg)
    up = (math.sin(r), -math.cos(r))
    fwd = (math.cos(r), math.sin(r))
    return Frame(origin, fwd, up)


def frame_from_axis(origin, axis):
    """Frame whose -y (down) is `axis`; forward = axis rotated so a downward axis gives +x."""
    d = v_norm(axis)
    fwd = (d[1], -d[0])
    return Frame(origin, fwd, (-d[0], -d[1]))


def ik2(s, t, l1, l2, bend):
    """Two-bone IK. bend=+1/-1 picks the elbow side. Returns elbow, clamped end."""
    d = v_sub(t, s)
    dist = min(max(v_len(d), 1e-3), l1 + l2 - 0.05)
    a = math.atan2(d[1], d[0])
    c = (l1 * l1 + dist * dist - l2 * l2) / (2 * l1 * dist)
    c = max(-1.0, min(1.0, c))
    ang = a + bend * math.acos(c)
    elbow = (s[0] + math.cos(ang) * l1, s[1] + math.sin(ang) * l1)
    end = (s[0] + math.cos(a) * dist, s[1] + math.sin(a) * dist)
    return elbow, end


# ----------------------------------------------------------------------------- canvas
_YY, _XX = np.mgrid[0:WORK, 0:WORK].astype(np.float64)
_XX += 0.5
_YY += 0.5


def poly_mask(points):
    im = Image.new("L", (WORK, WORK), 0)
    ImageDraw.Draw(im).polygon([(round(x), round(y)) for x, y in points], fill=255)
    return np.array(im) > 0


def disc_mask(c, r):
    return (_XX - c[0]) ** 2 + (_YY - c[1]) ** 2 <= r * r


def shift(m, dx, dy):
    out = np.zeros_like(m)
    h, w = m.shape
    ys = slice(max(0, dy), h + min(0, dy))
    yd = slice(max(0, -dy), h + min(0, -dy))
    xs = slice(max(0, dx), w + min(0, dx))
    xd = slice(max(0, -dx), w + min(0, -dx))
    out[ys, xs] = m[yd, xd]
    return out


def outside_at(m, dx, dy):
    """True where the pixel at offset (dx,dy) is outside the mask."""
    return ~shift(m, -dx, -dy)


def shade_index(m, sw=2, hl=1):
    """0..4 ramp index per pixel: base 2, highlight on the lit rim, shadow band on the far side."""
    idx = np.full(m.shape, 2, np.int8)
    lx, ly = LIGHT
    for k in range(1, sw + 1):
        sh = outside_at(m, -lx * k, 0) | outside_at(m, 0, -ly * k) | outside_at(m, -lx * k, -ly * k)
        idx[sh & (idx == 2)] = 1
    if hl:
        hi = outside_at(m, lx, 0) | outside_at(m, 0, ly) | outside_at(m, lx, ly)
        idx[hi & m] = 3
        if hl > 1:
            hi2 = outside_at(m, lx, ly) & ~outside_at(m, -lx, -ly)
            idx[hi2 & m & (idx == 3) & ~outside_at(m, -lx * 2, -ly * 2)] = 4
    deep = outside_at(m, -lx, 0) & outside_at(m, 0, -ly)
    idx[deep & m] = 0
    return idx


class Canvas:
    def __init__(self):
        self.rgb = np.zeros((WORK, WORK, 3), np.uint8)
        self.ids = np.zeros((WORK, WORK), np.int16)
        self.groups = {0: -1}
        self.dark = {}
        self.n = 0

    def put(self, mask, colors, group=None, dark=None):
        """colors: HxWx3 array (only masked pixels are used)."""
        self.n += 1
        self.groups[self.n] = self.n if group is None else group
        self.dark[self.n] = dark
        self.rgb[mask] = colors[mask]
        self.ids[mask] = self.n

    def paint(self, mask, ramp, sw=2, hl=1, group=None, detail=None, line=True):
        idx = shade_index(mask, sw, hl)
        cols = np.array(ramp, np.uint8)[np.clip(idx, 0, 4)]
        if detail is not None:
            detail(cols, idx, mask)
        self.put(mask, cols, group, ramp[0] if line else None)

    def pixel(self, p, color):
        x, y = int(math.floor(p[0])), int(math.floor(p[1]))
        if 0 <= x < WORK and 0 <= y < WORK and self.ids[y, x] > 0:
            self.rgb[y, x] = color

    def finish(self):
        ids = self.ids
        opaque = ids > 0
        rgb = self.rgb.copy()
        # inner lines: front part pixels touching a part drawn earlier (behind) get its dark tone
        for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            nb = np.zeros_like(ids)
            h, w = ids.shape
            nb[max(0, -dy):h - max(0, dy), max(0, -dx):w - max(0, dx)] = \
                ids[max(0, dy):h - max(0, -dy), max(0, dx):w - max(0, -dx)]
            cand = opaque & (nb > 0) & (nb < ids)
            ys, xs = np.nonzero(cand)
            for y, x in zip(ys, xs):
                i, j = ids[y, x], nb[y, x]
                if self.groups[i] != self.groups[j] and self.dark.get(i) is not None:
                    d = self.dark[i]
                    rgb[y, x] = ((d[0] + OUT[0]) // 2, (d[1] + OUT[1]) // 2, (d[2] + OUT[2]) // 2)
        # outer 1px outline
        ring = np.zeros_like(opaque)
        for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            ring |= shift(opaque, dx, dy)
        ring &= ~opaque
        out = np.zeros((WORK, WORK, 4), np.uint8)
        out[opaque, :3] = rgb[opaque]
        out[opaque, 3] = 255
        out[ring, :3] = OUT
        out[ring, 3] = 255
        return out


# ----------------------------------------------------------------------------- limb geometry
def limb_poly(p0, p1, profile):
    """profile: list of (t, front_halfwidth, back_halfwidth)."""
    d = v_norm(v_sub(p1, p0))
    n = (d[1], -d[0])            # 'front' normal (downward limb -> +x)
    L = v_len(v_sub(p1, p0))
    front, back = [], []
    for t, fw, bw in profile:
        c = v_add(p0, v_mul(d, t * L))
        front.append(v_add(c, v_mul(n, fw)))
        back.append(v_sub(c, v_mul(n, bw)))
    return front + back[::-1]


def limb_mask(p0, p1, profile, caps=True):
    m = poly_mask(limb_poly(p0, p1, profile))
    if caps:
        r0 = (profile[0][1] + profile[0][2]) / 2
        r1 = (profile[-1][1] + profile[-1][2]) / 2
        m |= disc_mask(p0, r0 - 0.2) | disc_mask(p1, r1 - 0.2)
    return m


def along(p0, p1):
    """Returns t-along-bone array for every pixel (0 at p0, 1 at p1)."""
    d = v_sub(p1, p0)
    L2 = d[0] * d[0] + d[1] * d[1]
    return ((_XX - p0[0]) * d[0] + (_YY - p0[1]) * d[1]) / L2


THIGH = [(0, 5.0, 6.2), (0.35, 4.8, 5.4), (0.75, 4.0, 4.2), (1, 3.5, 3.6)]
SHIN = [(0, 3.4, 3.5), (0.22, 3.0, 4.6), (0.55, 2.4, 3.2), (0.9, 1.9, 2.0), (1, 1.9, 2.0)]
UPARM = [(0, 3.0, 3.0), (0.5, 2.5, 2.7), (1, 2.1, 2.1)]
FOREARM = [(0, 2.1, 2.1), (0.3, 2.4, 2.3), (1, 1.6, 1.6)]
L_THIGH, L_SHIN = 25.0, 24.0
L_UP, L_FORE = 16.0, 15.0
TORSO_LEN = 34.0


# ----------------------------------------------------------------------------- pose
def default_pose():
    return dict(
        t=0.0,          # torso lean (deg, + forward)
        h=0.0,          # head tilt relative to torso (deg, + chin down)
        breath=0.0,     # 0..1 chest rise
        legF=(2.0, 2.0, 0.0),   # near leg: thigh angle, knee bend, foot angle (deg from +x, + toe down)
        legB=(-3.0, 3.0, 0.0),  # far leg
        handF=None,     # near hand target (world offset from shoulder) or None
        handB=None,
        bendF=1, bendB=1,
        gun=62.0,       # barrel angle (deg from +x, + pointing down)
        gunOnF=True,    # gun in near hand; else lying free at gunPos
        gunPos=None,
        flash=False,
        skirt=0.0,      # extra skirt sway (deg)
        feather=0.0,    # feather sway (deg)
        eyes=1,         # 1 open, 0 closed
        handBonHip=True,
        lowerNeck=0.0,
    )


def build(p):
    cv = Canvas()
    hip = ROOT
    tor = frame_from_up(hip, p["t"])
    br = p["breath"]

    # joints
    hipF = tor.w(1.5, -1.0)
    hipB = tor.w(-1.0, -0.5)
    shoulder_v = 30.0 + br * 0.8
    shF = tor.w(-0.5, shoulder_v)
    shB = tor.w(-1.5, shoulder_v + 0.5)
    neck_base = tor.w(0.6, TORSO_LEN - 0.5 + br * 0.6)

    def leg(hj, spec):
        a1, k, fang = spec
        knee = v_add(hj, v_mul(dir_down(a1), L_THIGH))
        ankle = v_add(knee, v_mul(dir_down(a1 - k), L_SHIN))
        return knee, ankle, fang

    kneeF, ankF, footF = leg(hipF, p["legF"])
    kneeB, ankB, footB = leg(hipB, p["legB"])

    def arm(sh, tgt, bend, default):
        if tgt is None:
            tgt = default
        target = v_add(sh, tgt)
        if bend == 0:  # pick whichever elbow solution sits lower (resting on the ground)
            a = ik2(sh, target, L_UP, L_FORE, 1)
            b = ik2(sh, target, L_UP, L_FORE, -1)
            return a if a[0][1] >= b[0][1] else b
        # bend sign relative to the shoulder->target direction: elbows point back/down
        elbow, wrist = ik2(sh, target, L_UP, L_FORE, bend)
        return elbow, wrist

    elbF, wrF = arm(shF, p["handF"], p["bendF"], v_add(tor.f and (0, 0), (4.0, 29.0)))
    if p["handBonHip"] and p["handB"] is None:
        # far hand resting on the hip (elbow out behind)
        hand_hip = v_sub(tor.w(-4.5, 9.0), shB)
        elbB, wrB = arm(shB, hand_hip, p["bendB"], hand_hip)
    else:
        elbB, wrB = arm(shB, p["handB"], p["bendB"], (-2.0, 29.0))

    # ---------------------------------------------------------------- far arm (behind all)
    draw_arm(cv, shB, elbB, wrB, far=True)

    # ---------------------------------------------------------------- legs
    draw_leg(cv, hipB, kneeB, ankB, footB, far=True)
    draw_leg(cv, hipF, kneeF, ankF, footF, far=False)

    # ---------------------------------------------------------------- torso + neck + skirt
    draw_torso(cv, tor, br)
    thigh_axis = v_norm(v_add(v_sub(kneeF, hipF), v_sub(kneeB, hipB)))
    draw_skirt(cv, tor, thigh_axis, p["skirt"])

    # ---------------------------------------------------------------- head
    head_frame = frame_from_up((0, 0), p["t"] + p["h"])
    head_c = v_add(neck_base, v_add(v_mul(head_frame.u, 11.5 - p["lowerNeck"]), v_mul(head_frame.f, 0.8)))
    head = Frame(head_c, head_frame.f, head_frame.u)
    draw_neck(cv, neck_base, head, tor)
    draw_head(cv, head, p)

    # ---------------------------------------------------------------- near arm + gun
    draw_arm(cv, shF, elbF, wrF, far=False, hand=False)
    fore_dir = v_norm(v_sub(wrF, elbF))
    if p["gunOnF"]:
        gun = Frame(v_add(wrF, v_mul(fore_dir, 2.0)), dir_x(p["gun"]), None)
        gdir = dir_x(p["gun"])
        # gun local: x along barrel, y "up" = rotate barrel dir by -90 in screen space
        gun = Frame(v_add(wrF, v_mul(fore_dir, 2.0)), gdir, (gdir[1], -gdir[0]))
        draw_gun(cv, gun, p["flash"])
        draw_hand(cv, wrF, fore_dir, grip=True)
    else:
        draw_hand(cv, wrF, fore_dir, grip=False)
        if p["gunPos"] is not None:
            gp, ga = p["gunPos"]
            gdir = dir_x(ga)
            draw_gun(cv, Frame(v_add(ROOT, gp), gdir, (gdir[1], -gdir[0])), False, behind=True)

    # far hand when it supports the barrel should sit over the gun
    if p.get("farHandFront"):
        draw_hand(cv, wrB, v_norm(v_sub(wrB, elbB)), grip=True, far=True)
    return cv.finish()


# ----------------------------------------------------------------------------- parts
def draw_arm(cv, sh, elb, wr, far, hand=True):
    d = 1 if not far else 0
    up = limb_mask(sh, elb, UPARM)
    fo = limb_mask(elb, wr, FOREARM)
    t_up = along(sh, elb)
    glove_up = up & (t_up > 0.62)
    skin_up = up & ~glove_up
    skin_r = R_SKIN if not far else [tuple(max(0, c - 22) for c in col) for col in R_SKIN]
    blk_r = R_BLACK
    g = cv.n + 100
    cv.paint(skin_up, skin_r, sw=1, group=g)
    glove = glove_up | fo

    def glove_detail(cols, idx, m):
        # satin sheen streak along the forearm
        tf = along(elb, wr)
        sheen = m & (idx == 2) & ((np.abs(tf - 0.45) < 0.12) | (np.abs(t_up - 0.8) < 0.06))
        cols[sheen] = R_BLACK[3]
        # glove top cuff line
        cuff = m & (np.abs(t_up - 0.64) < 0.05)
        cols[cuff] = R_BLACK[0]

    cv.paint(glove, blk_r, sw=1, hl=1, detail=glove_detail)
    if hand:
        draw_hand(cv, wr, v_norm(v_sub(wr, elb)), grip=False, far=far)


def draw_hand(cv, wr, fdir, grip, far=False):
    n = (fdir[1], -fdir[0])
    L = 4.5 if not grip else 3.5
    tip = v_add(wr, v_mul(fdir, L))
    pts = [v_add(wr, v_mul(n, 2.0)), v_add(tip, v_mul(n, 1.8)), v_add(tip, v_mul(fdir, 1.0)),
           v_sub(tip, v_mul(n, 1.6)), v_sub(wr, v_mul(n, 1.9))]
    m = poly_mask(pts) | disc_mask(v_add(wr, v_mul(fdir, L * 0.55)), 2.3)
    cv.paint(m, R_BLACK, sw=1, hl=1)


def draw_leg(cv, hj, knee, ank, fang, far):
    th = limb_mask(hj, knee, THIGH)
    sh = limb_mask(knee, ank, SHIN)
    t_th = along(hj, knee)
    skin_part = th & (t_th < 0.52)
    garter = th & (t_th >= 0.52) & (t_th < 0.62)
    stock = (th & (t_th >= 0.62)) | sh
    dim = 18 if far else 0
    darker = lambda r: [tuple(max(0, c - dim) for c in col) for col in r]
    g = cv.n + 200
    cv.paint(skin_part, darker(R_SKIN), sw=2, group=g)

    def garter_detail(cols, idx, m):
        cols[m & (idx >= 3)] = R_BLACK[3]

    cv.paint(garter, darker(R_BLACK), sw=1, group=g, detail=garter_detail)

    def stock_detail(cols, idx, m):
        # back seam line
        d = v_norm(v_sub(ank, knee))
        n = (d[1], -d[0])
        # signed distance from shin axis toward 'back'
        sd = -((_XX - knee[0]) * n[0] + (_YY - knee[1]) * n[1])
        tt = along(knee, ank)
        seam = m & (np.abs(sd - 1.6) < 0.5) & (tt > 0.05) & (tt < 0.9)
        cols[seam] = darker(R_STOCK)[0]
        # sheen on the shin front
        sheen = m & (idx == 2) & (np.abs(sd + 1.3) < 0.55) & (tt > 0.15) & (tt < 0.7)
        cols[sheen] = darker(R_STOCK)[3]

    cv.paint(stock, darker(R_STOCK), sw=2, group=g, detail=stock_detail)
    # red bow on the garter (near leg only)
    if not far:
        c = v_add(hj, v_mul(v_sub(knee, hj), 0.57))
        d = v_norm(v_sub(knee, hj))
        n = (d[1], -d[0])
        bow = v_add(c, v_mul(n, 3.4))
        cv.pixel(bow, R_CRIM[3])
        cv.pixel(v_add(bow, v_mul(d, 1)), R_CRIM[2])
        cv.pixel(v_sub(bow, v_mul(d, 1)), R_CRIM[2])
    draw_boot(cv, ank, fang, far)


def draw_boot(cv, ank, fang, far):
    fd = dir_x(fang)
    fr = Frame(ank, fd, (fd[1], -fd[0]))  # y up
    pts = [(-2.6, 6.5), (2.4, 6.5), (2.8, 1.5), (5.5, -1.5), (8.4, -4.5), (8.6, -5.8), (6.5, -6.3),
           (3.2, -5.2), (1.2, -3.6), (-0.4, -3.4), (-1.1, -6.6), (-2.6, -6.6), (-2.9, -3.2), (-3.4, 1.0)]
    m = poly_mask(fr.pts(pts))
    dim = 14 if far else 0
    ramp = [tuple(max(0, c - dim) for c in col) for col in R_BLACK]

    def boot_detail(cols, idx, mm):
        X, Y = fr.local(_XX, _YY)
        cols[mm & (np.abs(X - 1.7) < 0.5) & (Y > -1.5) & (Y < 5.5) & ((np.floor(Y) % 2) == 0)] = R_GOLD[2]
        cols[mm & (Y > 5.3)] = R_CRIM[1]   # red boot top trim

    cv.paint(m, ramp, sw=1, hl=2, detail=boot_detail)


def torso_outline(br):
    b = br * 0.8
    return [(-6.6, -2.0), (-6.4, 4), (-5.2, 9), (-4.6, 13), (-5.0, 19), (-5.7, 25), (-5.4, 29.5),
            (-4.0, 32.5), (-2.4, 34.5), (1.8, 34.5), (2.6, 32.5), (3.4, 30), (5.2, 27 + b),
            (7.6 + b * 0.5, 24.6 + b), (8.5 + b * 0.5, 22 + b), (7.8, 19.6 + b * 0.5), (5.2, 17.6),
            (4.0, 13.5), (4.6, 8), (5.8, 3), (6.2, -2.0)]


def draw_torso(cv, tor, br):
    body = poly_mask(tor.pts(torso_outline(br)))
    b = br * 0.8
    cv.paint(body, R_SKIN, sw=2, hl=1, group=500)
    # corset over the body
    cor_pts = [(-6.8, -2.2), (-6.6, 4), (-5.4, 9), (-4.8, 13), (-5.2, 19), (-5.9, 25), (-5.8, 28.6),
               (-2.0, 28.0), (1.5, 26.8), (4.6, 25.0 + b), (6.8, 23.2 + b), (8.7 + b * 0.5, 22.3 + b),
               (8.0, 19.4 + b * 0.5), (5.4, 17.4), (4.2, 13.5), (4.8, 8), (6.0, 3), (6.4, -2.2)]
    cor = poly_mask(tor.pts(cor_pts)) & (body | poly_mask(tor.pts(cor_pts)))

    def cor_detail(cols, idx, m):
        U, V = tor.local(_XX, _YY)
        # boning seams
        for u0 in (-2.6, 1.0):
            cols[m & (np.abs(U - u0 - (V - 12) * 0.03) < 0.45) & (V > 1) & (V < 26) & (idx > 0)] = R_CRIM[1]
        # front busk with gold hooks
        front_edge = m & (U > 3.0) & (V > 3) & (V < 17)
        hooks = front_edge & ((np.floor(V) % 3) == 0) & (idx >= 1)
        cols[hooks] = R_GOLD[3]
        # black lace trim at the top (follows the neckline) and a bottom band
        top = m & ~shift_frame_mask(m, tor, 0, -2.2)
        cols[top] = R_BLACK[2]
        cols[top & ((np.floor(U * 1.3) % 2) == 0)] = R_BLACK[3]
        bottom = m & (V < 0.2)
        cols[bottom] = R_BLACK[2]
        # waist sheen
        sheen = m & (idx == 2) & (np.abs(U - 3.0) < 0.6) & (V > 17) & (V < 23)
        cols[sheen] = R_CRIM[3]

    cv.paint(cor, R_CRIM, sw=2, hl=2, group=500, detail=cor_detail)
    # thin black shoulder strap
    s0, s1 = tor.w(-3.6, 28.2), tor.w(-2.2, 34.0)
    strap = limb_mask(s0, s1, [(0, 0.7, 0.7), (1, 0.7, 0.7)], caps=False)
    cv.paint(strap & poly_mask(tor.pts(torso_outline(br))), R_BLACK, sw=0, hl=0, group=500)
    # cleavage line / collarbone
    cv.pixel(tor.w(6.3, 25.4 + b), R_SKIN[1])
    cv.pixel(tor.w(5.8, 26.3 + b), R_SKIN[1])
    cv.pixel(tor.w(0.5, 32.3), R_SKIN[1])
    cv.pixel(tor.w(1.5, 32.0), R_SKIN[1])


def shift_frame_mask(m, fr, du, dv):
    """Mask shifted by a local offset (rounded to whole pixels in world space)."""
    dx = fr.f[0] * du + fr.u[0] * dv
    dy = fr.f[1] * du + fr.u[1] * dv
    return shift(m, int(round(dx)), int(round(dy)))


def draw_skirt(cv, tor, axis, sway):
    # skirt hangs from the waist band; its axis follows the thighs (so it lies along the legs)
    waist = tor.w(-0.3, 2.0)
    ax = v_norm(v_add(v_mul(axis, 0.8), v_mul(v_mul(tor.u, -1), 0.2)))
    if sway:
        ax = v_norm(v_add(ax, v_mul((ax[1], -ax[0]), math.tan(math.radians(sway)))))
    sk = frame_from_axis(waist, ax)  # y up (towards waist), x forward
    # lying down: the flare collapses flat instead of standing up off the legs
    flat = max(0.0, min(1.0, -sk.f[1] * 1.3 - 0.2))
    squash = lambda pts: [(x * (1 - (0.55 if x > 0 else 0.45) * flat), y) for x, y in pts]

    def scallop(y_bottom, x0, x1, n, depth):
        pts = []
        for i in range(n * 4 + 1):
            u = i / (n * 4)
            x = x1 + (x0 - x1) * u
            y = y_bottom - depth * abs(math.sin(math.pi * u * n))
            pts.append((x, y))
        return pts

    # petticoat (black ruffles with cream lace edge), a bit longer and wider
    pet = [(-7.4, 2.5), (6.6, 2.5)] + [(10.0, -8)] + scallop(-14.5, -13.6, 12.0, 5, 1.8) + [(-11.5, -8)]
    pm = poly_mask(sk.pts(squash(pet)))

    def pet_detail(cols, idx, m):
        X, Y = sk.local(_XX, _YY)
        edge = m & ~shift_frame_mask(m, sk, 0, -1.6)
        cols[edge] = R_LACE[2]
        cols[edge & (idx <= 1)] = R_LACE[1]
        cols[edge & (idx >= 3)] = R_LACE[3]
        mid = m & (np.abs(Y + 12.0 + 0.8 * np.sin(X * 1.3)) < 0.5)
        cols[mid] = R_BLACK[3]

    cv.paint(pm, R_BLACK, sw=2, hl=1, group=600, detail=pet_detail)
    # outer crimson skirt, open towards the front so the ruffles show
    outer = [(-7.0, 2.8), (6.2, 2.8), (7.0, -4)] + scallop(-11.8, -12.8, 8.2, 4, 1.6) + [(-10.6, -6)]
    om = poly_mask(sk.pts(squash(outer)))

    def out_detail(cols, idx, m):
        X, Y = sk.local(_XX, _YY)
        # vertical folds
        for x0 in (-8.0, -4.0, 0.5, 4.5):
            fold = m & (np.abs(X - x0 - (Y + 6) * (x0 / 30.0)) < 0.5) & (Y < -3.5)
            cols[fold & (idx >= 2)] = R_CRIM[1]
            cols[shift(fold, 1, 0) & m & (idx >= 2)] = R_CRIM[3]
        band = m & (Y > 0.8)
        cols[band] = R_BLACK[2]
        cols[band & (np.abs(X - 3.4) < 0.7)] = R_GOLD[3]
        hem = m & ~shift_frame_mask(m, sk, 0, -1.3)
        cols[hem] = R_CRIM[0]

    cv.paint(om, R_CRIM, sw=2, hl=1, group=601, detail=out_detail)


def draw_neck(cv, base, head, tor):
    top = head.w(-0.6, -4.5)
    m = limb_mask(base, top, [(0, 2.8, 2.8), (1, 2.3, 2.5)])
    cv.paint(m, R_SKIN, sw=1, hl=1, group=700)
    # choker with a red gem
    c0 = v_add(base, v_mul(v_sub(top, base), 0.45))
    d = v_norm(v_sub(top, base))
    n = (d[1], -d[0])
    band = m & (np.abs(along(base, top) - 0.45) < 0.13)
    cols = np.zeros((WORK, WORK, 3), np.uint8)
    cols[:] = R_BLACK[1]
    cols[band & (_XX - c0[0] > 0.5)] = R_BLACK[2]
    cv.put(band, cols, group=700)
    cv.pixel(v_add(c0, v_mul(n, 1.8)), R_CRIM[4])
    cv.pixel(v_add(c0, v_add(v_mul(n, 1.8), v_mul(d, -1))), R_GOLD[2])


def draw_head(cv, hd, p):
    # hair behind the head: long curl hanging at the nape + bun
    curl = [(-4.2, -1.0), (-6.8, -3.5), (-7.6, -7.5), (-6.8, -11.2), (-5.2, -12.4), (-4.4, -10.6),
            (-5.3, -8.6), (-5.0, -5.8), (-3.6, -3.8)]
    back_hair = poly_mask(hd.pts(curl)) | disc_mask(hd.w(-4.4, 8.2), 4.6) | disc_mask(hd.w(-6.5, 4.8), 3.2)

    def hair_detail(cols, idx, m):
        X, Y = hd.local(_XX, _YY)
        strand = m & (idx >= 2) & (np.abs(np.sin(X * 1.1 + Y * 0.9)) < 0.22)
        cols[strand] = R_HAIR[1]

    cv.paint(back_hair, R_HAIR, sw=2, hl=2, group=800, detail=hair_detail)

    # feather: a curved plume leaning back from the bun
    fsw = p["feather"]
    fdir = dir_down(180 - 24 - fsw)  # mostly upward, leaning back
    fb = hd.w(-2.8, 10.5)
    fb_frame = Frame(fb, (hd.f[0], hd.f[1]), (hd.u[0], hd.u[1]))
    pl = []
    sp = []
    for i in range(9):
        t = i / 8
        # local: goes up and bends back
        x = -2.0 * t - 5.5 * t * t - fsw * 0.08 * t * t
        y = 5.2 * t
        w = 1.4 + 2.4 * math.sin(math.pi * min(1, t * 1.15))
        pl.append((x - w * 0.8, y + w * 0.35))
        sp.append((x + w * 0.9, y - w * 0.2))
    fm = poly_mask(fb_frame.pts(pl + sp[::-1]))

    def feather_detail(cols, idx, m):
        X, Y = fb_frame.local(_XX, _YY)
        quill = m & (np.abs(X - (-2.0 * (Y / 5.2) - 5.5 * (Y / 5.2) ** 2)) < 0.5)
        cols[quill] = R_FEATH[0]
        barbs = m & (np.abs(np.sin(Y * 1.6 - X * 0.8)) < 0.25) & (idx >= 2)
        cols[barbs] = R_FEATH[1]
        cols[m & (Y > 4.3)] = R_BLACK[2]

    cv.paint(fm, R_FEATH, sw=1, hl=1, group=801, detail=feather_detail)

    # face / skull
    face = [(0.0, 7.4), (2.4, 6.9), (3.9, 5.2), (4.6, 3.2), (4.7, 1.2), (4.9, 0.2), (6.1, -1.9),
            (5.3, -2.6), (5.3, -3.3), (4.9, -3.7), (5.3, -4.3), (4.8, -4.9), (4.9, -6.0), (4.1, -7.2),
            (2.6, -7.8), (0.4, -7.0), (-2.2, -5.2), (-4.4, -3.4), (-5.9, -0.6), (-6.2, 2.6),
            (-5.2, 5.4), (-3.0, 7.0)]
    fm = poly_mask(hd.pts(face))
    cv.paint(fm, R_SKIN, sw=2, hl=1, group=810)

    # features
    eye_c = hd.w(2.9, 0.6)
    if p["eyes"]:
        cv.pixel(hd.w(3.4, 0.4), EYE)
        cv.pixel(hd.w(2.5, 0.4), EYE_WHITE)
        cv.pixel(hd.w(3.4, 1.4), EYE)      # upper lash line
        cv.pixel(hd.w(2.4, 1.4), EYE)
        cv.pixel(hd.w(4.3, 1.9), EYE)      # flicked lash
        cv.pixel(hd.w(1.5, 1.4), SHADOW_LID)
        cv.pixel(hd.w(2.5, 2.4), SHADOW_LID)
    else:
        cv.pixel(hd.w(2.4, 0.7), EYE)
        cv.pixel(hd.w(3.4, 0.4), EYE)
        cv.pixel(hd.w(4.2, 0.6), EYE)
        cv.pixel(hd.w(2.5, 1.7), SHADOW_LID)
    cv.pixel(hd.w(2.2, 3.3), R_HAIR[0])    # brow
    cv.pixel(hd.w(3.3, 3.4), R_HAIR[0])
    cv.pixel(hd.w(4.1, 3.0), R_HAIR[1])
    cv.pixel(hd.w(4.6, -3.9), LIPS)
    cv.pixel(hd.w(4.6, -4.5), LIPS_HI)
    cv.pixel(hd.w(3.7, -4.1), LIPS)
    cv.pixel(hd.w(1.6, -2.0), BLUSH)
    cv.pixel(hd.w(2.6, -2.2), BLUSH)
    cv.pixel(hd.w(5.2, -2.3), R_SKIN[1])   # nostril
    # beauty mark
    cv.pixel(hd.w(3.7, -2.9), R_HAIR[0])

    # front hair: swept wave over the forehead + side covering the ear
    front = [(4.5, 5.2), (3.3, 7.4), (0.8, 9.2), (-2.6, 9.6), (-5.6, 8.0), (-6.8, 4.8), (-6.7, 1.0),
             (-5.6, -2.8), (-3.6, -4.2), (-2.4, -2.8), (-2.2, 0.0), (-0.6, 2.2), (1.4, 3.8),
             (2.8, 4.0), (3.6, 3.2), (4.4, 3.6)]
    hm = poly_mask(hd.pts(front)) | disc_mask(hd.w(3.4, 5.6), 1.9)

    def front_detail(cols, idx, m):
        X, Y = hd.local(_XX, _YY)
        wave = m & (idx >= 2) & (np.abs(np.sin(X * 0.9 - Y * 1.2)) < 0.2)
        cols[wave] = R_HAIR[1]
        hl_band = m & (idx == 2) & (np.abs(np.sin(X * 0.9 - Y * 1.2) - 0.8) < 0.15)
        cols[hl_band] = R_HAIR[3]

    cv.paint(hm, R_HAIR, sw=2, hl=2, group=820, detail=front_detail)
    # gold drop earring below the hairline
    cv.pixel(hd.w(-1.8, -4.6), R_GOLD[3])
    cv.pixel(hd.w(-1.8, -5.6), R_GOLD[2])
    cv.pixel(hd.w(-1.8, -6.6), R_GOLD[4])
    # hair pin (gold) near the feather
    cv.pixel(hd.w(-1.8, 9.3), R_GOLD[3])


def draw_gun(cv, g, flash, behind=False):
    """Sawed-off double barrel. Local: x along barrel (muzzle +x), y up. Origin at the grip."""
    # pistol-grip stock (wood)
    stock = [(-0.8, 2.2), (-2.4, 2.5), (-6.5, 1.5), (-8.8, -2.4), (-8.4, -4.2), (-6.4, -4.0),
             (-3.6, -1.2), (-0.6, -0.8)]
    sm = poly_mask(g.pts(stock))

    def wood_detail(cols, idx, m):
        X, Y = g.local(_XX, _YY)
        grain = m & (idx >= 2) & (np.abs(np.sin(X * 0.8 + Y * 2.2)) < 0.2)
        cols[grain] = R_WOOD[1]

    cv.paint(sm, R_WOOD, sw=1, hl=1, detail=wood_detail)
    # receiver
    rec = [(-1.2, 2.6), (4.2, 2.6), (4.2, -1.2), (2.0, -1.6), (-1.2, -0.6)]
    rm = poly_mask(g.pts(rec))

    def rec_detail(cols, idx, m):
        X, Y = g.local(_XX, _YY)
        cols[m & (np.abs(X - 1.4) < 0.5) & (np.abs(Y - 0.8) < 0.6)] = R_GOLD[2]   # engraving
    cv.paint(rm, R_STEEL, sw=1, hl=2, detail=rec_detail)
    # trigger guard
    tg = [(0.2, -1.0), (2.8, -1.0), (2.4, -3.0), (0.8, -3.0)]
    tm = poly_mask(g.pts(tg)) & ~poly_mask(g.pts([(0.9, -1.4), (2.2, -1.4), (1.9, -2.4), (1.2, -2.4)]))
    cv.paint(tm, R_STEEL, sw=0, hl=0)
    # forend (wood) under the barrels
    fe = [(4.0, 0.2), (11.0, 0.2), (11.4, -1.4), (4.0, -1.8)]
    cv.paint(poly_mask(g.pts(fe)), R_WOOD, sw=1, hl=1, detail=wood_detail)
    # twin barrels
    bar = [(4.0, 2.8), (17.0, 2.8), (17.0, -0.4), (4.0, -0.4)]
    bm = poly_mask(g.pts(bar))

    def bar_detail(cols, idx, m):
        X, Y = g.local(_XX, _YY)
        cols[m & (np.abs(Y - 1.2) < 0.45)] = R_STEEL[0]   # rib between the barrels
        cols[m & (X > 16.2)] = R_STEEL[1]                  # muzzle ring
    cv.paint(bm, R_STEEL, sw=1, hl=2, detail=bar_detail)
    # hammers
    cv.pixel(g.w(-0.4, 3.1), R_STEEL[3])
    cv.pixel(g.w(0.4, 3.3), R_STEEL[2])

    if flash:
        fl0 = [(17.2, 4.2), (21.0, 5.6), (19.6, 2.6), (25.5, 1.0), (19.6, -0.6), (21.0, -3.2), (17.2, -1.8)]
        fl1 = [(17.2, 3.0), (19.8, 3.4), (22.0, 1.1), (19.8, -1.0), (17.2, -0.8)]
        fl2 = [(17.2, 2.1), (19.4, 1.1), (17.2, 0.1)]
        for pts, col in ((fl0, FLASH[2]), (fl1, FLASH[1]), (fl2, FLASH[0])):
            m = poly_mask(g.pts(pts))
            cols = np.zeros((WORK, WORK, 3), np.uint8)
            cols[:] = col
            cv.put(m, cols, group=950, dark=None)


# ----------------------------------------------------------------------------- animations
def pose(**kw):
    p = default_pose()
    p.update(kw)
    return p


IDLE_GUN_HAND = (5.0, 28.0)


def idle_frames():
    out = []
    for i in range(FRAMES):
        ph = 2 * math.pi * i / FRAMES
        b = (1 - math.cos(ph)) / 2
        out.append(pose(
            breath=b,
            h=1.5 * math.sin(ph + 0.6),
            t=0.6 * math.sin(ph),
            handF=(IDLE_GUN_HAND[0] + 0.4 * math.sin(ph), IDLE_GUN_HAND[1] - 0.9 * b),
            gun=62 + 2.5 * math.sin(ph),
            skirt=1.2 * math.sin(ph + 1.0),
            feather=4 * math.sin(ph + 1.4),
            legF=(3.0, 2.0, 0.0), legB=(-4.0, 3.0, 0.0),
        ))
    return out


def walk_frames():
    thigh = [22, 13, 3, -9, -18, -10, 10, 24]
    knee = [4, 14, 6, 6, 24, 48, 58, 26]
    foot = [-4, 0, 0, 4, 32, 30, 14, 0]
    out = []
    for i in range(FRAMES):
        j = (i + 4) % FRAMES
        ph = 2 * math.pi * i / FRAMES
        legF = (thigh[i], knee[i], foot[i])
        legB = (thigh[j], knee[j], foot[j])
        # far arm swings opposite to the far leg (i.e. with the near leg)
        sw = -thigh[j] * 0.45
        fa = dir_down(sw)
        handB = (fa[0] * 26 + 1, fa[1] * 26)
        swF = -thigh[i] * 0.18
        out.append(pose(
            t=4 + 1.0 * math.sin(2 * ph),
            h=-2,
            breath=0.3,
            legF=legF, legB=legB,
            handF=(IDLE_GUN_HAND[0] + 2 + swF, IDLE_GUN_HAND[1] - 1),
            gun=58 + 3 * math.sin(ph),
            handB=handB, handBonHip=False, bendB=1,
            skirt=3 * math.sin(ph),
            feather=6 * math.sin(2 * ph + 1),
        ))
    return out


def shoot_frames():
    base = dict(legF=(6.0, 3.0, 0.0), legB=(-8.0, 4.0, 0.0), breath=0.4)
    aim = (21.0, 3.0)        # near hand relative to shoulder when aiming (x fwd, y down)
    # (handF, gun angle, torso lean, head tilt, far-hand support?, flash)
    keys = [
        (IDLE_GUN_HAND, 62, 0, 0, False, False),
        ((12.0, 18.0), 34, 2, 2, False, False),
        (aim, 0, 3, 4, True, False),
        ((aim[0] - 1, aim[1]), -2, 2, 4, True, True),
        ((aim[0] - 4, aim[1] - 3), -26, -5, -3, True, False),
        ((aim[0] - 2, aim[1] - 1), -9, -1, 2, True, False),
        ((13.0, 16.0), 30, 1, 2, False, False),
        ((7.0, 25.0), 55, 0, 1, False, False),
    ]
    out = []
    for hand, gang, lean, head, support, flash in keys:
        p = pose(t=lean, h=head, handF=hand, gun=gang, flash=flash, **base)
        if support:
            # far hand under the forend: world point of gun-local (8.5, -1.0) relative to far shoulder
            p["support"] = True
        out.append(p)
    return out


def death_frames():
    return [
        pose(handF=IDLE_GUN_HAND, gun=64, legF=(3, 2, 0), legB=(-4, 3, 0)),
        # 2: hit — head & torso thrown back, knees bend, arms fling
        pose(t=-16, h=-16, breath=1.0, legF=(18, 30, 0), legB=(-2, 20, 8),
             handF=(-14.0, 18.0), bendF=1, gun=110, handB=(18.0, 12.0), handBonHip=False, bendB=-1,
             skirt=-6, feather=-14),
        # 3: slump forward, head down, deep crouch
        pose(t=34, h=30, legF=(62, 96, 4), legB=(40, 100, 24),
             handF=(8.0, 25.0), gun=78, handB=(4.0, 25.0), handBonHip=False,
             skirt=6, feather=12, eyes=0),
        # 4: down on one knee, gun near the ground
        pose(t=42, h=34, legF=(84, 86, 0), legB=(14, 104, 62),
             handF=(9.0, 25.0), gun=30, handB=(7.0, 24.0), handBonHip=False,
             skirt=8, feather=18, eyes=0),
        # 5: toppling backwards, propped on the far arm, torso nearly horizontal
        pose(t=-62, h=6, legF=(130, 104, -10), legB=(120, 110, 4),
             handF=(10.0, 8.0), bendF=0, gun=10, handB=(-2.0, 22.0), handBonHip=False, bendB=-1,
             skirt=0, feather=10, eyes=0),
        # 6: torso keeps sinking, legs stretch to the right
        pose(t=-80, h=-8, legF=(126, 86, -22), legB=(120, 80, -16),
             handF=(12.0, 4.0), bendF=0, gun=-6, handB=(-4.0, 16.0), handBonHip=False, bendB=0,
             skirt=0, feather=24, eyes=0),
        # 7: lying on her back — head left, legs right (near knee still raised)
        pose(t=-84, h=-20, legF=(128, 82, -20), legB=(120, 76, -18),
             handF=(13.0, -2.0), bendF=0, gunOnF=False, gunPos=((20.0, 3.0), 176),
             handB=(-2.0, 13.0), handBonHip=False, bendB=0, skirt=0, feather=34, eyes=0),
        # 8: final hold — small changes (head lolls, knee relaxes, hand slips)
        pose(t=-85, h=-26, legF=(125, 78, -24), legB=(118, 72, -14),
             handF=(15.0, 0.0), bendF=0, gunOnF=False, gunPos=((20.0, 3.0), 176),
             handB=(-3.0, 14.0), handBonHip=False, bendB=0, skirt=1, feather=38, eyes=0),
    ]


# ----------------------------------------------------------------------------- far-hand support
def resolve_support(p):
    """For two-handed aiming: aim the far hand at the forend of the gun."""
    if not p.get("support"):
        return p
    tor = frame_from_up(ROOT, p["t"])
    shF = tor.w(-0.5, 30.0 + p["breath"] * 0.8)
    shB = tor.w(-1.5, 30.5 + p["breath"] * 0.8)
    elbF, wrF = ik2(shF, v_add(shF, p["handF"]), L_UP, L_FORE, p["bendF"])
    fore_dir = v_norm(v_sub(wrF, elbF))
    gdir = dir_x(p["gun"])
    g = Frame(v_add(wrF, v_mul(fore_dir, 2.0)), gdir, (gdir[1], -gdir[0]))
    target = g.w(8.0, -1.4)
    p = dict(p)
    p["handB"] = v_sub(target, shB)
    p["handBonHip"] = False
    p["bendB"] = 1
    p["farHandFront"] = False
    return p


# ----------------------------------------------------------------------------- export
def render_sheet(poses, center_mode):
    """center_mode: 'bbox' centres every frame's bbox on x=72; 'root' keeps the hip x of frame 0."""
    imgs = [build(resolve_support(p)) for p in poses]
    sheet = np.zeros((CELL, CELL * FRAMES, 4), np.uint8)
    ref_dx = None
    for i, im in enumerate(imgs):
        a = im[:, :, 3] > 0
        ys, xs = np.nonzero(a)
        y1 = ys.max()
        x0, x1 = xs.min(), xs.max()
        dy = GROUND_Y - y1
        if center_mode == "bbox":
            dx = int(round(72 - (x0 + x1 + 1) / 2))
        else:
            if ref_dx is None:
                ref_dx = int(round(72 - (x0 + x1 + 1) / 2))
            dx = ref_dx
        cell = np.zeros((CELL, CELL, 4), np.uint8)
        for y, x in zip(ys, xs):
            cy, cx = y + dy, x + dx
            if not (0 <= cy < CELL and 0 <= cx < CELL):
                raise RuntimeError(f"frame {i} clipped at ({cx},{cy})")
            cell[cy, cx] = im[y, x]
        sheet[:, i * CELL:(i + 1) * CELL] = cell
    return sheet


def report(name, sheet):
    print(name)
    for i in range(FRAMES):
        a = sheet[:, i * CELL:(i + 1) * CELL, 3] > 0
        ys, xs = np.nonzero(a)
        print(f"  f{i + 1}: x {xs.min()}-{xs.max()}  y {ys.min()}-{ys.max()}  "
              f"size {xs.max() - xs.min() + 1}x{ys.max() - ys.min() + 1}")


def main():
    out_dir = HERE
    sheets = {
        "Idle": render_sheet(idle_frames(), "root"),
        "Walk": render_sheet(walk_frames(), "root"),
        "Shoot": render_sheet(shoot_frames(), "root"),
        "Death": render_sheet(death_frames(), "bbox"),
    }
    for k, s in sheets.items():
        report(k, s)
        Image.fromarray(s, "RGBA").save(os.path.join(out_dir, f"{NAME}_{k}.png"))
    if "--preview" in sys.argv:
        prev = Image.new("RGBA", (CELL * FRAMES, CELL * 4), (96, 88, 110, 255))
        for r, k in enumerate(["Idle", "Walk", "Shoot", "Death"]):
            prev.alpha_composite(Image.fromarray(sheets[k], "RGBA"), (0, r * CELL))
        d = ImageDraw.Draw(prev)
        for i in range(1, FRAMES):
            d.line([(i * CELL, 0), (i * CELL, CELL * 4)], fill=(70, 64, 84, 255))
        dest = sys.argv[sys.argv.index("--preview") + 1] if len(sys.argv) > sys.argv.index("--preview") + 1 \
            else os.path.join(out_dir, "preview.png")
        prev.resize((prev.width * 2, prev.height * 2), Image.NEAREST).save(dest)


if __name__ == "__main__":
    main()
