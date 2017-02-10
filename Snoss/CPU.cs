using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Timers;
using System.Threading;

namespace Snoss
{
    class CPU
    {
        //max size of program/instruction is 1000 - 520 = 480 or 120 instructions
        private byte[][] registers = new byte[6][];
        private bool theI;

        //constants
        private short pcbMetaDataSize = 20;
        private short ramMetaDataSize = 12;
        private short processSize = 1000;
        private int pcbSize = 500;

        Ram ram = new Ram();
        System.Timers.Timer timer = new System.Timers.Timer(500);

        private string instructionWords = null;

        //Change this to use the ram as well?
        //Store all process ids in 8 set locations. (8 processes max currently)
        //when new process is added loop thru until empty location is found and put processId there.
        //When next process is selected, start at current processId and loop until location is not empty.
        private LinkedList<int> processIds;
        private LinkedListNode<int> currentProcessNode;

        public CPU()
        {
            processIds = new LinkedList<int>();
            for (int i = 0; i < registers.Length; i++)
            {
                registers[i] = new byte[2];
            }
            Thread th = new Thread(() => RunProgram());
            th.Start();
        }

        public void LoadProgram(string fileName, bool i)
        {
            int nextProcessId = 0;
            if (processIds.Count > 0)
            {
                nextProcessId = processIds.Last.Value + 1;
            }
            //fill out pcb header, pcb and instructions for process
            int pcbMetaDataStart = GetStartOfProccess(nextProcessId);
            //save instruction pointer to header
            ram.WriteToMemoryAtIndex(pcbMetaDataStart, 0, BitConverter.GetBytes(0));
            //save instructions after pcb
            byte[] instructionBytes = File.ReadAllBytes(fileName);
            int instructionStart = pcbMetaDataStart + pcbMetaDataSize + pcbSize;
            ram.WriteToMemoryAtIndex(instructionStart, 0, instructionBytes);
            //save instruction size to header
            ram.WriteToMemoryAtIndex(pcbMetaDataStart, 4, BitConverter.GetBytes(instructionBytes.Length));
            theI = i;
            LinkedListNode<int> newProccessNode = new LinkedListNode<int>(nextProcessId);
            if (processIds.Count == 0)
            {
                currentProcessNode = newProccessNode;
                SetProcessInformation();
            }
            processIds.AddLast(newProccessNode);
        }
        bool run = true;
        public void RunProgram()
        {
            //loop to execute instruction lines
            run = true;
            while (run)
            {

                //Console.WriteLine("Instruction pointer set to: {0}", instructionPointer);
                //if (i)
                //{
                //    Console.WriteLine("State of registers before instruction: ");
                //    PrintRegisters();
                //}
                if (processIds.Count > 0)
                {
                    if (TimeToSwitch())
                    {
                        SwitchProgram();
                    }
                    int instructionPointer = ram.GetInstructionPointer(GetStartOfProccess(currentProcessNode.Value));
                      //  Console.WriteLine("instruction pointer: " + instructionPointer);
                    byte[] instruction = ram.GetMemoryAtIndex(ram.GetInstructionStart(), instructionPointer, 4);
                    instructionPointer += 4;
                    ram.SetInstructionPointer(instructionPointer, GetStartOfProccess(currentProcessNode.Value));
                    ExecuteInstruction(instruction, theI);

                    if (theI)
                    {
                        Console.WriteLine("State of registers after instruction: ");
                        PrintRegisters();
                    }
                }

            }

        }

        private void LoadProcess()
        {
            SetNextProcess();
            SetProcessInformation();
            SetRegisters();
        }

        private void SaveProcess()
        {
            SaveRegisters();
        }

        private void SaveRegisters()
        {
            int pcbMetaDataStart = ram.GetPcbHeaderStart();
            for (int i = 0; i < registers.Length; i+=2)
            {
                ram.WriteToMemoryAtIndex(pcbMetaDataStart, i + 8, registers[i]);
            }
        }

        private void SwitchProgram()
        {
            SaveProcess();
            LoadProcess();
        }

        private int GetStartOfProccess(int processId)
        {
            return processId * processSize + ramMetaDataSize;
        }

