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
        public int Acceleration { get; set; }
        public int TopSpeed { get; set; }
        public int Weight { get; set; }
        
        public float Speed { get; private set; } = 0f;
        public Vector2 Position { get; private set; } = new Vector2(0, 0);
        public Vector2 Direction { get; private set; } = new Vector2(1, 0);
        private float AngularVelocity { get; set; } = 90f;
        public Tyre Tyres { get; private set; }
        
        public Car(string model, string team, int year, int horsepower, int acceleration, int topSpeed, int weight)
        {
            Model = model;
            Year = year;
            Team = team;
            Horsepower = horsepower;
            Acceleration = acceleration;
            TopSpeed = topSpeed;
            Weight = weight;
        }

        public void ChangeTyres(TyreType type)
        {
            Tyres = new Tyre(type);
        }

        public void UpdateSpeed(float accelInput, float dT)
        {
            if (Tyres == null) return;

            float Accel = Acceleration * (Tyres.GripLevel / 100f);
            Speed += Accel * accelInput * dT;
            if (Speed > TopSpeed) Speed = TopSpeed;
            if (Speed < 0) Speed = 0;
        }

        public void UpdateDirection(float dT, float turnInput)
        {
            if (Tyres == null || turnInput == 0) return;

            float effectiveAngularVelocity = AngularVelocity * (Tyres.GripLevel / 100f);
            float angleChange = effectiveAngularVelocity * turnInput * dT;
            float angleRadians = MathF.PI / 180f * angleChange;

            Vector2 dir = Direction;

            float newX = dir.X * MathF.Cos(angleRadians) - dir.Y * MathF.Sin(angleRadians);
            float newY = dir.X * MathF.Sin(angleRadians) + dir.Y * MathF.Cos(angleRadians);

            Direction = Vector2.Normalize(new Vector2(newX, newY));
        }

        public void Move(float dT)
        {
            Position += Direction * Speed * dT;
        }
    }
}
