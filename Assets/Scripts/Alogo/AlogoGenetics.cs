using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Port of Alogo.txt Horse genetics (random*, determine*, set*) plus Breeze cosmetic layers and anomaly gating.
/// </summary>
public static class AlogoGenetics
{
    static int R1_100() => Random.Range(1, 101);
    static int R0_1() => Random.Range(0, 2);

    static T Pick<T>(T[] a) => a[Random.Range(0, a.Length)];

    public static void RollFoundationAppearance(HorseTemplate h)
    {
        h.modifiers.Clear();
        h.markings.Clear();
        h.modifierAnomaly = "";
        h.dilutionAnomaly = "";
        h.markingAnomaly = "";
        h.anomaly = "N/A";

        RandomMane(h);
        RandomTail(h);
        RandomNose(h);
        RandomCoat(h);
        SetModifier(h);
        SetMarking(h);
        RandomDilution(h);
        if (R1_100() <= 3)
            h.markingAnomaly = BreezeCosmeticTables.MarkingAnomalyLeopard;
        RandomAnomaly(h);

        if (R1_100() <= 2)
            h.modifierAnomaly = BreezeCosmeticTables.ModifierAnomalyNonDun1;
        if (R1_100() <= 2)
        {
            var da = new[] { BreezeCosmeticTables.DilutionAnomalyMushroom, BreezeCosmeticTables.DilutionAnomalySnowdrop, BreezeCosmeticTables.DilutionAnomalySunshine };
            h.dilutionAnomaly = Pick(da);
        }

        ClearGeneticSnapshotsForFoundation(h);
    }

    static void ClearGeneticSnapshotsForFoundation(HorseTemplate h)
    {
        h.geneticSireMane = "N/A";
        h.geneticSireTail = "N/A";
        h.geneticSireNose = "N/A";
        h.geneticDamCoat = "N/A";
        h.geneticDamCoatRarity = "N/A";
        h.geneticDamModifiers.Clear();
        h.geneticDamMarkings.Clear();
        h.geneticDamMane = "N/A";
        h.geneticDamTail = "N/A";
        h.geneticDamNose = "N/A";
    }

    public static void ApplyFoalGenetics(HorseTemplate foal, HorseTemplate dam, HorseTemplate sire)
    {
        foal.modifiers.Clear();
        foal.markings.Clear();
        foal.modifierAnomaly = "";
        foal.dilutionAnomaly = "";
        foal.markingAnomaly = "";
        foal.anomaly = "N/A";

        RandomFoalGender(foal);
        DetermineCoat(foal, dam);
        DetermineModifier(foal, dam);
        DetermineMarking(foal, dam);
        DetermineMane(foal, dam, sire);
        DetermineTail(foal, dam, sire);
        DetermineNose(foal, dam, sire);
        DetermineDilution(foal, dam);
        if (R1_100() <= 3)
            foal.markingAnomaly = BreezeCosmeticTables.MarkingAnomalyLeopard;
        RandomAnomaly(foal);

        if (R1_100() <= 2)
            foal.modifierAnomaly = BreezeCosmeticTables.ModifierAnomalyNonDun1;
        if (R1_100() <= 2)
        {
            var da = new[] { BreezeCosmeticTables.DilutionAnomalyMushroom, BreezeCosmeticTables.DilutionAnomalySnowdrop, BreezeCosmeticTables.DilutionAnomalySunshine };
            foal.dilutionAnomaly = Pick(da);
        }

        CopyGeneticSnapshotFromParents(foal, dam, sire);
    }

    public static void RandomFoalGender(HorseTemplate foal)
    {
        foal.gender = R0_1() == 0 ? Gender.Colt : Gender.Filly;
    }

    public static void RandomMane(HorseTemplate h)
    {
        int result = R1_100();
        if (result <= 5) h.mane = BreezeCosmeticTables.ThreeStarMane[0];
        else if (result <= 10) h.mane = BreezeCosmeticTables.ThreeStarMane[1];
        else if (result <= 20) h.mane = BreezeCosmeticTables.TwoStarMane[0];
        else if (result <= 30) h.mane = BreezeCosmeticTables.TwoStarMane[1];
        else h.mane = BreezeCosmeticTables.OneStarMane[0];
    }

    public static void RandomTail(HorseTemplate h)
    {
        int result = R1_100();
        if (result <= 5) h.tail = BreezeCosmeticTables.ThreeStarTail[0];
        else if (result <= 10) h.tail = BreezeCosmeticTables.ThreeStarTail[1];
        else if (result <= 20) h.tail = BreezeCosmeticTables.TwoStarTail[0];
        else if (result <= 30) h.tail = BreezeCosmeticTables.TwoStarTail[1];
        else h.tail = BreezeCosmeticTables.OneStarTail[0];
    }

