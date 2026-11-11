# Verkleint de met de hand bewerkte tegels van 600x600 terug naar 30x30 en zet
# ze in MastermindWin\Bronplaatjes, waar het spel ze uit haalt.
#
#   pwsh -File verklein.ps1              -> naar Bronplaatjes
#   pwsh -File verklein.ps1 -Uit proef   -> naar een eigen map, om eerst te kijken
#
# Er wordt gemiddeld over elk blokje van 20 bij 20 beeldpunten, dus een met de
# hand gladgemaakte rand komt er als een nette overgang uit.

param([string]$Uit = "")

Add-Type -AssemblyName System.Drawing

$hier = Split-Path -Parent $MyInvocation.MyCommand.Path
if ($Uit -eq "") { $Uit = Join-Path (Split-Path -Parent $hier) "MastermindWin\Bronplaatjes" }
if (-not (Test-Path $Uit)) { New-Item -ItemType Directory -Path $Uit | Out-Null }

$breed = 30
$hoog = 30
$gedaan = 0

foreach ($bestand in Get-ChildItem -Path $hier -Filter *.png) {
    $bron = [System.Drawing.Bitmap]::FromFile($bestand.FullName)
    try {
        if ($bron.Width % $breed -ne 0 -or $bron.Height % $hoog -ne 0) {
            Write-Warning "$($bestand.Name): $($bron.Width)x$($bron.Height) is geen veelvoud van ${breed}x${hoog} - overgeslagen"
            continue
        }
        $f = $bron.Width / $breed

        # Zelf middelen over elk blokje: dat is zuiverder dan GDI+ laten
        # verkleinen, want daar zit een filter overheen dat randen kan laten
        # nagalmen.
        $doel = New-Object System.Drawing.Bitmap $breed, $hoog
        for ($y = 0; $y -lt $hoog; $y++) {
            for ($x = 0; $x -lt $breed; $x++) {
                $r = 0; $g = 0; $b = 0
                for ($j = 0; $j -lt $f; $j++) {
                    for ($i = 0; $i -lt $f; $i++) {
                        $c = $bron.GetPixel($x * $f + $i, $y * $f + $j)
                        $r += $c.R; $g += $c.G; $b += $c.B
                    }
                }
                $n = $f * $f
                $doel.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(
                    [int][Math]::Round($r / $n), [int][Math]::Round($g / $n), [int][Math]::Round($b / $n)))
            }
        }
        $pad = Join-Path $Uit $bestand.Name
        $doel.Save($pad, [System.Drawing.Imaging.ImageFormat]::Png)
        $doel.Dispose()
        $gedaan++
    }
    finally { $bron.Dispose() }
}

Write-Host "$gedaan tegels verkleind naar $Uit"
