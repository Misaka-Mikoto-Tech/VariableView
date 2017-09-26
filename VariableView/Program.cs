using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariableView
{
    class Program
    {
        static void Main(string[] args)
        {
            Robot.Robot robot = new VariableView.Robot.Robot();
            robot.Run();
            Console.ReadKey();
        }
    }
}
