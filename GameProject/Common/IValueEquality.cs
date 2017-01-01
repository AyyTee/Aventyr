namespace Game.Common
{
    /// <summary>
    /// Equality comparison for reference types.
    /// </summary>
    public interface IValueEquality<in T> where T : class, IValueEquality<T>
    {
        bool EqualsValue(T other);
    }
}
