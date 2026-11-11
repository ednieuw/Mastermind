namespace Mastermind.Engine;

/// <summary>
/// Onthoudt de taal, de grootte en het geluid tussen twee keer spelen door.
/// Het bestandje staat in %APPDATA%\Mastermind; het programma zelf en de map
/// waarin het staat blijven onaangeroerd, zodat Mastermind ook vanaf een
/// geheugenstick of een alleen-lezen map werkt.
///
/// Het origineel van 1995 onthield niets. Dit is dus nieuw, maar het valt niet
/// op: er verandert alleen iets als de speler het zelf omzet.
/// </summary>
public static class Instellingen
{
    private static readonly string Bestand = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Mastermind", "instellingen.txt");

    /// <summary>0 = zelf uitzoeken, 1..3 = die vergroting.</summary>
    public static int Schaal { get; set; }

    /// <summary>Klinkt er een geluidje als de code geraden is?</summary>
    public static bool Geluid { get; set; } = true;

    /// <summary>Welke uitvoering van de balletjes en paaltjes.</summary>
    public static Tekenwijze Tekening { get; set; } = Tekenwijze.Gerekend;

    /// <summary>Leest het bestandje. Ontbreekt het, dan blijven de standaardwaarden staan.</summary>
    public static void Lees()
    {
        try
        {
            if (!File.Exists(Bestand)) return;
            foreach (string regel in File.ReadAllLines(Bestand))
            {
                int is_ = regel.IndexOf('=');
                if (is_ < 0) continue;
                string sleutel = regel[..is_].Trim().ToLowerInvariant();
                string waarde = regel[(is_ + 1)..].Trim();
                switch (sleutel)
                {
                    case "taal": Taal.Engels = waarde.Equals("en", StringComparison.OrdinalIgnoreCase); break;
                    case "schaal": if (int.TryParse(waarde, out int s)) Schaal = Math.Clamp(s, 0, 3); break;
                    case "geluid": Geluid = waarde != "0" && !waarde.Equals("nee", StringComparison.OrdinalIgnoreCase); break;
                    case "tekening":
                        // "nieuw" is van voor er drie stellen waren en betekende
                        // het gerekende stel; die keuze blijft dus staan.
                        Tekening = waarde.ToLowerInvariant() switch
                        {
                            "1995" => Tekenwijze.Origineel1995,
                            "getekend" => Tekenwijze.Getekend,
                            _ => Tekenwijze.Gerekend
                        };
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            // Een onleesbaar bestandje mag het spelen niet in de weg zitten.
            Fout = ex;
        }
    }

    /// <summary>Schrijft het bestandje. Lukt dat niet, dan gaat het spel gewoon door.</summary>
    public static void Schrijf()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Bestand)!);
            File.WriteAllLines(Bestand, new[]
            {
                "# Instellingen van Mastermind. Weghalen mag; dan gelden de standaardwaarden.",
                $"taal={(Taal.Engels ? "en" : "nl")}",
                $"schaal={Schaal}",
                $"geluid={(Geluid ? 1 : 0)}",
                $"tekening={Tekening switch { Tekenwijze.Origineel1995 => "1995", Tekenwijze.Gerekend => "gerekend", _ => "getekend" }}"
            });
        }
        catch (Exception ex)
        {
            Fout = ex;
        }
    }

    /// <summary>Wat er misging bij lezen of schrijven; alleen om vast te leggen.</summary>
    public static Exception Fout { get; private set; }
}
