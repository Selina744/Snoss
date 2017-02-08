using System;
using System.IO;

namespace Snoss
{
    class ShellUi
    {
        private readonly CPU cpu = new CPU();
        private string programFolderPath = "./programs";
        public void ExecuteProgram(string fileName, bool i)
        {
            string totalPath = programFolderPath + "/" + fileName;
            if (File.Exists(totalPath))
            {
                Console.WriteLine("Executing " + fileName + "...");
                string location = Assembler.TranslateFile(totalPath);
                cpu.LoadProgram(location, i);
            }
            else
            {
                Console.WriteLine("File does not exist.");
            }

        }

        public void KillProcess(string processId)
        {
            Console.WriteLine("Killing process " + processId + "...");
            //Immediately stop the process associated with process_id and unload it from RAM

        }

        public void ListAvailablePrograms()
        {
            Console.WriteLine("Available Processes:");
            string[] files = System.IO.Directory.GetFiles(programFolderPath, "*.txt");
            foreach (var file in files)
            {
                Console.WriteLine(file);
            }
        }

        public void ListRunningProcessesWithIds()
        {
            //Displays the Pid, file which the process is executing, the register values and the instruction pointer 
            //for EACH of the current processes.
        }

        public void Exit()
        {
            Console.WriteLine("Goodbye");
            run = false;
        }

        public void ParseCommand(string command)
        {
            switch (command.Split(' ')[0])
            {
                case "exit":
                    Exit();
                    break;
                case "ls":
                    ListAvailablePrograms();
                    break;
                case "ps":
                    ListRunningProcessesWithIds();
                    break;
                case "exec":
                    ExecuteProgram(command.Split(' ')[1], false);
                    break;
                case "exec_i":
                    ExecuteProgram(command.Split(' ')[1], true);
                    break;
                case "kill":
                    KillProcess(command.Split(' ')[1]);
                    break;
                default:
                    Console.WriteLine("Error Unknown command.");
                    break;
            }
        }

        bool run = true;

        public void RunShell()
        {
            Console.WriteLine("Welcome to SNOSS. Please input commands.");
            while (run)
            {
                string command = Console.ReadLine();
                ParseCommand(command);
            }
        }
    }
}
