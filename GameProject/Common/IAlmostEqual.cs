namespace Game.Common
{
    public interface IAlmostEqual<in T, TDelta> where T : IAlmostEqual<T, TDelta>
    {
        /// <summary>
        /// Check if this instance is within a delta of a comparison instance.
        /// </summary>
        bool AlmostEqual(T comparison, TDelta delta);
        /// <summary>
        /// Check if this instance is within a delta of a comparison instance 
        /// or if the ratio between comparison and this instance is less than a given percentage.
        /// </summary>
        bool AlmostEqual(T comparison, TDelta delta, TDelta ratioDelta);
    }
}
