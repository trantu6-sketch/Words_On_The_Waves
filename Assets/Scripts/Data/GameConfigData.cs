using System.Collections.Generic;

namespace WordsOnTheWaves.Data
{
    [System.Serializable]
    public class DropRateData
    {
        public string genre;
        public int rate;
    }

    [System.Serializable]
    public class CrateData
    {
        public string id;
        public string name;
        public float cost;
        public List<DropRateData> dropRates;
    }

    [System.Serializable]
    public class LocationData
    {
        public string id;
        public string name;
        public float travelCost;
        public List<string> targetAudience;
    }

    [System.Serializable]
    public class GameConfig
    {
        public List<LocationData> locations;
        public List<CrateData> crates;
    }
}
