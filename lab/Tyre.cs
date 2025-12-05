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
		public int Durability { get; private set; } = 100;
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

		public void WearDown()
		{
			Durability -= (int)(10 * WearRate);
			if (Durability < 0) Durability = 0;

			if (Durability == 0)
			{
				GripLevel = 0;
				return;
			}

			GripLevel = (int)(initialGrip*Durability/100f);
		}
	}
}
