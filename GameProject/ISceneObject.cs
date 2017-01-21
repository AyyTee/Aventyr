namespace Game
{
    /// <summary>Something that can exist within an IScene.</summary>
    public interface ISceneObject
    {
        void Remove();
        //Simple name used to quickly identify objects.  Cannot be assumed to be unique or constant.
        string Name { get; }
    }
}
