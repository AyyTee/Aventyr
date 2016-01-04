
namespace Game
{
    public class Program
    {
        static void Main(string[] args)
        {
            using (Window Window = new Window())
            {
                Window.Run(Controller.StepsPerSecond, Controller.DrawsPerSecond);
                //Window.Run(Controller.StepsPerSecond);
            }
        }
    }
}