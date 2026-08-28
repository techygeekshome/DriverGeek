#!/usr/bin/env python3
"""TechyGeeksHome app-icon standard - DriverGeek and CleanGeek.

Copied from Branding/App Logos/gen.py, which was itself copied from
GitHub/AppGeek/tools/make-icon.py. Everything above the glyph section is
byte-for-byte the house standard: gradient, gloss, corner radius, extent,
supersampling and the export sizes. Only draw_glyph() differs per product,
which is the whole point of the pattern.

Usage:  python make-icon.py ./out
"""
import os, math, sys
from PIL import Image, ImageDraw, ImageFilter

GRADIENT_TOP=(0x6B,0xA3,0xF7); GRADIENT_BOTTOM=(0x25,0x63,0xEB); GLYPH=(0xFF,0xFF,0xFF)
CORNER=0.22; GLOSS=0.12; EXTENT=0.60; SUPER=8; MASTER=1024
SIZES=[1024,512,256,128,96,64,48,32,16]
ICO_SIZES=[256,128,64,48,32,16]

def badge(size):
    grad=Image.new("RGB",(1,size))
    for y in range(size):
        t=y/max(1,size-1)
        grad.putpixel((0,y),tuple(round(GRADIENT_TOP[i]+(GRADIENT_BOTTOM[i]-GRADIENT_TOP[i])*t) for i in range(3)))
    grad=grad.resize((size,size),Image.NEAREST)
    g=Image.new("L",(size,size),0)
    ImageDraw.Draw(g).ellipse([-size*0.35,-size*0.62,size*1.35,size*0.52],fill=int(255*GLOSS))
    g=g.filter(ImageFilter.GaussianBlur(size*0.05))
    b=grad.convert("RGBA"); b.paste(Image.new("RGBA",(size,size),(255,255,255,255)),(0,0),g)
    m=Image.new("L",(size,size),0)
    ImageDraw.Draw(m).rounded_rectangle([0,0,size-1,size-1],radius=int(size*CORNER),fill=255)
    out=Image.new("RGBA",(size,size),(0,0,0,0)); out.paste(b,(0,0),m); return out

def _rot(px,py,cx,cy,ang):
    s,c=math.sin(ang),math.cos(ang); dx,dy=px-cx,py-cy
    return (cx+dx*c-dy*s, cy+dx*s+dy*c)

# ---- glyphs -------------------------------------------------------------
def glyph_drivergeek(d,s,detail):
    """A microchip: square die with pins on all four sides.

    Chosen over a gear (that reads 'settings', which is Ultimate Settings
    Panel's glyph) and over a download arrow (AppGeek already owns that).
    A chip says hardware, which is what a driver is about, and it survives
    16px because it is one solid block with regular teeth.
    """
    e=s*EXTENT; cx,cy=s/2,s/2
    body=e*0.62
    half=body/2
    r=body*0.16
    # pins first, so the body sits on top of them cleanly
    pin_len=e*0.155; pin_w=body*0.13
    n=3 if detail>=2 else 2
    spread=body*0.62
    for i in range(n):
        off=(i-(n-1)/2)*(spread/max(1,n-1)) if n>1 else 0
        # top and bottom
        d.rounded_rectangle([cx+off-pin_w/2, cy-half-pin_len, cx+off+pin_w/2, cy-half+pin_w],
                            radius=pin_w*0.35, fill=GLYPH)
        d.rounded_rectangle([cx+off-pin_w/2, cy+half-pin_w, cx+off+pin_w/2, cy+half+pin_len],
                            radius=pin_w*0.35, fill=GLYPH)
        # left and right
        d.rounded_rectangle([cx-half-pin_len, cy+off-pin_w/2, cx-half+pin_w, cy+off+pin_w/2],
                            radius=pin_w*0.35, fill=GLYPH)
        d.rounded_rectangle([cx+half-pin_w, cy+off-pin_w/2, cx+half+pin_len, cy+off+pin_w/2],
                            radius=pin_w*0.35, fill=GLYPH)
    d.rounded_rectangle([cx-half,cy-half,cx+half,cy+half],radius=r,fill=GLYPH)
    if detail>=2:
        # inner die cut out in the badge colour
        ih=half*0.52; ir=ih*0.30
        d.rounded_rectangle([cx-ih,cy-ih,cx+ih,cy+ih],radius=ir,fill=GRADIENT_BOTTOM)
    if detail>=3:
        # a single notch at the top-left corner, the way real chips are keyed
        nr=half*0.15
        d.ellipse([cx-half*0.72-nr,cy-half*0.72-nr,cx-half*0.72+nr,cy-half*0.72+nr],fill=GRADIENT_BOTTOM)

