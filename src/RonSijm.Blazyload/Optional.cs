namespace RonSijm.Blazyload
{
    public class Optional<T> where T : class
    {
        public T Value { get; internal set; }

        internal void SetValue(object value)
        {
            Value = value as T;
        }

        public static bool operator ==(Optional<T> obj1, Optional<T> obj2)
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

        public static bool operator !=(Optional<T> obj1, Optional<T> obj2)
        {
            return !(obj1 == obj2);
        }

        public static implicit operator T(Optional<T> input)
        {
            return input.Value;
        }
    }
}
