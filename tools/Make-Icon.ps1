$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$target = Join-Path $PSScriptRoot '..\MHWsTool\Assets\app.ico'
[System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName([System.IO.Path]::GetFullPath($target))) | Out-Null

$bitmap = [System.Drawing.Bitmap]::new(256, 256)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.Clear([System.Drawing.Color]::Transparent)
$navy = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 20, 37, 51))
$cyan = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(255, 87, 210, 222), 13)
$gold = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(255, 236, 185, 92), 9)
$graphics.FillEllipse($navy, 8, 8, 240, 240)
$graphics.DrawEllipse($cyan, 15, 15, 226, 226)
$graphics.DrawLine($gold, 78, 67, 178, 178)
$graphics.DrawLine($gold, 178, 67, 78, 178)
$font = [System.Drawing.Font]::new('Segoe UI', 74, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
$white = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::White)
$format = [System.Drawing.StringFormat]::new()
$format.Alignment = [System.Drawing.StringAlignment]::Center
$format.LineAlignment = [System.Drawing.StringAlignment]::Center
$graphics.FillEllipse($navy, 72, 72, 112, 112)
$graphics.DrawString('W', $font, $white, [System.Drawing.RectangleF]::new(65, 66, 126, 126), $format)

$png = [System.IO.MemoryStream]::new()
$bitmap.Save($png, [System.Drawing.Imaging.ImageFormat]::Png)
$file = [System.IO.File]::Create([System.IO.Path]::GetFullPath($target))
$writer = [System.IO.BinaryWriter]::new($file)
$writer.Write([uint16]0)
$writer.Write([uint16]1)
$writer.Write([uint16]1)
$writer.Write([byte]0)
$writer.Write([byte]0)
$writer.Write([byte]0)
$writer.Write([byte]0)
$writer.Write([uint16]1)
$writer.Write([uint16]32)
$writer.Write([uint32]$png.Length)
$writer.Write([uint32]22)
$writer.Write($png.ToArray())
$writer.Dispose()
$png.Dispose()
$graphics.Dispose()
$bitmap.Dispose()
$navy.Dispose()
$cyan.Dispose()
$gold.Dispose()
$white.Dispose()
$font.Dispose()
$format.Dispose()
Write-Host "Created $target"
