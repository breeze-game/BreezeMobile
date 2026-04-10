using UnityEngine;

// actual race performance: mirrors calculatePerformance() in RaceGenerator.txt / Breeze Stats and Inheritance.md
// actualCondition = round(condition * (conditionPercent/100)); field names follow JS; UI labels those "form" + "condition".
// winner is picked from ticket weights proportional to actualPerformance
public static class RacePerformanceCalculator
{
    // main entry: mirrors calculatePerformance() from RaceGenerator.txt
    public static int CalculateActualPerformance(HorseTemplate h, int furlong, Surface surface, Going going)
    {
        // sum of core stats
        int stats =
            h.spd + h.stamina + h.acceleration + h.start +
            h.strength + h.intelligence + h.movement;

        // condition and fitness stack
        int actualCondition = Mathf.RoundToInt(h.condition * (h.conditionPercent / 100f));
        int actualFitness = Mathf.RoundToInt(h.fitness * (actualCondition / 100f));
        int performance = Mathf.RoundToInt((h.reputation * (actualFitness / 100f)) + stats);
        int actualPerformance = Mathf.RoundToInt(performance * (h.realizedPotential / 100f));

        // derived sub-stats (same formulas as js)
        int actualBattlingQualities = Mathf.RoundToInt(h.battlingQualities * ((h.intelligence + h.strength + h.enthusiasm) / 100f));
        int actualCruisingBurst = Mathf.RoundToInt(h.cruisingBurst * ((h.movement + h.spd + h.acceleration) / 50f));
        int actualExtraSpeedRating = Mathf.RoundToInt(h.extraSpeedRating * ((h.start + h.movement + h.acceleration) / 100f));
        int actualFinishApplication = Mathf.RoundToInt(h.finishApplication * ((h.spd + h.confidence + h.tenacity) / 100f));

        // potential roll-up
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

        // surface
        if (surface == h.preferredSurface)
            actualPerformance = Mathf.RoundToInt(actualPerformance * 1.25f);
        else if (Random.Range(0, 101) >= h.goingSurfaceAdaptability)
            actualPerformance = Mathf.RoundToInt(actualPerformance * (h.goingSurfaceAdaptability / 100f));

        // going
        if (going == h.preferredGoing)
            actualPerformance = Mathf.RoundToInt(actualPerformance * 1.25f);
        else if (Random.Range(0, 101) >= h.goingSurfaceAdaptability)
            actualPerformance = Mathf.RoundToInt(actualPerformance * (h.goingSurfaceAdaptability / 100f));

        // speed vs distance
        if (h.spd >= 15 && furlong <= 6)
            actualPerformance = Mathf.RoundToInt(actualPerformance * 1.25f);
        else if (h.spd > 5 && h.spd < 15 && furlong >= 7 && furlong <= 9)
            actualPerformance = Mathf.RoundToInt(actualPerformance * 1.25f);
        else if (h.spd <= 5 && furlong >= 10)
            actualPerformance = Mathf.RoundToInt(actualPerformance * 1.25f);
        else if (Random.Range(0, 101) >= h.distanceAdaptability)
            actualPerformance = Mathf.RoundToInt(actualPerformance * (h.distanceAdaptability / 100f));

        // stamina vs distance
        if (h.stamina <= 5 && furlong <= 6)
            actualPerformance = Mathf.RoundToInt(actualPerformance * 1.25f);
        else if (h.stamina > 5 && h.stamina < 15 && furlong >= 7 && furlong <= 9)
            actualPerformance = Mathf.RoundToInt(actualPerformance * 1.25f);
        else if (h.stamina >= 15 && furlong >= 10)
            actualPerformance = Mathf.RoundToInt(actualPerformance * 1.25f);
        else if (Random.Range(0, 101) >= h.distanceAdaptability)
            actualPerformance = Mathf.RoundToInt(actualPerformance * (h.distanceAdaptability / 100f));

        // enthusiasm disposition
        if (h.enthusiasm <= 25) { }
        else if (h.enthusiasm < 75) { }
        else if (h.enthusiasm <= 100 && Random.Range(0, 101) <= h.enthusiasm)
            actualPerformance = Mathf.RoundToInt(actualPerformance * ((100f - h.enthusiasm) / 100f));

        // quirks
        if (Random.Range(0, 101) <= h.quirks)
            actualPerformance = Mathf.RoundToInt(actualPerformance * ((100f - h.quirks) / 100f));

        // consistency (simplified nested random like js)
        int outer = Random.Range(Random.Range(50, 100), 101);
        if (Random.Range(0, outer + 1) >= h.consistency)
        {
            int factorPct = Random.Range(Random.Range(25, 50), Random.Range(150, 200));
            actualPerformance = Mathf.RoundToInt(actualPerformance * (factorPct / 100f));
        }

        // keep ticket system sane
        return Mathf.Max(1, actualPerformance);
    }
}