        private void SetProcessInformation()
        {
            int pcbHeaderStart = GetStartOfProccess(currentProcessNode.Value);
            ram.SetPcbHeaderStart(pcbHeaderStart);
            ram.SetInstructionStart(pcbHeaderStart + pcbMetaDataSize + pcbSize);
            //get instruction size
            //ram.GetMemoryAtIndex(_pcbMetaDataStart, 4, BitConverter.GetBytes(instructionBytes.Length));
        }

        private void SetRegisters()
        {
            int pcbHeaderStart = ram.GetPcbHeaderStart();
            for (int i = 0; i < registers.Length; i+=2)
            {
                registers[i] = ram.GetMemoryAtIndex(pcbHeaderStart, i + 8, 2);
            }
        }

        private void SetNextProcess()
        {
            if (currentProcessNode == processIds.Last)
            {
                currentProcessNode = processIds.First;
            }
            else
            {
                currentProcessNode = currentProcessNode.Next;
            }
        }

        DateTime switchTime = DateTime.Now.AddMilliseconds(500);
        private bool TimeToSwitch()
        {
            DateTime now = DateTime.Now;

            //time to milliseconds : 5
            bool switchProcess = false;
            if (processIds.Contains(ram.GetCurrentProcessId()))
            {
                if (processIds.Count > 1)
                {
                    if (now > switchTime)
                    {
                        switchProcess = true;
                        switchTime = now.AddMilliseconds(500);
                    }
                }
            }
            else
            {
                switchProcess = true;
            }
            //if(switchProcess)
             //   Console.WriteLine("Switcher: time: " + switchTime + " did switch:" + switchProcess);
            //int newTime = DateTime.
            return switchProcess;
        }

        /// <summary>
        ///  Get the process information 
        /// </summary>
        public void retrieveProcessInfo()
        {
            //for each process in process id:
            //display 
            if(processIds.Count != 0)
            {
                foreach (var process in  processIds)
                {
                    Console.WriteLine("Process ID : " + process);
                    Console.WriteLine("Process State : ");
                    Console.WriteLine("Process File Name : "  );
                    Console.WriteLine("Process instruction pointer : " );
                    Console.WriteLine("Register Values : ");

                }
            }
        }
        
        /// kill process or remove all data of process with the id from the list of ids
        /// 
        public void removeProcessFromList(int id)
        {  //if the process contains the id
            if(processIds.Contains(id))
            {
                //remove the process id from the list
                processIds.Remove(id);
                //scape off the process data from the RAM 
            }
        }
        public void PrintRegisters()
        {
            for (int i = 0; i < registers.Length; i++)
            {
                int value = BitConverter.ToInt16(registers[i], 0);
                Console.WriteLine("Register {0} has value {1}", i, value);
            }
        }

