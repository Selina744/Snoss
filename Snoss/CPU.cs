using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Snoss
{
    class CPU
    {
        private byte[][] registers = new byte[6][];
        private int pcbSize = 500;
        private int instructionPointer;
        private int instructionStart;
        private int pcbStart;
        Ram ram = new Ram();

        private string instructionWords = null;

        private List<int> processIds;

        public CPU()
        {
            for (int i = 0; i < registers.Length; i++)
            {
                registers[i] = new byte[2];
            }
        }

        public void LoadProgram(string fileName, bool i)
        {
            //save instructions into memory
            byte[] instructionBytes = File.ReadAllBytes(fileName);
            ram.WriteToMemoryAtIndex(0, 0, instructionBytes);
            //create pcb
            int pcbMetaDataStart = instructionBytes.Length;
            //set pcb metadata size
            short pcbMetaDataSize = 8;
            ram.WriteToMemoryAtIndex(pcbMetaDataStart, 0, BitConverter.GetBytes(pcbMetaDataSize));
            //set pcb size
            ram.WriteToMemoryAtIndex(pcbMetaDataStart, 2, BitConverter.GetBytes(pcbSize));
            //set instruction start
            instructionStart = 0;
            ram.WriteToMemoryAtIndex(pcbMetaDataStart, 4, BitConverter.GetBytes(instructionStart));
            //set instruction pointer
            instructionPointer = 0;
            ram.WriteToMemoryAtIndex(pcbMetaDataStart, 6, BitConverter.GetBytes(instructionPointer));
            //other info? Where do we store the start of the pcb data?
            pcbStart = pcbMetaDataStart + pcbMetaDataSize;
            //start program at instruction counter
            RunProgram(i);
            
        }
        bool run = true;
        public void RunProgram(bool i)
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

                if (TimeToSwitch())
                {
                    SwitchProgram();
                }
                byte[] instruction = ram.GetMemoryAtIndex(instructionStart, instructionPointer, 4);
                instructionPointer += 4;
                ExecuteInstruction(instruction, i);
                
                if (i)
                {
                    Console.WriteLine("State of registers after instruction: ");
                    PrintRegisters();
                }
            }

        }

        private void SwitchProgram()
        {
            throw new NotImplementedException();
        }

        private int lastTime = 0;
        private bool TimeToSwitch()
        {
            //int newTime = DateTime.
            return true;
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
                    run = false;
                    break;
            }
        }

        public void Load(int regIndex, int memoryAddress)
        {
            SetRegister(regIndex, ram.GetMemoryAtIndex(pcbStart, memoryAddress, 2));
            //Console.WriteLine("Loaded value in memory address {0}, into reg {1}", memoryAddress, regIndex);
        }

        public void LoadC(int regIndex, short num)
        {
            SetRegister(regIndex, BitConverter.GetBytes(num));
            //Console.WriteLine("Loaded value {0}, into reg {1}", num, regIndex);
        }

        public void Store(int memoryAddress, int regIndex)
        {
            WriteToPcb(pcbStart, memoryAddress, registers[regIndex]);
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
            instructionPointer = memoryAddress;
        }

        public void GoToIf(int regIndex, int memoryAddress)
        {
            //if byte at register = 0x01 then set instruction pointer to memory address
            if (BitConverter.ToBoolean(registers[regIndex], 0))
            {
                instructionPointer = memoryAddress;
            }
        }

        public void Print(int memoryAddress)
        {
            //get value at memory address and print to console.
            byte[] valueBytes = ReadFromPcb(pcbStart, memoryAddress);
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
            WriteToPcb(pcbStart, memoryAddress, BitConverter.GetBytes(n));
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
                stream.WriteLine("Instruction Pointer: {0}", instructionPointer);

                if(optionalInfo != null)
                    stream.WriteLine("Optional info: {0}", optionalInfo);
            }
            run = false;

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
