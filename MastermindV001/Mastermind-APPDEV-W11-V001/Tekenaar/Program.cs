using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Mastermind.Tekenaar;

/// <summary>
/// Gereedschap voor de plaatjes van het spel. Alles is 600 bij 600 beeldpunten
/// per vakje: twintig keer de maat op het scherm, en vierkant.
///
///   dotnet run --project Tekenaar                  vult MastermindWin\Gerekend
///   dotnet run --project Tekenaar -- knoppen       maakt de knoppen uit de ballen
///   dotnet run --project Tekenaar -- vierkant      maakt de bitmaps van 1995 30x30
///   dotnet run --project Tekenaar -- handwerk      zet die bitmaps 20x vergroot klaar
///
/// Zonder opdracht wordt er gerekend: een bol met licht van linksboven, een
/// glanspunt volgens Blinn-Phong en wat weerlicht van rechtsonder, liggend in een
/// kuiltje met vier wanden. De maten, de plaats van de bal en de kleuren komen uit
/// de bitmaps van 1995, zodat het bord er hetzelfde uitziet als toen.
///
/// Het spel kent drie stellen plaatjes, en dit gereedschap raakt er twee aan:
///
///   MastermindWin\Tekening    met de hand getekend door Ed  (blijft met rust)
///   MastermindWin\Gerekend    wat hier uitgerekend wordt
///   MastermindWin\Bronplaatjes  de bitmaps van 1995
///
/// Zonder opdracht wordt alleen `Gerekend` gevuld; het handwerk blijft buiten
/// schot. Met "knoppen" maak je uit de getekende ballen het invoerblokje van
/// negen: dezelfde bol, maar op een vlak plaatje in plaats van in een kuiltje.
/// Geef er een map achteraan om een ander stel te bedienen.
/// </summary>
internal static class Program
{
    /// <summary>Eén beeldpunt van 1995 is twintig beeldpunten hier.</summary>
    private const int F = 20;

    /// <summary>
    /// Maten van een vakje. Vierkant: 30 bij 30 eenheden, dus 600 bij 600
    /// beeldpunten. De bitmaps van 1995 waren 29 bij 28 — net niet vierkant,
    /// wat bij het tekenen met de hand alleen maar in de weg zit.
    /// </summary>
    private const int Breed = 30, Hoog = 30;

    /// <summary>Het midden van de bal en zijn straal; de bal is even groot als in 1995.</summary>
    private const double BalX = 15.0, BalY = 15.0, BalR = 7.0;

    /// <summary>
    /// Waar de met de hand getekende bal in zijn vakje staat. Opgemeten aan de
    /// tekeningen van Ed (bal1, bal7 en bal9): midden op (15,0 , 15,6), straal
    /// ruim 7,5 eenheden — iets groter en iets lager dan de gerekende bal. De
    /// opdracht "knoppen" knipt daar de bol uit.
    /// </summary>
    private const double HandBalX = 15.0, HandBalY = 15.6, HandBalR = 7.6;

    /// <summary>
    /// Hoe breed een rand uitvloeit, in eenheden. Twee beeldpunten op 600 — net
    /// genoeg om de trapjes van het rekenwerk weg te nemen, en verder zo scherp
    /// mogelijk. Het gladstrijken hoort van het verkleinen te komen: dat middelt
    /// over het hele blok beeldpunten dat op één nieuw beeldpunt valt. Vloeide de
    /// tekening zelf al een halve eenheid uit, dan zou er op twee of drie keer zo
    /// groot een zichtbare zoom om alles staan.
    /// </summary>
    private const double Zoom = 0.08;

    /// <summary>De negen kleuren zoals ze in 1995 waren; 0 is het lege vakje.</summary>
    private static readonly Color[] Kleuren =
    {
        Color.FromArgb(  0,   0,   0),   // 0 zwart, het lege vakje
        Color.FromArgb(  0,  76, 255),   // 1 blauw
        Color.FromArgb(  0, 224, 255),   // 2 cyaan
        Color.FromArgb(  0, 224,   0),   // 3 groen
        Color.FromArgb(  0, 120,  40),   // 4 donkergroen
        Color.FromArgb(158, 158, 162),   // 5 grijs
        Color.FromArgb(178, 140,  38),   // 6 oker
        Color.FromArgb(238, 224,   0),   // 7 geel
        Color.FromArgb(178,   0,  38),   // 8 donkerrood
        Color.FromArgb(238,   0, 238),   // 9 magenta
    };

