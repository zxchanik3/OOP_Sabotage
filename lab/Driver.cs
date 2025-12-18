using System;

namespace lab
{
    public class Driver
    {
        public string Name { get; set; } = "Unknown";
        public int Number { get; set; }
        public string Team { get; set; } = "Independent";

        public int Wins { get; set; }
        public int Races { get; set; }
        public int Podiums { get; set; }
        public int Position { get; set; }

        public bool Lock { get; protected set; }

        protected Driver() { }

        protected Driver(string name, int number, bool lockStatus)
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

        public virtual void Drive(Car car, float dT) { }
    }
}
