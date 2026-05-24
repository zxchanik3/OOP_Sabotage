namespace lab
{
    public class RaceEngine
    {
        public float RaceTime { get; private set; }
        public bool IsRaceFinished { get; private set; }

        public Dictionary<Car, int> CarLaps { get; } = new();
        public Dictionary<Car, int> CarLastWaypoint { get; } = new();

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
            int lastWp = CarLastWaypoint[car];

            if (closestNodeIndex > lastWp && closestNodeIndex <= lastWp + 5)
            {
                CarLastWaypoint[car] = closestNodeIndex;
            }
            else if (lastWp >= totalNodes - 5 && closestNodeIndex <= 5)
            {
                CarLaps[car]++;
                CarLastWaypoint[car] = closestNodeIndex;

                if (carIndex == 0 && CarLaps[car] > requiredLaps)
                {
                    IsRaceFinished = true;
                    return true; // Повертаємо true, якщо гравець завершив гонку
                }
            }
            return false;
        }

        public void Reset()
        {
            RaceTime = 0f;
            IsRaceFinished = false;
            CarLaps.Clear();
            CarLastWaypoint.Clear();
        }
    }
}