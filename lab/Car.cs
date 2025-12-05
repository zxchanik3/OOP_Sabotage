using System;
using System.Numerics;

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
        
        public float Speed { get; private set; } = 0f;
        
        public Car() { }
        
        public Vector2 Position { get; set; } = new Vector2(0, 0);
        public Vector2 Direction { get; set; } = new Vector2(1, 0);

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
        public void UpdateSpeed()
        {
            //ïîò³ì
        }
        
        public void UpdateDirection(float angle)
        {
            //ïîò³ì
        }
    }
}
