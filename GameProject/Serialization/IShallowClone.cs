namespace Game.Serialization
{
    public interface IShallowClone<T> where T : IShallowClone<T>
    {
        T ShallowClone();
    }
}
