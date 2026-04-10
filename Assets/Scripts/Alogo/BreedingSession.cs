using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BreedingSession : MonoBehaviour
{
    // breeding pool size
    [Header("Parent Pool Size")]
    [SerializeField] int poolSizeEach = 6; // number of horses in each breeding pool

    // breeding pool lists
    public List<HorseTemplate> mares = new();
    public List<HorseTemplate> stallions = new();

    // breeding pool indices
    [HideInInspector] public int mareIndex;
    [HideInInspector] public int stallionIndex;

    // horses you own (bred foals + starter)
    [Header("Owned Horses")]
    public List<HorseTemplate> ownedHorses = new();
    // which owned horse is selected for racing
    [HideInInspector] public int ownedPickIndex;

    // ui listens here when mares/stallions pools reroll (triple tap on background)
    public UnityEvent onPoolsChanged = new UnityEvent();

    // when the script is enabled, regenerate the breeding pools
    void Awake()
    {
        // make sure event exists even if inspector never touched this component
        if (onPoolsChanged == null)
            onPoolsChanged = new UnityEvent();

        // regenerate horses
        RegeneratePools();
    }

    // helper function to regenerate the breeding pools
    public void RegeneratePools()
    {
        // clear the current breeding pools
        mares.Clear();
        stallions.Clear();

        // iterate through the pool size and generate random horses
        for (int i = 0; i < poolSizeEach; i++)
        {
            // add mares and stallions
            mares.Add(GenerateRandomParent(isMare: true, i));
            stallions.Add(GenerateRandomParent(isMare: false, i));
        }

        // reset the breeding pool indices
        mareIndex = 0;
        stallionIndex = 0;

        // notify ui (BreedingUIBuilder) to redraw name lists
        if (onPoolsChanged != null)
            onPoolsChanged.Invoke();
    }

    // helper to get selected mare
    public HorseTemplate SelectedMare =>
        mares.Count == 0 ? null : mares[Mathf.Clamp(mareIndex, 0, mares.Count - 1)];

    // helper to get selected stallion
    public HorseTemplate SelectedStallion =>
        stallions.Count == 0 ? null : stallions[Mathf.Clamp(stallionIndex, 0, stallions.Count - 1)];

    // helper to get selected owned horse for races
    public HorseTemplate SelectedOwned =>
        ownedHorses.Count == 0 ? null : ownedHorses[Mathf.Clamp(ownedPickIndex, 0, ownedHorses.Count - 1)];

    // cycle mare index
    public void CycleMare(int delta)
    {
        if (mares.Count == 0) return;
        mareIndex = (mareIndex + delta + mares.Count) % mares.Count;
    }

    // cycle stallion index
    public void CycleStallion(int delta)
    {
        if (stallions.Count == 0) return;
        stallionIndex = (stallionIndex + delta + stallions.Count) % stallions.Count;
    }

    // cycle which owned horse is picked
    public void CycleOwned(int delta)
    {
        if (ownedHorses.Count == 0) return;
        ownedPickIndex = (ownedPickIndex + delta + ownedHorses.Count) % ownedHorses.Count;
    }

    // add foal to barn
    public void AddOwnedHorse(HorseTemplate h)
    {
        ownedHorses.Add(h);
        ownedPickIndex = ownedHorses.Count - 1;
    }

    // breed foal from current mare + stallion (simplified blend: doc-ready fields)
    public HorseTemplate BreedFoalFromSelection()
    {
        var dam = SelectedMare;
        var sire = SelectedStallion;
        if (dam == null || sire == null) return null;

        var foal = new HorseTemplate();
        foal.id = "FOAL_" + Random.Range(10000, 99999);
        foal.name = "Álogo " + (ownedHorses.Count + 1);
        foal.gender = Random.value < 0.5f ? Gender.Colt : Gender.Filly;
        foal.sireName = sire.name;
        foal.damName = dam.name;

        // appearance: mix from parents (simple rules; full alogo.txt later)
        foal.coat = Random.value < 0.5f ? dam.coat : sire.coat;
        foal.coatRarity = Random.value < 0.5f ? dam.coatRarity : sire.coatRarity;
        foal.mane = sire.mane;
        foal.tail = dam.tail;
        foal.nose = Random.value < 0.5f ? dam.nose : sire.nose;
        foal.anomaly = Random.value < 0.08f ? PickAnomaly() : "";

        // stat blend + small roll
        foal.spd = BlendStat(dam.spd, sire.spd);
        foal.stamina = BlendStat(dam.stamina, sire.stamina);
        foal.acceleration = BlendStat(dam.acceleration, sire.acceleration);
        foal.start = BlendStat(dam.start, sire.start);
        foal.strength = BlendStat(dam.strength, sire.strength);
        foal.intelligence = BlendStat(dam.intelligence, sire.intelligence);
        foal.movement = BlendStat(dam.movement, sire.movement);
        foal.tenacity = BlendStat(dam.tenacity, sire.tenacity);
        foal.enthusiasm = BlendStat(dam.enthusiasm, sire.enthusiasm);
        foal.confidence = BlendStat(dam.confidence, sire.confidence);
        foal.battlingQualities = BlendStat(dam.battlingQualities, sire.battlingQualities);
        foal.cruisingBurst = BlendStat(dam.cruisingBurst, sire.cruisingBurst);
        foal.extraSpeedRating = BlendStat(dam.extraSpeedRating, sire.extraSpeedRating);
        foal.finishApplication = BlendStat(dam.finishApplication, sire.finishApplication);
        foal.consistency = BlendStat(dam.consistency, sire.consistency);
        foal.distanceAdaptability = BlendStat(dam.distanceAdaptability, sire.distanceAdaptability);
        foal.goingSurfaceAdaptability = BlendStat(dam.goingSurfaceAdaptability, sire.goingSurfaceAdaptability);
        foal.quirks = Mathf.Clamp((dam.quirks + sire.quirks) / 2 + Random.Range(-2, 3), 0, 40);

        foal.potential = Mathf.Clamp(Mathf.RoundToInt((dam.potential + sire.potential) / 2f) + Random.Range(-5, 6), 1, 100);
        foal.realizedPotential = Mathf.Clamp(Mathf.RoundToInt((dam.realizedPotential + sire.realizedPotential) / 2f) + Random.Range(-5, 6), 1, foal.potential);
        foal.fitness = Mathf.Clamp(Mathf.RoundToInt((dam.fitness + sire.fitness) / 2f) + Random.Range(-5, 6), 1, 100);
        foal.reputation = Mathf.Clamp(Mathf.RoundToInt((dam.reputation + sire.reputation) / 2f) + Random.Range(-30, 31), 0, 2000);
        foal.condition = Mathf.Clamp(Mathf.RoundToInt((dam.condition + sire.condition) / 2f), 1, 100);
        foal.conditionPercent = Mathf.Clamp(Mathf.RoundToInt((dam.conditionPercent + sire.conditionPercent) / 2f), 1, 100);
        foal.preferredSurface = Random.value < 0.5f ? dam.preferredSurface : sire.preferredSurface;
        foal.preferredGoing = Random.value < 0.5f ? dam.preferredGoing : sire.preferredGoing;

        AddOwnedHorse(foal);
        return foal;
    }

    int BlendStat(int a, int b)
    {
        int baseVal = Mathf.RoundToInt((a + b) / 2f);
        return Mathf.Clamp(baseVal + Random.Range(-2, 3), 1, 25);
    }

    string PickAnomaly()
    {
        string[] opts =
        {
            "birdcatcher spots", "bend-or spots", "chimera", "brindle"
        };
        return "*** " + opts[Random.Range(0, opts.Length)];
    }

    // random npc for race field
    public HorseTemplate GenerateRandomRacer(string racerName)
    {
        var h = new HorseTemplate();
        h.id = "R_" + Random.Range(1000, 9999);
        h.name = racerName;
        h.gender = Random.value < 0.5f ? Gender.Colt : Gender.Filly;
        h.coat = "* bay";
        h.coatRarity = "*";
        h.mane = "* medium";
        h.tail = "* medium";
        h.nose = "* evident";

        h.spd = Random.Range(5, 16);
        h.stamina = Random.Range(5, 16);
        h.acceleration = Random.Range(5, 16);
        h.start = Random.Range(5, 16);
        h.strength = Random.Range(5, 16);
        h.intelligence = Random.Range(5, 16);
        h.movement = Random.Range(5, 16);
        h.tenacity = Random.Range(40, 91);
        h.enthusiasm = Random.Range(20, 80);
        h.confidence = Random.Range(40, 91);
        h.battlingQualities = Random.Range(40, 91);
        h.cruisingBurst = Random.Range(20, 80);
        h.extraSpeedRating = Random.Range(40, 91);
        h.finishApplication = Random.Range(40, 91);
        h.consistency = Random.Range(60, 91);
        h.distanceAdaptability = Random.Range(60, 91);
        h.goingSurfaceAdaptability = Random.Range(60, 91);
        h.quirks = Random.Range(0, 15);
        h.potential = Random.Range(70, 93);
        h.realizedPotential = Random.Range(65, Mathf.Min(92, h.potential));
        h.fitness = Random.Range(80, 99);
        h.reputation = Random.Range(400, 800);
        h.condition = Random.Range(60, 90);
        h.conditionPercent = Random.Range(75, 95);
        h.preferredSurface = (Surface)Random.Range(0, 3);
        h.preferredGoing = (Going)Random.Range(0, 5);
        return h;
    }

    // helper function to generate a random parent horse
    HorseTemplate GenerateRandomParent(bool isMare, int slot)
    {
        // create a new horse object
        var horse = new HorseTemplate();

        // set the horse's id, name and gender
        horse.id = (isMare ? "M" : "S") + "_" + slot + "_" + Random.Range(1000, 9999);
        horse.name = (isMare ? "Mare " : "Stallion ") + (slot + 1);
        horse.gender = isMare ? Gender.Mare : Gender.Stallion;
        horse.coat = Random.value < 0.33f ? "* bay" : (Random.value < 0.5f ? "* chestnut" : "** buckskin");
        horse.coatRarity = "*";
        horse.mane = "* medium";
        horse.tail = "* medium";
        horse.nose = "* evident";

        // set the horse's stats
        horse.spd = Random.Range(5, 16);
        horse.stamina = Random.Range(5, 16);
        horse.acceleration = Random.Range(5, 16);
        horse.start = Random.Range(5, 16);
        horse.strength = Random.Range(5, 16);
        horse.intelligence = Random.Range(5, 16);
        horse.movement = Random.Range(5, 16);

        // set the horse's mentality stats
        horse.tenacity = Random.Range(40, 91);
        horse.enthusiasm = Random.Range(20, 80);
        horse.confidence = Random.Range(40, 91);

        // set the horse's preferred surface and going
        // set extra racing / adaptability stats (see RaceGenerator.txt)
        horse.battlingQualities = Random.Range(40, 91);
        horse.cruisingBurst = Random.Range(20, 80);
        horse.extraSpeedRating = Random.Range(40, 91);
        horse.finishApplication = Random.Range(40, 91);
        horse.consistency = Random.Range(60, 91);
        horse.distanceAdaptability = Random.Range(60, 91);
        horse.goingSurfaceAdaptability = Random.Range(60, 91);
        horse.quirks = Random.Range(0, 12);
        horse.potential = Random.Range(75, 92);
        horse.realizedPotential = Random.Range(70, Mathf.Min(91, horse.potential));
        horse.fitness = Random.Range(82, 99);
        horse.reputation = Random.Range(450, 850);
        horse.condition = Random.Range(65, 90);
        horse.conditionPercent = Random.Range(78, 95);
        horse.preferredSurface = (Surface)Random.Range(0, 3);
        horse.preferredGoing = (Going)Random.Range(0, 5);

        // horse is created
        return horse;
    }

    /*
    // helper function to get the selected mare
    public HorseTemplate SelectedMare =>
        mares.Count == 0 ? null : mares[Mathf.Clamp(mareIndex, 0, mares.Count - 1)];

    // helper function to get the selected stallion
    public HorseTemplate SelectedStallion =>
        stallions.Count == 0 ? null : stallions[Mathf.Clamp(stallionIndex, 0, stallions.Count - 1)];

    // helper function to cycle the selected mare
    public void CycleMare(int delta)
    {
        if (mares.Count == 0) return;
        mareIndex = (mareIndex + delta + mares.Count) % mares.Count;
    }
    */

    /*
    // helper function to cycle the selected stallion
    public void CycleStallion(int delta)
    {
        if (stallions.Count == 0) return;
        stallionIndex = (stallionIndex + delta + stallions.Count) % stallions.Count;
    }
    */
}