    public static void RandomNose(HorseTemplate h)
    {
        int result = R1_100();
        if (result <= 5) h.nose = BreezeCosmeticTables.ThreeStarNose[0];
        else if (result <= 20) h.nose = BreezeCosmeticTables.TwoStarNose[0];
        else h.nose = BreezeCosmeticTables.OneStarNose[0];
    }

    public static void RandomCoat(HorseTemplate h)
    {
        int result = R1_100();
        if (result <= 5)
        {
            h.coat = Pick(BreezeCosmeticTables.ThreeStarCoats);
            h.coatRarity = BreezeCosmeticTables.Tier3;
        }
        else if (result <= 25)
        {
            h.coat = Pick(BreezeCosmeticTables.TwoStarCoats);
            h.coatRarity = BreezeCosmeticTables.Tier2;
        }
        else
        {
            h.coat = Pick(BreezeCosmeticTables.OneStarCoats);
            h.coatRarity = BreezeCosmeticTables.Tier1;
        }
    }

    static string RollModifierString()
    {
        int result = R1_100();
        if (result <= 5) return Pick(BreezeCosmeticTables.ThreeStarModifiers);
        if (result <= 30) return Pick(BreezeCosmeticTables.TwoStarModifiers);
        return Pick(BreezeCosmeticTables.OneStarModifiers);
    }

    static string RollMarkingString()
    {
        int result = R1_100();
        if (result <= 5) return Pick(BreezeCosmeticTables.ThreeStarMarkings);
        if (result <= 15) return Pick(BreezeCosmeticTables.TwoStarMarkings);
        return Pick(BreezeCosmeticTables.OneStarMarkings);
    }

    public static void SetModifier(HorseTemplate h)
    {
        h.modifiers.Add(RollModifierString());
    }

    public static void SetMarking(HorseTemplate h)
    {
        h.markings.Add(RollMarkingString());
    }

    public static void RandomDilution(HorseTemplate h)
    {
        int result = R1_100();
        if (result <= 5) h.dilution = Pick(BreezeCosmeticTables.ThreeStarDilution);
        else if (result <= 30) h.dilution = Pick(BreezeCosmeticTables.TwoStarDilution);
        else h.dilution = Pick(BreezeCosmeticTables.OneStarDilution);
    }

    public static void DetermineCoat(HorseTemplate foal, HorseTemplate dam)
    {
        RandomCoat(foal);
        int coin = R0_1();
        if (dam.coatRarity == foal.coatRarity && coin == 0)
            foal.coat = dam.coat;
    }

    static string DamModifierJoined(HorseTemplate dam)
    {
        return BreezeCosmeticTables.JoinList(dam.modifiers);
    }

    static string DamMarkingJoined(HorseTemplate dam)
    {
        return BreezeCosmeticTables.JoinList(dam.markings);
    }

    public static void DetermineModifier(HorseTemplate foal, HorseTemplate dam)
    {
        foal.modifiers.Add(RollModifierString());
        int coin = R0_1();
        if (coin == 0)
        {
            string d = DamModifierJoined(dam);
            if (!string.IsNullOrEmpty(d) && d != "none" && d != "N/A")
                foal.modifiers.Add(d);
        }
    }

    public static void DetermineMarking(HorseTemplate foal, HorseTemplate dam)
    {
        foal.markings.Add(RollMarkingString());
        int r = Random.Range(1, 5); // 1..4 inclusive
        if (r == 1)
        {
            string d = DamMarkingJoined(dam);
            if (!string.IsNullOrEmpty(d) && d != "none" && d != "N/A")
                foal.markings.Add(d);
        }
    }

    public static void DetermineMane(HorseTemplate foal, HorseTemplate dam, HorseTemplate sire)
    {
        int result = R1_100();
        if (result <= 25)
        {
            int inner = R1_100();
            foal.mane = inner <= 25 ? sire.mane : dam.mane;
        }
        else RandomMane(foal);
    }

    public static void DetermineTail(HorseTemplate foal, HorseTemplate dam, HorseTemplate sire)
    {
        int result = R1_100();
        if (result <= 25)
        {
            int inner = R1_100();
            foal.tail = inner <= 25 ? sire.tail : dam.tail;
        }
        else RandomTail(foal);
    }

    public static void DetermineNose(HorseTemplate foal, HorseTemplate dam, HorseTemplate sire)
    {
        int result = R1_100();
        if (result <= 25)
        {
            int inner = R1_100();
            foal.nose = inner <= 25 ? sire.nose : dam.nose;
        }
        else RandomNose(foal);
    }

