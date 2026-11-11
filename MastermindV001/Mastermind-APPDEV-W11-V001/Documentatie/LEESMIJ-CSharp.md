# Mastermind van Borland C++ (1995) naar C# (2026)

Dit is de volledige verantwoording van de omzetting: waar elk stuk vandaan komt,
wat er anders is dan in 1995 en waarom.

Het oorspronkelijke programma is van Ed Nieuwenhuys en Ronald Loggen, geschreven in
Borland C++ met OWL (ObjectWindows Library) en BWCC (Borland Windows Custom
Controls). Het draaide onder Windows 3.1 en later nog onder Windows 7 (zie
`MM-in-W7.jpg`). Onder Windows 11 start het niet meer: het is 16-bits en de
knoppen komen uit `BWCC.DLL`, die er niet meer is.

---

## 1. Waar het uit bestaat

    Mastermind-APPDEV-W11-V001\
      MastermindWin\            het programma zelf
        Engine\                 de spelregels, zonder scherm
          MmSpel.cs             het spel: code, regels, paaltjes
          Instellingen.cs       taal, grootte, gladstrijken en geluid onthouden
          Taal.cs               alle teksten, Nederlands en Engels
        Ui\                     alles wat met het venster te maken heeft
          BordPaneel.cs         het speelveld: tekenen, muis, toetsen
          SpelForm.cs           venster, menu en grootte
          Plaatjes.cs           de bitmaps van 1995 laden, gladstrijken en vergroten
        Tekening\               de nieuwe tekeningen, 600 bij 600 per vakje
        Bronplaatjes\           de bitmaps van 1995, het geluid, het pictogram
        Program.cs              opstarten en foutafhandeling
      MastermindTest\           controles zonder venster
      Tekenaar\                 tekent de nieuwe balletjes en paaltjes
      Origineel-1995\           de C++-bronbestanden waar dit uit komt
      Handwerk\                de tegels 20x zo groot, om met de hand bij te werken
      Documentatie\             dit bestand
      bouw-exe.cmd              maakt Mastermind.exe
      test-motor.cmd            draait de controles

