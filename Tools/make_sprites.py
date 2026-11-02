# Рисует спрайты для FlappyClone (запуск: python Tools/make_sprites.py Assets/Sprites):
# птица 64x64, труба 128x512, земля 512x128, фон 1080x1920; фон и земля бесшовные по горизонтали.
import math
import random
import sys
from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter

out = Path(sys.argv[1])
SS = 4


def lerp(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(len(a)))


# ---------- птица 64x64 ----------
def bird():
    s = 64 * SS
    im = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    u = SS
    # тело
    d.ellipse([6 * u, 10 * u, 54 * u, 56 * u], fill=(255, 107, 107, 255), outline=(120, 30, 50, 255), width=3 * u)
    # брюшко
    d.ellipse([14 * u, 32 * u, 44 * u, 54 * u], fill=(255, 196, 170, 255))
    # крыло
    d.ellipse([6 * u, 28 * u, 30 * u, 44 * u], fill=(214, 64, 92, 255), outline=(120, 30, 50, 255), width=2 * u)
    # глаз
    d.ellipse([32 * u, 16 * u, 50 * u, 34 * u], fill=(255, 255, 255, 255), outline=(120, 30, 50, 255), width=2 * u)
    d.ellipse([40 * u, 21 * u, 47 * u, 29 * u], fill=(30, 24, 48, 255))
    # клюв
    d.polygon([(48 * u, 34 * u), (62 * u, 38 * u), (48 * u, 43 * u)], fill=(255, 183, 3, 255), outline=(150, 80, 0, 255))
    return im.resize((64, 64), Image.LANCZOS)


# ---------- труба 128x512 (шапка сверху, тело тянется вниз) ----------
def pipe():
    w, h = 128, 512
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    px = im.load()
    body_l, body_r = 12, 116
    cap_h = 56
    base, light, dark, edge = (64, 145, 230), (150, 205, 255), (32, 82, 160), (20, 45, 95)
    for x in range(w):
        for y in range(h):
            in_cap = y < cap_h
            l, r = (0, w) if in_cap else (body_l, body_r)
            if not (l <= x < r):
                continue
            t = (x - l) / (r - l - 1)
            # горизонтальное освещение: блик слева, тень справа
            if t < 0.25:
                c = lerp(light, base, t / 0.25)
            elif t < 0.8:
                c = base
            else:
                c = lerp(base, dark, (t - 0.8) / 0.2)
            if x in (l, l + 1, r - 1, r - 2) or (in_cap and y in (0, 1, cap_h - 1, cap_h - 2)):
                c = edge
            px[x, y] = c + (255,)
    return im


# ---------- земля 512x128, бесшовная ----------
def ground():
    w, h = 512, 128
    im = Image.new("RGBA", (w, h), (0, 0, 0, 255))
    d = ImageDraw.Draw(im)
    d.rectangle([0, 0, w, h], fill=(120, 88, 70, 255))
    # верхняя кромка из песка
    d.rectangle([0, 0, w, 22], fill=(233, 196, 106, 255))
    d.rectangle([0, 22, w, 27], fill=(180, 130, 70, 255))
    # диагональные полосы с периодом, делящим 512
    for x0 in range(-64, w + 64, 32):
        d.polygon([(x0, 27), (x0 + 14, 27), (x0 + 14 - 100, h), (x0 - 100, h)], fill=(104, 76, 60, 255))
    # камешки с повтором по краям
    rnd = random.Random(7)
    for _ in range(26):
        x, y, r = rnd.randrange(w), rnd.randrange(40, 118), rnd.randrange(3, 7)
        for dx in (-w, 0, w):
            d.ellipse([x + dx - r, y - r, x + dx + r, y + r], fill=(150, 118, 96, 255))
    return im


