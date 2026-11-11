namespace Mastermind.Engine;

/// <summary>Welke uitvoering van de balletjes en paaltjes er getekend wordt.</summary>
public enum Tekenwijze
{
    /// <summary>Met de hand getekend, 600 bij 600 per vakje.</summary>
    Getekend,

    /// <summary>Uitgerekend: een bol met licht, glans en weerlicht, ook 600 bij 600.</summary>
    Gerekend,

    /// <summary>De bitmaps uit 1995, beeldpunt voor beeldpunt.</summary>
    Origineel1995
}
