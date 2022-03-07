using System;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Collections.Generic;

namespace Spider
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> AllFoundStrings = new List<string>();
            ModuleDefMD module = ModuleDefMD.Load(args[0]);
            foreach (TypeDef type in module.Types)
            {
                foreach (MethodDef method in type.Methods)
                {
                    if (method.Body == null)
                    {
                        continue;
                    }
                    else
                    {
                        for (int i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr)
                                AllFoundStrings.Add(method.Body.Instructions[i].Operand.ToString());
                        }
                    }
                }
            }
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"Spider found {AllFoundStrings.Count} strings.\n");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Option 1 > Replace String");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Option 2 > Write Strings To File");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Option 3 > Write to Stdout");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[?] : ");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            int option = Convert.ToInt32(Console.ReadLine());

            switch (option)
            {
                case 1:
                    Dictionary<int, string> keyValuePairs = new Dictionary<int, string>();
                    for (int i = 0; i < AllFoundStrings.Count; i++) keyValuePairs.Add(i, AllFoundStrings[i]);
                    ReplaceString(keyValuePairs, module, args[0]);
                    break;
                case 2:
                    WriteToFile(AllFoundStrings, module.Name);
                    break;
                case 3:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("[+] Writing all strings to stdout...");
                    for (int i = 0; i < AllFoundStrings.Count; i++)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write($"String {i}: ");
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"{AllFoundStrings.ElementAt(i)}");
                    }
                    break;
                default:
                    Main(args);
                    break;
            }
        }

        public static void ReplaceString(Dictionary<int, string> StringPairs, ModuleDefMD2 module, string filename)
        {
            Console.Clear();
            foreach (KeyValuePair<int, string> parsedString in StringPairs)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"String ID -> [{parsedString.Key}]: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($" { parsedString.Value}");
            }
            Console.Write("\n\nSelect a string via the String ID: ");
            Console.ForegroundColor = ConsoleColor.White;
            int option = Convert.ToInt32(Console.ReadLine());
            if (StringPairs.ContainsKey(option))
            {
                Console.Clear();
                Console.WriteLine($"String Selected -> {StringPairs[option]}\n");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("[?] Replacement String: ");
                string replaceString = Console.ReadLine();
                foreach (TypeDef type in module.Types)
                {
                    foreach (MethodDef method in type.Methods)
                    {
                        if (method.Body == null)
                        {
                            continue;
                        }

                        else
                        {
                            for (int i = 0; i < method.Body.Instructions.Count; i++)
                            {
                                if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr)
                                {
                                    if (method.Body.Instructions[i].Operand.ToString() == StringPairs[option])
                                    {
                                        method.Body.Instructions.RemoveAt(i);
                                        method.Body.Instructions.Insert(i, new Instruction(OpCodes.Ldstr, replaceString));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                module.Write($"rewritten_{filename}");
                Console.WriteLine("Replaced string successfully :)");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("Would you like to replace another string?");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("[1 for Yes / 2 for No] : ");
                int editOption = Convert.ToInt32(Console.ReadLine());
                switch (editOption)
                {
                    case 1:
                        StringPairs.ElementAt(option).Value.Replace(StringPairs.ElementAt(option).Value, replaceString);
                        ReplaceString(StringPairs, module, filename);
                        break;
                    case 2:
                        return;
                    default:
                        return;
                }
            }
            else
            {
                return;
            }
        }

        public static void WriteToFile(List<string> Strings, string module_name)
        {
            System.IO.FileStream fileStream = System.IO.File.Create($"{module_name}_strings.txt");
            foreach (string parsedString in Strings)
            {
                byte[] parsedByte = System.Text.Encoding.UTF8.GetBytes($"{parsedString}\n");
                fileStream.Write(parsedByte, 0, parsedByte.Length);
            }
            fileStream.Close();
            Console.WriteLine($"Written all strings to {module_name}_strings.txt!");
            Console.ReadKey();
        }
    }
}
