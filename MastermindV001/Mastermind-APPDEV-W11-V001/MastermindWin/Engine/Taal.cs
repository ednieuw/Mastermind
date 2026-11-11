namespace Mastermind.Engine;

/// <summary>
/// Alle teksten die de speler te zien krijgt, in het Nederlands en het Engels.
/// Zowel het spel als het scherm halen hun teksten hier op, zodat er bij het
/// omschakelen niets in de verkeerde taal kan achterblijven.
/// Het oorspronkelijke programma uit 1995 was Engels; dat is hier de standaard.
/// </summary>
public static class Taal
{
    /// <summary>false = Nederlands, true = Engels. Engels is de standaard, net als in 1995.</summary>
    public static bool Engels { get; set; } = true;

    private static string T(string nl, string en) => Engels ? en : nl;

    // -------------------------------------------------------------------- menu
    public static string MenuSpel => T("&Spel", "&Game");
    public static string MenuNieuw => T("&Nieuw spel", "&New game");
    public static string MenuOplossing => T("&Oplossing tonen", "Show s&olution");
    public static string MenuAfsluiten => T("&Afsluiten", "E&xit");

    public static string MenuKopieer => T("Regels &kopiëren", "&Copy rows");

    public static string MenuBeeld => T("&Beeld", "&View");
    public static string MenuSchaal => T("&Grootte", "&Size");
    public static string MenuSchaalAuto => T("&Automatisch", "&Automatic");
    public static string MenuSchaalN(int n) => T($"{n} keer zo groot", $"{n}x");
    public static string MenuGeluid => T("Ge&luid", "&Sound");
    public static string MenuTekening => T("&Tekening", "&Artwork");
    public static string MenuGetekend => T("Met de hand ge&tekend", "&Hand-drawn");
    public static string MenuGerekend => T("Ge&rekend", "&Computed");
    public static string MenuOrigineel1995 => T("Zoals in &1995", "As in &1995");

    public static string MenuTaal => T("&Taal", "&Language");
    public static string MenuNederlands => "&Nederlands";
    public static string MenuEngels => "&English";

    public static string MenuHelp => T("&Help", "&Help");
    public static string MenuSpelregels => T("&Spelregels", "How to &play");
    public static string MenuOver => T("&Over Mastermind...", "&About Mastermind...");

    // ------------------------------------------------------------------ scherm
    public static string Titel => "Mastermind";
    public static string NieuwSpel => T("Nieuw spel", "Start New Game");
    public static string Oplossing => T("Oplossing", "Solution");
    public static string Wissen => T("Wissen", "Clear");
    public static string Goed => T("Klaar", "OK");
    public static string Plaats => T("Plaats", "Position");

    public static string RegelVanTotaal(int regel, int totaal) =>
        T($"Regel {regel} van {totaal}", $"Row {regel} of {totaal}");

    public static string KiesEenKleur => T("Kies vijf kleuren en druk op Klaar",
                                           "Pick five colours and press OK");
    public static string GewonnenIn(int regels) =>
        T(regels == 1 ? "Geraden in één regel!" : $"Geraden in {regels} regels!",
          regels == 1 ? "Cracked in one row!" : $"Cracked in {regels} rows!");
    public static string Verloren => T("De regels zijn op. Dit was de code.",
                                       "Out of rows. This was the code.");

    // --------------------------------------------------------------- meldingen
    public static string GewonnenTitel => T("Gewonnen", "You win");
    public static string VerlorenTitel => T("Game over", "Game over");
    public static string VerlorenTekst => T("De code is niet geraden. Nog een spel?",
                                            "The code was not cracked. Another game?");
    public static string GewonnenTekst(int regels) =>
        T($"De code is geraden in {regels} regels. Nog een spel?",
          $"You cracked the code in {regels} rows. Another game?");

    public static string IetsMisMaarLooptDoor =>
        T("Er ging iets mis, maar het programma loopt door.",
          "Something went wrong, but the program keeps running.");
    public static string VolledigeMeldingIn =>
        T("De volledige melding staat in:", "The full message is in:");

    // ---------------------------------------------------------------- over/uitleg
    public static string OverTekst =>
        T("Mastermind\n\n" +
          "Windows 11-uitvoering van het programma dat Ed Nieuwenhuys en Ronald Loggen\n" +
          "in 1995 in Borland C++ met OWL schreven.\n\n" +
          "De balletjes en paaltjes zijn de oorspronkelijke afbeeldingen uit 1995.",
          "Mastermind\n\n" +
          "Windows 11 edition of the program Ed Nieuwenhuys and Ronald Loggen wrote\n" +
          "in Borland C++ with OWL in 1995.\n\n" +
          "The balls and pegs are the original 1995 images.");

    public static string SpelregelsTekst =>
        T("De computer kiest vijf verschillende kleuren uit negen. Raad ze in de\n" +
          "goede volgorde.\n\n" +
          "Klik een kleur uit het blokje van negen om hem op de volgende plaats te\n" +
          "zetten, of klik eerst op een plaats (1 tot en met 5) of op een vakje in de\n" +
          "regel zelf. Met Klaar wordt de regel beoordeeld.\n\n" +
          "Naast de regel verschijnen paaltjes:\n" +
          "   groen = goede kleur op de goede plaats\n" +
          "   rood  = goede kleur op de verkeerde plaats\n\n" +
          "De paaltjes zeggen niet wélke kleur goed staat. Er zijn zeventien regels.\n\n" +
          "Toetsen: 1 tot en met 9 zetten een kleur, pijltjes kiezen een plaats,\n" +
          "Backspace haalt er een weg, Enter is Klaar, Esc wist de regel, F2 begint\n" +
          "een nieuw spel.",
          "The computer picks five different colours out of nine. Guess them in the\n" +
          "right order.\n\n" +
          "Click a colour in the block of nine to put it on the next position, or first\n" +
          "click a position (1 to 5) or a square in the row itself. OK judges the row.\n\n" +
          "Pegs appear beside the row:\n" +
          "   green = right colour in the right place\n" +
          "   red   = right colour in the wrong place\n\n" +
          "The pegs do not say which colour is right. You get seventeen rows.\n\n" +
          "Keys: 1 to 9 place a colour, arrows pick a position, Backspace removes one,\n" +
          "Enter is OK, Esc clears the row, F2 starts a new game.");
}
