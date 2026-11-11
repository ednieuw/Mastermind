using System.Media;
using System.Text;
using Mastermind.Engine;

namespace Mastermind.Ui;

/// <summary>
/// Het speelveld: zeventien regels van vijf balletjes met de paaltjes ernaast,
/// en rechts het invoerblokje van negen kleuren met de knoppen.
///
/// De indeling volgt het scherm van 1995 (IDD_CLIENT uit MSTRMAPP.RC), maar
/// alles wordt in beeldpunten uitgerekend uit één vergrotingsfactor, zodat het
/// venster op een scherm met veel beeldpunten net zo groot oogt als toen op een
/// gewoon scherm.
/// </summary>
internal sealed class BordPaneel : Panel
{
    private readonly Button knopNieuw = new();
    private readonly Button knopGoed = new();
    private readonly Button knopWissen = new();
    private readonly Button knopOplossing = new();
    private readonly Button[] knopPositie = new Button[MmSpel.MaxKolommen];

    private SoundPlayer gevonden;
    private int schaal = 1;
    private int zweefKleur;      // kleur waar de muis boven hangt, 0 = geen
    private Font tekstFont;

    public BordPaneel()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(64, 128, 64);   // SetBkgndColor(RGB(64,128,64)) uit het origineel

        Spel = new MmSpel();

        knopNieuw.Click += (_, _) => NieuwSpel();
        knopGoed.Click += (_, _) => Beoordelen();
        knopWissen.Click += (_, _) => { Spel.Wissen(); Ververs(); };
        knopOplossing.Click += (_, _) => ToonOplossing();

        foreach (var knop in new[] { knopNieuw, knopGoed, knopWissen, knopOplossing })
        {
            knop.FlatStyle = FlatStyle.System;
            knop.TabStop = false;
            Controls.Add(knop);
        }

        for (int m = 0; m < MmSpel.MaxKolommen; m++)
        {
            int kolom = m;                       // eigen kopie voor de knop
            var knop = new Button { FlatStyle = FlatStyle.System, TabStop = false, Text = (m + 1).ToString() };
            knop.Click += (_, _) => { Spel.Positie(kolom); Ververs(); };
            knopPositie[m] = knop;
            Controls.Add(knop);
        }

