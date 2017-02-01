using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snoss
{
    class Ram
    {
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
    }
}
