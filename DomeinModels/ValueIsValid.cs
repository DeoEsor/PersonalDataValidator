namespace Models;

    public class ValueIsValid<T>
    {
        public ValueIsValid(T value, ValidState state = ValidState.NotStated)
        {
            Value = value;
            State = state;
        }

        public T Value { get; set; }
        public ValidState State { get; set; }

        public static explicit operator ValueIsValid<T>(T value)
        {
            return new ValueIsValid<T>(value);
        }
    }

    public enum ValidState
    {
        True,
        False,
        NotStated
    }