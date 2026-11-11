using Mastermind.Engine;
using Mastermind.Ui;

namespace Mastermind;

internal static class Program
{
    /// <summary>Waar een onverwachte fout wordt vastgelegd.</summary>
    internal static readonly string FoutLog =
        Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? ".", "mastermind-fout.txt");

    [STAThread]
    private static void Main(string[] args)
    {
        // Zonder dit sluit Windows Forms het programma af zodra er ergens in de
        // schermafhandeling een fout optreedt: het venster verdwijnt dan zonder
        // melding. Met CatchException blijft het programma staan en krijg je te
        // zien wat er misging.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => Meld(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => Log(e.ExceptionObject as Exception);

        // De taal, de grootte en het geluid van de vorige keer.
        Instellingen.Lees();

        // Meteen doorgeven welk stel plaatjes het wordt. Dit hoort hier en niet
        // pas in het venster: "tegels" hieronder maakt geen venster aan en zou
        // anders altijd het standaardstel schrijven.
        Ui.Plaatjes.Wijze = Instellingen.Tekening;

        // Het programma begint in het Engels, zoals de uitvoering van 1995. Met
        // "Mastermind.exe /nl" start het in het Nederlands; handig voor een
        // aparte snelkoppeling, zonder dat er een tweede versie nodig is.
        if (Vlag(args, "nl")) Taal.Engels = false;
        if (Vlag(args, "en")) Taal.Engels = true;

        Log(Instellingen.Fout);

        ApplicationConfiguration.Initialize();

        // "Mastermind.exe tegels blad.png 1" zet alle vakjes op ware grootte naast
        // elkaar in één afbeelding, precies zoals het spel ze tekent. Handig om na
        // te meten of een tegel er op een bepaalde grootte nog goed uitziet,
        // zonder dat er een schermafdruk aan te pas komt.
        if (args.Length >= 2 && args[0] == "tegels")
        {
            int schaal = args.Length > 2 && int.TryParse(args[2], out int s) ? s : 1;
            if (args.Length > 3)
                Ui.Plaatjes.Wijze = args[3].ToLowerInvariant() switch
                {
                    "1995" => Tekenwijze.Origineel1995,
                    "getekend" => Tekenwijze.Getekend,
                    _ => Tekenwijze.Gerekend
                };
            Ui.Tegelblad.Schrijf(args[1], schaal);
            return;
        }

        Application.Run(new SpelForm());
    }

    private static bool Vlag(string[] args, string naam) =>
        args.Any(a => a.Equals("/" + naam, StringComparison.OrdinalIgnoreCase)
                   || a.Equals("-" + naam, StringComparison.OrdinalIgnoreCase));

    internal static void Log(Exception ex)
    {
        if (ex == null) return;
        try { File.AppendAllText(FoutLog, $"{DateTime.Now:s}\n{ex}\n\n"); } catch { }
    }

    private static void Meld(Exception ex)
    {
        Log(ex);
        try
        {
            MessageBox.Show(
                $"{Taal.IetsMisMaarLooptDoor}\n\n{ex.GetType().Name}: {ex.Message}\n\n" +
                $"{Taal.VolledigeMeldingIn}\n{FoutLog}",
                Taal.Titel, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch { }
    }
}
