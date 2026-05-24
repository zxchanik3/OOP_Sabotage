namespace lab
{
    public class RaceEngine
    {
        public float RaceTime { get; private set; }
        public bool IsRaceFinished { get; private set; }

        public Dictionary<Car, int> CarLaps { get; } = new();
        public Dictionary<Car, int> CarLastWaypoint { get; } = new();
        public Dictionary<Car, bool> FinishedCars = new Dictionary<Car, bool>();

        public List<Car> FinishOrder { get; } = new();

        public void UpdateTime(float deltaTime)
        {
            if (!IsRaceFinished) RaceTime += deltaTime;
        }

        public void RegisterCarInRace(Car car)
        {
            CarLaps[car] = 1;
            CarLastWaypoint[car] = 0;
        }

        public bool CheckLapAndCheckpoints(Car car, int carIndex, int closestNodeIndex, int totalNodes, int requiredLaps)
        {
            if (!CarLaps.ContainsKey(car)) RegisterCarInRace(car);

            if (IsCarFinished(car)) return false;

            int lastWp = CarLastWaypoint[car];

            if (closestNodeIndex > lastWp && closestNodeIndex <= lastWp + 5)
            {
                CarLastWaypoint[car] = closestNodeIndex;
            }
            else if (lastWp >= totalNodes - 5 && closestNodeIndex <= 5)
            {
                CarLaps[car]++;
                CarLastWaypoint[car] = closestNodeIndex;

                if (CarLaps[car] > requiredLaps)
                {
                    MarkAsFinished(car);
                    FinishOrder.Add(car);

                    if (carIndex == 0)
                    {
                        IsRaceFinished = true;
                        return true;
                    }
                }
            }
            return false;
        }
        
        public bool IsCarFinished(Car car) 
        {
            return FinishedCars.ContainsKey(car);
        }

        public void MarkAsFinished(Car car) 
        {
            if (!FinishedCars.ContainsKey(car)) FinishedCars.Add(car, true);
        }

        public void Reset()
        {
            RaceTime = 0f;
            IsRaceFinished = false;
            CarLaps.Clear();
            CarLastWaypoint.Clear();
            FinishedCars.Clear();
            FinishOrder.Clear();
        }
    }
}