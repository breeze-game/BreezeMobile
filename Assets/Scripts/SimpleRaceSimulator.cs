using System.Collections.Generic;
using UnityEngine;

// ticket-style finish order: each horse gets weight = actualPerformance, repeat until all placed
public static class SimpleRaceSimulator
{
    // returns names in finish order (1st place first)
    public static List<string> RunRace(
        List<HorseTemplate> runners,
        int furlong,
        Surface surface,
        Going going)
    {
        // string list we will return
        var names = new List<string>();
        // working copy so we can remove winners
        var pool = new List<HorseTemplate>(runners);

        // keep drawing winners until nobody left
        while (pool.Count > 0)
        {
            // sum of all ticket weights this round
            int totalTickets = 0;
            // parallel list: ticket count per horse in pool
            var tickets = new List<int>(pool.Count);
            // compute tickets from actualPerformance for each remaining horse
            foreach (var h in pool)
            {
                // one roll of the doc formula for this horse today
                int ap = RacePerformanceCalculator.CalculateActualPerformance(h, furlong, surface, going);
                // at least 1 ticket so horse always has a chance
                int w = Mathf.Max(1, ap);
                tickets.Add(w);
                totalTickets += w;
            }

            // random ticket index in [0, totalTickets)
            int pick = Random.Range(0, totalTickets);
            // running sum to find which horse owns that ticket
            int acc = 0;
            // default 0 safe if loop always breaks
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

            // horse that won this placement
            var winner = pool[winnerIdx];
            names.Add(winner.name);
            pool.RemoveAt(winnerIdx);
        }

        return names;
    }
}
