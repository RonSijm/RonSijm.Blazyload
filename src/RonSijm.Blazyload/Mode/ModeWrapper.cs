namespace RonSijm.Blazyload.Mode
{
    public class ModeWrapper<T> where T : class
    {
        public T Value { get; private set; }

        internal void SetValue(object value)
        {
            Value = value as T;
        }

        public static bool operator ==(ModeWrapper<T> obj1, ModeWrapper<T> obj2)
        {
            var firstNull = (object)obj1 == null || obj1.Value == null;
            var secondNull = (object)obj2 == null || obj2.Value == null;

            if (firstNull && secondNull)
            {
                return true;
            }

            if (firstNull || secondNull)
            {
                return false;
            }

            return obj1.Value.Equals(obj2.Value);
        }

        public static bool operator !=(ModeWrapper<T> obj1, ModeWrapper<T> obj2)
        {
            return !(obj1 == obj2);
        }

        public static implicit operator T(ModeWrapper<T> input)
        {
            return input.Value;
        }
    }
}
