using Mastermind.Engine;

namespace Mastermind.Test;

/// <summary>
/// Proefprogramma zonder venster. Het rekent aan dezelfde MmSpel als het spel
/// zelf, zodat een wijziging in de spelregels hier meteen opvalt.
///
///   dotnet run --project MastermindTest -- test          controleert het tellen
///   dotnet run --project MastermindTest -- oud           oud en nieuw tellen naast elkaar
///   dotnet run --project MastermindTest -- oplos 2000    laat de computer raden
/// </summary>
internal static class Program
{
    private static int fouten;

    private static int Main(string[] args)
    {
        string wat = args.Length > 0 ? args[0].ToLowerInvariant() : "test";
        switch (wat)
        {
            case "test": Tellen(); Spelverloop(); break;
            case "oud": OudEnNieuw(); break;
            case "oplos":
                Oplossen(args.Length > 1 ? int.Parse(args[1]) : 1000,
                         args.Length > 2 ? int.Parse(args[2]) : 1);
                break;
            default:
                Console.WriteLine("Gebruik: test | oud | oplos [aantal] [zaad]");
                return 2;
        }

        if (fouten > 0) Console.WriteLine($"\n{fouten} fout(en).");
        else Console.WriteLine("\nAlles goed.");
        return fouten == 0 ? 0 : 1;
    }

    private static void Klopt(bool goed, string wat)
    {
        if (goed) return;
        fouten++;
        Console.WriteLine($"FOUT: {wat}");
    }

    // ------------------------------------------------------------- het tellen

    private static void Tellen()
    {
        Console.WriteLine("Paaltjes tellen");
        Console.WriteLine("---------------");

        Controle("12345", "12345", 5, 0, "alles goed");
        Controle("12354", "12345", 3, 2, "twee verwisseld");
        Controle("54321", "12345", 1, 4, "omgekeerd");
        Controle("67891", "12345", 0, 1, "alleen de 1 zit erin");
        Controle("67890", "12345", 0, 0, "niets ervan, met een leeg vakje");
        Controle("00000", "12345", 0, 0, "lege regel levert niets op");
        Controle("11111", "12345", 1, 0, "vijf gelijke: één paaltje, niet vijf");
        Controle("22222", "12345", 1, 0, "vijf gelijke op een andere plaats");
        Controle("11234", "12345", 1, 3, "een dubbele kleur");
        Controle("12300", "12345", 3, 0, "half ingevulde regel");
        Controle("00345", "12345", 3, 0, "andere kant half ingevuld");

        // Bij elke gok geldt: nooit meer paaltjes dan er plaatsen zijn.
        var toeval = new Random(20260819);
        for (int i = 0; i < 200000; i++)
        {
            int[] gok = new int[MmSpel.MaxKolommen];
            for (int m = 0; m < gok.Length; m++) gok[m] = toeval.Next(0, MmSpel.AantalKleuren + 1);
            int[] code = Code(toeval);
            MmSpel.Tel(gok, code, out int zwart, out int wit);
            if (zwart + wit > MmSpel.MaxKolommen || zwart < 0 || wit < 0)
            {
                Klopt(false, $"te veel paaltjes bij {string.Concat(gok)} tegen {string.Concat(code)}");
                break;
            }
        }
        Console.WriteLine("200000 willekeurige regels: nooit meer dan vijf paaltjes.  goed");
    }

    private static void Controle(string gok, string code, int zwart, int wit, string wat)
    {
        MmSpel.Tel(Cijfers(gok), Cijfers(code), out int z, out int w);
        Klopt(z == zwart && w == wit,
              $"{wat}: {gok} tegen {code} gaf {z} groen en {w} rood, verwacht {zwart} en {wit}");
        Console.WriteLine($"{gok} tegen {code}  {z} groen {w} rood   {wat}");
    }

    // --------------------------------------------------------- oud en nieuw

    private static void OudEnNieuw()
    {
        Console.WriteLine("Hoe het origineel van 1995 telde, en hoe het nu telt");
        Console.WriteLine("----------------------------------------------------");
        Console.WriteLine("gok    code    1995        nu");
        foreach (string gok in new[] { "12345", "12354", "11111", "11234", "22222", "13579" })
        {
            const string code = "12345";
            MmSpel.TelZoalsOrigineel(Cijfers(gok), Cijfers(code), out int oz, out int ow);
            MmSpel.Tel(Cijfers(gok), Cijfers(code), out int nz, out int nw);
            string merk = oz == nz && ow == nw ? "" : "   <- verschil";
            Console.WriteLine($"{gok}  {code}   {oz} groen {ow} rood   {nz} groen {nw} rood{merk}");
        }
        Console.WriteLine();
        Console.WriteLine("Het origineel telde een balletje van de code opnieuw voor elke gok die");
        Console.WriteLine("erop leek. Vijf gelijke balletjes gaven daardoor vier rode paaltjes,");
        Console.WriteLine("terwijl er maar één kleur van de code bij zat.");
    }

    // ------------------------------------------------------------- spelverloop

