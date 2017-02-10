using System;
using System.IO;
using System.Threading;

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
            var id = Int32.Parse(processId);
            cpu.removeProcessFromList(id);
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
            cpu.retrieveProcessInfo();
        }

        public void Exit()
        {
            Console.WriteLine("Goodbye");
            run = false;
        }

        public void ParseCommand(string command)
        {
            string[] commandArray = command.Split(' ');
            bool threaded = false;
            if (commandArray[commandArray.Length - 1].Equals("&"))
            {
                threaded = true;
            }
            switch (commandArray[0])
            {
                case "exit":
                    Exit();
                    break;
                case "ls":
                    if (threaded)
                    {
                        //Console.WriteLine("Running threaded ls");
                        new Thread(ListAvailablePrograms).Start();
                    }
                    else
                    {
                        ListAvailablePrograms();
                    }
                    break;
                case "ps":
                    if (threaded)
                    {
                        new Thread(ListRunningProcessesWithIds).Start();
                    }
                    else
                    {
                        ListRunningProcessesWithIds();
                    }
                    break;
                case "exec":
                    if (threaded)
                    {
                        new Thread(() => ExecuteProgram(commandArray[1], false)).Start();
                    }
                    else
                    {
                        ExecuteProgram(commandArray[1], false);
                    }
                    break;
                case "exec_i":
                    if (threaded)
                    {
                        new Thread(() => ExecuteProgram(commandArray[1], true)).Start();
                    }
                    else
                    {
                        ExecuteProgram(commandArray[1], true);
                    }
                    break;
                case "kill":
                    if (threaded)
                    {
                        new Thread(() => ExecuteProgram(commandArray[1], true)).Start();
                    }
                    else
                    {
                        KillProcess(commandArray[1]);
                    }
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
            //might need to kill process remove all process ids
        }
    }
}
