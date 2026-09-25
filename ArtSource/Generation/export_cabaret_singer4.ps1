$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$refs = @('System.Drawing.Common','System.Drawing.Primitives','System.Private.Windows.GdiPlus','System.Private.Windows.Core','System.Collections')
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
public static class ExportSinger4 {
 public static string Run(string source,string dest,bool side) {
  using(var src=new Bitmap(source)) using(var dst=new Bitmap(1536,768,PixelFormat.Format32bppArgb)) {
   int cut=src.Height/2, best=999999;
   for(int y=src.Height*45/100;y<src.Height*62/100;y++) {
    int count=0; for(int x=0;x<src.Width;x++) if(src.GetPixel(x,y).A>=128) count++;
    int score=count*10000+Math.Abs(y-src.Height/2);
    if(score<best){best=score;cut=y;}
   }
   var bounds=new Rectangle[8];
   for(int i=0;i<8;i++) {
    int l=src.Width,t=src.Height,r=-1,b=-1;
    for(int y=i<4?0:cut;y<(i<4?cut:src.Height);y++) for(int x=(i%4)*src.Width/4;x<(i%4+1)*src.Width/4;x++) {
     if(src.GetPixel(x,y).A<128)continue;
     l=Math.Min(l,x);t=Math.Min(t,y);r=Math.Max(r,x);b=Math.Max(b,y);
    }
    if(r<l)throw new Exception("Empty frame");
    bounds[i]=new Rectangle(l,t,r-l+1,b-t+1);
   }
   double scale=80.0/bounds[0].Height;
   var report=new List<string>();
   for(int i=0;i<8;i++) {
    var b=bounds[i]; int w=(int)Math.Round(b.Width*scale),h=(int)Math.Round(b.Height*scale);
    if(w>94||h>88)throw new Exception("Frame exceeds cell");
    int ox=(i%4)*384+((96-w)/2)*4,oy=(i/4)*384+352-h*4;
    for(int y=0;y<h;y++)for(int x=0;x<w;x++) {
     var c=src.GetPixel(b.X+Math.Min(b.Width-1,(int)((x+0.5)*b.Width/w)),b.Y+Math.Min(b.Height-1,(int)((y+0.5)*b.Height/h)));
     if(c.A<128)continue;
     for(int yy=0;yy<4;yy++)for(int xx=0;xx<4;xx++)dst.SetPixel(ox+x*4+xx,oy+y*4+yy,Color.FromArgb(255,c.R,c.G,c.B));
    }
    int bottom=-1;
    for(int y=0;y<384;y++)for(int x=0;x<384;x++) if(dst.GetPixel((i%4)*384+x,(i/4)*384+y).A>0)bottom=Math.Max(bottom,y);
    if(bottom!=351)throw new Exception("Baseline mismatch "+i+" "+bottom);
    report.Add("frame "+(i+1)+": "+w*4+"x"+h*4+", bottom="+bottom);
   }
   dst.Save(dest,ImageFormat.Png);
   if(side)using(var single=dst.Clone(new Rectangle(0,0,384,384),PixelFormat.Format32bppArgb))single.Save(dest.Replace("_Idle.png","_Side.png"),ImageFormat.Png);
   return string.Join("; ",report);
  }
 }
}
'@
$generated = 'C:\Users\kirik\.codex\generated_images\01a0d525-24e5-7c10-8e15-d15b4a3e17bc'
$sources = @{
 Idle='exec-7233bcb5-65d6-4841-bf78-a5ac1aa7c077.png'
 Walk='exec-6790dd56-abc5-487d-bc82-cfe84f126ea1.png'
 Shoot='exec-a79b4169-6c42-4f21-af64-6fb98b60550e.png'
 Death='exec-94819686-213a-43ed-81b2-9b5ff75a812f.png'
}
$target = 'G:\Projects\HighNoon\ArtSource\Characters\CabaretSinger4'
$report = @{}
foreach($name in @('Idle','Walk','Shoot','Death')) {
 $source = Join-Path $generated $sources[$name]
 $raw = Join-Path $PSScriptRoot "CabaretSinger4_$($name)_source.png"
 Copy-Item -LiteralPath $source -Destination $raw -Force
 $report[$name]=[ExportSinger4]::Run($source,(Join-Path $target "CabaretSinger4_$name.png"),($name -eq 'Idle'))
}
$report | ConvertTo-Json | Set-Content (Join-Path $PSScriptRoot 'CabaretSinger4_validation.json')
$report | ConvertTo-Json
