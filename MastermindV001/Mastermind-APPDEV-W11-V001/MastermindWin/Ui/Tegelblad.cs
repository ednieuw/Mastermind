using System.Drawing.Imaging;

namespace Mastermind.Ui;

/// <summary>
/// Zet alle vakjes naast elkaar in één afbeelding, precies zoals
/// <see cref="Plaatjes"/> ze aan het bord geeft. Bedoeld om na te meten: een
/// schermafdruk moet je uitknippen en op het raster leggen, en dan meet je net zo
/// goed de omlijning of de verkeerde tegel. Dit is wat het spel echt tekent.
/// </summary>
internal static class Tegelblad
{
    public static void Schrijf(string pad, int schaal)
    {
        int b = Plaatjes.TegelBreed * schaal, h = Plaatjes.TegelHoog * schaal;

        var vakjes = new List<Bitmap>();
        for (int k = 0; k <= 9; k++) vakjes.Add(Plaatjes.Bal(k, schaal));
        for (int k = 1; k <= 9; k++) vakjes.Add(Plaatjes.Knop(k, schaal));
        vakjes.Add(Plaatjes.Paal(Plaatjes.PaalGroen, schaal));
        vakjes.Add(Plaatjes.Paal(Plaatjes.PaalRood, schaal));
        vakjes.Add(Plaatjes.Paal(Plaatjes.PaalLeeg, schaal));

        using var blad = new Bitmap(vakjes.Count * b, h, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(blad))
            for (int i = 0; i < vakjes.Count; i++)
                Plaatjes.Teken(g, vakjes[i], i * b, 0);

        blad.Save(pad, ImageFormat.Png);
        Console.WriteLine($"{vakjes.Count} vakjes van {b} bij {h} in {pad}");
    }
}
