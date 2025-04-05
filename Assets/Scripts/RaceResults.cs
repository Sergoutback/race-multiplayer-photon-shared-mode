using System.Collections.Generic;

public static class RaceResults
{
    [System.Serializable]
    public class Entry
    {
        public string Name;
        public int Position;
        public float Time;
    }

    public static List<Entry> Players = new List<Entry>();
}