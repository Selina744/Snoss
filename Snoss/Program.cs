using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snoss
{
    class Program
    {
        /*
        1 CPU that can execute one instruction (see below) at a time
    The CPU should have 6 registers
    You should have 10,000 bytes of RAM
    Your simulated disk will be your actual hard drive 
    (just a single directory)
    */
        static void Main(string[] args)
        {
            new ShellUi().RunShell();
        }
    }
}
