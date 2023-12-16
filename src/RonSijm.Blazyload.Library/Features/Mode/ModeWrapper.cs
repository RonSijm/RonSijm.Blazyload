namespace RonSijm.Blazyload.Library.Features.Mode
{
    public abstract class ModeWrapper<T> where T : class
    {
        protected bool Equals(ModeWrapper<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((ModeWrapper<T>)obj);
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return EqualityComparer<T>.Default.GetHashCode(Value);
        }

        public T Value { get; private set; }

        // ReSharper disable once UnusedMember.Global
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