    private static readonly Color PaalGroen = Color.FromArgb(0, 162, 0);
    private static readonly Color PaalRood = Color.FromArgb(247, 51, 85);

    // Het licht valt van linksboven, zoals de glans in de bitmaps van 1995.
    private static readonly double[] Licht = Richting(-0.52, -0.62, 0.59);

    // En er komt wat licht terug van de lichte ondergrond rechtsonder; dat randje
    // maakt dat een bol als een bol leest en niet als een platte schijf.
    private static readonly double[] Weerlicht = Richting(0.55, 0.62, 0.56);

    private static int Main(string[] args)
    {
        string wortel = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        string bronplaatjes = Path.Combine(wortel, "MastermindWin", "Bronplaatjes");

        string getekend = Path.Combine(wortel, "MastermindWin", "Tekening");
        string gerekend = Path.Combine(wortel, "MastermindWin", "Gerekend");

        if (args.Length > 0 && args[0] == "vierkant") return Vierkant(bronplaatjes);
        if (args.Length > 0 && args[0] == "handwerk") return Handwerk(bronplaatjes, Path.Combine(wortel, "Handwerk"));
        if (args.Length > 0 && args[0] == "knoppen") return Knoppen(args.Length > 1 ? args[1] : getekend);

        // Zonder opdracht komen de gerekende tekeningen in hun eigen map. De map
        // met handwerk blijft zo buiten schot; wie daar toch overheen wil, moet
        // hem noemen én "overschrijf" erbij zeggen.
        bool overschrijven = args.Contains("overschrijf");
        string map = args.Length > 0 && args[0] != "overschrijf" ? args[0] : gerekend;
        map = Path.GetFullPath(map);
        Directory.CreateDirectory(map);

        if (!overschrijven && map != gerekend && Directory.GetFiles(map, "*.png").Length > 0)
        {
            Console.Error.WriteLine($"In {map} staan al tekeningen.");
            Console.Error.WriteLine("Alles opnieuw laten tekenen wist die -- ook wat met de hand gemaakt is.");
            Console.Error.WriteLine($"Weet je het zeker, geef dan: dotnet run --project Tekenaar -- \"{map}\" overschrijf");
            return 1;
        }

        for (int k = 0; k < Kleuren.Length; k++)
        {
            using Bitmap bal = Balletje(Kleuren[k], welvend: true);
            bal.Save(Path.Combine(map, $"bal{k}.png"), ImageFormat.Png);
        }

        // Het invoerblokje van negen: dezelfde ballen, maar op een vlakke
        // ondergrond in plaats van in een kuiltje — het zijn knoppen.
        for (int k = 1; k < Kleuren.Length; k++)
        {
            using Bitmap knop = Balletje(Kleuren[k], welvend: false);
            knop.Save(Path.Combine(map, $"knop{k}.png"), ImageFormat.Png);
        }

        using (Bitmap p = Paaltje(PaalGroen)) p.Save(Path.Combine(map, "paal0.png"), ImageFormat.Png);
        using (Bitmap p = Paaltje(PaalRood)) p.Save(Path.Combine(map, "paal1.png"), ImageFormat.Png);
        using (Bitmap p = Paaltje(null)) p.Save(Path.Combine(map, "paalleeg.png"), ImageFormat.Png);

        Console.WriteLine($"22 tekeningen van {Breed * F} bij {Hoog * F} in {map}");
        return 0;
    }

    // ---------------------------------------------------------------- knoppen

