param([string]$Source, [string]$Name)
Add-Type -AssemblyName System.Drawing
if (-not ('SpriteExport' -as [type])) {
Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
public static class SpriteExport {
 public static void Run(string source,string dest,string name) {
  using(var input=new Bitmap(source)) using(var output=new Bitmap(768,96,PixelFormat.Format32bppArgb)) {
   int[] left=new int[8],right=new int[8],top=new int[8],bottom=new int[8];
   int[] boundaries=new int[9]; boundaries[8]=input.Width;
   for(int i=1;i<8;i++) {
    int mid=(int)Math.Round(i*input.Width/8.0), best=mid, bestCount=int.MaxValue;
    for(int d=0;d<input.Width/24;d++)for(int sign=-1;sign<=1;sign+=2) {
     int x=mid+d*sign,count=0;
     for(int y=0;y<input.Height;y++)if(input.GetPixel(x,y).A>=128)count++;
     if(count<bestCount){bestCount=count;best=x;}
    }
    boundaries[i]=best;
   }
   for(int i=0;i<8;i++) {
    int start=boundaries[i], end=boundaries[i+1];
    left[i]=end;right[i]=-1;top[i]=input.Height;bottom[i]=-1;
    for(int y=0;y<input.Height;y++) for(int x=start;x<end;x++) if(input.GetPixel(x,y).A>=128) {
     left[i]=Math.Min(left[i],x);right[i]=Math.Max(right[i],x);top[i]=Math.Min(top[i],y);bottom[i]=Math.Max(bottom[i],y);
    }
   }
   double scale=80.0/(bottom[0]-top[0]+1);
   for(int i=0;i<8;i++)scale=Math.Min(scale,86.0/(bottom[i]-top[i]+1));
   for(int i=0;i<8;i++) {
    int w=(int)Math.Round((right[i]-left[i]+1)*scale),h=(int)Math.Round((bottom[i]-top[i]+1)*scale);
    if(w>94 || h>88 || w<1 || h<1) throw new Exception("Frame does not fit: "+i+" "+w+"x"+h);
    int dx=i*96+(96-w)/2,dy=88-h;
    for(int y=0;y<h;y++) for(int x=0;x<w;x++) {
     int sx=left[i]+Math.Min(right[i]-left[i],(int)((x+0.5)*(right[i]-left[i]+1)/w));
     int sy=top[i]+Math.Min(bottom[i]-top[i],(int)((y+0.5)*(bottom[i]-top[i]+1)/h));
     var c=input.GetPixel(sx,sy);
     if(c.A>=128) output.SetPixel(dx+x,dy+y,Color.FromArgb(255,c.R,c.G,c.B));
    }
    int low=-1;
    for(int y=0;y<96;y++)for(int x=i*96;x<(i+1)*96;x++)if(output.GetPixel(x,y).A==255)low=y;
    if(low!=87)throw new Exception("Ground alignment failed "+i+": "+low);
    Console.WriteLine(name+" frame "+(i+1)+": "+w+"x"+h+", bottom="+low);
   }
   output.Save(dest,ImageFormat.Png);
  }
 }
}
'@
}
[SpriteExport]::Run($Source, (Join-Path $PSScriptRoot ('native_archer_'+$Name+'_sheet.png')), $Name)
