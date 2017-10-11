using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BBTItemCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("              ____              ____   ____          ____   ____   o       ____  ___");
            Console.WriteLine("   /\\      / |     \\  / |    | |      |    | |   |  |    | |    )  | | /  |     |   |");
            Console.WriteLine("  /  \\    /  |____  \\/  |    | |____  |____| |___|  |    | |____)  | |/   |____ |___|");
            Console.WriteLine(" /    \\  /   |      /\\  |    |      | |      |   |  |    | |     ] | |\\   |     |\\");
            Console.WriteLine("/      \\/    |____ /  \\ |____|  ____| |      |   |  |____| |_____] | | \\  |____ | \\");
            //Console.ReadKey();

            Process process = Process.GetProcessesByName("BattleBlockTheater").First();
            IntPtr hProcess = WINAPI.OpenProcess(WINAPI.ProcessAccessFlags.All, true, process.Id);
            if(hProcess == null)
            {
                Console.WriteLine("Attaching failed.");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("Attached");

            IntPtr caveAddr = WINAPI.VirtualAllocEx(hProcess, IntPtr.Zero, 2048, WINAPI.AllocationType.Commit | WINAPI.AllocationType.Reserve, WINAPI.MemoryProtection.ExecuteReadWrite);
            Console.WriteLine("Address:" + caveAddr.ToString("X"));
            byte[] asm = new byte[] { 0x8B, 0x1D, 0xC4, 0xE7, 0x65, 0x01, 0x8B, 0x8B, 0x50, 0x0D, 0x00, 0x00, 0x8B, 0x01, 0xBA, 0x65, 0x00, 0x00, 0x00, 0x52, 0xBA, 0x36, 0x00, 0x52, 0x01, 0x52, 0x68, 0x01, 0x00, 0xFF, 0x7F, 0xFF, 0x10, 0xC3 };
            byte[] jsonDataPre = new byte[] { 0x7B, 0x0A, 0x09, 0x22, 0x72, 0x65, 0x71, 0x75, 0x65, 0x73, 0x74, 0x69, 0x64, 0x22, 0x3A, 0x35, 0x2C, 0x0A, 0x09, 0x22, 0x66, 0x69, 0x65, 0x6C, 0x64, 0x73, 0x22, 0x3A, 0x5B, 0x22, 0x69, 0x74, 0x65, 0x6D, 0x69, 0x64, 0x22, 0x2C, 0x22, 0x64, 0x65, 0x66, 0x69, 0x6E, 0x64, 0x65, 0x78, 0x22, 0x2C, 0x22, 0x71, 0x75, 0x61, 0x6E, 0x74, 0x69, 0x74, 0x79, 0x22, 0x5D, 0x2C, 0x0A, 0x09, 0x22, 0x67, 0x65, 0x6E, 0x65, 0x72, 0x61, 0x74, 0x65, 0x22, 0x3A, 0x5B };
            byte[] jsonDataSuf = new byte[] { 0x5D,0x0A, 0x7D };

            //Set object address
            Int32 objectAddress = process.MainModule.BaseAddress.ToInt32() + 0x30E7C4;
            Array.Copy(BitConverter.GetBytes(objectAddress), 0, asm, 2, 4);

            //Set buff address
            Int32 buffAddress = caveAddr.ToInt32() + asm.Length;
            Array.Copy(BitConverter.GetBytes(buffAddress), 0, asm, 0x15, 4);

            //inject 
            int bytesWritten = 0;
            WINAPI.WriteProcessMemory(hProcess, caveAddr, asm, asm.Length, ref bytesWritten);

            //Loop
            while (true)
            {
                Console.WriteLine("Input a number between 50000 and 51000 (50000 jewels | 50001 yarn | 50002 - 50321 masks | 50322 - ? weapons )");
                Console.InputEncoding = Encoding.ASCII;
                string input = Console.ReadLine();
                int tempInput = 0;
                if (int.TryParse(input, out tempInput))
                {
                    if (tempInput >= 50000 && tempInput <= 51000)
                    {
                        byte[] inputData = Encoding.ASCII.GetBytes(input);
                        //this could probably be increased by increasing the allocated memory but i didnt look at the limits of the steam function
                        Console.WriteLine("Input the amount you want to get (maximum " + (2048 - asm.Length - jsonDataPre.Length - jsonDataSuf.Length) / 6 + " at once)");
                        int inputAmount = 0;
                        if (int.TryParse(Console.ReadLine(), out inputAmount))
                        {
                            byte[] jsonDataNum = new byte[inputAmount * 6 - 1];
                            int i = 0;
                            while (i < jsonDataNum.Length)
                            {
                                if (jsonDataNum.Length > i + 6)
                                {
                                    jsonDataNum[i + 5] = 0x2C;
                                }
                                Array.Copy(inputData, 0, jsonDataNum, i, 5);
                                i += 6;
                            }
                            //Pre
                            WINAPI.WriteProcessMemory(hProcess, IntPtr.Add(caveAddr, asm.Length), jsonDataPre, jsonDataPre.Length, ref bytesWritten);
                            //Data
                            WINAPI.WriteProcessMemory(hProcess, IntPtr.Add(caveAddr, asm.Length + jsonDataPre.Length), jsonDataNum, jsonDataNum.Length, ref bytesWritten);
                            //Suf
                            WINAPI.WriteProcessMemory(hProcess, IntPtr.Add(caveAddr, asm.Length + jsonDataPre.Length + jsonDataNum.Length), jsonDataSuf, jsonDataSuf.Length, ref bytesWritten);

                            //Set buff size
                            Int32 buffSize = jsonDataNum.Length + jsonDataPre.Length + jsonDataSuf.Length;
                            WINAPI.WriteProcessMemory(hProcess, IntPtr.Add(caveAddr, 0x0F), BitConverter.GetBytes(buffSize), BitConverter.GetBytes(buffSize).Length, ref bytesWritten);

                            Console.WriteLine("Executing...");
                            //Console.ReadKey();
                            IntPtr threadID = IntPtr.Zero;
                            IntPtr threadHandle = WINAPI.CreateRemoteThread(hProcess, IntPtr.Zero, 0, caveAddr, IntPtr.Zero, 0, out threadID);
                            Console.WriteLine("Executed " + threadID);
                            IntPtr exitCode = (IntPtr)777;
                            WINAPI.GetExitCodeThread(threadHandle, out exitCode);
                            //still running... should probably add a timer to prevent infinite loops as im not sure if the function uses 259 as an error case
                            while (exitCode.ToInt32() == 259 && WINAPI.GetExitCodeThread(threadHandle, out exitCode))
                            {
                                System.Threading.Thread.Sleep(100);
                            }
                            if (exitCode.ToInt32() == 0)
                            {
                                Console.WriteLine("Successful");
                            }
                            else
                            {
                                Console.WriteLine("Unknown return " + exitCode.ToInt32());
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Number not in range " + tempInput);
                    }
                }
                else
                {
                    Console.WriteLine("Failed parsing " + input);
                }
            }
        }
    }

    public class WINAPI
    {
        //Win
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetExitCodeThread(IntPtr hThread, out IntPtr exitCode);

        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess,  IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,  uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);
    }
}
