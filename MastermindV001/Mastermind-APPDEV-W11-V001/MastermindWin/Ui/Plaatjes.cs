using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using Mastermind.Engine;

namespace Mastermind.Ui;

/// <summary>
/// De balletjes en de paaltjes, in drie uitvoeringen.
///
///   <b>Gerekend</b> (de standaard) — uitgerekende bollen met licht en glans, uit
///     <c>Gerekend\</c>.
///
///   <b>Getekend</b> — met de hand getekend, uit <c>Tekening\</c>.
///
///   <b>Zoals in 1995</b> — de bitmaps uit <c>Bronplaatjes\</c>, tot op het
///     beeldpunt zoals ze toen waren. Vergroten gebeurt met vierkante blokjes,
///     want alles wat je daar tussenin verzint is niet wat er stond.
///
/// De eerste twee zijn 600 bij 600 beeldpunten per vakje en worden verkleind naar
/// de maat die op het scherm nodig is. Omdat er van groot naar klein gerekend
/// wordt, is de bol op elke maat rond en de glans op elke maat een glans; hoe
/// groter het venster, hoe meer er te zien is.
///
/// Een vakje is 30 bij 30: vierkant, zodat een bal een bal blijft en het tekenen
/// met de hand niet met twee maten hoeft te werken. De bitmaps van 1995 waren
/// 29 bij 28 en hebben er links en bovenaan een beeldpunt lijst bij gekregen.
///
/// Verkleinen gebeurt door te middelen over het hele blok beeldpunten dat op één
/// nieuw beeldpunt terechtkomt. Dat is het eerlijke gemiddelde: elk beeldpunt van
/// de tekening telt precies één keer mee. Een filter dat maar naar een paar buren
/// kijkt, laat bij twintig keer verkleinen de helft van de tekening links liggen
/// en gaat rammelen zodra er iets beweegt.
/// </summary>
internal static class Plaatjes
{
    /// <summary>Breedte van een vakje op ware grootte.</summary>
    public const int TegelBreed = 30;

    /// <summary>Hoogte van een vakje op ware grootte. Een vakje is vierkant.</summary>
    public const int TegelHoog = 30;

    /// <summary>Groen paaltje: goede kleur op de goede plaats.</summary>
    public const int PaalGroen = 0;

    /// <summary>Rood paaltje: goede kleur op de verkeerde plaats.</summary>
    public const int PaalRood = 1;

    /// <summary>Leeg paaltjesvakje.</summary>
    public const int PaalLeeg = 2;

    private static readonly Dictionary<string, Bitmap> Bronnen = new();
    private static readonly Dictionary<(string, int, Tekenwijze), Bitmap> Geschaald = new();

    private static Tekenwijze wijze = Tekenwijze.Gerekend;

    /// <summary>Welke uitvoering er getekend wordt.</summary>
    public static Tekenwijze Wijze
    {
        get => wijze;
        set
        {
            if (wijze == value) return;
            wijze = value;
            VergeetGeschaald();
        }
    }

    /// <summary>Een balletje voor op het bord; kleur 0 is het lege zwarte balletje.</summary>
    public static Bitmap Bal(int kleur, int schaal) => Haal($"bal{kleur}", schaal);

    /// <summary>Een balletje voor het invoerblokje van negen.</summary>
    public static Bitmap Knop(int kleur, int schaal) => Haal($"knop{kleur}", schaal);

    /// <summary>Een paaltje naast de regel.</summary>
    public static Bitmap Paal(int soort, int schaal) => Haal(
        soort switch { PaalGroen => "paal0", PaalRood => "paal1", _ => "paalleeg" }, schaal);

    private static Bitmap Haal(string naam, int schaal)
    {
        if (schaal < 1) schaal = 1;
        if (Geschaald.TryGetValue((naam, schaal, wijze), out var klaar)) return klaar;

        Bitmap bron = Bron(naam);
        int breed = TegelBreed * schaal, hoog = TegelHoog * schaal;

        Bitmap doel;
        if (bron.Width == breed && bron.Height == hoog)
            doel = bron;                                   // precies goed, niets te doen
        else if (bron.Width > breed)
            doel = Verkleind(bron, breed, hoog);           // de grote tekening
        else
            doel = Blokjes(bron, schaal);                  // de bitmap van 1995

        Geschaald[(naam, schaal, wijze)] = doel;
        return doel;
    }

    private static void VergeetGeschaald()
    {
        foreach (var b in Geschaald.Values)
            if (!Bronnen.ContainsValue(b)) b.Dispose();
        Geschaald.Clear();
    }

    // -------------------------------------------------------------- verkleinen

