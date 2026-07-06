Add-Type -AssemblyName System.Drawing
$bmp = New-Object System.Drawing.Bitmap(256, 256)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$rect = New-Object System.Drawing.Rectangle(0, 0, 256, 256)

# Koyu mavi/cyan arkaplan (Uygulama temasýna uygun)
$brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 6, 182, 212))
$g.FillEllipse($brush, $rect)

# Beyaz 'V' harfi
$font = New-Object System.Drawing.Font("Segoe UI", 120, [System.Drawing.FontStyle]::Bold)
$textBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
$format = New-Object System.Drawing.StringFormat()
$format.Alignment = [System.Drawing.StringAlignment]::Center
$format.LineAlignment = [System.Drawing.StringAlignment]::Center
$g.DrawString("V", $font, $textBrush, $rect, $format)

$g.Dispose()

# Icon'a dönüþtürüp kaydetme
$iconStream = New-Object System.IO.FileStream("src\Voxplux.App\icon.ico", [System.IO.FileMode]::Create)
$bmp.Save($iconStream, [System.Drawing.Imaging.ImageFormat]::Icon) # Bu bazen sorun çýkarabilir, memory stream üzerinden Png -> Ico yapalým.
$iconStream.Close()
