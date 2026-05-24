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
        private readonly Dictionary<Button, bool> _buttonState = new();

        public UserDriver(string name, int number)
            : base(name, number, lockStatus: false)
        {
            foreach (Button b in Enum.GetValues(typeof(Button)))
                _buttonState[b] = false;
        }

        public void Press(Button button)
        {
            _buttonState[button] = true;
        }

        public void Release(Button button)
        {
            _buttonState[button] = false;
        }

        public override CarInput GetInput(Car car, float dT)
        {
            CarInput input = new();

            if (_buttonState[Button.Forward])
                input.Throttle = 1f;

            if (_buttonState[Button.Backward])
                input.Brake = 1f;

            if (_buttonState[Button.Left])
                input.Steering = -1f;

            if (_buttonState[Button.Right])
                input.Steering = 1f;

            return input;
        }
    }
}