def glyph_cleangeek(d,s,detail):
    """A brush sweeping down-left, with a sparkle.

    A brush silhouette is the one cleaning symbol that survives being shrunk,
    because it is a single diagonal stroke with a flare at one end. The
    bristle grooves stop short of the tip so the head still reads as one
    solid block rather than splitting into prongs, and the sparkle is
    detail-gated so it never smudges into the head at 16px.
    """
    e=s*EXTENT; cx,cy=s/2,s/2
    ang=math.radians(38)
    hx,hy=cx+e*0.05, cy+e*0.02

    hl=e*0.46; hw=e*0.105
    p=[(hx-hw/2,hy-hl),(hx+hw/2,hy-hl),(hx+hw/2,hy),(hx-hw/2,hy)]
    d.polygon([_rot(x,y,hx,hy,ang) for x,y in p],fill=GLYPH)
    cap=_rot(hx,hy-hl,hx,hy,ang)
    d.ellipse([cap[0]-hw/2,cap[1]-hw/2,cap[0]+hw/2,cap[1]+hw/2],fill=GLYPH)

    top_w=e*0.34; bot_w=e*0.56; head_h=e*0.40
    q=[(hx-top_w/2,hy),(hx+top_w/2,hy),(hx+bot_w/2,hy+head_h),(hx-bot_w/2,hy+head_h)]
    d.polygon([_rot(x,y,hx,hy,ang) for x,y in q],fill=GLYPH)
    if detail>=3:
        for t in (-0.26,0.26):
            gx=hx+top_w*t
            g=[(gx-e*0.017,hy+head_h*0.42),(gx+e*0.017,hy+head_h*0.42),
               (gx+e*0.026,hy+head_h*0.90),(gx-e*0.026,hy+head_h*0.90)]
            d.polygon([_rot(x,y,hx,hy,ang) for x,y in g],fill=GRADIENT_BOTTOM)
    if detail>=2:
        sx,sy=cx-e*0.36, cy-e*0.26; a=e*0.145; b=a*0.22
        d.polygon([(sx,sy-a),(sx+b,sy-b),(sx+a,sy),(sx+b,sy+b),
                   (sx,sy+a),(sx-b,sy+b),(sx-a,sy),(sx-b,sy-b)],fill=GLYPH)
    if detail>=3:
        sx,sy=cx-e*0.11, cy-e*0.46; a=e*0.085; b=a*0.22
        d.polygon([(sx,sy-a),(sx+b,sy-b),(sx+a,sy),(sx+b,sy+b),
                   (sx,sy+a),(sx-b,sy+b),(sx-a,sy),(sx-b,sy-b)],fill=GLYPH)

PRODUCTS={'drivergeek':glyph_drivergeek,'cleangeek':glyph_cleangeek}
DISPLAY={'drivergeek':'DriverGeek','cleangeek':'CleanGeek'}

def detail_for(t): return 3 if t>=48 else (2 if t>=32 else 1)

def render(name,target):
    work=max(min(MASTER*2,target*SUPER),256)
    icon=badge(work); PRODUCTS[name](ImageDraw.Draw(icon),work,detail_for(target))
    if work!=target: icon=icon.resize((target,target),Image.LANCZOS)
    return icon

def main():
    out=sys.argv[1] if len(sys.argv)>1 else './out'
    for name in PRODUCTS:
        disp=DISPLAY[name]
        d=os.path.join(out,disp); os.makedirs(d,exist_ok=True)
        for s in SIZES: render(name,s).save(os.path.join(d,f"{name}-{s}.png"))
        render(name,256).save(os.path.join(d,f"{name}.png"))
        # multi-resolution .ico for <ApplicationIcon>
        render(name,256).save(os.path.join(d,f"{name}.ico"),
                              sizes=[(n,n) for n in ICO_SIZES])
        print("  wrote",disp)
if __name__=="__main__": main()