    /// <summary>
    /// Maakt de negen knoppen van het invoerblokje uit de balletjes van het bord:
    /// dezelfde bol, maar op een vlak plaatje in plaats van in een kuiltje. Het
    /// zijn knoppen om op te drukken, geen gaatjes om een bal in te leggen.
    ///
    /// De bol wordt uit het vakje geknipt met de cirkel waar hij in staat; alles
    /// daarbuiten is de kuil van het bord en hoort op een knop niet thuis. Zo
    /// werkt het ook voor de zwarte en de grijze bal, waar geen kleur is om op af
    /// te gaan.
    /// </summary>
    private static int Knoppen(string map)
    {
        if (!Directory.Exists(map)) { Console.Error.WriteLine($"{map} bestaat niet"); return 1; }

        int gedaan = 0;
        for (int k = 1; k <= 9; k++)
        {
            string balPad = Path.Combine(map, $"bal{k}.png");
            if (!File.Exists(balPad)) { Console.Error.WriteLine($"{balPad} ontbreekt"); return 1; }

            using var bal = new Bitmap(balPad);
            if (bal.Width != Breed * F || bal.Height != Hoog * F)
            {
                Console.Error.WriteLine($"bal{k}.png is {bal.Width}x{bal.Height}, verwacht {Breed * F}x{Hoog * F}");
                return 1;
            }

            using var knop = new Bitmap(Breed * F, Hoog * F, PixelFormat.Format32bppArgb);
            for (int y = 0; y < knop.Height; y++)
                for (int x = 0; x < knop.Width; x++)
                {
                    double u = (x + 0.5) / F, v = (y + 0.5) / F;

                    (double r, double g, double b) = Vlak(u, v);

                    // Een zachte schaduw onder de bal, op de plek waar hij staat.
                    double sx = (u - (HandBalX + 0.9)) / (HandBalR * 1.02);
                    double sy = (v - (HandBalY + 1.1)) / (HandBalR * 0.92);
                    double kracht = 0.26 * Overgang(1.45, 0.55, Math.Sqrt(sx * sx + sy * sy));
                    r *= 1 - kracht; g *= 1 - kracht; b *= 1 - kracht;

                    double dx = u - HandBalX, dy = v - HandBalY;
                    double dekking = Overgang(HandBalR + Zoom, HandBalR - Zoom, Math.Sqrt(dx * dx + dy * dy));
                    if (dekking > 0)
                    {
                        Color c = bal.GetPixel(x, y);
                        r = r * (1 - dekking) + c.R * dekking;
                        g = g * (1 - dekking) + c.G * dekking;
                        b = b * (1 - dekking) + c.B * dekking;
                    }

                    knop.SetPixel(x, y, Color.FromArgb(Byte(r), Byte(g), Byte(b)));
                }

            knop.Save(Path.Combine(map, $"knop{k}.png"), ImageFormat.Png);
            gedaan++;
        }

        Console.WriteLine($"{gedaan} knoppen gemaakt uit de balletjes in {map}");
        return 0;
    }

    // ------------------------------------------------------- de bitmaps van 1995

    /// <summary>
    /// Maakt de bitmaps van 1995 vierkant: 29 bij 28 wordt 30 bij 30 door links
    /// een kolom en bovenaan een rij bij te plakken, gekopieerd van de rand die er
    /// al ligt. Geen bestaand beeldpunt verandert; alleen de lichte lijst wordt aan
    /// die twee kanten een beeldpunt breder.
    ///
    /// Links en boven is geen willekeurige keuze: de bal van 1995 staat op
    /// (13,5 , 13,5) en komt daarmee op (14,5 , 14,5) in een vak van 30 — vrijwel
    /// het midden, en dus op dezelfde plek als de bal in de nieuwe tekening.
    /// </summary>
    private static int Vierkant(string map)
    {
        if (!Directory.Exists(map)) { Console.Error.WriteLine($"{map} bestaat niet"); return 1; }

        int gedaan = 0, overgeslagen = 0;
        foreach (string pad in Directory.GetFiles(map, "*.png"))
        {
            Bitmap groter;
            using (var bron = new Bitmap(pad))
            {
                if (bron.Width == Breed && bron.Height == Hoog) { overgeslagen++; continue; }
                if (bron.Width > Breed || bron.Height > Hoog)
                {
                    Console.Error.WriteLine($"{Path.GetFileName(pad)} is {bron.Width}x{bron.Height}, dat past niet in {Breed}x{Hoog}");
                    return 1;
                }

                int dx = Breed - bron.Width;      // links bijplakken
                int dy = Hoog - bron.Height;      // bovenaan bijplakken
                groter = new Bitmap(Breed, Hoog, PixelFormat.Format32bppArgb);
                for (int y = 0; y < Hoog; y++)
                    for (int x = 0; x < Breed; x++)
                        groter.SetPixel(x, y, bron.GetPixel(
                            Math.Clamp(x - dx, 0, bron.Width - 1),
                            Math.Clamp(y - dy, 0, bron.Height - 1)));
            }

            using (groter) groter.Save(pad, ImageFormat.Png);
            gedaan++;
        }

        Console.WriteLine($"{gedaan} bitmaps vierkant gemaakt op {Breed}x{Hoog}" +
                          (overgeslagen > 0 ? $", {overgeslagen} waren dat al" : ""));
        return 0;
    }

