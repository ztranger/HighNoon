$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$refs = @('System.Drawing.Common','System.Drawing.Primitives','System.Private.Windows.GdiPlus','System.Private.Windows.Core','System.Collections')
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
public static class CabaretExport3 {
 public static string Export(string source,string target) {
  using(var src=new Bitmap(source)) using(var dst=new Bitmap(3072,384,PixelFormat.Format32bppArgb)) {
   int[] counts=new int[src.Width];
   for(int x=0;x<src.Width;x++) for(int y=0;y<src.Height;y++) if(src.GetPixel(x,y).A>=128) counts[x]++;
   bool twoRows=target.Contains("_Death");
   var runs=new List<int[]>(); int start=-1;
   for(int x=0;x<=src.Width;x++) {
    if(x<src.Width && counts[x]>0) {if(start<0) start=x;}
    else if(start>=0) {runs.Add(new int[]{start,x});start=-1;}
   }
   int[] cuts=new int[9]; cuts[8]=src.Width;
   for(int i=1;i<8;i++) {
    if(twoRows) {cuts[i]=src.Width*i/8;continue;}
    if(runs.Count==8) {cuts[i]=(runs[i-1][1]+runs[i][0])/2;continue;}
    int nominal=src.Width*i/8,best=-1,dist=int.MaxValue;
    for(int x=Math.Max(0,nominal-src.Width/24);x<Math.Min(src.Width,nominal+src.Width/24);x++) if(counts[x]==0 && Math.Abs(x-nominal)<dist) {best=x;dist=Math.Abs(x-nominal);}
    if(best<0) throw new Exception("No empty frame separation "+i);
    cuts[i]=best;
   }
   var bounds=new Rectangle[8];
   for(int i=0;i<8;i++) {
    int l=src.Width,t=src.Height,r=-1,b=-1;
    int left=twoRows?(i%4)*src.Width/4:cuts[i], right=twoRows?(i%4+1)*src.Width/4:cuts[i+1];
    int rowCut=(int)(src.Height*0.61);
    int top=twoRows && i>=4?rowCut:0, bottom=twoRows && i<4?rowCut:src.Height;
    for(int x=left;x<right;x++) for(int y=top;y<bottom;y++) if(src.GetPixel(x,y).A>=128) {l=Math.Min(l,x);r=Math.Max(r,x);t=Math.Min(t,y);b=Math.Max(b,y);}
    if(r<l) throw new Exception("Empty frame");
    bounds[i]=new Rectangle(l,t,r-l+1,b-t+1);
   }
   double scale=320.0/bounds[0].Height;
   var report=new List<string>();
   for(int i=0;i<8;i++) {
    var b=bounds[i]; int w=(int)Math.Round(b.Width*scale),h=(int)Math.Round(b.Height*scale);
    if(w>376 || h>352) throw new Exception("Pose exceeds cell: "+i+" "+w+"x"+h);
    int ox=i*384+(384-w)/2,oy=352-h;
    for(int y=0;y<h;y++) for(int x=0;x<w;x++) {
     var c=src.GetPixel(b.X+Math.Min(b.Width-1,(int)((x+0.5)*b.Width/w)),b.Y+Math.Min(b.Height-1,(int)((y+0.5)*b.Height/h)));
     if(c.A>=128) dst.SetPixel(ox+x,oy+y,Color.FromArgb(255,c.R,c.G,c.B));
    }
    int bottom=-1;
    for(int y=0;y<384;y++) for(int x=i*384;x<(i+1)*384;x++) if(dst.GetPixel(x,y).A>0) bottom=Math.Max(bottom,y);
    if(bottom!=351) throw new Exception("Baseline mismatch "+i+": "+bottom);
    report.Add("frame "+(i+1)+": "+w+"x"+h+", bottom="+bottom);
   }
   dst.Save(target,ImageFormat.Png);
   return string.Join("; ",report);
  }
 }
}
'@
$manifest = Get-Content (Join-Path $PSScriptRoot 'CabaretSinger3_prompts.json') -Raw | ConvertFrom-Json
$destination = Join-Path $PSScriptRoot '..\Characters\CabaretSinger3'
New-Item -ItemType Directory -Path $destination -Force | Out-Null
$reports = @{}
foreach($animation in @('Idle','Walk','Shoot','Death')) {
 $source = $manifest.$animation.source
 if(-not $source) {continue}
 $target = Join-Path $destination "CabaretSinger3_$animation.png"
 $reports[$animation] = [CabaretExport3]::Export($source,$target)
 Write-Output "$animation : $($reports[$animation])"
}
$reports | ConvertTo-Json | Set-Content (Join-Path $PSScriptRoot 'CabaretSinger3_validation.json')
