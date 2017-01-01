using System;

namespace Game
{
    public class Program
    {
        static void Main(string[] args)
        {
            using (Window window = new Window())
            {
                window.Controller = new Controller(window);
                window.Run(Controller.StepsPerSecond, Controller.DrawsPerSecond);
            }
        }
    }
}