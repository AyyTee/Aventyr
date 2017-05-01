namespace Game
{
    /// <summary>Something that can exist within an IScene.</summary>
    public interface ISceneObject : IName
    {
        void Remove();
    }
}
