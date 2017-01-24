namespace Game.Serialization
{
    public interface IShallowClone<out T> where T : IShallowClone<T>
    {
        T ShallowClone();
    }
}
