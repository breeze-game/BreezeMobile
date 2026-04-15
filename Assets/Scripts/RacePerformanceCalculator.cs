using UnityEngine;

/// <summary>
/// Mirrors calculatePerformance() in RaceGenerator.txt / Breeze Stats and Inheritance.md.
/// Maximum performance is computed for future use but not shown in UI.
/// </summary>
internal readonly struct RacePerformanceOutcome
{
    public int ActualPerformance { get; }
    /// <summary>performance * (potential/100) before bonus cascade — design doc ceiling reference.</summary>
    public int MaximumPerformance { get; }

    public RacePerformanceOutcome(int actualPerformance, int maximumPerformance)
    {
        ActualPerformance = actualPerformance;
        MaximumPerformance = maximumPerformance;
    }
}

public static class RacePerformanceCalculator
{
    public static int CalculateActualPerformance(HorseTemplate h, int furlong, Surface surface, Going going)
    {
        return CalculatePerformanceOutcome(h, furlong, surface, going).ActualPerformance;
    }

    internal static RacePerformanceOutcome CalculatePerformanceOutcome(HorseTemplate h, int furlong, Surface surface, Going going)
    {
        int stats =
            h.spd + h.stamina + h.acceleration + h.start +
            h.strength + h.intelligence + h.movement;

        int actualCondition = Mathf.RoundToInt(h.condition * (h.conditionPercent / 100f));
        int actualFitness = Mathf.RoundToInt(h.fitness * (actualCondition / 100f));
        int performance = Mathf.RoundToInt((h.reputation * (actualFitness / 100f)) + stats);
        int maximumPerformance = Mathf.RoundToInt(performance * (h.potential / 100f));
        int actualPerformance = Mathf.RoundToInt(performance * (h.realizedPotential / 100f));

        int actualBattlingQualities = Mathf.RoundToInt(h.battlingQualities * ((h.intelligence + h.strength + h.enthusiasm) / 100f));
        int actualCruisingBurst = Mathf.RoundToInt(h.cruisingBurst * ((h.movement + h.spd + h.acceleration) / 50f));
        int actualExtraSpeedRating = Mathf.RoundToInt(h.extraSpeedRating * ((h.start + h.movement + h.acceleration) / 100f));
        int actualFinishApplication = Mathf.RoundToInt(h.finishApplication * ((h.spd + h.confidence + h.tenacity) / 100f));

        if (h.realizedPotential >= 90)
            actualPerformance = Mathf.RoundToInt(actualPerformance * 1.5f);

        if (actualFinishApplication >= 100)
            actualPerformance = Mathf.RoundToInt(actualPerformance * 1.5f);

        if (actualExtraSpeedRating >= 100)
            actualPerformance = Mathf.RoundToInt(actualPerformance * 1.5f);

        if (actualCruisingBurst >= 90)
            actualPerformance = Mathf.RoundToInt(actualPerformance * 1.3f);

        if (actualBattlingQualities >= 80)
            actualPerformance = Mathf.RoundToInt(actualPerformance * 1.1f);

        if (h.acceleration >= 15)
            actualPerformance = Mathf.RoundToInt(actualPerformance * 1.05f);

        if (h.start >= 15)
            actualPerformance = Mathf.RoundToInt(actualPerformance * 1.5f);

        if (surface == h.preferredSurface)
            actualPerformance = Mathf.RoundToInt(actualPerformance * 1.25f);
        else if (Random.Range(0, 101) >= h.goingSurfaceAdaptability)
            actualPerformance = Mathf.RoundToInt(actualPerformance * (h.goingSurfaceAdaptability / 100f));

        if (going == h.preferredGoing)
            actualPerformance = Mathf.RoundToInt(actualPerformance * 1.25f);
        else if (Random.Range(0, 101) >= h.goingSurfaceAdaptability)
            actualPerformance = Mathf.RoundToInt(actualPerformance * (h.goingSurfaceAdaptability / 100f));

        if (h.spd >= 15 && furlong <= 6)
            actualPerformance = Mathf.RoundToInt(actualPerformance * 1.25f);
        else if (h.spd > 5 && h.spd < 15 && furlong >= 7 && furlong <= 9)
            actualPerformance = Mathf.RoundToInt(actualPerformance * 1.25f);
        else if (h.spd <= 5 && furlong >= 10)
            actualPerformance = Mathf.RoundToInt(actualPerformance * 1.25f);
        else if (Random.Range(0, 101) >= h.distanceAdaptability)
            actualPerformance = Mathf.RoundToInt(actualPerformance * (h.distanceAdaptability / 100f));

        if (h.stamina <= 5 && furlong <= 6)
            actualPerformance = Mathf.RoundToInt(actualPerformance * 1.25f);
        else if (h.stamina > 5 && h.stamina < 15 && furlong >= 7 && furlong <= 9)
            actualPerformance = Mathf.RoundToInt(actualPerformance * 1.25f);
        else if (h.stamina >= 15 && furlong >= 10)
            actualPerformance = Mathf.RoundToInt(actualPerformance * 1.25f);
        else if (Random.Range(0, 101) >= h.distanceAdaptability)
            actualPerformance = Mathf.RoundToInt(actualPerformance * (h.distanceAdaptability / 100f));

        if (h.enthusiasm <= 25) { }
        else if (h.enthusiasm < 75) { }
        else if (h.enthusiasm <= 100 && Random.Range(0, 101) <= h.enthusiasm)
            actualPerformance = Mathf.RoundToInt(actualPerformance * ((100f - h.enthusiasm) / 100f));

        if (Random.Range(0, 101) <= h.quirks)
            actualPerformance = Mathf.RoundToInt(actualPerformance * ((100f - h.quirks) / 100f));

        int outer = Random.Range(Random.Range(50, 100), 101);
        if (Random.Range(0, outer + 1) >= h.consistency)
        {
            int factorPct = Random.Range(Random.Range(25, 50), Random.Range(150, 200));
            actualPerformance = Mathf.RoundToInt(actualPerformance * (factorPct / 100f));
        }

        actualPerformance = Mathf.Max(1, actualPerformance);
        maximumPerformance = Mathf.Max(1, maximumPerformance);
        return new RacePerformanceOutcome(actualPerformance, maximumPerformance);
    }
}