        ZetTeksten();
    }

    /// <summary>Het spel waar dit paneel bij hoort.</summary>
    public MmSpel Spel { get; }

    /// <summary>Wordt gemeld zodra er iets aan de stand verandert.</summary>
    public event EventHandler StandVeranderd;

    /// <summary>Mag er een geluidje klinken als de code geraden is?</summary>
    public bool Geluid { get; set; } = true;

    /// <summary>Hoeveel keer zo groot alles getekend wordt: 1, 2 of 3.</summary>
    public int Schaal
    {
        get => schaal;
        set
        {
            int nieuw = Math.Clamp(value, 1, 3);
            if (nieuw == schaal) return;
            schaal = nieuw;
            Herschik();
            Invalidate();
        }
    }

    // ------------------------------------------------------------------ maten

    private int TB => Plaatjes.TegelBreed * schaal;
    private int TH => Plaatjes.TegelHoog * schaal;
    private int Marge => 10 * schaal;
    private int BordX => Marge;
    private int BordY => Marge;
    private int PaalX => BordX + MmSpel.MaxKolommen * TB + Marge;
    private int RechtsX => PaalX + MmSpel.MaxKolommen * TB + 2 * Marge;
    private int RechtsBreed => 124 * schaal;
    private int OplossingY => BordY + MmSpel.MaxRegels * TH + Marge;

    /// <summary>Hoe groot het paneel moet zijn bij een bepaalde vergroting.</summary>
    public Size GewensteGrootte(int voorSchaal)
    {
        int b = Plaatjes.TegelBreed * voorSchaal;
        int h = Plaatjes.TegelHoog * voorSchaal;
        int m = 10 * voorSchaal;
        return new Size(
            m + MmSpel.MaxKolommen * b + m + MmSpel.MaxKolommen * b + 2 * m + 124 * voorSchaal + m,
            m + MmSpel.MaxRegels * h + m + h + m);
    }

    // ------------------------------------------------------------- spelverloop

    /// <summary>Begint een nieuw spel.</summary>
    public void NieuwSpel()
    {
        Spel.NieuwSpel();
        Ververs();
    }

    /// <summary>Zet de code onderaan het bord.</summary>
    public void ToonOplossing()
    {
        Spel.ToonOplossing();
        Ververs();
    }

    /// <summary>Beoordeelt de regel en handelt winst of verlies af.</summary>
    public void Beoordelen()
    {
        if (Spel.Stand != Stand.Bezig) return;
        if (!Spel.Beoordelen()) return;

        Ververs();

        if (Spel.Stand == Stand.Gewonnen)
        {
            Spel.ToonOplossing();
            Ververs();
            SpeelGeluid();
            Vraag(Taal.GewonnenTekst(Spel.GebruikteRegels), Taal.GewonnenTitel);
        }
        else if (Spel.Stand == Stand.Verloren)
        {
            Spel.ToonOplossing();
            Ververs();
            Vraag(Taal.VerlorenTekst, Taal.VerlorenTitel);
        }
    }

    private void Vraag(string tekst, string titel)
    {
        // Het venster eerst laten bijtekenen, anders staat de melding voor een
        // bord waar de laatste regel nog niet op staat.
        Update();
        if (MessageBox.Show(this, tekst, titel, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            NieuwSpel();
    }

    /// <summary>Verwerkt een toets. Geeft terug of de toets iets deed.</summary>
    public bool Toets(Keys toets)
    {
        bool gedaan = true;
        switch (toets)
        {
            case >= Keys.D1 and <= Keys.D9: Spel.Invoeren(toets - Keys.D0); break;
            case >= Keys.NumPad1 and <= Keys.NumPad9: Spel.Invoeren(toets - Keys.NumPad0); break;
            case Keys.Left: Spel.Positie(Math.Max(0, Spel.PosInRegel - 1)); break;
            case Keys.Right: Spel.Positie(Math.Min(MmSpel.MaxKolommen - 1, Spel.PosInRegel + 1)); break;
            case Keys.Back: Spel.Terug(); break;
            case Keys.Escape: Spel.Wissen(); break;
            case Keys.Enter: Beoordelen(); return true;
            default: gedaan = false; break;
        }
        if (gedaan) Ververs();
        return gedaan;
    }

    /// <summary>Zet alle opschriften opnieuw; nodig na het wisselen van taal.</summary>
    public void ZetTeksten()
    {
        knopNieuw.Text = Taal.NieuwSpel;
        knopGoed.Text = Taal.Goed;
        knopWissen.Text = Taal.Wissen;
        knopOplossing.Text = Taal.Oplossing;
        Invalidate();
    }

    private void Ververs()
    {
        Invalidate();
        StandVeranderd?.Invoke(this, EventArgs.Empty);
    }

    private void SpeelGeluid()
    {
        if (!Geluid) return;
        try
        {
            if (gevonden == null)
            {
                Stream stroom = typeof(BordPaneel).Assembly
                    .GetManifestResourceStream("Mastermind.Bronplaatjes.gevonden.wav");
                if (stroom == null) return;
                gevonden = new SoundPlayer(stroom);
                gevonden.Load();
            }
            gevonden.Play();
        }
        catch (Exception ex)
        {
            // Zonder geluidskaart of met een bezet apparaat gaat het spel gewoon door.
            Program.Log(ex);
        }
    }

    // --------------------------------------------------------------- indeling

    private void Herschik()
    {
        tekstFont?.Dispose();
        tekstFont = new Font(SystemFonts.MessageBoxFont.FontFamily, 11f * schaal, GraphicsUnit.Pixel);

        int knopHoog = 26 * schaal;
        int y = Marge;

        knopNieuw.Font = tekstFont;
        knopNieuw.SetBounds(RechtsX, y, RechtsBreed, knopHoog);
        y += knopHoog + 2 * Marge;

        // Het invoerblokje van negen staat gecentreerd onder de startknop.
        BlokX = RechtsX + (RechtsBreed - 3 * TB) / 2;
        BlokY = y;
        y += 3 * TH + Marge;

        int posBreed = (RechtsBreed - 4 * (2 * schaal)) / MmSpel.MaxKolommen;
        int posHoog = 20 * schaal;
        for (int m = 0; m < MmSpel.MaxKolommen; m++)
        {
            knopPositie[m].Font = tekstFont;
            knopPositie[m].SetBounds(RechtsX + m * (posBreed + 2 * schaal), y, posBreed, posHoog);
        }
        y += posHoog + Marge;

        int half = (RechtsBreed - Marge) / 2;
        knopGoed.Font = tekstFont;
        knopGoed.SetBounds(RechtsX, y, half, knopHoog);
        knopWissen.Font = tekstFont;
        knopWissen.SetBounds(RechtsX + half + Marge, y, half, knopHoog);
        y += knopHoog + Marge;

        GrijsOnder = y;                      // onderkant van het grijze vlak

        y += Marge;
        knopOplossing.Font = tekstFont;
        knopOplossing.SetBounds(RechtsX, y, RechtsBreed, knopHoog);
        y += knopHoog + Marge;

        TekstY = y;
    }

    private int BlokX, BlokY, GrijsOnder, TekstY;

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        Herschik();
    }

    // --------------------------------------------------------------- tekenen

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (tekstFont == null) Herschik();
        Graphics g = e.Graphics;

        // Het grijze vlak achter het invoerblokje; in 1995 een BorShade-groep.
        using (var grijs = new SolidBrush(Color.FromArgb(192, 192, 192)))
            g.FillRectangle(grijs, RechtsX - Marge / 2, BlokY - Marge / 2,
                            RechtsBreed + Marge, GrijsOnder - BlokY);

        // De zeventien regels met hun paaltjes.
        for (int r = 0; r < MmSpel.MaxRegels; r++)
        {
            int y = BordY + r * TH;
            for (int m = 0; m < MmSpel.MaxKolommen; m++)
                Plaatjes.Teken(g, Plaatjes.Bal(Spel[r, m], schaal), BordX + m * TB, y);

            int groen = Spel.IsBeoordeeld(r) ? Spel.PosGoedVan(r) : 0;
            int rood = Spel.IsBeoordeeld(r) ? Spel.KleurGoedVan(r) : 0;
            for (int m = 0; m < MmSpel.MaxKolommen; m++)
            {
                int soort = m < groen ? Plaatjes.PaalGroen
                          : m < groen + rood ? Plaatjes.PaalRood
                          : Plaatjes.PaalLeeg;
                Plaatjes.Teken(g, Plaatjes.Paal(soort, schaal), PaalX + m * TB, y);
            }
        }

        // De oplossingsregel onderaan.
        for (int m = 0; m < MmSpel.MaxKolommen; m++)
            Plaatjes.Teken(g, Plaatjes.Bal(Spel.OplossingZichtbaar ? Spel[MmSpel.MaxRegels, m] : 0, schaal),
                           BordX + m * TB, OplossingY);

        // Waar de speler nu aan toe is: een lijn om de regel en om het vakje.
        if (Spel.Stand == Stand.Bezig)
        {
            using var pen = new Pen(Color.White, schaal);
            int y = BordY + Spel.Regel * TH;
            g.DrawRectangle(pen, BordX - schaal, y - schaal,
                            MmSpel.MaxKolommen * TB + schaal, TH + schaal);
            if (Spel.PosInRegel < MmSpel.MaxKolommen)
            {
                using var dik = new Pen(Color.Yellow, 2 * schaal);
                g.DrawRectangle(dik, BordX + Spel.PosInRegel * TB + schaal, y + schaal,
                                TB - 2 * schaal, TH - 2 * schaal);
            }
        }

        // Het invoerblokje van negen kleuren, drie bij drie zoals in 1995.
        for (int k = 1; k <= MmSpel.AantalKleuren; k++)
        {
            Point p = KleurPlaats(k);
            Plaatjes.Teken(g, Plaatjes.Knop(k, schaal), p.X, p.Y);
            if (k == zweefKleur)
            {
                using var pen = new Pen(Color.White, 2 * schaal);
                g.DrawRectangle(pen, p.X + schaal, p.Y + schaal, TB - 2 * schaal, TH - 2 * schaal);
            }
        }

        // De stand in woorden onder de knoppen.
        using var inkt = new SolidBrush(Color.White);
        string melding = Spel.Stand switch
        {
            Stand.Gewonnen => Taal.GewonnenIn(Spel.GebruikteRegels),
            Stand.Verloren => Taal.Verloren,
            _ => Taal.RegelVanTotaal(Spel.Regel + 1, MmSpel.MaxRegels) + "\n" + Taal.KiesEenKleur
        };
        g.DrawString(melding, tekstFont, inkt,
                     new RectangleF(RechtsX, TekstY, RechtsBreed, Height - TekstY));
    }

    private Point KleurPlaats(int kleur)
    {
        int n = kleur - 1;
        return new Point(BlokX + n % 3 * TB, BlokY + n / 3 * TH);
    }

    // ------------------------------------------------------------------- muis

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();

        int kleur = KleurOnder(e.Location);
        if (kleur > 0) { Spel.Invoeren(kleur); Ververs(); return; }

        // Rechtstreeks op een vakje van de eigen regel klikken kiest de plaats.
        if (Spel.Stand == Stand.Bezig)
        {
            int y = BordY + Spel.Regel * TH;
            if (e.Y >= y && e.Y < y + TH && e.X >= BordX && e.X < BordX + MmSpel.MaxKolommen * TB)
            {
                Spel.Positie((e.X - BordX) / TB);
                Ververs();
            }
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        int kleur = KleurOnder(e.Location);
        if (kleur == zweefKleur) return;

        zweefKleur = kleur;
        Cursor = kleur > 0 ? Cursors.Hand : Cursors.Default;
        Invalidate(new Rectangle(BlokX, BlokY, 3 * TB, 3 * TH));
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (zweefKleur == 0) return;
        zweefKleur = 0;
        Invalidate(new Rectangle(BlokX, BlokY, 3 * TB, 3 * TH));
    }

    private int KleurOnder(Point p)
    {
        if (p.X < BlokX || p.X >= BlokX + 3 * TB) return 0;
        if (p.Y < BlokY || p.Y >= BlokY + 3 * TH) return 0;
        return (p.Y - BlokY) / TH * 3 + (p.X - BlokX) / TB + 1;
    }

    /// <summary>De regels tot nu toe als tekst, voor het klembord.</summary>
    public string AlsTekst()
    {
        var sb = new StringBuilder();
        for (int r = 0; r < Spel.Regel; r++)
        {
            for (int m = 0; m < MmSpel.MaxKolommen; m++) sb.Append(Spel[r, m]);
            sb.Append("   ").Append(new string('+', Spel.PosGoedVan(r)))
                            .Append(new string('-', Spel.KleurGoedVan(r)))
              .AppendLine();
        }
        return sb.ToString();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            tekstFont?.Dispose();
            gevonden?.Dispose();
        }
        base.Dispose(disposing);
    }
}
