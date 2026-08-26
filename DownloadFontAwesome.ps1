# Download FontAwesome Free Regular font
$source = "https://github.com/FortAwesome/Font-Awesome/releases/download/6.5.1/fontawesome-free-6.5.1-desktop.zip"
$dest = "C:\Users\jc_x1\AppData\Local\Temp\fa.zip"
Invoke-WebRequest -Uri $source -OutFile $dest
Write-Host "Downloaded to $dest"
