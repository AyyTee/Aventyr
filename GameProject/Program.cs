using System;

namespace Game
{
    public class Program
    {
        static void Main(string[] args)
        {
            using (Window Window = new Window())
            {
                Window.controller = new Controller(Window, args);
                Window.Run(Controller.StepsPerSecond, Controller.DrawsPerSecond);
            }
        }
    }
}