using System.Collections.Generic;
using UnityEngine;

// ticket-style finish order: each horse gets weight = actualPerformance, repeat until all placed
public static class SimpleRaceSimulator
{
    /// <param name="ticketWeightsForRace">If set, each horse uses the given weight for every placement round (same roll for whole race).</param>
    public static List<string> RunRace(
        List<HorseTemplate> runners,
        int furlong,
        Surface surface,
        Going going,
        Dictionary<HorseTemplate, int> ticketWeightsForRace = null)
    {
        var names = new List<string>();
        var pool = new List<HorseTemplate>(runners);

        while (pool.Count > 0)
        {
            int totalTickets = 0;
            var tickets = new List<int>(pool.Count);
            foreach (var h in pool)
            {
                int ap;
                if (ticketWeightsForRace != null && ticketWeightsForRace.TryGetValue(h, out int fixedW))
                    ap = fixedW;
                else
                    ap = RacePerformanceCalculator.CalculatePerformanceOutcome(h, furlong, surface, going).ActualPerformance;
                int w = Mathf.Max(1, ap);
                tickets.Add(w);
                totalTickets += w;
            }

            int pick = Random.Range(0, totalTickets);
            int acc = 0;
            int winnerIdx = 0;
            for (int i = 0; i < pool.Count; i++)
            {
                acc += tickets[i];
                if (pick < acc)
                {
                    winnerIdx = i;
                    break;
                }
            }

            var winner = pool[winnerIdx];
            names.Add(winner.name);
            pool.RemoveAt(winnerIdx);
        }

        return names;
    }
}