        public void ExecuteInstruction(byte[] instruction, bool i)
        {
            int instructionType = Convert.ToInt32(instruction[0]);
            int regIndex1;
            int regIndex2;
            int regIndex3;
            int memoryAddress;
            string instructionString = Assembler.instructionTypes[instructionType];
            //Console.WriteLine("Executing " + instructionString);

            switch (instructionString)
            {
                case "LOAD":
                    regIndex1 = Convert.ToInt32(instruction[1]);;
                    memoryAddress = GetMemoryAddress(instruction, 2);
                    instructionWords = string.Format("Executing Load, destReg: {0}, memAddress: {1}", regIndex1, memoryAddress);
                    if (i)
                    {
                        Console.WriteLine(instructionWords);
                    }
                    if (regIndex1 < 6)
                    {
                        Load(regIndex1, memoryAddress);
                    }
                    else
                    {
                        Dump();
                    }
                    
                    break;
                case "LOADC":
                    regIndex1 = Convert.ToInt32(instruction[1]);
                    short constant = BitConverter.ToInt16(instruction, 2);
                    instructionWords = string.Format("Executing Load, destReg: {0}, constant: {1}", regIndex1, constant);
                    if (i)
                    {
                        Console.WriteLine(instructionWords);
                    }
                    if (regIndex1 < 6)
                    {
                        LoadC(regIndex1, constant);
                    }
                    else
                    {
                        Dump();
                    }
                    break;
                case "STORE":
                    memoryAddress = GetMemoryAddress(instruction, 1);
                    regIndex1 = Convert.ToInt32(instruction[3]);
                    instructionWords = string.Format("Executing Store, srcReg: {0}, memAddress: {1}", regIndex1,
                        memoryAddress);
                    if (i)
                    {
                        Console.WriteLine(instructionWords);
                    }
                    if (regIndex1 < 6)
                    {
                        Store(memoryAddress, regIndex1);
                    }
                    else
                    {
                        Dump();
                    }
                    break;
                case "ADD":
                    regIndex1 = Convert.ToInt32(instruction[1]);
                    regIndex2 = Convert.ToInt32(instruction[2]);
                    regIndex3 = Convert.ToInt32(instruction[3]);
                    string instructions = string.Format("Executing Add, destReg: {0}, srcReg1: {1}, srcReg2: {2}",
                        regIndex1, regIndex2, regIndex3);
                    if (i)
                    {
                        Console.WriteLine(instructions);
                    }
                    if (regIndex1 < 6 || regIndex2 < 6 || regIndex3 < 6)
                    {
                        Add(regIndex2, regIndex3, regIndex1);
                    }
                    else
                    {
                        Dump();
                    }
                    break;
                case "SUB":
                    regIndex1 = Convert.ToInt32(instruction[1]);
                    regIndex2 = Convert.ToInt32(instruction[2]);
                    regIndex3 = Convert.ToInt32(instruction[3]);
                    instructionWords = string.Format("Executing Subtract, destReg: {0}, srcReg1: {1}, srcReg2: {2}",
                        regIndex1, regIndex2, regIndex3);
                    if (i)
                    {
                        Console.WriteLine(instructionWords);
                    }
                    if (regIndex1 < 6 || regIndex2 < 6 || regIndex3 < 6)
                    {
                        Subtract(regIndex2, regIndex3, regIndex1);
                    }
                    else
                    {
                        Dump();
                    }
                    break;
                case "MUL":
                    regIndex1 = Convert.ToInt32(instruction[1]);
                    regIndex2 = Convert.ToInt32(instruction[2]);
                    regIndex3 = Convert.ToInt32(instruction[3]);
                    instructionWords = string.Format("Executing Multiply, destReg: {0}, srcReg1: {1}, srcReg2: {2}",
                        regIndex1, regIndex2, regIndex3);
                    if (i)
                    {
                        Console.WriteLine(instructionWords);
                    }
                    if (regIndex1 < 6 || regIndex2 < 6 || regIndex3 < 6)
                    {
                        Multiply(regIndex2, regIndex3, regIndex1);
                    }
                    else
                    {
                        Dump();
                    }
                    break;
                case "DIV":
                    regIndex1 = Convert.ToInt32(instruction[1]);
                    regIndex2 = Convert.ToInt32(instruction[2]);
                    regIndex3 = Convert.ToInt32(instruction[3]);
                    instructionWords = string.Format("Executing Divide, destReg: {0}, srcReg1: {1}, srcReg2: {2}",
                        regIndex1, regIndex2, regIndex3);
                    if (i)
                    {
                        Console.WriteLine(instructionWords);
                    }
                    Divide(regIndex2, regIndex3, regIndex1);
                    break;
                case "EQ":
                    //first is answer, next 2 are comparing nums
                    regIndex1 = Convert.ToInt32(instruction[1]);
                    regIndex2 = Convert.ToInt32(instruction[2]);
                    regIndex3 = Convert.ToInt32(instruction[3]);
                    instructionWords = string.Format("Executing Equals, destReg: {0}, srcReg1: {1}, srcReg2: {2}",
                        regIndex1, regIndex2, regIndex3);
                    if (i)
                    {
                        Console.WriteLine(instructionWords);
                    }
                    if (regIndex1 < 6 || regIndex2 < 6 || regIndex3 < 6)
                    {
                        Equals(regIndex2, regIndex3, regIndex1);
                    }
                    else
                    {
                        Dump();
                    }
                    break;
                case "GOTO":
                    memoryAddress = GetMemoryAddress(instruction, 1);
                    if (i)
                    {
                        Console.WriteLine("Executing GoTo, memoryAddress: {0}", memoryAddress);
                    }
                    GoTo(memoryAddress);
                    break;
                case "GOTOIF":
                    memoryAddress = GetMemoryAddress(instruction, 1);
                    int otherNum = Convert.ToInt32(instruction[1]);
                    int otherNum2 = Convert.ToInt32(instruction[2]);
                    //Console.WriteLine("Othernum: {0}, otherNum2: {1}", otherNum, otherNum2);
                    regIndex1 = Convert.ToInt32(instruction[3]);
                    instructionWords = string.Format("Executing GoToIf, memoryAddress: {0}, srcReg1: {1}", memoryAddress,
                        regIndex1);
                    if (i)
                    {
                        Console.WriteLine(instructionWords);
                    }
                    if (regIndex1 < 6)
                    {
                        GoToIf(regIndex1, memoryAddress);
                    }
                    else
                    {
                        Dump();
                    }
                    break;
                case "CPRINT":
                    memoryAddress = GetMemoryAddress(instruction, 1);
                    //Console.WriteLine("Memory Address for print: {0}", memoryAddress);
                    instructionWords = string.Format("Executing Print, memoryAddress: {0}", memoryAddress);
                    if (i)
                    {
                        Console.WriteLine(instructionWords);
                    }
                    Print(memoryAddress);
                    break;
                case "CREAD":
                    memoryAddress = BitConverter.ToInt16(instruction, 1);
                    instructionWords = string.Format("Executing Read, memoryAddress: {0}", memoryAddress);
                    if (i)
                    {
                        Console.WriteLine(instructionWords);
                    }
                    Read(memoryAddress);
                    break;
                case "EXIT":
                    if (i)
                    {
                        Console.WriteLine("Executing Exit");
                    }
                    processIds.Remove(ram.GetCurrentProcessId());
                    break;
            }
        }

