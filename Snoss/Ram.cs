using System;

namespace Snoss
{
    class Ram
    {
        private int instructionPointer;
        private int instructionStart;
        private int pcbHeaderStart;
        private int currentProcessId;

        private byte[] ram = new byte[10000];

        public byte[] GetMemoryAtIndex(int processMemoryStart, int index, int size)
        {
            //Console.WriteLine("Getting data from ram location: {0}, size: {1}", processMemoryStart + index, size);
            byte[] data = null;
            data = new byte[size];
            Array.Copy(ram, processMemoryStart + index, data, 0, size);
            return data;
        }

        public void WriteToMemoryAtIndex(int processMemoryStart, int index, byte[] data)
        {
            int ramLocation = processMemoryStart + index;
            //Console.WriteLine("Storing data: array length: {0}, ram location: {1}", data.Length, ramLocation);
            for (int i = 0; i < data.Length; i++)
            {
                ram[ramLocation + i] = data[i];
            }
        }

        public void SetInstructionPointer(int instructionPointer)
        {
            this.instructionPointer = instructionPointer;
            //WriteToMemoryAtIndex(pcbMetaDataStart, 0, BitConverter.GetBytes(instructionPointer));
        }

        public int GetInstructionPointer()
        {
            return instructionPointer;
            //return Convert.ToInt32(ram.GetMemoryAtIndex(_pcbMetaDataStart, 0, 4));
        }

        public void SetInstructionStart(int instructionStart)
        {
            //WriteToMemoryAtIndex(0, 4, BitConverter.GetBytes(instructionStart));
            this.instructionStart = instructionStart;
        }

        public int GetInstructionStart()
        {
            //return GetMemoryAtIndex(0, 4, 4);
            return instructionStart;
        }

        //The start of the current running program
        public int GetPcbHeaderStart()
        {
           //return GetMemoryAtIndex(0, 0, 4);
           return pcbHeaderStart; 
        }

        public void SetPcbHeaderStart(int pcbHeaderStart)
        {
            //WriteToMemoryAtIndex(0, 0, BitConverter.GetBytes(instructionStart));
            this.pcbHeaderStart = pcbHeaderStart;
        }

        public void SetCurrentProcessId(int processId)
        {
            //WriteToMemoryAtIndex(0, 8, BitConverter.GetBytes(instructionStart));
            currentProcessId = processId;
        }

        public int GetCurrentProcessId()
        {
            //return GetMemoryAtIndex(0, 8, 4);
            return currentProcessId;
        }
    }
}
