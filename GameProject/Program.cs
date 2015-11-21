
namespace Game
{
    public class Program
    {
        static void Main(string[] args)
        {
            using (Window Window = new Window())
            {
                Window.Run(Controller.StepsPerSecond, Controller.DrawsPerSecond);
            }
            /*using (Controller Controller = new Controller())
            {
                Controller.Run(Controller.StepsPerSecond, Controller.DrawsPerSecond);
            }*/
        }
    }
}