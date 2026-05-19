using System;
using System.Collections.Generic;

namespace lab
{
    public enum Button
    {
        Forward,
        Backward,
        Left,
        Right
    }

    public class UserDriver : Driver
    {
        private readonly Dictionary<Button, bool> buttonState = new();

        public UserDriver(string name, int number)
            : base(name, number, lockStatus: false)
        {
            foreach (Button b in Enum.GetValues(typeof(Button)))
                buttonState[b] = false;
        }

        public void Press(Button button)
        {
            if (buttonState.ContainsKey(button))
                buttonState[button] = true;
        }

        public void Release(Button button)
        {
            if (buttonState.ContainsKey(button))
                buttonState[button] = false;
        }

        public override void Drive(Car car, float dT)
        {
            if (car == null) return;

            float accelInput = 0f;
            float turnInput = 0f;

            if (buttonState[Button.Forward]) accelInput += 1f;
            if (buttonState[Button.Backward]) accelInput -= 1f;
            if (buttonState[Button.Left]) turnInput -= 1f;
            if (buttonState[Button.Right]) turnInput += 1f;

            car.UpdateSpeed(accelInput, dT);
            car.UpdateDirection(dT, turnInput);
            car.Move(dT);
        }
    }
}
