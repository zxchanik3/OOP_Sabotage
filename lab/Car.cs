using System;

namespace lab
{
    public class Car
    {
        public string Model { get; set; }
        public string Team { get; set; } = "Independent";
        public int Year { get; set; }
        public int Horsepower { get; set; }
        public int Acceleration { get; set; }
        public int TopSpeed { get; set; }
        public int Weight { get; set; }
        public int Speed { get; private set; } = 0;
        public Tyre Tyres { get; set; }

        private const float g = 9.81f;

        public Car() { }

        ~Car() { }

        public Car(string model, int year, int horsepower, Tyre tyre, int acceleration)
        {
            Model = model;
            Year = year;
            Horsepower = horsepower;
            Acceleration = acceleration;

            Tyres = new Tyre(tyre.Type, tyre.Durability, tyre.GripLevel, tyre.WearRate);
        }

        public void PlayerControlSpeed(float accelInput, float brakeInput, float steerInput, float dt)
        {
            float v = Speed / 3.6f;

            float tyreCondition = MathF.Max(0.4f, Tyres.Durability / 100f);
            
            v += Acceleration * accelInput * dt;
            v -= Acceleration * 1.5f * brakeInput * dt;

            float vmax = TopSpeed / 3.6f;
            if (v > vmax) v = vmax;
            if (v < 0) v = 0;

            float turnPenalty = 1f - MathF.Abs(steerInput) * 0.3f * tyreCondition;
            v *= turnPenalty;

            Speed = (int)(v * 3.6f);
        }
    }
}