    /// <summary>
    /// Verkleint door te middelen over elk blok beeldpunten dat op één nieuw
    /// beeldpunt valt. Randen die maar half in een blok vallen tellen naar
    /// verhouding mee, zodat er ook bij een niet-passende maat niets rammelt.
    /// </summary>
    private static Bitmap Verkleind(Bitmap bron, int breed, int hoog)
    {
        int bw = bron.Width, bh = bron.Height;
        BitmapData data = bron.LockBits(new Rectangle(0, 0, bw, bh),
                                        ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        byte[] bytes = new byte[data.Stride * bh];
        Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
        int stride = data.Stride;
        bron.UnlockBits(data);

        var doel = new Bitmap(breed, hoog, PixelFormat.Format32bppPArgb);
        double sx = (double)bw / breed, sy = (double)bh / hoog;

        for (int y = 0; y < hoog; y++)
        {
            double v0 = y * sy, v1 = (y + 1) * sy;
            int j0 = (int)Math.Floor(v0), j1 = (int)Math.Ceiling(v1) - 1;

            for (int x = 0; x < breed; x++)
            {
                double u0 = x * sx, u1 = (x + 1) * sx;
                int i0 = (int)Math.Floor(u0), i1 = (int)Math.Ceiling(u1) - 1;

                double r = 0, g = 0, b = 0, gewicht = 0;
                for (int j = j0; j <= j1 && j < bh; j++)
                {
                    double hj = Math.Min(v1, j + 1) - Math.Max(v0, j);
                    if (hj <= 0) continue;

                    for (int i = i0; i <= i1 && i < bw; i++)
                    {
                        double wi = Math.Min(u1, i + 1) - Math.Max(u0, i);
                        if (wi <= 0) continue;

                        double w = wi * hj;
                        int p = j * stride + i * 4;
                        b += bytes[p] * w;
                        g += bytes[p + 1] * w;
                        r += bytes[p + 2] * w;
                        gewicht += w;
                    }
                }

                if (gewicht <= 0) continue;
                doel.SetPixel(x, y, Color.FromArgb(
                    (int)Math.Round(r / gewicht), (int)Math.Round(g / gewicht), (int)Math.Round(b / gewicht)));
            }
        }

        return doel;
    }

    /// <summary>
    /// Tekent een afbeelding beeldpunt voor beeldpunt op zijn plek.
    ///
    /// Dit moet met opgegeven rechthoeken in beeldpunten. <c>DrawImageUnscaled</c>
    /// lijkt hetzelfde te doen, maar tekent op ware *fysieke* grootte: het rekent
    /// de punten per duim van de afbeelding om naar die van waar het heen gaat.
    /// Een PNG meldt 95,99 punten per duim, en de vellen waarop het spel tekent
    /// staan op de puntdichtheid van het scherm. Op een scherm van 120 werd elke
    /// tegel daardoor 120/96 = 1,25 keer zo groot getekend, waarbij een vijfde
    /// rechts en een vijfde onderaan buiten het vakje viel — precies de rechter-
    /// en onderwand van het bakje.
    /// </summary>
    public static void Teken(Graphics g, Bitmap wat, int x, int y) =>
        g.DrawImage(wat, new Rectangle(x, y, wat.Width, wat.Height),
                    new Rectangle(0, 0, wat.Width, wat.Height), GraphicsUnit.Pixel);

    private static Bitmap Blokjes(Bitmap bron, int schaal)
    {
        var doel = new Bitmap(bron.Width * schaal, bron.Height * schaal, PixelFormat.Format32bppPArgb);
        using var g = Graphics.FromImage(doel);
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.SmoothingMode = SmoothingMode.None;
        g.DrawImage(bron, 0, 0, doel.Width, doel.Height);
        return doel;
    }

    // ------------------------------------------------------------------ laden

    private static Bitmap Bron(string naam)
    {
        string map = wijze switch
        {
            Tekenwijze.Getekend => "Tekening",
            Tekenwijze.Gerekend => "Gerekend",
            _ => "Bronplaatjes"
        };
        string sleutel = $"Mastermind.{map}.{naam}.png";
        if (Bronnen.TryGetValue(sleutel, out var b)) return b;

        Assembly asm = typeof(Plaatjes).Assembly;
        using Stream stroom = asm.GetManifestResourceStream(sleutel)
            ?? throw new FileNotFoundException($"Afbeelding {sleutel} zit niet in het programma.");

        // De bitmap wordt uit de stroom gekopieerd: een Bitmap houdt zijn stroom
        // anders open, en die sluiten we hier meteen.
        using var tijdelijk = new Bitmap(stroom);
        b = new Bitmap(tijdelijk.Width, tijdelijk.Height, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(b)) Teken(g, tijdelijk, 0, 0);

        Bronnen[sleutel] = b;
        return b;
    }
}
