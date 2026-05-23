using System;

namespace lab
{
    public enum TyreType
    {
        Soft,
        Medium,
        Hard
    }

    public class Tyre
    {
        public TyreType Type { get; private set; }

        // Çáåð³ãàºìî ì³öí³ñòü ÿê float äëÿ òî÷íîãî ðîçðàõóíêó çíîñó ÷åðåç dT
        private float preciseDurability = 100f;
        public int Durability => (int)Math.Ceiling(preciseDurability);

        public int GripLevel { get; private set; }
        public float WearRate { get; }

        private int initialGrip;

        public Tyre(TyreType type)
        {
            Type = type;

            switch (type)
            {
                case TyreType.Soft:
                    GripLevel = 100;
                    WearRate = 2.0f;
                    break;

                case TyreType.Medium:
                    GripLevel = 80;
                    WearRate = 1.0f;
                    break;

                case TyreType.Hard:
                    GripLevel = 60;
                    WearRate = 0.5f;
                    break;

                default:
                    throw new ArgumentException("Unknown tyre type.");
            }

            initialGrip = GripLevel;
        }

        public void WearDown(float speed, float dT)
        {
            float wear = (10f * WearRate * (speed / 100f) * dT) / 40f;

            preciseDurability -= wear;
            if (preciseDurability < 0) preciseDurability = 0;

            if (Durability == 0)
            {
                GripLevel = 10;
                return;
            }

            GripLevel = (int)(initialGrip * preciseDurability / 100f);
        }
    }
}