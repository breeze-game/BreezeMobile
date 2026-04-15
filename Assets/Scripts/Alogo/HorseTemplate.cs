using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class HorseTemplate
{
    // basic information
    public string id;
    public string name;
    public Gender gender;

    // pedigree names
    public string sireName;
    public string damName;

    /// <summary>Snapshot of sire phenotype at conception (for display / future serialization).</summary>
    public string recordedSireMane = "N/A";
    public string recordedSireTail = "N/A";
    public string recordedSireNose = "N/A";

    /// <summary>Snapshot of dam phenotype at conception.</summary>
    public string recordedDamCoat = "N/A";
    public string recordedDamCoatRarity = "N/A";
    public List<string> recordedDamModifiers = new();
    public List<string> recordedDamMarkings = new();
    public string recordedDamMane = "N/A";
    public string recordedDamTail = "N/A";
    public string recordedDamNose = "N/A";

    /// <summary>Genetic dam-side inputs used by determine* logic when this horse is read as parent (mirrors Java dam* fields).</summary>
    public string geneticDamCoat = "N/A";
    public string geneticDamCoatRarity = "N/A";
    public List<string> geneticDamModifiers = new();
    public List<string> geneticDamMarkings = new();
    public string geneticDamMane = "N/A";
    public string geneticDamTail = "N/A";
    public string geneticDamNose = "N/A";

    public string geneticSireMane = "N/A";
    public string geneticSireTail = "N/A";
    public string geneticSireNose = "N/A";

    // appearance
    public string coat;
    public string coatRarity;
    public List<string> modifiers = new();
    public List<string> markings = new();
    /// <summary>General phenotype anomaly (Java-style pool).</summary>
    public string anomaly = "N/A";
    public string mane;
    public string tail;
    public string nose;

    /// <summary>Breeze dilution layer (separate from body modifiers).</summary>
    public string dilution = BreezeCosmeticTables.DilutionNone;
    public string dilutionAnomaly = "";
    public string modifierAnomaly = "";
    public string markingAnomaly = "";

    /// <summary>Mature height in hands (TB-scale placeholder).</summary>
    public float heightHands = 16f;

    /// <summary>0–2: narrows or widens inherited racing stat spread (Breeze breeding doc).</summary>
    [Range(0, 2)]
    public int progenyPotential = 1;

    // RACING STATS
    public Surface preferredSurface = Surface.Dirt;
    public Going preferredGoing = Going.Good;
    public int potential = 80;
    public int realizedPotential = 80;
    public int fitness = 80;
    public int reputation = 500;
    public int condition = 70;
    public int conditionPercent = 85;
    public int spd = 10;
    public int stamina = 10;
    public int acceleration = 10;
    public int start = 10;
    public int strength = 10;
    public int intelligence = 10;
    public int movement = 10;
    public int tenacity = 50;
    public int enthusiasm = 50;
    public int confidence = 50;
    public int battlingQualities = 50;
    public int cruisingBurst = 50;
    public int extraSpeedRating = 50;
    public int finishApplication = 50;
    public int consistency = 80;
    public int distanceAdaptability = 80;
    public int goingSurfaceAdaptability = 80;
    public int quirks = 0;

    /// <summary>
    /// Rolled performance for the <b>next</b> run (prefs: distance + preferred surface/going). Updated after each race; the race itself uses fixed weights for all entries.
    /// </summary>
    public int nextRunPerformancePreview;
}

public enum Gender { Colt, Filly, Stallion, Mare }
public enum Surface { Dirt, Turf, Synthetic }
public enum Going { Good, Firm, Fast, Slow, Muddy }
