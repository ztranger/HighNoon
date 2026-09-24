param([string]$SourceDirectory = 'C:/Users/kirik/.codex/generated_images/01a0d40d-9116-7693-b5aa-e148be817051')
Add-Type -AssemblyName System.Drawing
$sources = [ordered]@{
    Walk = 'exec-aef46546-e72a-4646-bf3d-4e9083f11434.png'
    Shoot = 'exec-47c2d0e7-1e4b-423f-a013-d2fb3efa07c4.png'
    Death = 'exec-41de06c4-63a7-4c64-b667-ef15736e7db4.png'
}
foreach ($animation in $sources.Keys) {
    $src = [System.Drawing.Bitmap]::new((Join-Path $SourceDirectory $sources[$animation]))
    $runs = @()
    $start = -1
    for ($x = 0; $x -lt $src.Width; $x++) {
        $occupied = $false
        for ($y = 0; $y -lt $src.Height; $y++) {
            if ($src.GetPixel($x, $y).A -ge 128) { $occupied = $true; break }
        }
        if ($occupied -and $start -lt 0) { $start = $x }
        if (-not $occupied -and $start -ge 0) { $runs += ,@($start, ($x - 1)); $start = -1 }
    }
    if ($start -ge 0) { $runs += ,@($start, ($src.Width - 1)) }
    if ($runs.Count -ne 8) { throw "$animation has $($runs.Count) silhouettes instead of 8" }
    $bounds = @()
    foreach ($run in $runs) {
        $top = $src.Height; $bottom = -1
        for ($x = $run[0]; $x -le $run[1]; $x++) {
            for ($y = 0; $y -lt $src.Height; $y++) {
                if ($src.GetPixel($x, $y).A -ge 128) { $top = [Math]::Min($top, $y); $bottom = [Math]::Max($bottom, $y) }
            }
        }
        $bounds += ,@($run[0], $top, $run[1], $bottom)
    }
    # One uniform scale per sheet, based on the standing pose. Never stretch individual poses.
    $scale = 120.0 / ($bounds[0][3] - $bounds[0][1] + 1)
    $dst = [System.Drawing.Bitmap]::new(1152, 144, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    for ($i = 0; $i -lt 8; $i++) {
        $b = $bounds[$i]
        $w = [int][Math]::Round(($b[2] - $b[0] + 1) * $scale)
        $h = [int][Math]::Round(($b[3] - $b[1] + 1) * $scale)
        if ($w -gt 140 -or $h -gt 132) { throw "$animation frame $i exceeds cell" }
        $tile = [System.Drawing.Bitmap]::new($w, $h, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $occupiedBottom = -1
        for ($y = 0; $y -lt $h; $y++) {
            for ($x = 0; $x -lt $w; $x++) {
                $sx = [Math]::Min($b[2], $b[0] + [int][Math]::Floor(($x + 0.5) / $scale))
                $sy = [Math]::Min($b[3], $b[1] + [int][Math]::Floor(($y + 0.5) / $scale))
                $c = $src.GetPixel($sx, $sy)
                if ($c.A -ge 128) {
                    $tile.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(255, $c.R, $c.G, $c.B))
                    $occupiedBottom = $y
                }
            }
        }
        $dx = $i * 144 + [int][Math]::Floor((144 - $w) / 2)
        $dy = 131 - $occupiedBottom
        for ($y = 0; $y -lt $h; $y++) {
            for ($x = 0; $x -lt $w; $x++) {
                $c = $tile.GetPixel($x, $y)
                if ($c.A -eq 255) { $dst.SetPixel($dx + $x, $dy + $y, $c) }
            }
        }
        $tile.Dispose()
    }
    $dst.Save((Join-Path $PSScriptRoot "CabaretSinger_$animation.png"), [System.Drawing.Imaging.ImageFormat]::Png)
    $dst.Dispose(); $src.Dispose()
}
$report = @()
foreach ($animation in @('Idle', 'Walk', 'Shoot', 'Death')) {
    $bmp = [System.Drawing.Bitmap]::new((Join-Path $PSScriptRoot "CabaretSinger_$animation.png"))
    if ($bmp.Width -ne 1152 -or $bmp.Height -ne 144) { throw "Wrong dimensions: $animation" }
    for ($i = 0; $i -lt 8; $i++) {
        $minX = 144; $minY = 144; $maxX = -1; $maxY = -1
        for ($y = 0; $y -lt 144; $y++) {
            for ($x = 0; $x -lt 144; $x++) {
                $a = $bmp.GetPixel($i * 144 + $x, $y).A
                if ($a -ne 0 -and $a -ne 255) { throw 'Nonbinary alpha' }
                if ($a -eq 255) {
                    $minX = [Math]::Min($minX, $x); $maxX = [Math]::Max($maxX, $x)
                    $minY = [Math]::Min($minY, $y); $maxY = [Math]::Max($maxY, $y)
                }
            }
        }
        if ($maxY -ne 131 -or $minX -le 0 -or $maxX -ge 143) { throw "Invalid boundaries: $animation frame $i" }
        $report += [pscustomobject]@{ animation = $animation; frame = $i + 1; x = $minX; y = $minY; width = $maxX - $minX + 1; height = $maxY - $minY + 1; bottom = $maxY }
    }
    $bmp.Dispose()
}
$report | ConvertTo-Json | Set-Content (Join-Path $PSScriptRoot 'validation.json')
$report | Format-Table