# ---------- фон 1080x1920: сумеречное небо, звёзды, холмы; бесшовный ----------
def background():
    w, h = 1080, 1920
    im = Image.new("RGBA", (w, h))
    px = im.load()
    stops = [(0.0, (34, 36, 82)), (0.55, (98, 70, 140)), (0.8, (226, 120, 120)), (1.0, (248, 174, 120))]
    for y in range(h):
        t = y / (h - 1)
        for i in range(len(stops) - 1):
            if stops[i][0] <= t <= stops[i + 1][0]:
                k = (t - stops[i][0]) / (stops[i + 1][0] - stops[i][0])
                c = lerp(stops[i][1], stops[i + 1][1], k)
                break
        for x in range(w):
            px[x, y] = c + (255,)
    # звёзд и луны на фоне нет: это отдельные неподвижные спрайты (самые дальние слои)

    # облака (с повтором через край)
    clouds = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    cd = ImageDraw.Draw(clouds)
    for cx, cy, sc in ((180, 1050, 1.0), (640, 900, 0.8), (980, 1180, 1.2)):
        for dx in (-w, 0, w):
            for ox, oy, r in ((0, 0, 60), (70, -25, 70), (150, 0, 55), (75, 20, 60)):
                rr = int(r * sc)
                x, y = cx + dx + int(ox * sc), cy + int(oy * sc)
                cd.ellipse([x - rr, y - rr, x + rr, y + rr], fill=(255, 220, 230, 90))
    clouds = clouds.filter(ImageFilter.GaussianBlur(6))
    im = Image.alpha_composite(im, clouds)

    # два слоя холмов; периоды кратны ширине, поэтому края стыкуются
    d = ImageDraw.Draw(im)
    for base_y, amp, k1, k2, col in ((1500, 90, 2, 5, (86, 62, 120)), (1640, 70, 3, 7, (58, 44, 92))):
        pts = [(0, h)]
        for x in range(0, w + 1, 4):
            y = base_y - amp * (0.6 * math.sin(2 * math.pi * k1 * x / w) + 0.4 * math.sin(2 * math.pi * k2 * x / w + 1.3))
            pts.append((x, y))
        pts.append((w, h))
        d.polygon(pts, fill=col + (255,))
    return im


# ---------- звёзды 1440x1000, прозрачный фон ----------
# Неподвижный слой шириной 14,4 ед. (с запасом для широких экранов) у верхнего края:
# объект Stars стоит в (0; 4,6), объект Moon — в (2,9; 6,6), т. е. в пикселе (1010; 300)
# этого спрайта. Вокруг луны звёзд нет, иначе они просвечивали бы сквозь её тёмную часть.
def stars():
    w, h = 1440, 1000
    moon_x, moon_y, moon_r = 1010, 300, 95
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    rnd = random.Random(3)
    placed = 0
    while placed < 120:
        x, y = rnd.randrange(w), rnd.randrange(h)
        if (x - moon_x) ** 2 + (y - moon_y) ** 2 < moon_r ** 2:
            continue
        r = rnd.choice([1, 1, 2, 2, 3])
        # ниже звёзды тусклее: небо там светлее
        a = int(rnd.randrange(160, 256) * (1 - 0.4 * y / h))
        d.ellipse([x - r, y - r, x + r, y + r], fill=(255, 245, 220, a))
        placed += 1
    return im


# ---------- луна 256x256: серп с мягким свечением, прозрачный фон ----------
def moon():
    s = 256
    c = s // 2
    # серп: диск минус смещённый диск
    mask = Image.new("L", (s * SS, s * SS), 0)
    md = ImageDraw.Draw(mask)
    md.ellipse([(c - 70) * SS, (c - 70) * SS, (c + 70) * SS, (c + 70) * SS], fill=255)
    md.ellipse([(c - 30) * SS, (c - 85) * SS, (c + 100) * SS, (c + 45) * SS], fill=0)
    mask = mask.resize((s, s), Image.LANCZOS)
    # свечение — размытая копия самого серпа, поэтому «тёмная» часть диска не подсвечивается
    glow_alpha = mask.filter(ImageFilter.MaxFilter(9)).filter(ImageFilter.GaussianBlur(12))
    glow = Image.new("RGBA", (s, s), (255, 236, 200, 255))
    glow.putalpha(glow_alpha.point(lambda v: v * 45 // 100))
    crescent = Image.new("RGBA", (s, s), (255, 236, 200, 255))
    crescent.putalpha(mask)
    return Image.alpha_composite(glow, crescent)


bird().save(out / "Bird.png")
moon().save(out / "Moon.png")
stars().save(out / "Stars.png")
pipe().save(out / "Pipe.png")
ground().save(out / "Ground.png")
background().convert("RGB").save(out / "Background.png")
print("ok")
