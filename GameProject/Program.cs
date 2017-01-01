namespace Game
{
    public class Program
    {
        static void Main(string[] args)
        {
            using (var window = new Window())
            {
                window.Controller = new Controller(window);
                window.Run(Controller.StepsPerSecond, Controller.DrawsPerSecond);
            }
        }
    }
}