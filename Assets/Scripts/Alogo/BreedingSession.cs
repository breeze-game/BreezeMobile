using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BreedingSession : MonoBehaviour
{
    static readonly string[] FoundationMareNames = { "Adobe", "Eclipse", "Tranquility", "Wildfire", "Zahara" };
    static readonly string[] FoundationStallionNames = { "Goliath", "Milo", "Moondance", "Otto", "Razuli" };

    public List<HorseTemplate> mares = new();
    public List<HorseTemplate> stallions = new();

    [HideInInspector] public int mareIndex;
    [HideInInspector] public int stallionIndex;

    [Header("Owned Horses")]
    public List<HorseTemplate> ownedHorses = new();
    [HideInInspector] public int ownedPickIndex;

    public UnityEvent onPoolsChanged = new UnityEvent();

    [Tooltip("Furlongs used when rolling next-run performance preview (keep in sync with BreedingUIBuilder race distance).")]
    public int racePreviewFurlongs = 8;

    void Awake()
    {
        if (onPoolsChanged == null)
            onPoolsChanged = new UnityEvent();
        RegeneratePools();
    }

    public void RegeneratePools()
    {
        mares.Clear();
        stallions.Clear();

        for (int i = 0; i < FoundationMareNames.Length; i++)
            mares.Add(CreateFoundationHorse(isMare: true, FoundationMareNames[i], i));
        for (int i = 0; i < FoundationStallionNames.Length; i++)
            stallions.Add(CreateFoundationHorse(isMare: false, FoundationStallionNames[i], i));

        mareIndex = 0;
        stallionIndex = 0;
        onPoolsChanged?.Invoke();
    }

    HorseTemplate CreateFoundationHorse(bool isMare, string displayName, int slot)
    {
        var horse = new HorseTemplate
        {
            id = (isMare ? "M" : "S") + "_" + slot + "_" + Random.Range(1000, 9999),
            name = displayName,
            gender = isMare ? Gender.Mare : Gender.Stallion,
            sireName = "Foundation",
            damName = "Foundation"
        };
        RollRacingStats(horse);
        horse.progenyPotential = Random.Range(0, 3);
        horse.heightHands = AlogoGenetics.RandomHeightHands();
        AlogoGenetics.RollFoundationAppearance(horse);
        return horse;
    }

    public HorseTemplate SelectedMare =>
        mares.Count == 0 ? null : mares[Mathf.Clamp(mareIndex, 0, mares.Count - 1)];

    public HorseTemplate SelectedStallion =>
        stallions.Count == 0 ? null : stallions[Mathf.Clamp(stallionIndex, 0, stallions.Count - 1)];

    public HorseTemplate SelectedOwned =>
        ownedHorses.Count == 0 ? null : ownedHorses[Mathf.Clamp(ownedPickIndex, 0, ownedHorses.Count - 1)];

    public void CycleMare(int delta)
    {
        if (mares.Count == 0) return;
        mareIndex = (mareIndex + delta + mares.Count) % mares.Count;
    }

    public void CycleStallion(int delta)
    {
        if (stallions.Count == 0) return;
        stallionIndex = (stallionIndex + delta + stallions.Count) % stallions.Count;
    }

    public void CycleOwned(int delta)
    {
        if (ownedHorses.Count == 0) return;
        ownedPickIndex = (ownedPickIndex + delta + ownedHorses.Count) % ownedHorses.Count;
    }

    public void AddOwnedHorse(HorseTemplate h)
    {
        ownedHorses.Add(h);
        ownedPickIndex = ownedHorses.Count - 1;
    }

    /// <summary>Build foal without adding to barn — call <see cref="CommitFoalToBarn"/> after naming.</summary>
    public HorseTemplate BuildFoalFromSelection()
    {
        var dam = SelectedMare;
        var sire = SelectedStallion;
        if (dam == null || sire == null) return null;

        int ppAvg = Mathf.Clamp(Mathf.RoundToInt((dam.progenyPotential + sire.progenyPotential) / 2f), 0, 2);

        var foal = new HorseTemplate
        {
            id = "FOAL_" + Random.Range(10000, 99999),
            name = "",
            sireName = sire.name,
            damName = dam.name,
            progenyPotential = Mathf.Clamp(ppAvg + Random.Range(-1, 2), 0, 2)
        };

        AlogoGenetics.ApplyFoalGenetics(foal, dam, sire);

        foal.spd = BlendCoreStat(dam.spd, sire.spd, ppAvg);
        foal.stamina = BlendCoreStat(dam.stamina, sire.stamina, ppAvg);
        foal.acceleration = BlendCoreStat(dam.acceleration, sire.acceleration, ppAvg);
        foal.start = BlendCoreStat(dam.start, sire.start, ppAvg);
        foal.strength = BlendCoreStat(dam.strength, sire.strength, ppAvg);
        foal.intelligence = BlendCoreStat(dam.intelligence, sire.intelligence, ppAvg);
        foal.movement = BlendCoreStat(dam.movement, sire.movement, ppAvg);

        foal.tenacity = BlendWideStat(dam.tenacity, sire.tenacity, ppAvg, 1, 100);
        foal.enthusiasm = BlendWideStat(dam.enthusiasm, sire.enthusiasm, ppAvg, 1, 100);
        foal.confidence = BlendWideStat(dam.confidence, sire.confidence, ppAvg, 1, 100);
        foal.battlingQualities = BlendWideStat(dam.battlingQualities, sire.battlingQualities, ppAvg, 1, 100);
        foal.cruisingBurst = BlendWideStat(dam.cruisingBurst, sire.cruisingBurst, ppAvg, 1, 100);
        foal.extraSpeedRating = BlendWideStat(dam.extraSpeedRating, sire.extraSpeedRating, ppAvg, 1, 100);
        foal.finishApplication = BlendWideStat(dam.finishApplication, sire.finishApplication, ppAvg, 1, 100);
        foal.consistency = BlendWideStat(dam.consistency, sire.consistency, ppAvg, 1, 100);
        foal.distanceAdaptability = BlendWideStat(dam.distanceAdaptability, sire.distanceAdaptability, ppAvg, 1, 100);
        foal.goingSurfaceAdaptability = BlendWideStat(dam.goingSurfaceAdaptability, sire.goingSurfaceAdaptability, ppAvg, 1, 100);
        foal.quirks = Mathf.Clamp((dam.quirks + sire.quirks) / 2 + QuirkJitter(ppAvg), 0, 40);

        foal.potential = BlendWideStat(dam.potential, sire.potential, ppAvg, 1, 100);
        foal.realizedPotential = Mathf.Clamp(BlendWideStat(dam.realizedPotential, sire.realizedPotential, ppAvg, 1, 100), 1, foal.potential);
        foal.fitness = BlendWideStat(dam.fitness, sire.fitness, ppAvg, 1, 100);
        foal.reputation = Mathf.Clamp(BlendRep(dam.reputation, sire.reputation, ppAvg), 0, 2000);
        foal.condition = Mathf.Clamp(Mathf.RoundToInt((dam.condition + sire.condition) / 2f), 1, 100);
        foal.conditionPercent = Mathf.Clamp(Mathf.RoundToInt((dam.conditionPercent + sire.conditionPercent) / 2f), 1, 100);
        foal.preferredSurface = Random.value < 0.5f ? dam.preferredSurface : sire.preferredSurface;
        foal.preferredGoing = Random.value < 0.5f ? dam.preferredGoing : sire.preferredGoing;
        foal.heightHands = AlogoGenetics.FoalHeightHands(dam, sire);

        return foal;
    }

    public void CommitFoalToBarn(HorseTemplate foal, string chosenName)
    {
        if (foal == null) return;
        string fallback = "Álogo " + (ownedHorses.Count + 1);
        foal.name = string.IsNullOrWhiteSpace(chosenName) ? fallback : chosenName.Trim();
        AddOwnedHorse(foal);
        foal.nextRunPerformancePreview = RacePerformanceCalculator.CalculateActualPerformance(
            foal, racePreviewFurlongs, foal.preferredSurface, foal.preferredGoing);
    }

    static int CoreSpread(int progenyAvg)
    {
        return progenyAvg switch
        {
            0 => Random.Range(-4, 2),
            2 => Random.Range(-5, 6),
            _ => Random.Range(-2, 3)
        };
    }

    static int WideSpread(int progenyAvg)
    {
        return progenyAvg switch
        {
            0 => Random.Range(-8, 4),
            2 => Random.Range(-12, 13),
            _ => Random.Range(-5, 6)
        };
    }

    static int QuirkJitter(int progenyAvg)
    {
        return progenyAvg switch
        {
            0 => Random.Range(-3, 2),
            2 => Random.Range(-4, 5),
            _ => Random.Range(-2, 3)
        };
    }

    static int BlendCoreStat(int a, int b, int progenyAvg)
    {
        int baseVal = Mathf.RoundToInt((a + b) / 2f);
        return Mathf.Clamp(baseVal + CoreSpread(progenyAvg), 1, 25);
    }

    static int BlendWideStat(int a, int b, int progenyAvg, int min, int max)
    {
        int baseVal = Mathf.RoundToInt((a + b) / 2f);
        return Mathf.Clamp(baseVal + WideSpread(progenyAvg), min, max);
    }

    static int BlendRep(int a, int b, int progenyAvg)
    {
        int baseVal = Mathf.RoundToInt((a + b) / 2f);
        int spread = progenyAvg switch
        {
            0 => Random.Range(-45, 20),
            2 => Random.Range(-60, 61),
            _ => Random.Range(-30, 31)
        };
        return baseVal + spread;
    }

    /// <summary>
    /// NPC rivals: uniform random bands — similar spread to parents, not deliberately weak.
    /// </summary>
    public HorseTemplate GenerateRandomRacer(string racerName)
    {
        var h = new HorseTemplate();
        h.id = "R_" + Random.Range(1000, 9999);
        h.name = racerName;
        h.gender = Random.value < 0.5f ? Gender.Colt : Gender.Filly;
        h.sireName = "NPC";
        h.damName = "NPC";
        h.progenyPotential = Random.Range(0, 3);
        h.heightHands = AlogoGenetics.RandomHeightHands();
        RollRacingStats(h);
        AlogoGenetics.RollFoundationAppearance(h);
        return h;
    }

    void RollRacingStats(HorseTemplate horse)
    {
        horse.spd = Random.Range(5, 16);
        horse.stamina = Random.Range(5, 16);
        horse.acceleration = Random.Range(5, 16);
        horse.start = Random.Range(5, 16);
        horse.strength = Random.Range(5, 16);
        horse.intelligence = Random.Range(5, 16);
        horse.movement = Random.Range(5, 16);
        horse.tenacity = Random.Range(40, 91);
        horse.enthusiasm = Random.Range(20, 80);
        horse.confidence = Random.Range(40, 91);
        horse.battlingQualities = Random.Range(40, 91);
        horse.cruisingBurst = Random.Range(20, 80);
        horse.extraSpeedRating = Random.Range(40, 91);
        horse.finishApplication = Random.Range(40, 91);
        horse.consistency = Random.Range(60, 91);
        horse.distanceAdaptability = Random.Range(60, 91);
        horse.goingSurfaceAdaptability = Random.Range(60, 91);
        horse.quirks = Random.Range(0, 15);
        horse.potential = Random.Range(70, 93);
        horse.realizedPotential = Random.Range(65, Mathf.Min(92, horse.potential));
        horse.fitness = Random.Range(80, 99);
        horse.reputation = Random.Range(400, 800);
        horse.condition = Random.Range(60, 90);
        horse.conditionPercent = Random.Range(75, 95);
        horse.preferredSurface = (Surface)Random.Range(0, 3);
        horse.preferredGoing = (Going)Random.Range(0, 5);
    }
}
