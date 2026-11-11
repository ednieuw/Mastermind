namespace Mastermind.Engine;

/// <summary>Stand van het spel.</summary>
public enum Stand
{
    /// <summary>Er kan nog geraden worden.</summary>
    Bezig,
    /// <summary>Alle vijf de kleuren staan op de goede plaats.</summary>
    Gewonnen,
    /// <summary>Alle regels zijn op zonder dat de code geraden is.</summary>
    Verloren
}

/// <summary>
/// Het spel zelf: de te raden code, de geraden regels en de paaltjes ernaast.
/// Deze klasse kent geen scherm — hij hangt nergens van WinForms af, zodat het
/// rekenwerk ook zonder venster te draaien is (zie MastermindTest).
///
/// Herkomst: MSTTDLGC.CPP uit het Borland C-programma van 1995. De velden
/// MMarray, GekozenGetal, PosGoed, KleurGoed, regel en pos_in_regel van toen
/// heten hier Bord, geheim, PosGoed, KleurGoed, Regel en PosInRegel.
/// </summary>
public sealed class MmSpel
{
    /// <summary>Aantal regels dat de speler mag raden (MAXROW uit het origineel).</summary>
    public const int MaxRegels = 17;

    /// <summary>Lengte van de code (MAXCOL uit het origineel).</summary>
    public const int MaxKolommen = 5;

    /// <summary>De kleuren heten 1..9; net als in het origineel.</summary>
    public const int AantalKleuren = 9;

    /// <summary>Een nog niet ingevulde plaats. In het origineel het teken '0' (het zwarte balletje).</summary>
    public const int Leeg = 0;

    // Regel MaxRegels is de extra regel onderaan waarin de oplossing komt te
    // staan; die telt niet mee als raadbeurt. Zo deed het origineel het ook
    // (strcpy(MMarray[MAXROW], GekozenGetal)).
    private readonly int[,] bord = new int[MaxRegels + 1, MaxKolommen];
    private readonly int[] posGoed = new int[MaxRegels];
    private readonly int[] kleurGoed = new int[MaxRegels];
    private readonly int[] geheim = new int[MaxKolommen];
    private readonly Random toeval;

    /// <summary>Begint meteen een spel. Zonder zaad is elke partij anders.</summary>
    public MmSpel(int? zaad = null)
    {
        toeval = zaad.HasValue ? new Random(zaad.Value) : new Random();
        NieuwSpel();
    }

    /// <summary>De regel waarop nu geraden wordt, 0..MaxRegels-1.</summary>
    public int Regel { get; private set; }

    /// <summary>De plaats binnen die regel waar het volgende balletje komt, 0..MaxKolommen.</summary>
    public int PosInRegel { get; private set; }

    /// <summary>Bezig, gewonnen of verloren.</summary>
    public Stand Stand { get; private set; }

    /// <summary>Staat de oplossing onderaan het bord?</summary>
    public bool OplossingZichtbaar { get; private set; }

    /// <summary>Hoeveel regels de speler nodig had; alleen zinvol na winst.</summary>
    public int GebruikteRegels => Regel;

    /// <summary>Kleur op een plaats van het bord; regel MaxRegels is de oplossingsregel.</summary>
    public int this[int regel, int kolom] => bord[regel, kolom];

    /// <summary>Aantal groene paaltjes (goede kleur op de goede plaats) bij een beoordeelde regel.</summary>
    public int PosGoedVan(int regel) => posGoed[regel];

    /// <summary>Aantal rode paaltjes (goede kleur, verkeerde plaats) bij een beoordeelde regel.</summary>
    public int KleurGoedVan(int regel) => kleurGoed[regel];

    /// <summary>Is deze regel al beoordeeld, en horen er dus paaltjes bij?</summary>
    public bool IsBeoordeeld(int regel) => regel < Regel;

    /// <summary>De te raden code. Alleen voor het scherm ná afloop en voor de test.</summary>
    public IReadOnlyList<int> Geheim => geheim;

    /// <summary>Staan er vijf kleuren op de huidige regel?</summary>
    public bool RegelVol
    {
        get
        {
            for (int m = 0; m < MaxKolommen; m++)
                if (bord[Regel, m] == Leeg) return false;
            return true;
        }
    }

    // --------------------------------------------------------------- spelverloop

    /// <summary>
    /// Wist het bord en trekt een nieuwe code: vijf verschillende kleuren uit
    /// 1..9, precies zoals BNClickedNewGame in het origineel.
    /// </summary>
    public void NieuwSpel()
    {
        Stand = Stand.Bezig;
        OplossingZichtbaar = false;
        Regel = 0;
        PosInRegel = 0;

        for (int n = 0; n <= MaxRegels; n++)
        {
            if (n < MaxRegels) { posGoed[n] = 0; kleurGoed[n] = 0; }
            for (int m = 0; m < MaxKolommen; m++) bord[n, m] = Leeg;
        }

        // Het origineel trok net zo lang een nieuw cijfer tot het nog niet
        // voorkwam; dat komt op hetzelfde neer als vijf grepen uit een geschudde
        // zak, maar dan zonder de kans op eindeloos herhalen.
        Span<int> zak = stackalloc int[AantalKleuren];
        for (int k = 0; k < AantalKleuren; k++) zak[k] = k + 1;
        for (int k = AantalKleuren - 1; k > 0; k--)
        {
            int j = toeval.Next(k + 1);
            (zak[k], zak[j]) = (zak[j], zak[k]);
        }
        for (int m = 0; m < MaxKolommen; m++) geheim[m] = zak[m];
    }

