using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snoss
{
    class Logger
    {


        //Create a "logging level" for your OS.Log OS output to a file (or the console - your choice).  
        //If the logging level is set high enough log "kernel" level process management info about which PID is executing and what instruction is being executed.
        //Also, log when your OS decides to time slice and switch which process is in the CPU.

        public LoggerLevels Level {get; private set;}
        private StreamWriter outputFile;

        public Logger(LoggerLevels level)
        {
            Level = level;
        }

        public void SetWriter(String outputName)
        {
            outputFile = new StreamWriter(@"./ " + outputName);
        }

        public void Log(String loggerData)
        {
            outputFile.Write(loggerData);
        }








    }
}
