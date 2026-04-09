using System.Collections.Generic;
using UnityEngine;

public class BreedingSession : MonoBehaviour
{
    // breeding pool size
    [Header("Session parent pool size")]
    [SerializeField] int poolSizeEach = 6; // number of horses in each breeding pool

    // breeding pool lists
    public List<HorseTemplate> mares = new();
    public List<HorseTemplate> stallions = new();

    // breeding pool indices
    [HideInInspector] public int mareIndex;
    [HideInInspector] public int stallionIndex;

    // when the script is enabled, regenerate the breeding pools
    void Awake()
    {
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
    }

    // helper function to generate a random parent horse
    HorseTemplate GenerateRandomParent(bool isMare, int slot)
    {
        // NOTE: PLACEHOLDER

        // create a new horse object
        var horse = new HorseTemplate();

        // set the horse's id, name and gender
        horse.id = (isMare ? "M" : "S") + "_" + slot + "_" + Random.Range(1000, 9999);
        horse.name = (isMare ? "Mare " : "Stallion ") + (slot + 1);
        horse.gender = isMare ? Gender.Mare : Gender.Stallion;

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
        horse.enthusiasm = Random.Range(40, 91);
        horse.confidence = Random.Range(40, 91);

        // set the horse's preferred surface and going
        horse.preferredSurface = (Surface)Random.Range(0, 3);
        horse.preferredGoing = (Going)Random.Range(0, 5);

        // horse is created
        return horse;
    }

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

    // helper function to cycle the selected stallion
    public void CycleStallion(int delta)
    {
        if (stallions.Count == 0) return;
        stallionIndex = (stallionIndex + delta + stallions.Count) % stallions.Count;
    }
}