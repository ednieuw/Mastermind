# Mastermind bouwen en spelen

Mastermind uit 1995 (Borland C++ met OWL), omgezet naar .NET 8 WinForms voor
Windows 11. De balletjes, de paaltjes en het geluidje zijn de oorspronkelijke.

## Bouwen

Dubbelklik **`bouw-exe.cmd`**. Het resultaat is
`Mastermind-app\Mastermind.exe`: één bestand dat op elke Windows 11-machine
draait, ook zonder dat .NET geïnstalleerd is.

```
bouw-exe.cmd
```

De .NET 8 SDK staat in `%USERPROFILE%\.dotnet`; het script zoekt hem daar.

> Bouw je met `dotnet build` in plaats van met dit script, dan is het resultaat
> een programma dat de .NET 8 desktop runtime nodig heeft. Die staat op deze
> machine niet, en dan komt bij het starten het venster "You must install or
> update .NET". Gebruik dus `bouw-exe.cmd`, of start het met
> `%USERPROFILE%\.dotnet\dotnet.exe MastermindWin\bin\Release\net8.0-windows\Mastermind.dll`.

## Spelen

De computer kiest **vijf verschillende kleuren uit negen**. Raad ze in de goede
volgorde; er zijn zeventien regels.

* Klik een kleur in het blokje van negen, of druk **1** tot en met **9**.
* Een plaats kiezen: de knoppen **1** tot en met **5**, de **pijltjes**, of klik
  in de regel zelf.
* **Backspace** haalt het laatste balletje weg, **Esc** wist de hele regel.
* **Klaar** (of **Enter**) beoordeelt de regel.
* Naast de regel verschijnen de paaltjes: **groen** = goede kleur op de goede
  plaats, **rood** = goede kleur op de verkeerde plaats. Ze staan op volgorde en
  verraden dus niet wélke kleur goed staat.
* **F2** begint een nieuw spel, **F1** toont de spelregels.

Menu **Beeld → Grootte** vergroot alles voor een scherm met veel beeldpunten en
**Beeld → Tekening** kiest tussen drie stellen balletjes (gerekend — de
standaard —, met de hand getekend, of die van 1995);
menu **Taal** zet het om tussen Nederlands en Engels. Die keuzes worden
onthouden. `Mastermind.exe /nl` start meteen in het Nederlands.

## Controleren

```
test-motor.cmd
```

Controleert de spelregels zonder venster. `test-motor oud` laat zien waar het
tellen van 1995 en dat van nu uiteenlopen, `test-motor oplos 2000` laat de
computer zelf raden.

## De plaatjes met de hand bijwerken

De balletjes en paaltjes staan als tekening van **600 bij 600** in
`MastermindWin\Tekening\` (met de hand getekend) en `MastermindWin\Gerekend\`
(uitgerekend). Werk ze daar bij en draai `bouw-exe.cmd`; verkleinen
hoeft niet, want het spel doet dat zelf. `Tekenaar\` maakt ze opnieuw aan met
`dotnet run --project Tekenaar` — dat overschrijft handwerk, dus bewaar je eigen
versies apart.

Wil je zelf tekenen, begin dan bij `Handwerk\`: daar staan de **balletjes van
1995** blokkerig uitvergroot tot dezelfde 600 bij 600. Een afgemaakt vakje gaat
naar `Tekening\`. `Handwerkerklein.ps1` is er alleen voor als je de kleine
bitmaps van 1995 zelf wilt bijwerken.

## Verantwoording

`Documentatie\LEESMIJ-CSharp.md` beschrijft per bestand waar het vandaan komt en
wat er anders is dan in 1995 — vooral het tellen van de paaltjes, dat bij een
herhaalde kleur in het origineel te veel rode paaltjes gaf. De C++-bronbestanden
van toen staan in `Origineel-1995\`.