        public int GetPcbStart()
        {
            return ram.GetPcbHeaderStart() + pcbMetaDataSize;
        }

        public void Load(int regIndex, int memoryAddress)
        {
            SetRegister(regIndex, ram.GetMemoryAtIndex(GetPcbStart(), memoryAddress, 2));
            //Console.WriteLine("Loaded value in memory address {0}, into reg {1}", memoryAddress, regIndex);
        }

        public void LoadC(int regIndex, short num)
        {
            SetRegister(regIndex, BitConverter.GetBytes(num));
            //Console.WriteLine("Loaded value {0}, into reg {1}", num, regIndex);
        }

        public void Store(int memoryAddress, int regIndex)
        {
            WriteToPcb(GetPcbStart(), memoryAddress, registers[regIndex]);
        }

        public void Add(int regIndexNum1, int regIndexNum2, int regIndexAnswer)
        {
            short a = BitConverter.ToInt16(registers[regIndexNum1], 0);
            short b = BitConverter.ToInt16(registers[regIndexNum2], 0);
            int c = a + b;
            short d = (short) c;
            byte[] bytes = BitConverter.GetBytes(d);
            Array.Copy(bytes, 0, registers[regIndexAnswer], 0, bytes.Length);
        }

        public void Subtract(int regIndexNum1, int regIndexNum2, int regIndexAnswer)
        {
            short a = BitConverter.ToInt16(registers[regIndexNum1], 0);
            short b = BitConverter.ToInt16(registers[regIndexNum2], 0);
            int c = a - b;
            short d = (short) c;
            byte[] bytes = BitConverter.GetBytes(d);
            Array.Copy(bytes, 0, registers[regIndexAnswer], 0, bytes.Length);
        }

        public void Multiply(int regIndexNum1, int regIndexNum2, int regIndexAnswer)
        {
            short a = BitConverter.ToInt16(registers[regIndexNum1], 0);
            short b = BitConverter.ToInt16(registers[regIndexNum2], 0);
            int c = a * b;
            short d = (short) c;
            byte[] bytes = BitConverter.GetBytes(d);
            Array.Copy(bytes, 0, registers[regIndexAnswer], 0, bytes.Length);
        }

        public void Divide(int regIndexNum1, int regIndexNum2, int regIndexAnswer)
        {
            short a = BitConverter.ToInt16(registers[regIndexNum1], 0);
            short b = BitConverter.ToInt16(registers[regIndexNum2], 0);
            int c = a / b;
            short d = (short)c;
            byte[] bytes = BitConverter.GetBytes(d);
            Array.Copy(bytes, 0, registers[regIndexAnswer], 0, bytes.Length);
        }