    /// <summary>
    /// Zet een kleur op de huidige plaats en schuift een plaats op
    /// (Controle_Invoer uit het origineel).
    /// </summary>
    public bool Invoeren(int kleur)
    {
        if (Stand != Stand.Bezig) return false;
        if (kleur < 1 || kleur > AantalKleuren) return false;
        if (PosInRegel >= MaxKolommen) return false;

        bord[Regel, PosInRegel++] = kleur;
        return true;
    }

    /// <summary>Kiest de plaats waar het volgende balletje komt, 0..MaxKolommen-1.</summary>
    public bool Positie(int kolom)
    {
        if (Stand != Stand.Bezig) return false;
        if (kolom < 0 || kolom >= MaxKolommen) return false;

        PosInRegel = kolom;
        return true;
    }

    /// <summary>Haalt het balletje vóór de invoerplaats weer weg (backspace).</summary>
    public bool Terug()
    {
        if (Stand != Stand.Bezig || PosInRegel == 0) return false;

        bord[Regel, --PosInRegel] = Leeg;
        return true;
    }

    /// <summary>Maakt de hele regel leeg (BNClickedClear uit het origineel).</summary>
    public bool Wissen()
    {
        if (Stand != Stand.Bezig) return false;

        for (int m = 0; m < MaxKolommen; m++) bord[Regel, m] = Leeg;
        PosInRegel = 0;
        return true;
    }

    /// <summary>
    /// Beoordeelt de huidige regel, zet de paaltjes en gaat een regel verder
    /// (BNClickedOK uit het origineel). Geeft terug of er iets beoordeeld is.
    /// </summary>
    public bool Beoordelen()
    {
        if (Stand != Stand.Bezig) return false;

        Span<int> gok = stackalloc int[MaxKolommen];
        for (int m = 0; m < MaxKolommen; m++) gok[m] = bord[Regel, m];

        Tel(gok, geheim, out int zwart, out int wit);
        posGoed[Regel] = zwart;
        kleurGoed[Regel] = wit;

        Regel++;
        PosInRegel = 0;

        if (zwart == MaxKolommen) Stand = Stand.Gewonnen;
        else if (Regel >= MaxRegels) Stand = Stand.Verloren;

        return true;
    }

    /// <summary>Zet de code in de extra regel onderaan (BNClickedSolution).</summary>
    public void ToonOplossing()
    {
        for (int m = 0; m < MaxKolommen; m++) bord[MaxRegels, m] = geheim[m];
        OplossingZichtbaar = true;
    }

    // ------------------------------------------------------------------ tellen

    /// <summary>
    /// Telt de paaltjes volgens de gewone mastermindregels: eerst de goede
    /// kleuren op de goede plaats, en van wat overblijft per kleur zoveel rode
    /// paaltjes als er aan beide kanten van zijn. Elk balletje van de code telt
    /// dus hoogstens één keer mee. Lege plaatsen tellen nergens voor.
    /// </summary>
    public static void Tel(ReadOnlySpan<int> gok, ReadOnlySpan<int> geheim, out int posGoed, out int kleurGoed)
    {
        Span<int> overGok = stackalloc int[AantalKleuren + 1];
        Span<int> overGeheim = stackalloc int[AantalKleuren + 1];

        posGoed = 0;
        for (int m = 0; m < gok.Length; m++)
        {
            if (gok[m] != Leeg && gok[m] == geheim[m]) { posGoed++; continue; }
            if (gok[m] != Leeg) overGok[gok[m]]++;
            if (geheim[m] != Leeg) overGeheim[geheim[m]]++;
        }

        kleurGoed = 0;
        for (int k = 1; k <= AantalKleuren; k++)
            kleurGoed += Math.Min(overGok[k], overGeheim[k]);
    }

    /// <summary>
    /// Hoe het origineel telde. Wordt in het spel niet gebruikt; hij staat hier
    /// zodat MastermindTest kan laten zien waar oud en nieuw uiteenlopen. Het
    /// origineel telde elk balletje van de code opnieuw voor elke gok die erop
    /// leek, zodat vijf gelijke balletjes meer paaltjes konden opleveren dan er
    /// plaatsen zijn.
    /// </summary>
    public static void TelZoalsOrigineel(ReadOnlySpan<int> gok, ReadOnlySpan<int> geheim, out int posGoed, out int kleurGoed)
    {
        posGoed = 0;
        kleurGoed = 0;
        for (int m = 0; m < gok.Length; m++)
        {
            if (gok[m] == geheim[m]) posGoed++;
            for (int n = 0; n < geheim.Length; n++)
                if (gok[m] == geheim[n] && m != n) kleurGoed++;
        }
    }
}
