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

    // lineage
    public string sireName;
    public string damName;

    // appearance
    public string coat;
    public string coatRarity;
    public List<string> modifiers = new();
    public List<string> markings = new();
    public string anomaly;
    public string mane;
    public string tail;
    public string nose;

    // RACING STATS
    // preferences
    public Surface preferredSurface = Surface.Dirt;
    public Going preferredGoing = Going.Good;
    // race state
    public int potential = 80;
    public int realizedPotential = 80;
    public int fitness = 80;
    public int reputation = 500;
    public int condition = 70; // "form"
    public int conditionPercent = 85; // "condition"
    // core
    public int spd = 10;
    public int stamina = 10;
    public int acceleration = 10;
    public int start = 10;
    public int strength = 10;
    public int intelligence = 10;
    public int movement = 10;
    // mentality
    public int tenacity = 50;
    public int enthusiasm = 50;
    public int confidence = 50;
    public int battlingQualities = 50;
    public int cruisingBurst = 50;
    public int extraSpeedRating = 50;
    public int finishApplication = 50;
    // adaptability
    public int consistency = 80;
    public int distanceAdaptability = 80;
    public int goingSurfaceAdaptability = 80;
    public int quirks = 0;
}

// gender options
public enum Gender { Colt, Filly, Stallion, Mare }
// race surface options
public enum Surface { Dirt, Turf, Synthetic }
// race going options (track conditions)
public enum Going { Good, Firm, Fast, Slow, Muddy }