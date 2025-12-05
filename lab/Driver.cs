using System;

namespace lab
{
    public class Driver
    {
        public string Name { get; set; } = "Unknown";
        public int Number { get; set; }
        public string Team { get; set; } = "Independent";
        public int Wins { get; set; } = 0;
        public int Races { get; set; } = 0;
        public int Podiums { get; set; } = 0;
        public int Position { get; set; }
        public bool Lock { get; set; }

        public Driver() { }

        public Driver(string name, int number, bool lockStatus)
        {
            Name = name;
            Number = number;
            Lock = lockStatus;
        }
        
        public void SetTeam(string team)
        {
            Team = team;
        }
        public void AddRaceResult()
        {
            if (Position == 1) Wins++;
            if (Position <= 3) Podiums++;
            Races++;
        }
    }
}
