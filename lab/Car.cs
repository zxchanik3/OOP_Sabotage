using System.Numerics;

namespace lab
{
    public class Car
    {
        public string Model { get; set; }
        public string Team { get; set; }
        public int Year { get; set; }
        public int Horsepower { get; set; }
        public int Acceleration { get; set; }
        public int TopSpeed { get; set; }
        public int Weight { get; set; }
        
        public string ImagePath { get; set; }

        public float SpeedScale { get; set; } = 0.4f;

        public float Speed { get; set; }
        
        public Vector2 Position { get; private set; } = new Vector2(0, 0);
        public Vector2 Direction { get; private set; } = new Vector2(1, 0);

        public Tyre Tyres { get; private set; }
        public float MaxReverseSpeed { get; set; } = 40f;

        public Car(string model, string team, int year, int horsepower, int acceleration, int topSpeed, int weight, string imagePath = "")
        {
            Model = model;
            Year = year;
            Team = team;
            Horsepower = horsepower;
            Acceleration = acceleration;
            TopSpeed = topSpeed;
            Weight = weight;
            ImagePath = imagePath;
            Tyres = new Tyre(TyreType.Medium);
        }

        public void ChangeTyres(TyreType type)
        {
            Tyres = new Tyre(type);
        }

        public void UpdateSpeed(float accelInput, float dT)
        {
            float gripFactor = Tyres.GripLevel / 100f;

            float currentAccel = Acceleration * gripFactor * 10f;

            if (accelInput != 0)
            {
                Speed += currentAccel * accelInput * dT;
            }
            else
            {
                float friction = 15f * dT;
                if (Speed > 0)
                {
                    Speed -= friction;
                    if (Speed < 0) Speed = 0;
                }
                else if (Speed < 0)
                {
                    Speed += friction;
                    if (Speed > 0) Speed = 0;
                }
            }

            if (Speed > TopSpeed)
                Speed = TopSpeed;

            if (Speed < -MaxReverseSpeed)
                Speed = -MaxReverseSpeed;
        }

        public void UpdateDirection(float dT, float turnInput)
        {
            float absoluteSpeed = MathF.Abs(Speed);
            if (turnInput == 0 || absoluteSpeed < 0.1f) return;

            float gripFactor = Tyres.GripLevel / 100f;

            float baseAngularVelocity = 150f;
            float speedSensitivityFactor = 1f / (1f + absoluteSpeed / 600f);

            float effectiveAngularVelocity = baseAngularVelocity * gripFactor * speedSensitivityFactor;
            float angleChange = effectiveAngularVelocity * turnInput * dT;

            if (Speed < 0)
            {
                angleChange = -angleChange;
            }

            float angleRadians = MathF.PI / 180f * angleChange;
            Vector2 dir = Direction;

            float newX = dir.X * MathF.Cos(angleRadians) - dir.Y * MathF.Sin(angleRadians);
            float newY = dir.X * MathF.Sin(angleRadians) + dir.Y * MathF.Cos(angleRadians);

            Direction = Vector2.Normalize(new Vector2(newX, newY));
        }
        
        public void SetPosition(Vector2 newPos, Vector2 startDirection)
        {
            Position = newPos;
            Direction = startDirection;
        }

        public void Update(CarInput input, float dT)
        {
            UpdateDirection(dT, input.Steering);

            float accel = input.Throttle - input.Brake;

            UpdateSpeed(accel, dT);

            Position += Direction * (Speed * SpeedScale) * dT;

            if (MathF.Abs(Speed) > 10f)
                Tyres.WearDown(MathF.Abs(Speed), dT);
        }
    }
}