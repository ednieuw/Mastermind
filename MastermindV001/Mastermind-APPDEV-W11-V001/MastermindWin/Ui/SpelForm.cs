using System.Reflection;
using Mastermind.Engine;

namespace Mastermind.Ui;

/// <summary>
/// Het venster om het speelveld heen: het menu, de grootte en de meldingen.
/// Het venster heeft een vaste maat, net als het dialoogvenster van 1995.
/// </summary>
internal sealed class SpelForm : Form
{
    private readonly BordPaneel bord = new();
    private readonly MenuStrip menu = new();

    private readonly ToolStripMenuItem itemSpel = new();
    private readonly ToolStripMenuItem itemNieuw = new();
    private readonly ToolStripMenuItem itemOplossing = new();
    private readonly ToolStripMenuItem itemKopieer = new();
    private readonly ToolStripMenuItem itemAfsluiten = new();

    private readonly ToolStripMenuItem itemBeeld = new();
    private readonly ToolStripMenuItem itemSchaal = new();
    private readonly ToolStripMenuItem itemSchaalAuto = new();
    private readonly ToolStripMenuItem[] itemSchaalN = new ToolStripMenuItem[3];
    private readonly ToolStripMenuItem itemGeluid = new();
    private readonly ToolStripMenuItem itemTekening = new();
    private readonly ToolStripMenuItem itemGetekend = new();
    private readonly ToolStripMenuItem itemGerekend = new();
    private readonly ToolStripMenuItem itemOrigineel1995 = new();

    private readonly ToolStripMenuItem itemTaal = new();
    private readonly ToolStripMenuItem itemNederlands = new();
    private readonly ToolStripMenuItem itemEngels = new();

    private readonly ToolStripMenuItem itemHelp = new();
    private readonly ToolStripMenuItem itemSpelregels = new();
    private readonly ToolStripMenuItem itemOver = new();

    /// <summary>Kiest het programma de grootte zelf?</summary>
    private bool schaalAutomatisch = true;

    public SpelForm()
    {
        // De indeling wordt in beeldpunten uitgerekend (zie BordPaneel); Windows
        // Forms moet er niet nog eens overheen schalen.
        AutoScaleMode = AutoScaleMode.None;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;
        Text = Taal.Titel;
        schaalAutomatisch = Instellingen.Schaal == 0;
        Icon = LaadPictogram();

        BouwMenu();

        bord.Geluid = Instellingen.Geluid;
        bord.Dock = DockStyle.Fill;
        bord.StandVeranderd += (_, _) => ZetVinkjes();
        Controls.Add(bord);
        Controls.Add(menu);
        MainMenuStrip = menu;

        ZetTeksten();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        ZetSchaal(schaalAutomatisch ? PassendeSchaal() : Instellingen.Schaal);
        bord.Focus();
    }

    // ------------------------------------------------------------------- menu

    private void BouwMenu()
    {
        itemNieuw.ShortcutKeys = Keys.F2;
        itemNieuw.Click += (_, _) => bord.NieuwSpel();
        itemOplossing.Click += (_, _) => bord.ToonOplossing();
        itemKopieer.ShortcutKeys = Keys.Control | Keys.C;
        itemKopieer.Click += (_, _) => Kopieer();
        // Alt+F4 komt van Windows zelf; hier alleen vermelden, niet vastleggen.
        itemAfsluiten.ShortcutKeyDisplayString = "Alt+F4";
        itemAfsluiten.Click += (_, _) => Close();
        itemSpel.DropDownItems.AddRange(new ToolStripItem[]
        {
            itemNieuw, itemOplossing, new ToolStripSeparator(), itemKopieer,
            new ToolStripSeparator(), itemAfsluiten
        });

        for (int n = 1; n <= 3; n++)
        {
            int keuze = n;
            itemSchaalN[n - 1] = new ToolStripMenuItem();
            itemSchaalN[n - 1].Click += (_, _) =>
            {
                schaalAutomatisch = false;
                Instellingen.Schaal = keuze;
                ZetSchaal(keuze);
                Instellingen.Schrijf();
            };
        }
        itemSchaalAuto.Click += (_, _) =>
        {
            schaalAutomatisch = true;
            Instellingen.Schaal = 0;
            ZetSchaal(PassendeSchaal());
            Instellingen.Schrijf();
        };
        itemSchaal.DropDownItems.Add(itemSchaalAuto);
        itemSchaal.DropDownItems.Add(new ToolStripSeparator());
        itemSchaal.DropDownItems.AddRange(itemSchaalN);

        itemGeluid.Click += (_, _) =>
        {
            bord.Geluid = !bord.Geluid;
            Instellingen.Geluid = bord.Geluid;
            Instellingen.Schrijf();
            ZetVinkjes();
        };
        itemGetekend.Click += (_, _) => KiesTekening(Tekenwijze.Getekend);
        itemGerekend.Click += (_, _) => KiesTekening(Tekenwijze.Gerekend);
        itemOrigineel1995.Click += (_, _) => KiesTekening(Tekenwijze.Origineel1995);
        itemTekening.DropDownItems.AddRange(new ToolStripItem[] { itemGetekend, itemGerekend, itemOrigineel1995 });

        itemBeeld.DropDownItems.AddRange(new ToolStripItem[] { itemSchaal, itemTekening, itemGeluid });

        itemNederlands.Click += (_, _) => KiesTaal(false);
        itemEngels.Click += (_, _) => KiesTaal(true);
        itemTaal.DropDownItems.AddRange(new ToolStripItem[] { itemNederlands, itemEngels });

        itemSpelregels.ShortcutKeys = Keys.F1;
        itemSpelregels.Click += (_, _) => MessageBox.Show(this, Taal.SpelregelsTekst,
            Taal.MenuSpelregels.Replace("&", ""), MessageBoxButtons.OK, MessageBoxIcon.Information);
        itemOver.Click += (_, _) => MessageBox.Show(this, Taal.OverTekst,
            Taal.MenuOver.Replace("&", "").Replace("...", ""), MessageBoxButtons.OK, MessageBoxIcon.Information);
        itemHelp.DropDownItems.AddRange(new ToolStripItem[] { itemSpelregels, itemOver });

        menu.Items.AddRange(new ToolStripItem[] { itemSpel, itemBeeld, itemTaal, itemHelp });
    }