    private static void Spelverloop()
    {
        Console.WriteLine();
        Console.WriteLine("Spelverloop");
        Console.WriteLine("-----------");

        var spel = new MmSpel(4);
        Klopt(spel.Stand == Stand.Bezig, "een nieuw spel staat op bezig");
        Klopt(spel.Geheim.Distinct().Count() == MmSpel.MaxKolommen, "de code heeft vijf verschillende kleuren");
        Klopt(spel.Geheim.All(k => k >= 1 && k <= 9), "de code gebruikt alleen 1 tot en met 9");

        // De code meteen goed raden geeft winst.
        for (int m = 0; m < MmSpel.MaxKolommen; m++) spel.Invoeren(spel.Geheim[m]);
        Klopt(spel.RegelVol, "vijf kleuren maken de regel vol");
        spel.Beoordelen();
        Klopt(spel.Stand == Stand.Gewonnen, "de goede code wint");
        Klopt(spel.PosGoedVan(0) == 5, "vijf groene paaltjes");
        Klopt(!spel.Invoeren(1), "na afloop kan er niets meer ingevoerd worden");
        Console.WriteLine("winnen in één regel  goed");

        // Zeventien keer misgokken geeft verlies.
        spel = new MmSpel(5);
        int[] mis = Mis(spel.Geheim);
        for (int r = 0; r < MmSpel.MaxRegels; r++)
        {
            foreach (int k in mis) spel.Invoeren(k);
            spel.Beoordelen();
        }
        Klopt(spel.Stand == Stand.Verloren, "na zeventien regels is het spel verloren");
        Console.WriteLine("verliezen na zeventien regels  goed");

        // Wissen en terug.
        spel = new MmSpel(6);
        spel.Invoeren(3); spel.Invoeren(4);
        Klopt(spel.PosInRegel == 2, "twee ingevoerd, dus plaats twee");
        spel.Terug();
        Klopt(spel.PosInRegel == 1 && spel[0, 1] == MmSpel.Leeg, "terug haalt het laatste balletje weg");
        spel.Positie(4); spel.Invoeren(9);
        Klopt(spel[0, 4] == 9, "op een gekozen plaats zetten");
        spel.Wissen();
        Klopt(spel.PosInRegel == 0 && spel[0, 0] == MmSpel.Leeg && spel[0, 4] == MmSpel.Leeg,
              "wissen maakt de hele regel leeg");
        Console.WriteLine("invoeren, terug, plaats kiezen en wissen  goed");
    }

    /// <summary>Een gok van vijf kleuren die zeker geen enkele plaats goed heeft.</summary>
    private static int[] Mis(IReadOnlyList<int> code)
    {
        int[] gok = new int[MmSpel.MaxKolommen];
        for (int m = 0; m < gok.Length; m++)
            for (int k = 1; k <= MmSpel.AantalKleuren; k++)
                if (k != code[m]) { gok[m] = k; break; }
        return gok;
    }

    // ----------------------------------------------------------- laten raden

    /// <summary>
    /// Laat de computer zelf raden: hij houdt alle codes bij die nog met de
    /// paaltjes kloppen en gokt er telkens één van. Zo blijkt of zeventien
    /// regels ruim genoeg zijn — dat is de vraag die er voor de speler toe doet.
    /// </summary>
    private static void Oplossen(int aantal, int zaad)
    {
        int[][] alle = AlleCodes().ToArray();
        Console.WriteLine($"{alle.Length} mogelijke codes van vijf verschillende kleuren uit negen.");

        var toeval = new Random(zaad);
        var telling = new int[MmSpel.MaxRegels + 2];
        int totaal = 0, ergste = 0;

        for (int spelnr = 0; spelnr < aantal; spelnr++)
        {
            int[] code = alle[toeval.Next(alle.Length)];
            var over = new List<int[]>(alle);
            int beurten = 0;

            while (true)
            {
                int[] gok = over[0];
                beurten++;
                MmSpel.Tel(gok, code, out int zwart, out int wit);
                if (zwart == MmSpel.MaxKolommen) break;

                over.RemoveAll(k =>
                {
                    MmSpel.Tel(gok, k, out int z, out int w);
                    return z != zwart || w != wit;
                });
            }

            totaal += beurten;
            ergste = Math.Max(ergste, beurten);
            telling[Math.Min(beurten, MmSpel.MaxRegels + 1)]++;
        }

        Console.WriteLine();
        Console.WriteLine("regels  spellen");
        for (int r = 1; r < telling.Length; r++)
            if (telling[r] > 0)
                Console.WriteLine($"{r,6}  {telling[r],7}  {new string('#', Math.Min(60, telling[r] * 60 / aantal))}");

        Console.WriteLine();
        Console.WriteLine($"gemiddeld {(double)totaal / aantal:0.00} regels, hoogstens {ergste} van de {MmSpel.MaxRegels}.");
        Klopt(ergste <= MmSpel.MaxRegels, "zeventien regels zijn genoeg om de code te vinden");
    }

    private static IEnumerable<int[]> AlleCodes()
    {
        var code = new int[MmSpel.MaxKolommen];
        var gebruikt = new bool[MmSpel.AantalKleuren + 1];

        IEnumerable<int[]> Verder(int m)
        {
            if (m == MmSpel.MaxKolommen) { yield return (int[])code.Clone(); yield break; }
            for (int k = 1; k <= MmSpel.AantalKleuren; k++)
            {
                if (gebruikt[k]) continue;
                gebruikt[k] = true;
                code[m] = k;
                foreach (var c in Verder(m + 1)) yield return c;
                gebruikt[k] = false;
            }
        }

        return Verder(0);
    }

    private static int[] Cijfers(string s) => s.Select(c => c - '0').ToArray();

    private static int[] Code(Random toeval)
    {
        int[] zak = Enumerable.Range(1, MmSpel.AantalKleuren).ToArray();
        for (int k = zak.Length - 1; k > 0; k--)
        {
            int j = toeval.Next(k + 1);
            (zak[k], zak[j]) = (zak[j], zak[k]);
        }
        return zak.Take(MmSpel.MaxKolommen).ToArray();
    }
}
