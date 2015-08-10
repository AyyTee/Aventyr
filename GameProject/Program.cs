
namespace Game
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Controller Controller = new Controller())
            {
                Controller.Run(60, 60);
            }
        }
    }
}