    private void ZetTeksten()
    {
        Text = Taal.Titel;
        itemSpel.Text = Taal.MenuSpel;
        itemNieuw.Text = Taal.MenuNieuw;
        itemOplossing.Text = Taal.MenuOplossing;
        itemKopieer.Text = Taal.MenuKopieer;
        itemAfsluiten.Text = Taal.MenuAfsluiten;

        itemBeeld.Text = Taal.MenuBeeld;
        itemSchaal.Text = Taal.MenuSchaal;
        itemSchaalAuto.Text = Taal.MenuSchaalAuto;
        for (int n = 1; n <= 3; n++) itemSchaalN[n - 1].Text = Taal.MenuSchaalN(n);
        itemGeluid.Text = Taal.MenuGeluid;
        itemTekening.Text = Taal.MenuTekening;
        itemGetekend.Text = Taal.MenuGetekend;
        itemGerekend.Text = Taal.MenuGerekend;
        itemOrigineel1995.Text = Taal.MenuOrigineel1995;

        itemTaal.Text = Taal.MenuTaal;
        itemNederlands.Text = Taal.MenuNederlands;
        itemEngels.Text = Taal.MenuEngels;

        itemHelp.Text = Taal.MenuHelp;
        itemSpelregels.Text = Taal.MenuSpelregels;
        itemOver.Text = Taal.MenuOver;

        bord.ZetTeksten();
        ZetVinkjes();
    }

    private void ZetVinkjes()
    {
        itemSchaalAuto.Checked = schaalAutomatisch;
        for (int n = 1; n <= 3; n++) itemSchaalN[n - 1].Checked = !schaalAutomatisch && bord.Schaal == n;
        itemGeluid.Checked = bord.Geluid;
        itemGetekend.Checked = Plaatjes.Wijze == Tekenwijze.Getekend;
        itemGerekend.Checked = Plaatjes.Wijze == Tekenwijze.Gerekend;
        itemOrigineel1995.Checked = Plaatjes.Wijze == Tekenwijze.Origineel1995;
        itemNederlands.Checked = !Taal.Engels;
        itemEngels.Checked = Taal.Engels;
        itemOplossing.Enabled = !bord.Spel.OplossingZichtbaar;
    }

    private void KiesTekening(Tekenwijze nieuw)
    {
        if (Plaatjes.Wijze == nieuw) return;
        Plaatjes.Wijze = nieuw;
        Instellingen.Tekening = nieuw;
        Instellingen.Schrijf();
        ZetVinkjes();
        Invalidate(true);
    }

    private void KiesTaal(bool engels)
    {
        if (Taal.Engels == engels) return;
        Taal.Engels = engels;
        ZetTeksten();
        Instellingen.Schrijf();
    }

    private void Kopieer()
    {
        string tekst = bord.AlsTekst();
        if (tekst.Length > 0) Clipboard.SetText(tekst);
    }

    // ---------------------------------------------------------------- grootte

    /// <summary>
    /// De grootste vergroting die nog op het scherm past. Op een gewoon scherm
    /// van 96 punten per duim is dat 1, op een 4K-scherm 2 of 3.
    /// </summary>
    private int PassendeSchaal()
    {
        int s = Math.Clamp((int)Math.Round(DeviceDpi / 96.0), 1, 3);
        while (s > 1 && !Past(s)) s--;
        return s;
    }

    private bool Past(int s)
    {
        Size paneel = bord.GewensteGrootte(s);
        Size venster = SizeFromClientSize(new Size(paneel.Width, paneel.Height + menu.Height));
        Rectangle werkgebied = Screen.FromControl(this).WorkingArea;
        return venster.Width <= werkgebied.Width && venster.Height <= werkgebied.Height;
    }

    private void ZetSchaal(int s)
    {
        // Ook een zelf gekozen grootte mag het venster niet groter maken dan het
        // scherm: het venster heeft een vaste maat, dus wat eraf valt is weg.
        while (s > 1 && !Past(s)) s--;

        bord.Schaal = s;
        Size paneel = bord.GewensteGrootte(bord.Schaal);
        ClientSize = new Size(paneel.Width, paneel.Height + menu.Height);
        ZetVinkjes();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        Instellingen.Schrijf();
        base.OnFormClosing(e);
    }

    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
        base.OnDpiChanged(e);
        if (schaalAutomatisch) ZetSchaal(PassendeSchaal());
    }

    // ---------------------------------------------------------------- toetsen

    protected override bool ProcessCmdKey(ref Message msg, Keys toets)
    {
        // De pijltjes en Enter mogen niet naar de knoppen lopen; het bord doet
        // er zelf iets mee.
        if (bord.Toets(toets)) return true;
        return base.ProcessCmdKey(ref msg, toets);
    }

    private static Icon LaadPictogram()
    {
        try
        {
            Assembly asm = typeof(SpelForm).Assembly;
            using Stream stroom = asm.GetManifestResourceStream("Mastermind.Bronplaatjes.mastermind.ico");
            return stroom == null ? null : new Icon(stroom);
        }
        catch
        {
            return null;
        }
    }
}
