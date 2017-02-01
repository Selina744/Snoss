using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snoss
{
    class Assembler
    {
        public static string[] instructionTypes = { "LOAD", "LOADC", "STORE", "ADD", "SUB", "MUL", "DIV", "EQ", "GOTO", "GOTOIF", "CPRINT", "CREAD", "EXIT" };

        //returns saved location
        public static string TranslateFile(string fileName)
        {
            string location = fileName + ".sno";
            if (File.Exists(location))
            {
                File.Delete(location);
            }
            System.IO.StreamReader file =
               new System.IO.StreamReader(fileName);
            using (var stream = new FileStream(location, FileMode.Append))
            {
                
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    Console.WriteLine("Translating line: " + line);
                    byte[] translatedBytes = TranslateSingleCommand(line);

                    stream.Write(translatedBytes, 0, translatedBytes.Length);

                }
            }
            file.Close();
            return location;
        }

        public static byte[] TranslateSingleCommand(string command)
        {
            byte[] bytes = new byte[4];
            string[] sections = command.Split(' ');
            int instruction = GetIntFromInstruction(sections[0]);
            bytes[0] = Convert.ToByte(instruction);
            //translate command into bytes
            switch (instruction)
            {
                case 0:
                case 1:
                    //set first to byte 2 and second to bytes 3 and 4
                    string register = sections[1].Trim(',');
                    int regIndex = GetRegister(register);
                    bytes[1] = Convert.ToByte(regIndex);

                    byte[] numBytes = instruction == 0 ? StringToByteArray(sections[2]) : BitConverter.GetBytes(int.Parse(sections[2]));
                    bytes[2] = numBytes[0];
                    bytes[3] = numBytes.Length > 1 ? numBytes[1] : Convert.ToByte(0);
                    break;
                case 2:
                case 8:
                case 9:
                case 10:
                    //set first to byte 2 and 3 and second to byte 4
                    byte[] memoryAddress = StringToByteArray(sections[1].Trim(','));
                    bytes[1] = memoryAddress[0];
                    bytes[2] = memoryAddress.Length > 1 ? memoryAddress[1] : Convert.ToByte(0);
                    bytes[3] = sections.Length > 2 ? Convert.ToByte(GetRegister(sections[2])) : Convert.ToByte(0);
                    break;
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    //set first to byte 2, second to byte 3, third to byte 4
                    bytes[1] = Convert.ToByte(GetRegister(sections[1].Trim(',')));
                    bytes[2] = Convert.ToByte(GetRegister(sections[2].Trim(',')));
                    bytes[3] = Convert.ToByte(GetRegister(sections[3]));
                    break;
            }

            return bytes;
        }

        public static int GetRegister(string reg)
        {
            return int.Parse(reg[1].ToString());
        }

        public static byte[] StringToByteArray(string hex)
        {
            byte[] hexBytes = null;
            hex = hex.Split('x')[1];
            try
            {
                hexBytes = Enumerable.Range(0, hex.Length)
                    .Where(x => x % 2 == 0)
                    .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                    .ToArray();
            }
            catch (FormatException e)
            {
                Console.WriteLine("Unable to convert hex number: " + hex);
            }
            return hexBytes;
        }

        public static int GetIntFromInstruction(string instruction)
        {
            int index = Array.IndexOf(instructionTypes, instruction);
            
            return index;
        }
    }
}
