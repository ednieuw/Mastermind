# De balletjes van 1995 om met de hand op door te tekenen

Dit is de **werkmaat**: de balletjes en paaltjes van 1995, uitvergroot tot 600 bij
600, om ze in een tekenprogramma over te tekenen of bij te werken. Ze staan er
blokkerig op — elk beeldpunt van toen is een blok van 20 bij 20 — zodat je precies
ziet wat er stond en welke kleuren het waren.

**Klaar met een vakje? Zet het in `MastermindWin\Tekening\`.** Dat is de map die
het spel standaard tekent, en die ligt al op dezelfde 600 bij 600. Verkleinen hoeft
niet; het spel doet dat zelf, en beter dan een tekenprogramma het zou doen.

Heb je een **bal** opnieuw getekend, draai dan daarna
`dotnet run --project Tekenaar -- knoppen`. Het invoerblokje van negen wordt uit de
balletjes gemaakt: dezelfde bol, maar op een vlak plaatje in plaats van in een
kuiltje. Anders staat op het bord je nieuwe bal en op de knoppen nog de oude.

Hier staan alle tegels **20 keer zo groot**: 600 bij 600 beeldpunten, waarbij elk
beeldpunt van 1995 een blokje van 20 bij 20 is. Het zijn de tegels zoals ze nu in
`Bronplaatjes\` staan (spikkels weggehaald, blauwe bal opnieuw gekleurd, en links
en bovenaan een beeldpunt lijst erbij om het vakje vierkant te maken).

Opnieuw aan te maken met `dotnet run --project Tekenaar -- handwerk`.

| bestand | wat het is |
|---|---|
| `bal0.png` | het lege vakje: het zwarte balletje |
| `bal1.png` … `bal9.png` | de negen kleuren op het bord |
| `knop1.png` … `knop9.png` | dezelfde negen voor het invoerblokje |
| `paal0.png` | groen paaltje: goede kleur op de goede plaats |
| `paal1.png` | rood paaltje: goede kleur op de verkeerde plaats |
| `paalleeg.png` | leeg paaltjesvakje |

## Waar je op moet letten

* **De maat moet 600 bij 600 blijven.** Een vakje is vierkant: 30 bij 30 op ware
  grootte, twintig keer vergroot.
* **Teken gerust met zachte randen.** Bij het verkleinen wordt er gemiddeld over
  elk blokje van 20 bij 20, dus een met de hand gladgemaakte rand komt er als
  een nette overgang uit. Blokkerig tekenen mag ook: dan blijft het beeldpunt
  voor beeldpunt zoals jij het zet.
* **Houd de bol ongeveer in het midden**, met een straal van rond de 140
  beeldpunten. Dan blijft hij even groot als op alle andere vakjes.
* `bal0` is geen kleur maar het **lege vakje**; het spel zet dat balletje neer
  waar nog niets geraden is.

## Terugzetten in het spel

**Maak je hier een mooie tekening van, zet hem dan in
`MastermindWin\Tekening\`.** Dat is de map die het spel standaard tekent, en die
ligt al op 600 bij 600 — verkleinen hoeft dus niet. Daarna `bouw-exe.cmd` en het
zit in `Mastermind.exe`.

> Let op: `dotnet run --project Tekenaar` (zonder opdracht) tekent `Tekening\`
> helemaal opnieuw en overschrijft daarmee je eigen werk. Bewaar je tekeningen
> ook ergens anders.

`verklein.ps1` is er voor het andere geval: wil je de **kleine bitmaps van 1995
zelf** bijwerken (menu Beeld → Tekening → Zoals in 1995), dan zet dat script de
bestanden hier twintig keer verkleind terug in `MastermindWin\Bronplaatjes\`.
Er wordt gemiddeld over elk blokje van 20 bij 20, dus een met de hand gladgemaakte
rand komt er als een nette overgang uit.

De **onaangeroerde bitmaps van 1995** staan nog in de map `Mastermind\` zelf:
`BTNBLK.BMP`, `BTNBLU.BMP`, … , `PAAL0.BMP`. Daar is niets aan veranderd, dus
teruggrijpen kan altijd.
