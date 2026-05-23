using System;
using System.Numerics;

namespace lab
{
    public class Car
    {
        public string Model { get; set; } = "Unknown";
        public string Team { get; set; } = "Independent";
        public int Year { get; set; }
        public int Horsepower { get; set; }
        public int Acceleration { get; set; } // Твій тип даних
        public int TopSpeed { get; set; }
        public int Weight { get; set; }
        
        // ФІЧА КОЛЕГИ: Шлях до картинки для Гаража
        public string ImagePath { get; set; } = ""; 

        public float SpeedScale { get; set; } = 0.4f;

        // ТВІЙ КОД: Speed має { get; set; } для роботи рестарту гри
        public float Speed { get; set; } = 0f; 
        
        public Vector2 Position { get; private set; } = new Vector2(0, 0);
        public Vector2 Direction { get; private set; } = new Vector2(1, 0);
        private float AngularVelocity { get; set; } = 90f;
        public Tyre Tyres { get; private set; }
        public float MaxReverseSpeed { get; set; } = 40f;

        // ОНОВЛЕНИЙ КОНСТРУКТОР: Твої типи даних + ImagePath від колеги
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

        public void UpdateSpeed(float accelInput, float dT, TrackSegment currentSegment)
        {
            if (Tyres == null) return;

            float gripFactor = Tyres.GripLevel / 100f;

            float currentAccel = Acceleration * gripFactor;

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
            if (Tyres == null || turnInput == 0 || absoluteSpeed < 0.1f) return;

            float gripFactor = Tyres.GripLevel / 100f;

            // ТВОЯ ФІЗИКА: Машина повертає швидше і краще тримає керування
            float baseAngularVelocity = 150f;
            float speedSensitivityFactor = 1f / (1f + absoluteSpeed / 300f);

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

        public void Update(CarInput input, float dT, TrackSegment segment)
        {
            UpdateDirection(dT, input.Steering);

            float accel = input.Throttle - input.Brake;

            UpdateSpeed(accel, dT, segment);

            Position += Direction * (Speed * SpeedScale) * dT;

            if (Tyres != null && MathF.Abs(Speed) > 10f)
                Tyres.WearDown(MathF.Abs(Speed), dT);
        }
    }
}