$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$references = @('System.Drawing.Common','System.Drawing.Primitives','System.Private.Windows.GdiPlus','System.Private.Windows.Core','System.Collections')
Add-Type -ReferencedAssemblies $references -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
public static class CabaretSpriteExport2 {
 public static string Export(string source, string destination) {
  using(var input = new Bitmap(source)) using(var output = new Bitmap(1536,192,PixelFormat.Format32bppArgb)) {
   int[] counts = new int[input.Width];
   for(int x=0;x<input.Width;x++) for(int y=0;y<input.Height;y++) if(input.GetPixel(x,y).A>=128) counts[x]++;
   var runs = new List<int[]>();
   int start=-1,last=-1;
   for(int x=0;x<input.Width;x++) {
    if(counts[x]>0) { if(start<0) start=x; last=x; }
    if(start>=0 && (x-last>0 || x==input.Width-1)) { runs.Add(new int[]{start,last+1});start=-1; }
   }
   int[] cuts = new int[9]; cuts[8]=input.Width;
   for(int i=1;i<8;i++) {
    if(runs.Count==8) {cuts[i]=(runs[i-1][1]+runs[i][0])/2;continue;}
    int nominal=(int)Math.Round(input.Width*i/8.0), radius=input.Width/24;
    int best=nominal, bestScore=int.MaxValue;
    for(int x=nominal-radius;x<=nominal+radius;x++) {
     int score=counts[x]*10000+Math.Abs(x-nominal);
     if(score<bestScore) {best=x;bestScore=score;}
    }
    if(counts[best]!=0) throw new Exception("No transparent separation near frame "+i);
    cuts[i]=best;
   }
   var bounds = new Rectangle[8];
   for(int i=0;i<8;i++) {
    int minX=input.Width,minY=input.Height,maxX=-1,maxY=-1;
    for(int x=cuts[i];x<cuts[i+1];x++) for(int y=0;y<input.Height;y++) if(input.GetPixel(x,y).A>=128) {
     minX=Math.Min(minX,x);maxX=Math.Max(maxX,x);minY=Math.Min(minY,y);maxY=Math.Max(maxY,y);
    }
    bounds[i]=new Rectangle(minX,minY,maxX-minX+1,maxY-minY+1);
   }
   double scale=160.0/bounds[0].Height;
   var report = new List<string>();
   for(int i=0;i<8;i++) {
    Rectangle b=bounds[i];
    // Correct the generator's smaller body rendering in the later fall poses.
    double correction=destination.Contains("_Death") ? new double[]{1,1.057,1.026,1.089,1.25,1.4,1.467,1.467}[i] : 1;
    int w=(int)Math.Round(b.Width*scale*correction),h=(int)Math.Round(b.Height*scale*correction);
    if(destination.Contains("_Death") && i>=6) {w=156;h=44;}
    if(w>188 || h>176) throw new Exception("Frame does not fit at consistent scale: "+i+" "+w+"x"+h);
    int ox=i*192+(192-w)/2,oy=176-h;
    for(int y=0;y<h;y++) for(int x=0;x<w;x++) {
     int sx=b.X+Math.Min(b.Width-1,(int)((x+0.5)*b.Width/w));
     int sy=b.Y+Math.Min(b.Height-1,(int)((y+0.5)*b.Height/h));
     Color c=input.GetPixel(sx,sy);
     if(c.A>=128) output.SetPixel(ox+x,oy+y,Color.FromArgb(255,c.R,c.G,c.B));
    }
    int bottom=-1;
    for(int y=0;y<192;y++) for(int x=i*192;x<(i+1)*192;x++) if(output.GetPixel(x,y).A==255) bottom=Math.Max(bottom,y);
    if(bottom!=175) throw new Exception("Bottom row mismatch frame "+i+": "+bottom);
    report.Add("frame "+(i+1)+": "+w+"x"+h+", bottom="+bottom);
   }
   output.Save(destination,ImageFormat.Png);
   return string.Join("; ",report);
  }
 }
}
'@
$generated = 'C:\Users\kirik\.codex\generated_images\01a0d506-c509-7f11-9dcb-0cd602dd0e47'
$destination = Join-Path $PSScriptRoot '..\Characters\CabaretSinger2'
New-Item -ItemType Directory -Path $destination -Force | Out-Null
$sources = @{
 Idle = 'exec-b392bf62-66e3-4b52-b017-cf8b89dd19fb.png'
 Walk = 'exec-18f792b2-5f98-4b4a-be5f-ae272565cc6f.png'
 Shoot = 'exec-269b43ef-c676-43f9-8cef-497748da0f14.png'
 Death = 'exec-12be262c-84e0-4a69-9222-4c8cee0c4e6b.png'
}
foreach($animation in @('Idle','Walk','Shoot','Death')) {
 if(-not $sources.ContainsKey($animation)) { continue }
 $target = Join-Path $destination "CabaretSinger2_$animation.png"
 $report = [CabaretSpriteExport2]::Export((Join-Path $generated $sources[$animation]),$target)
 Write-Output "$animation : $report"
}