        public void Equals(int regIndexNum1, int regIndexNum2, int regIndexAnswer)
        {
            //Console.WriteLine("Comparing reg {0} and reg {1}. Storing answer in reg {2}", regIndexNum1, regIndexNum2, regIndexAnswer);
            short a = BitConverter.ToInt16(registers[regIndexNum1], 0);
            short b = BitConverter.ToInt16(registers[regIndexNum2], 0);
            bool c = a == b;
            byte[] bytes = BitConverter.GetBytes(c);
            Array.Copy(bytes, 0, registers[regIndexAnswer], 0, bytes.Length);
            //Console.WriteLine("Stored value {0}, from {1} is equal to {2}", c, a, b);

        }

        public void GoTo(int memoryAddress)
        {
            ram.SetInstructionPointer(memoryAddress, GetStartOfProccess(currentProcessNode.Value));
        }

        public void GoToIf(int regIndex, int memoryAddress)
        {
            //if byte at register = 0x01 then set instruction pointer to memory address
            if (BitConverter.ToBoolean(registers[regIndex], 0))
            {
                ram.SetInstructionPointer(memoryAddress, GetStartOfProccess(currentProcessNode.Value));
            }
        }

        public void Print(int memoryAddress)
        {
            //get value at memory address and print to console.
            byte[] valueBytes = ReadFromPcb(GetPcbStart(), memoryAddress);
            short value = BitConverter.ToInt16(valueBytes, 0);
            Console.WriteLine(value);
        }

        public void Read(int memoryAddress)
        {
            bool isNumeric;
            short n;
            //read input from console and set it to memory address
            do
            {
                string input = Console.ReadLine();       
                isNumeric = short.TryParse(input, out n);
            } while (!isNumeric);
            WriteToPcb(GetPcbStart(), memoryAddress, BitConverter.GetBytes(n));
        }

        public int GetMemoryAddress(byte[] instructions, int startLocation)
        {
            byte[] memBytes = new byte[2];
            Array.Copy(instructions, startLocation, memBytes, 0, 2);
            Array.Reverse(memBytes);
            int memoryAddress = BitConverter.ToInt16(memBytes, 0);
            return memoryAddress;
        }

        public void WriteToPcb(int startLocation, int index, byte[] data)
        {
            int pcbStart = GetPcbStart();
            int rsl = startLocation + index;
            if (rsl >= pcbStart && rsl + data.Length <= pcbStart + pcbSize)
            {
                ram.WriteToMemoryAtIndex(startLocation, index, data);
            }
            else
            {
                Dump(string.Format("Write to pcb failed. RSL: {0}, pcbStart: {1}, pcbSize: {2}, data Length: {3}", rsl, pcbStart, pcbSize, data.Length));
            }
            
        }

        public byte[] ReadFromPcb(int startLocation, int index)
        {
            int pcbStart = GetPcbStart();
            int rsl = startLocation + index;
            byte[] bytes = new byte[2];
            if (rsl >= pcbStart && rsl + 2 <= pcbStart + pcbSize)
            {
                bytes = ram.GetMemoryAtIndex(pcbStart, index, 2);
            }
            else
            {
                Dump(string.Format("Read from pcb failed. RSL: {0}, pcbStart: {1}, pcbSize: {2}", rsl, pcbStart, pcbSize));
            }
            return bytes;
        }

        public void Dump(string optionalInfo = null)
        {
            Console.WriteLine("Dumping...");
            string location = "error.dump";
            if (File.Exists(location))
            {
                File.Delete(location);
            }
            using (var stream = new StreamWriter(location, true))
            {

                stream.WriteLine("Registers: ");
                for (int i = 0; i < registers.Length; i++)
                {
                    int value = BitConverter.ToInt16(registers[i], 0);
                    stream.WriteLine("Register {0} has value {1}", i, value);
                }
                stream.WriteLine("Instruction: {0}", instructionWords);
                stream.WriteLine("Instruction Pointer: {0}", ram.GetInstructionPointer(GetStartOfProccess(currentProcessNode.Value)));

                if(optionalInfo != null)
                    stream.WriteLine("Optional info: {0}", optionalInfo);
            }
        }

        public void SetRegister(int index, byte[] bytes)
        {
            if (bytes.Length == 2)
            {
                registers[index] = bytes;
            }
            else
            {
                Dump();

            }
        }

    }
}
