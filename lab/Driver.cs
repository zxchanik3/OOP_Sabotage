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

        public virtual CarInput GetInput(Car car, float dT)
        {
            return new CarInput();
        }
    }
}