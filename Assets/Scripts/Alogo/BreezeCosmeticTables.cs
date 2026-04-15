using System.Collections.Generic;

/// <summary>
/// Cosmetic pools — tier prefixes use Unicode BLACK STAR U+2605 (★★★). Legacy "* " strings still recognized where noted.
/// Setup: Assets/Scripts/Resources/FontSetup_NotoSansSymbols2.txt
/// </summary>
public static class BreezeCosmeticTables
{
    public const string Tier1 = "★";
    public const string Tier2 = "★★";
    public const string Tier3 = "★★★";
    public const string DilutionNone = "★ none";

    public static readonly string[] GenderFoalOptions = { "colt", "filly" };

    public static readonly string[] OneStarMane = { "★ medium" };
    public static readonly string[] TwoStarMane = { "★★ cropped", "★★ short" };
    public static readonly string[] ThreeStarMane = { "★★★ long curly", "★★★ long straight" };

    public static readonly string[] OneStarTail = { "★ medium" };
    public static readonly string[] TwoStarTail = { "★★ long straight", "★★ short" };
    public static readonly string[] ThreeStarTail = { "★★★ long curly", "★★★ cropped" };

    public static readonly string[] OneStarNose = { "★ evident" };
    public static readonly string[] TwoStarNose = { "★★ subtle" };
    public static readonly string[] ThreeStarNose = { "★★★ extreme" };

    public static readonly string[] OneStarCoats =
    {
        "★ bay", "★ dark bay", "★ chestnut", "★ gray"
    };

    public static readonly string[] TwoStarCoats =
    {
        "★★ liver chestnut", "★★ seal brown", "★★ wild bay", "★★ buckskin", "★★ palomino", "★★ smoky black"
    };

    public static readonly string[] ThreeStarCoats =
    {
        "★★★ black", "★★★ smoky cream", "★★★ cremello", "★★★ perlino", "★★★ wild buckskin"
    };

    public static readonly string[] OneStarModifiers = { "none" };
    public static readonly string[] TwoStarModifiers = { "★★ gray", "★★ flaxen" };
    public static readonly string[] ThreeStarModifiers = { "★★★ roan", "★★★ sooty" };

    public static readonly string[] OneStarMarkings = { "none" };
    public static readonly string[] TwoStarMarkings = { "★★ sabino", "★★ splash", "★★ dominant white" };
    public static readonly string[] ThreeStarMarkings = { "★★★ overo", "★★★ tobiano", "★★★ rabicano" };

    public static readonly string[] OneStarDilution = { DilutionNone };
    public static readonly string[] TwoStarDilution = { "★★ cream" };
    public static readonly string[] ThreeStarDilution =
    {
        "★★★ silver", "★★★ dun", "★★★ pangaré", "★★★ champagne", "★★★ pearl", "★★★ double cream"
    };

    public const string ModifierAnomalyNonDun1 = "★★★ non-dun 1";
    public const string DilutionAnomalyMushroom = "★★★ mushroom";
    public const string DilutionAnomalySnowdrop = "★★★ snowdrop";
    public const string DilutionAnomalySunshine = "★★★ sunshine";
    public const string MarkingAnomalyLeopard = "★★★ leopard complex";

    public static readonly string[] AnomalyPool =
    {
        "★★★ birdcatcher spots",
        "★★★ bend-or spots",
        "★★★ chubari spots (gray only)",
        "★★★ bloody shoulder (gray only)",
        "★★★ gulastra plume (sabino only)",
        "★★★ badger face (sabino only)",
        "★★★ reverse badger face (sabino only)",
        "★★★ chimera",
        "★★★ somatic mutation",
        "★★★ brindle",
        "★★★ reverse brindle",
        "★★★ manchado",
        "★★★ ermine spots",
        "★★★ belton pattern",
        "★★★ corn spots (roan only)",
        "★★★ ink spots (tobiano only)",
        "★★★ reverse markings (pinto family)",
        "★★★ mismarks (leopard or gray)",
        "★★★ varnish roan (leopard only)",
        "★★★ bider marks (dun only)",
        "★★★ lacing",
        "★★★ reverse dapples"
    };

    public static string JoinList(List<string> items)
    {
        if (items == null || items.Count == 0) return "none";
        return string.Join(", ", items);
    }

    public static bool ListContainsSubstring(List<string> items, string sub)
    {
        if (items == null) return false;
        foreach (var s in items)
        {
            if (s != null && s.IndexOf(sub, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }
        return false;
    }

    public static bool CoatLooksGray(string coat)
    {
        if (string.IsNullOrEmpty(coat)) return false;
        var c = coat.ToLowerInvariant();
        return c.Contains("gray") || c.Contains("grey") || c.Contains("smoky cream") || c.Contains("cremello");
    }

    public static bool HasRoanModifier(List<string> mods)
    {
        return ListContainsSubstring(mods, "roan");
    }

    public static bool HasDunDilution(string dilution)
    {
        return !string.IsNullOrEmpty(dilution) && dilution.IndexOf("dun", System.StringComparison.OrdinalIgnoreCase) >= 0
                                               && dilution.IndexOf("non-dun", System.StringComparison.OrdinalIgnoreCase) < 0;
    }

    public static bool HasLeopardMarkingAnomaly(string markingAnomaly)
    {
        return !string.IsNullOrEmpty(markingAnomaly) && markingAnomaly.IndexOf("leopard", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static bool PintoFamilyMarking(List<string> markings)
    {
        foreach (var m in markings)
        {
            if (m == null) continue;
            var x = m.ToLowerInvariant();
            if (x.Contains("sabino") || x.Contains("splash") || x.Contains("dominant white") ||
                x.Contains("overo") || x.Contains("tobiano") || x.Contains("rabicano"))
                return true;
        }
        return false;
    }

    public static bool TobianoPresent(List<string> markings)
    {
        return ListContainsSubstring(markings, "tobiano");
    }

    public static bool SabinoPresent(List<string> markings)
    {
        return ListContainsSubstring(markings, "sabino");
    }

    public static bool SplashOrOvero(List<string> markings)
    {
        return ListContainsSubstring(markings, "splash") || ListContainsSubstring(markings, "overo");
    }

    public static bool IsNoDilution(string dilution)
    {
        if (string.IsNullOrEmpty(dilution)) return true;
        var t = dilution.Trim().ToLowerInvariant();
        return t == "* none" || t == "★ none" || t == "none";
    }
}