`Engine\` hangt nergens van WinForms af. `MastermindTest` compileert die map
rechtstreeks mee als gewone `net8.0`, zonder Windows. Alleen `Ui\` is
Windows-gebonden. Dezelfde indeling als bij de klaverjasomzetting, en om dezelfde
reden: de spelregels blijven zo overdraagbaar.

## 2. Herkomst per bestand

| nu | toen | opmerking |
|---|---|---|
| `Engine\MmSpel.cs` | `MSTTDLGC.CPP` | `MMarray` → `bord`, `GekozenGetal` → `geheim`, `PosGoed`/`KleurGoed` blijven zo heten, `regel` en `pos_in_regel` → `Regel` en `PosInRegel` |
| `MmSpel.NieuwSpel` | `BNClickedNewGame` | vijf verschillende kleuren uit negen |
| `MmSpel.Invoeren` | `Controle_Invoer` | |
| `MmSpel.Wissen` | `BNClickedClear` | |
| `MmSpel.Beoordelen` | `BNClickedOK` | het tellen is gewijzigd, zie 4.1 |
| `MmSpel.ToonOplossing` | `BNClickedSolution` | |
| `Ui\BordPaneel.cs` | `SetLampjes`, `SetPaaltjes`, `DrawBitmap`, `EvPaint` | |
| `Ui\SpelForm.cs` | `MSTRMAPP.CPP`, `SDIDecFrame` | het frame met menu |
| `Ui\Plaatjes.cs` | `LoadBitmap` uit `MSTRMAPP.RC` | |
| `Bronplaatjes\bal*.png` | `BTNBLK/BLU/BLU1/GRN/GRN1/BRN/BRN1/YEL/RED/CYN.BMP` | ongewijzigde beeldpunten, behalve `bal1` (blauw), zie 4.4 |
| `Bronplaatjes\knop*.png` | `NBTN*.BMP` | de knopvariant van dezelfde balletjes, `knop1` idem |
| `Bronplaatjes\paal*.png` | `PAAL0/PAAL1/PAALLEEG.BMP` | |
| `Bronplaatjes\gevonden.wav` | `FOUND.WAV` | ongewijzigd |
| `Bronplaatjes\mastermind.ico` | `RL.ICO` | ongewijzigd |
| — | `MSTRMABD.CPP` (venster "Over") | vervangen door een gewone melding |
| — | `MASTERMI.HLP`, `*.RTF` | vervangen door Help → Spelregels |

De maten van het scherm komen uit `IDD_CLIENT` in `MSTRMAPP.RC`: negen
kleurknoppen in drie rijen van drie, daaronder vijf plaatsknoppen, dan Klaar en
Wissen, en apart daaronder Oplossing. De achtergrondkleur is die van toen:
`SetBkgndColor(RGB(64,128,64))`.

## 3. Wat hetzelfde is gebleven

* **17 regels van 5** (`MAXROW`, `MAXCOL`).
* **Negen kleuren**, in dezelfde volgorde als in 1995: 1 blauw, 2 cyaan,
  3 groen, 4 donkergroen, 5 grijs, 6 oker, 7 geel, 8 donkerrood, 9 magenta.
  (De bestandsnamen liegen: `BTNBRN.BMP` is grijs.) De blauwe bal is opnieuw
  getekend, zie 4.4.
* **De code bestaat uit vijf verschillende kleuren.** Dubbele kleuren komen er
  niet in voor. Dat maakt het spel makkelijker dan de gewone mastermindregels.
* **Een leeg vakje is het zwarte balletje** (`'0'` in het origineel).
* **De paaltjes staan op volgorde**: eerst alle groene, dan alle rode, dan de
  lege vakjes. Ze verraden dus niet wélke kleur goed staat.
* **De oplossing komt in een extra regel onder het bord**, precies zoals
  `strcpy(MMarray[MAXROW], GekozenGetal)` deed.
* **Het geluidje bij winst** is `FOUND.WAV`, hetzelfde bestand.
* **Het venster heeft een vaste maat**, zoals het dialoogvenster van toen.
* **Een half ingevulde regel mag beoordeeld worden.** Het origineel stond dat
  toe en dat is zo gelaten; de lege vakjes leveren simpelweg geen paaltje op.

## 4. Wat anders is dan in 1995

### 4.1 Het tellen van de paaltjes  — de enige wijziging in de spelregels

Het origineel telde zo (`BNClickedOK`):

```c
for(m=0;m<MAXCOL;m++) {
    if(MMarray[regel][m]==GekozenGetal[m]) PosGoed[regel]++;
    for(n=0;n<MAXCOL;n++)
        if(MMarray[regel][m]==GekozenGetal[n] && m!=n) KleurGoed[regel]++;
}
```

Elk balletje van de gok werd vergeleken met álle balletjes van de code. Zolang de
speler vijf verschillende kleuren invulde klopte dat, maar bij een herhaalde
kleur liep het mis:

| gok | code | 1995 | nu |
|---|---|---|---|
| `11111` | `12345` | 1 groen, **4 rood** | 1 groen, 0 rood |
| `22222` | `12345` | 1 groen, **4 rood** | 1 groen, 0 rood |
| `11234` | `12345` | 1 groen, **4 rood** | 1 groen, 3 rood |

Vijf gelijke balletjes gaven dus vijf paaltjes terwijl er maar één kleur van de
code bij zat — en wie dat wist, kon met negen zulke regels de hele code
uitlezen. `MmSpel.Tel` telt nu volgens de gewone mastermindregels: eerst de
goede kleuren op de goede plaats, en van wat overblijft per kleur zoveel rode
paaltjes als er aan beide kanten van zijn. Elk balletje van de code telt
hoogstens één keer mee.

Zolang de speler vijf verschillende kleuren invult, geeft het nieuwe tellen
precies dezelfde paaltjes als het oude. Alleen bij een herhaalde kleur wijkt het
af, en daar was het oude antwoord fout.

De oude manier staat nog in het programma, als `MmSpel.TelZoalsOrigineel`. Het
spel gebruikt hem niet; `test-motor oud` zet beide manieren naast elkaar.

### 4.2 Verbeteringen in de bediening

* **Toetsenbord.** 1 tot en met 9 zetten een kleur, de pijltjes kiezen een
  plaats, Backspace haalt er een weg, Enter is Klaar, Esc wist de regel, F2
  begint een nieuw spel, F1 toont de spelregels. Het origineel kon alleen met de
  muis.
* **Klikken in de regel zelf** kiest de plaats. De knoppen 1 tot en met 5 uit
  1995 zitten er nog steeds bij.
* **De regel waar je aan toe bent** heeft een witte omlijning, het vakje dat aan
  de beurt is een gele. Toen moest je zelf onthouden waar je was.
* **De stand in woorden** ("Regel 4 van 17") staat naast het bord.
* **Na afloop kan er niets meer ingevoerd worden**, en de code komt vanzelf in
  beeld. Het origineel liet je na de zeventiende regel gewoon doortikken.
* **Het geluidje kan uit** (Beeld → Geluid).
* **Regels kopiëren** (Ctrl+C) zet de gespeelde regels als tekst op het
  klembord.

### 4.3 Taal

Het origineel was Engels. Dat is de standaard gebleven; via Taal → Nederlands
kan het om, en die keuze wordt onthouden. `Mastermind.exe /nl` start meteen in
het Nederlands, `/en` in het Engels. Alle teksten staan in `Engine\Taal.cs`.

### 4.4 De balletjes bijgewerkt

**De blauwe bal.** De tien balletjes zijn twee aan twee dezelfde tekening in een
andere kleur — behalve `BTNBLU.BMP` en `BTNBRN.BMP`. Die twee zijn ooit opnieuw
opgeslagen (ze hebben een `BITMAPINFOHEADER` van 40 bytes, de rest nog de oude
OS/2-kop van 12) en de blauwe is daarbij slechter geworden: donker marineblauw
met een uitgelopen rand die wegvalt tegen het grijs, en een vaag paars vlekje als
glans. Naast de andere acht las hij als een kleiner balletje, terwijl de bol
precies even groot is — het contrast met de achtergrond ontbrak gewoon.

De blauwe bal is daarom opnieuw gekleurd uit de **cyaan** bal, die dezelfde
tekening is maar dan netjes: tint 222°, verzadiging en helderheid ongemoeid. Zo
staat er nu een blauwe bal met dezelfde rand, dezelfde schaduw en dezelfde witte
glans als zijn acht buren. De grijze bal is wél goed en is niet aangeraakt.

**De spikkels weg.** Om elke bal lag een raster van blauwe, paarse en witte
stippen — de manier waarop een tekenprogramma uit die tijd een grijsverloop
maakte op een scherm met weinig kleuren. Op een scherm van nu is dat geen
verloop meer maar ruis, en bij twee of drie keer zo groot wordt het een
spikkeldeken. Elke stip buiten de bol heeft daarom het grijs van zijn omgeving
gekregen: het gewogen gemiddelde van de grijze beeldpunten eromheen, dichtbij
zwaarder wegend dan veraf. Het verloop van licht linksboven naar donker
rechtsonder blijft daardoor staan, en de schaduw onder de bal ook — die is
gewoon donkergrijs. De bol zelf is niet aangeraakt; daar hoort de witte glans
thuis.

De paaltjes hadden dit al niet: die waren altijd al effen grijs (191,191,191).

Daarmee zijn `bal0..9.png` en `knop1..9.png` bijgewerkt ten opzichte van 1995.
De **onaangeroerde bitmaps staan nog in de map `Mastermind\` zelf**, dus
teruggrijpen kan altijd.

**Waar zit de bol?** Dat wordt niet met de hand ingevuld maar afgeleid uit de
groene bal: fel groen komt alleen in de bol voor, en daaruit volgt de cirkel om
(13,5 , 13,5) met straal 7 (`Plaatjes.Masker`). Het uit het *verschil* tussen de
tegels afleiden werkte niet — ook de spikkelrand verschilt per bestand, zodat een
vlakvulling de hele tegel overloopt.

### 4.5 Drie uitvoeringen van de balletjes

Beeld → Tekening laat kiezen tussen drie stellen plaatjes:

| menu | map | wat |
|---|---|---|
| Gerekend (standaard) | `MastermindWin\Gerekend\` | uitgerekende bollen, 600 bij 600 |
| Met de hand getekend | `MastermindWin\Tekening\` | door Ed getekend, 600 bij 600 |
| Zoals in 1995 | `MastermindWin\Bronplaatjes\` | de bitmaps van toen, 30 bij 30 |

De keuze wordt onthouden. Ze zitten alle drie in `Mastermind.exe`; samen kosten ze
ruim vier megabyte, wat op een programma van 160 niet opvalt.

**Gerekend** (de standaard) staat in `MastermindWin\Gerekend\` en **met de hand
getekend** in `MastermindWin\Tekening\`: elk vakje is daar **600 bij 600
beeldpunten**, twintig keer de maat op het scherm. Het spel verkleint ze naar wat
er nodig is. Omdat er van groot naar
klein gerekend wordt, is de bol op élke maat rond en de glans op élke maat een
glans; hoe groter het venster, hoe meer er te zien is. Uit een bitmap van 29 bij
28 valt dat niet te halen, hoe je hem ook vergroot.

`Tekenaar\` maakt het gerekende stel (`dotnet run --project Tekenaar`, dat vult
alleen `Gerekend\` en laat het handwerk met rust). De bol wordt
per beeldpunt uitgerekend: een boloppervlak met licht van linksboven, een
glanspunt volgens Blinn-Phong, en wat weerlicht van rechtsonder — dat laatste
randje maakt dat een bol als een bol leest en niet als een platte schijf. Het
paaltje is op dezelfde manier rond gemaakt, als een staande rol.

Eronder ligt een **echte kuil met vier wanden**: de bovenwand en de linkerwand in
de schaduw, de onderwand en de rechterwand in het licht, met op de hoeken de
schuine naad die een bakje hoort te hebben. De knoppen van het invoerblokje staan
juist een beetje omhoog; het zijn knoppen, geen gaatjes.

Dat de vier wanden ook echt te zíen zijn, kost drie dingen, en ze zijn alle drie
nodig gebleken:

* **De vloer ligt er middenin qua grijs** (136 tot 158), tussen de donkere wanden
  (84 en 104) en de lichte (208 en 196) in. Die lichte wanden zijn grijs en niet
  wit — ongeveer de tint van het paaltjesvlak (191); wit trekt naast de balletjes
  te veel aandacht en het moet een wandje blijven, geen lichtbak. Een verloop van donker linksboven naar
  licht rechtsonder — de eerste versie — lijkt logisch, maar dan raakt de vloer aan
  één kant de kleur van de wand daar, en valt dat paar wanden weg. De kuil lijkt
  dan open te staan, en omdat er nog maar twee wanden opvallen lijkt de bal ook
  niet meer in het midden te liggen terwijl hij dat wel is.
* **Een wand is een vlak bandje** in zijn volle kleur, dat pas aan de binnenkant
  overloopt in de vloer. Vervaagt hij over zijn hele breedte, dan haalt hij op ware
  grootte — waar een wand nog geen drie beeldpunten breed is — zijn eigen kleur
  nergens.
* **De lijst om de kuil is donkerder dan de verlichte wanden** (164). Is hij even
  licht, dan lopen de onder- en rechterwand erin over en lees je ze niet als wand
  van dít vakje maar als de scheidslijn met het volgende — waardoor het bakje aan
  die twee kanten open lijkt te staan.

**Tekenen op ware grootte, niet op ware afmeting.** Er zat een valstrik in het
tekenen zelf. `Graphics.DrawImageUnscaled` klinkt als "zet deze afbeelding
beeldpunt voor beeldpunt neer", maar het tekent op ware *fysieke* grootte: het
rekent de punten per duim van de afbeelding om naar die van waar het heen gaat.
Een PNG meldt 95,99 punten per duim, en de vellen waarop het spel tekent staan op
de puntdichtheid van het scherm. Op een scherm van 120 werd elke tegel daardoor
120/96 = 1,25 keer zo groot getekend, en viel er een vijfde rechts en een vijfde
onderaan buiten het vakje — precies de rechter- en onderwand van het bakje. Alle
tegels gaan nu door `Plaatjes.Teken`, dat de rechthoeken in beeldpunten opgeeft.

Deze fout was zo hardnekkig omdat hij **alleen in het venster zat**: de tekening
op schijf klopte, het terugrekenen naar 30 bij 30 klopte, en toch stond het
verkeerd op het scherm. Daarom kan het spel nu zijn eigen tegels uitschrijven:

    Mastermind.exe tegels blad.png 1

Dat zet alle vakjes naast elkaar zoals `Plaatjes` ze aan het bord geeft. Wijkt dat
af van de tekening zelf, dan zit het mis in het spel; is het gelijk, dan zit het
mis in de tekening. Met een schermafdruk kom je daar niet achter — die moet je
uitknippen en op het raster leggen, en dan meet je net zo goed de omlijning of het
verkeerde vakje. `Meet` in de kladmap drukt daarnaast een tegel op ware grootte af
als grijswaarden.

De randen in de tekening vloeien maar over **twee beeldpunten** uit (`Zoom`). Het
gladstrijken hoort van het verkleinen te komen, dat immers over het hele blok
middelt; vloeide de tekening zelf al een halve eenheid uit, dan stond er bij twee
of drie keer zo groot een zichtbare zoom om alles.

De **kleuren en de vormen komen uit de bitmaps van 1995**: de bal is even groot
(straal 7 van de 30), het paaltje heeft dezelfde omtrek, en de negen kleuren zijn
uit de oude balletjes gemeten. Het bord ziet er dus hetzelfde uit; alleen de
tekening is beter.

**Een vakje is vierkant**: 30 bij 30 op ware grootte. In 1995 was het 29 bij 28,
net niet vierkant, wat bij het tekenen met de hand alleen maar in de weg zit. De
bitmaps van 1995 zijn daarom vierkant gemaakt met
`dotnet run --project Tekenaar -- vierkant`: links een kolom en bovenaan een rij
erbij, gekopieerd van de lijst die er al lag. Geen bestaand beeldpunt verandert.
Links en boven is geen willekeurige keuze: de oude bal stond op (13,5 , 13,5) en
komt daarmee op (14,5 , 14,5) van de 30 — vrijwel het midden, en dus zo goed als
op dezelfde plek als de bal in de nieuwe tekening. De opdracht slaat over wat al
vierkant is, dus twee keer draaien kan geen kwaad.

**Zoals in 1995** zijn de bitmaps uit `Bronplaatjes\`, beeldpunt voor beeldpunt
zoals ze toen waren (met de blauwe bal en de spikkels uit 4.4, en de lijst van 4.5
erbij). Vergroten gebeurt
daar met vierkante blokjes: alles wat je daartussen verzint is niet wat er stond.

**Verkleinen gebeurt door te middelen** over het hele blok beeldpunten dat op één
nieuw beeldpunt terechtkomt (`Plaatjes.Verkleind`), met deelblokjes naar
verhouding. Elk beeldpunt van de tekening telt zo precies één keer mee. Een
filter dat maar naar een paar buren kijkt — het gewone bicubic — laat bij twintig
keer verkleinen het grootste deel van de tekening links liggen en gaat rammelen
zodra de maat verandert.

### 4.5.1 Grootte en schermen met veel beeldpunten

De maat van een vakje is 30 bij 30 beeldpunten. Op een 4K-scherm is een venster
van 474 bij 570 beeldpunten een postzegel. Beeld → Grootte vergroot alles 1, 2 of
3 keer; op **Automatisch** kiest het programma zelf aan de hand van het scherm.
Een zelf gekozen grootte wordt nooit groter gemaakt dan het scherm aankan: het
venster heeft een vaste maat, dus wat eraf valt zou onbereikbaar zijn.

### 4.6 De tekeningen met de hand bijwerken

**Er is ook een met de hand getekend stel.** Ed heeft de balletjes in augustus
2026 overgetekend op 600 bij 600 en ze staan in `MastermindWin\Tekening\`. Het
zijn geen gerekende bollen maar geschilderde: een bol met een paar tinten en een
witte glans, liggend in een met de hand geschilderde kuil. Het gerekende stel is
niet weg — dat staat in `MastermindWin\Gerekend\` en is in het menu te kiezen.

`Tekening\` is meteen de werkmaat. Werk een bestand daar bij in een
tekenprogramma, draai `bouw-exe.cmd`, en het zit in het spel. Verkleinen hoeft
niet — dit ís de maat waarop het spel de tekening bewaart. Houd de maat op 600 bij
600 en de bal ongeveer in het midden met een straal van rond de 150 beeldpunten.

Twee dingen om te weten:

* **Het invoerblokje van negen wordt uit de balletjes gemaakt.** Teken je een bal
  opnieuw, draai dan `dotnet run --project Tekenaar -- knoppen`. Dat knipt de bol
  uit `bal1..bal9` en zet hem op een vlak plaatje: een knop om op te drukken in
  plaats van een gaatje om een bal in te leggen. De cirkel waarmee geknipt wordt
  staat als `HandBalX/Y/R` in `Tekenaar\Program.cs` en is opgemeten aan de
  getekende ballen; verplaats je de bol flink, pas die dan aan.
* **Het rekenwerk komt niet meer in `Tekening\` terecht.** `dotnet run --project
  Tekenaar` vult `Gerekend\`. Wil je het handwerk toch laten overschrijven, dan
  moet je die map noemen én `overschrijf` erbij zeggen.

`Handwerk\` is het beginpunt om zelf te tekenen: de balletjes van 1995, blokkerig
uitvergroot tot dezelfde 600 bij 600, zodat je ziet wat er stond en welke kleuren
het waren. Opnieuw aan te maken met `dotnet run --project Tekenaar -- handwerk`.

Wil je juist de **kleine bitmaps van 1995 zelf** bijwerken, dan zet
`Handwerkerklein.ps1` de bestanden twintig keer verkleind terug in
`Bronplaatjes\`, middelend over elk blokje van 20 bij 20. Heen en weer zonder iets
te wijzigen levert exact dezelfde beeldpunten op — dat is nagerekend, nul verschil.

### 4.7 Onthouden tussen twee keer spelen

Taal, grootte, tekening en geluid komen in
`%APPDATA%\Mastermind\instellingen.txt`. Het
programma zelf en de map waarin het staat blijven onaangeroerd, zodat Mastermind
ook vanaf een geheugenstick werkt. Weghalen van dat bestandje zet alles terug op
de standaardwaarden.

### 4.8 Wat eruit is

* Het menu **File / Edit / Search** van de OWL-sjabloon deed niets (alles stond
  op `GRAYED`) en is weg. De knoppenbalk en de statusregel eronder ook.
* Het **Over-venster** las de VERSIONINFO-hulpbron uit (`ProjectRCVersion` in
  `MSTRMABD.CPP`); dat is nu een gewone melding.
* Het **hulpbestand** `MASTERMI.HLP` is een WinHelp-bestand; WinHelp bestaat niet
  meer onder Windows 11. De inhoud staat nu onder Help → Spelregels.
* De **knop End Game** uit een oudere schermversie (`mstrmapp.rc.txt`) zat al
  niet meer in de gebruikte versie.
* `MCI_PLAY` via `mciSendCommand` is vervangen door `System.Media.SoundPlayer`.
  Het geluidsbestand zit nu in het programma zelf, dus het kan niet meer
  ontbreken ("File Found.wav not found" bestaat niet meer).

### 4.9 Fouten in het origineel die hier niet terugkomen

* `GekozenGetal` is `char[5]`, maar `BNClickedNewGame` eindigde met
  `GekozenGetal[m]=0` waarbij `m` op dat moment 5 is: één byte voorbij het
  einde. `BNClickedSolution` deed daarna `strcpy` op dat niet-afgesloten veld.
* `BNClickedClear` schreef met dezelfde `m` ook `MMarray[regel][5]` — dat viel
  binnen de rij (`MAXCOL+1`), maar per ongeluk.
* `SetPaaltjes` gebruikte `m` na de lus om verder te tellen; dat werkte alleen
  doordat de lus altijd afliep.
* `DeleteDC` op een handvat van `GetDC` moest `ReleaseDC` zijn. Onder Windows
  3.1 lekte daar bij elke tekenbeurt een handvat weg.
* Bij elke tekenbeurt werd elke bitmap opnieuw uit de hulpbronnen geladen
  (`LoadBitmap` in de lus). Nu worden ze één keer geladen en bewaard.

## 5. Controles

`test-motor.cmd` draait `MastermindTest`, dat dezelfde `Engine\` gebruikt als het
spel:

* **`test`** — bekende gokken tegen bekende codes, en 200 000 willekeurige
  regels met de eis dat er nooit meer paaltjes uit komen dan er plaatsen zijn.
  Daarna het spelverloop: winnen, verliezen na zeventien regels, invoeren,
  terug, plaats kiezen en wissen.
* **`oud`** — het tellen van 1995 naast dat van nu.
* **`oplos [aantal] [zaad]`** — de computer raadt zelf. Hij houdt alle 15 120
  mogelijke codes bij die nog met de paaltjes kloppen en gokt er telkens één
  van. Over 500 spellen: gemiddeld 5,8 regels, in het slechtste geval 8. De
  zeventien regels van 1995 zijn dus ruim bemeten.

Wijzig je iets aan `Engine\`, draai dan `test-motor` en `test-motor oplos 2000`.

## 6. Bouwen

De .NET 8 SDK staat in `%USERPROFILE%\.dotnet` en niet systeembreed; `bouw-exe.cmd`
weet dat. Het maakt een **self-contained** `Mastermind-app\Mastermind.exe`, één
bestand dat draait zonder dat .NET geïnstalleerd is. Bouw je zelf met
`dotnet build`, dan start de exe alleen op een machine met de .NET 8 desktop
runtime — die staat hier niet, dus gebruik dan `dotnet MastermindWin.dll` met
`DOTNET_ROOT` gezet.