    /// <summary>
    /// Zet de bitmaps van 1995 twintig keer vergroot in de map Handwerk, zonder
    /// gladstrijken: elk beeldpunt van toen wordt een blokje van 20 bij 20. Dat is
    /// de maat om met de hand op door te tekenen, en dezelfde maat als de nieuwe
    /// tekeningen.
    /// </summary>
    private static int Handwerk(string van, string naar)
    {
        if (!Directory.Exists(van)) { Console.Error.WriteLine($"{van} bestaat niet"); return 1; }
        Directory.CreateDirectory(naar);

        int gedaan = 0;
        foreach (string pad in Directory.GetFiles(van, "*.png"))
        {
            using var bron = new Bitmap(pad);
            using var groot = new Bitmap(bron.Width * F, bron.Height * F, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(groot))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.SmoothingMode = SmoothingMode.None;
                g.DrawImage(bron, 0, 0, groot.Width, groot.Height);
            }
            groot.Save(Path.Combine(naar, Path.GetFileName(pad)), ImageFormat.Png);
            gedaan++;
        }

        Console.WriteLine($"{gedaan} bitmaps van 1995 uitvergroot naar {Breed * F}x{Hoog * F} in {naar}");
        return 0;
    }

    // ------------------------------------------------------------------ balletje

    private static Bitmap Balletje(Color kleur, bool welvend)
    {
        int W = Breed * F, H = Hoog * F;
        var bmp = new Bitmap(W, H, PixelFormat.Format32bppArgb);

        BitmapData data = bmp.LockBits(new Rectangle(0, 0, W, H), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        try
        {
            unsafe
            {
                byte* bits = (byte*)data.Scan0;
                for (int y = 0; y < H; y++)
                {
                    byte* rij = bits + y * data.Stride;
                    for (int x = 0; x < W; x++)
                    {
                        // Midden van het beeldpunt, uitgedrukt in eenheden van 1995.
                        double u = (x + 0.5) / F, v = (y + 0.5) / F;

                        (double r, double g, double b) = welvend ? Kuiltje(u, v) : Vlak(u, v);
                        (r, g, b) = MetSchaduw(r, g, b, u, v);

                        double dx = u - BalX, dy = v - BalY;
                        double afstand = Math.Sqrt(dx * dx + dy * dy);

                        // De rand van de bol over één beeldpunt van 1995 laten
                        // overlopen; op deze maat is dat twintig beeldpunten en
                        // dus een nette ronde rand.
                        double dekking = Overgang(BalR + Zoom, BalR - Zoom, afstand);
                        if (dekking > 0)
                        {
                            (double br, double bg, double bb) = Bol(kleur, dx, dy);
                            r = r * (1 - dekking) + br * dekking;
                            g = g * (1 - dekking) + bg * dekking;
                            b = b * (1 - dekking) + bb * dekking;
                        }

                        byte* p = rij + x * 4;
                        p[0] = Byte(b);
                        p[1] = Byte(g);
                        p[2] = Byte(r);
                        p[3] = 255;
                    }
                }
            }
        }
        finally { bmp.UnlockBits(data); }

        return bmp;
    }

    /// <summary>De bol zelf: licht van linksboven, een glans, en weerlicht van rechtsonder.</summary>
    private static (double, double, double) Bol(Color kleur, double dx, double dy)
    {
        // Oppervlaktenormaal van een bol. Net binnen de rand wordt de wortel nul;
        // daar staat het oppervlak loodrecht op het scherm.
        double nx = dx / BalR, ny = dy / BalR;
        double vlak = Math.Min(1.0, nx * nx + ny * ny);
        double nz = Math.Sqrt(1 - vlak);

        double diffuus = Math.Max(0, nx * Licht[0] + ny * Licht[1] + nz * Licht[2]);
        double terug = Math.Max(0, nx * Weerlicht[0] + ny * Weerlicht[1] + nz * Weerlicht[2]);

        // Blinn-Phong voor de glans: de kijker kijkt recht van voren.
        double[] h = Richting(Licht[0], Licht[1], Licht[2] + 1);
        double glans = Math.Pow(Math.Max(0, nx * h[0] + ny * h[1] + nz * h[2]), 60);

        const double Omgeving = 0.30;      // ook de schaduwkant is niet zwart
        double licht = Omgeving + 0.82 * diffuus + 0.22 * Math.Pow(terug, 3);

        double r = kleur.R * licht + 255 * glans * 0.95;
        double g = kleur.G * licht + 255 * glans * 0.95;
        double b = kleur.B * licht + 255 * glans * 0.95;

        // Een zwarte bal heeft niets om te belichten; die leeft van zijn glans en
        // van een grijze zweem aan de lichtkant, net als in 1995.
        if (kleur.R + kleur.G + kleur.B == 0)
        {
            double zweem = 46 * diffuus + 26 * Math.Pow(terug, 3);
            r = zweem + 255 * glans * 0.95;
            g = zweem + 255 * glans * 0.95;
            b = zweem + 255 * glans * 0.95;
        }

        return (r, g, b);
    }

    // ---------------------------------------------------------------- ondergrond

    /// <summary>
    /// Het bakje waar de bal in ligt. Het is een echte kuil met vier wanden: de
    /// bovenwand en de linkerwand liggen in de schaduw, de onderwand en de
    /// rechterwand vangen licht. Zo zag het vakje er in 1995 ook uit — een
    /// verloop alleen is te weinig, dan mist de kuil zijn rechter- en onderwand.
    /// </summary>
    private static (double, double, double) Kuiltje(double u, double v)
    {
        // De lijst om de kuil moet duidelijk donkerder zijn dan de wanden waar
        // het licht op valt. Is hij even licht, dan lopen de onder- en
        // rechterwand in de lijst over en lees je ze niet als wand van dit vakje
        // maar als de scheidslijn met het volgende — precies waardoor het bakje
        // aan die twee kanten open lijkt te staan.
        const double Rand = 164;                 // het opstaande vlak om de kuil
        const double Diep = 3.0;                 // hoe breed de wanden in beeld zijn
        const double L = 1.3, T = 1.3, R = Breed - 1.3, B = Hoog - 1.3;

        // De twee wanden waar het licht op valt moeten duidelijk lichter zijn dan
        // de vloer én te onderscheiden van de lijst eromheen; anders lijkt het
        // bakje aan die kanten open te staan, en dan lijkt de bal ook niet meer
        // in het midden te liggen omdat alleen de andere twee wanden opvallen.
        const double WandBoven = 84, WandLinks = 104;     // in de schaduw

        // De verlichte wanden zijn grijs, niet wit: ongeveer de tint van het
        // paaltjesvlak (191). Wit trekt te veel aandacht naast de balletjes; het
        // moet een wandje blijven, geen lichtbak.
        const double WandOnder = 208, WandRechts = 196;   // in het licht

        // Binnen of buiten de kuil, met een zachte overgang zodat de opening
        // geen trapje wordt.
        double binnen = Math.Min(
            Math.Min(Overgang(L - Zoom, L + Zoom, u), Overgang(R + Zoom, R - Zoom, u)),
            Math.Min(Overgang(T - Zoom, T + Zoom, v), Overgang(B + Zoom, B - Zoom, v)));
        if (binnen <= 0) return (Rand, Rand, Rand);

        // De vloer ligt er middenin: donker genoeg om de lichte onder- en
        // rechterwand te laten uitkomen, licht genoeg voor de donkere boven- en
        // linkerwand. Ligt hij te dicht bij één van de twee, dan valt dat paar
        // weg en lijkt het bakje aan die kant open te staan.
        double t = Math.Clamp(((u - L) / (R - L) + (v - T) / (B - T)) / 2, 0, 1);
        double vloer = 136 + 22 * Math.Pow(t, 1.2);

        // De dichtstbijzijnde wand wint; op de hoeken geeft dat vanzelf de
        // schuine naad die een bakje hoort te hebben.
        double grijs = vloer;
        double sterkste = 0;
        void Wand(double afstand, double kleur)
        {
            // Een wand is een vlak bandje in zijn volle kleur, dat pas aan de
            // binnenkant overloopt in de vloer. Liet je hem over zijn hele breedte
            // vervagen, dan haalt hij op ware grootte -- waar de wand nog geen drie
            // beeldpunten breed is -- zijn eigen kleur nergens, en valt hij weg.
            double w = 1 - Overgang(Diep * 0.6, Diep, afstand);
            if (w <= sterkste) return;
            sterkste = w;
            grijs = vloer + (kleur - vloer) * w;
        }
        Wand(v - T, WandBoven);
        Wand(u - L, WandLinks);
        Wand(B - v, WandOnder);
        Wand(R - u, WandRechts);

        double uit = Rand + (grijs - Rand) * binnen;
        return (uit, uit, uit);
    }

    /// <summary>
    /// De ondergrond van een knop in het invoerblokje: een vlakje dat juist
    /// omhoog staat in plaats van een kuil, met licht langs de bovenkant en de
    /// linkerkant. Het zijn knoppen, geen gaatjes.
    /// </summary>
    private static (double, double, double) Vlak(double u, double v)
    {
        const double Rand = 191, Boven = 212, Onder = 132, Midden = 196;
        const double Diep = 1.1;
        const double L = 1.2, T = 1.2, R = Breed - 1.2, B = Hoog - 1.2;

        double binnen = Math.Min(
            Math.Min(Overgang(L - Zoom, L + Zoom, u), Overgang(R + Zoom, R - Zoom, u)),
            Math.Min(Overgang(T - Zoom, T + Zoom, v), Overgang(B + Zoom, B - Zoom, v)));
        if (binnen <= 0) return (Rand, Rand, Rand);

        double grijs = Midden;
        double sterkste = 0;
        void Kant(double afstand, double kleur)
        {
            double w = 1 - Overgang(0, Diep, afstand);
            if (w <= sterkste) return;
            sterkste = w;
            grijs = Midden + (kleur - Midden) * w;
        }
        Kant(v - T, Boven);
        Kant(u - L, Boven);
        Kant(B - v, Onder);
        Kant(R - u, Onder);

        double uit = Rand + (grijs - Rand) * binnen;
        return (uit, uit, uit);
    }

    /// <summary>Een zachte schaduw onder de bal, iets naar rechtsonder.</summary>
    private static (double, double, double) MetSchaduw(double r, double g, double b, double u, double v)
    {
        double dx = (u - (BalX + 0.9)) / (BalR * 1.02);
        double dy = (v - (BalY + 1.1)) / (BalR * 0.92);
        double d = Math.Sqrt(dx * dx + dy * dy);

        double kracht = 0.26 * Overgang(1.45, 0.55, d);
        if (kracht <= 0) return (r, g, b);

        return (r * (1 - kracht), g * (1 - kracht), b * (1 - kracht));
    }

    // ------------------------------------------------------------------ paaltje

    /// <summary>
    /// Het paaltje: een brede kop die naar beneden versmalt tot een steel. De
    /// omtrek is die van 1995; nieuw is dat hij rond is gemaakt, alsof het een
    /// echt pennetje is dat van linksboven belicht wordt. Zonder kleur wordt het
    /// een leeg vakje.
    /// </summary>
    private static Bitmap Paaltje(Color? kleur)
    {
        int W = Breed * F, H = Hoog * F;
        var bmp = new Bitmap(W, H, PixelFormat.Format32bppArgb);

        // De omtrek uit de bitmaps van 1995: de kop loopt van x 7..21 op y 4
        // naar x 11,8..17,2 op y 8,2, en van daar zakt de steel tot y 24,4.
        // De omtrek van 1995, opgeschoven naar het midden van het vierkante vak:
        // in het vak van 29 bij 28 stond hij op x 7..21 en y 4..24,4, en dat lag
        // links van het midden en te hoog toen het vak 30 bij 30 werd.
        const double NaarRechts = 1.5, NaarBeneden = 0.8;
        const double KopY = 4.0 + NaarBeneden, HalsY = 8.2 + NaarBeneden, VoetY = 24.4 + NaarBeneden;
        const double KopL = 7.0 + NaarRechts, KopR = 21.0 + NaarRechts;
        const double SteelL = 11.8 + NaarRechts, SteelR = 17.2 + NaarRechts;

        // Licht van linksboven; voor een staande rol telt alleen links/rechts.
        double[] licht = Richting(-0.52, 0, 0.59);
        double[] h = Richting(licht[0], 0, licht[2] + 1);

        BitmapData data = bmp.LockBits(new Rectangle(0, 0, W, H), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        try
        {
            unsafe
            {
                byte* bits = (byte*)data.Scan0;
                for (int y = 0; y < H; y++)
                {
                    byte* rij = bits + y * data.Stride;
                    for (int x = 0; x < W; x++)
                    {
                        double u = (x + 0.5) / F, v = (y + 0.5) / F;
                        double r = 191, g = 191, b = 191;

                        if (kleur != null)
                        {
                            // Hoe breed is het paaltje op deze hoogte?
                            double deel = Math.Clamp((v - KopY) / (HalsY - KopY), 0, 1);
                            double links = v <= HalsY ? KopL + (SteelL - KopL) * deel : SteelL;
                            double rechts = v <= HalsY ? KopR + (SteelR - KopR) * deel : SteelR;

                            double dekking =
                                Math.Min(Math.Min(Overgang(links - Zoom, links + Zoom, u),
                                                  Overgang(rechts + Zoom, rechts - Zoom, u)),
                                         Math.Min(Overgang(KopY - Zoom, KopY + Zoom, v),
                                                  Overgang(VoetY + Zoom, VoetY - Zoom, v)));

                            if (dekking > 0)
                            {
                                Color k = kleur.Value;

                                // Rond van links naar rechts, als een staande rol.
                                double t = (u - links) / (rechts - links);
                                double nx = Math.Clamp(2 * t - 1, -1, 1);
                                double nz = Math.Sqrt(Math.Max(0, 1 - nx * nx));

                                double diffuus = Math.Max(0, nx * licht[0] + nz * licht[2]);
                                double glans = Math.Pow(Math.Max(0, nx * h[0] + nz * h[2]), 34);
                                double terug = Math.Pow(Math.Max(0, nx * 0.88 + nz * 0.47), 4);

                                // De schuine schouders van de kop vangen wat meer licht.
                                double kop = v <= HalsY ? 0.10 * (1 - deel) : 0;

                                double sterkte = 0.34 + 0.72 * diffuus + 0.18 * terug + kop;
                                double pr = k.R * sterkte + 255 * glans * 0.7;
                                double pg = k.G * sterkte + 255 * glans * 0.7;
                                double pb = k.B * sterkte + 255 * glans * 0.7;

                                r = r * (1 - dekking) + pr * dekking;
                                g = g * (1 - dekking) + pg * dekking;
                                b = b * (1 - dekking) + pb * dekking;
                            }
                        }

                        byte* p = rij + x * 4;
                        p[0] = Byte(b);
                        p[1] = Byte(g);
                        p[2] = Byte(r);
                        p[3] = 255;
                    }
                }
            }
        }
        finally { bmp.UnlockBits(data); }

        return bmp;
    }

    // --------------------------------------------------------------- gereedschap

    /// <summary>Vloeiende overgang van 0 naar 1 tussen twee grenzen (smoothstep).</summary>
    private static double Overgang(double nul, double een, double x)
    {
        double t = Math.Clamp((x - nul) / (een - nul), 0, 1);
        return t * t * (3 - 2 * t);
    }

    private static double[] Richting(double x, double y, double z)
    {
        double len = Math.Sqrt(x * x + y * y + z * z);
        return new[] { x / len, y / len, z / len };
    }

    private static Color Meng(Color a, Color b, double deel) => Color.FromArgb(
        (int)(a.R * (1 - deel) + b.R * deel),
        (int)(a.G * (1 - deel) + b.G * deel),
        (int)(a.B * (1 - deel) + b.B * deel));

    private static byte Byte(double v) => (byte)Math.Clamp(Math.Round(v), 0, 255);
}