    public static void DetermineDilution(HorseTemplate foal, HorseTemplate dam)
    {
        RandomDilution(foal);
        if (R0_1() == 0 && !string.IsNullOrEmpty(dam.dilution) && !BreezeCosmeticTables.IsNoDilution(dam.dilution))
            foal.dilution = dam.dilution;
    }

    public static void RandomAnomaly(HorseTemplate h)
    {
        int result = R1_100();
        if (result != 1)
        {
            h.anomaly = "N/A";
            return;
        }

        var eligible = new List<string>();
        foreach (var a in BreezeCosmeticTables.AnomalyPool)
        {
            if (CanHaveAnomaly(h, a))
                eligible.Add(a);
        }
        if (eligible.Count == 0)
        {
            h.anomaly = "N/A";
            return;
        }
        h.anomaly = eligible[Random.Range(0, eligible.Count)];
    }

    public static bool CanHaveAnomaly(HorseTemplate h, string a)
    {
        var x = a.ToLowerInvariant();
        if (x.Contains("gray only"))
            return BreezeCosmeticTables.CoatLooksGray(h.coat);

        if (x.Contains("gulastra plume"))
            return BreezeCosmeticTables.SabinoPresent(h.markings);

        if (x.Contains("badger face"))
            return BreezeCosmeticTables.SabinoPresent(h.markings) || BreezeCosmeticTables.ListContainsSubstring(h.markings, "splash")
                                                                  || BreezeCosmeticTables.ListContainsSubstring(h.markings, "dominant white");

        if (x.Contains("corn spots"))
            return BreezeCosmeticTables.HasRoanModifier(h.modifiers);

        if (x.Contains("ink spots"))
            return BreezeCosmeticTables.TobianoPresent(h.markings);

        if (x.Contains("reverse markings"))
            return BreezeCosmeticTables.PintoFamilyMarking(h.markings);

        if (x.Contains("mismarks"))
            return BreezeCosmeticTables.CoatLooksGray(h.coat) || BreezeCosmeticTables.HasLeopardMarkingAnomaly(h.markingAnomaly);

        if (x.Contains("varnish roan"))
            return BreezeCosmeticTables.HasLeopardMarkingAnomaly(h.markingAnomaly);

        if (x.Contains("bider marks"))
            return BreezeCosmeticTables.HasDunDilution(h.dilution);

        return true;
    }

    static void CopyGeneticSnapshotFromParents(HorseTemplate foal, HorseTemplate dam, HorseTemplate sire)
    {
        foal.geneticSireMane = sire.mane;
        foal.geneticSireTail = sire.tail;
        foal.geneticSireNose = sire.nose;
        foal.geneticDamCoat = dam.coat;
        foal.geneticDamCoatRarity = dam.coatRarity;
        foal.geneticDamModifiers.Clear();
        foreach (var m in dam.modifiers) foal.geneticDamModifiers.Add(m);
        foal.geneticDamMarkings.Clear();
        foreach (var m in dam.markings) foal.geneticDamMarkings.Add(m);
        foal.geneticDamMane = dam.mane;
        foal.geneticDamTail = dam.tail;
        foal.geneticDamNose = dam.nose;

        foal.recordedSireMane = foal.geneticSireMane;
        foal.recordedSireTail = foal.geneticSireTail;
        foal.recordedSireNose = foal.geneticSireNose;
        foal.recordedDamCoat = foal.geneticDamCoat;
        foal.recordedDamCoatRarity = foal.geneticDamCoatRarity;
        foal.recordedDamModifiers.Clear();
        foreach (var m in foal.geneticDamModifiers) foal.recordedDamModifiers.Add(m);
        foal.recordedDamMarkings.Clear();
        foreach (var m in foal.geneticDamMarkings) foal.recordedDamMarkings.Add(m);
        foal.recordedDamMane = foal.geneticDamMane;
        foal.recordedDamTail = foal.geneticDamTail;
        foal.recordedDamNose = foal.geneticDamNose;
    }

    /// <summary>Typical TB mature height in hands (placeholder scale).</summary>
    public static float RandomHeightHands()
    {
        return Mathf.Round(Random.Range(15.0f, 17.05f) * 10f) / 10f;
    }

    public static float FoalHeightHands(HorseTemplate dam, HorseTemplate sire)
    {
        float avg = (dam.heightHands + sire.heightHands) * 0.5f;
        return Mathf.Clamp(Mathf.Round((avg + Random.Range(-0.35f, 0.35f)) * 10f) / 10f, 14.8f, 17.2f);
    }
}
