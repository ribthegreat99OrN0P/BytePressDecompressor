using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BytePressDecompressor
{
    class Program
    {
        public static Assembly asm;
        public static Assembly lib;
        public static byte[] decompressed;
        
        static void Main(string[] args)
        {
            asm = Assembly.LoadFile(args[0]);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Loaded!");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Is the library a custom name? Yes/No");
            string choice = Console.ReadLine();
            Console.WriteLine("Decompressing..");
            unpack(choice);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Decompresed!");
            
            File.WriteAllBytes(Path.GetFileNameWithoutExtension(args[0]) + "-decompressed.exe", decompressed);
            Console.WriteLine("Saved!");
            Console.ReadKey();
        }
        private static void unpack(string choice)
        {
           if (choice == "No")
           {
                stage1();
           }
           if (choice == "Yes")
            {
                Console.Clear();
                 Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Enter resource library name: ");
                string ln = Console.ReadLine();
                stage1custom(ln);

            }
            
        }
        private static byte[] Decompress(byte[] data, int compressionType)
        {
            MethodInfo method = Program.lib.GetType("bytepress.lib.Main").GetMethod("Decompress");
            return (byte[])method.Invoke(null, new object[]
            {
                data,
                compressionType
            });
        }
        private static byte[] Decompresscustom(byte[] data, int compressionType, string libname)
        {
            MethodInfo method = Program.lib.GetType(libname).GetMethod("Decompress");
            return (byte[])method.Invoke(null, new object[]
            {
                data,
                compressionType
            });
        }
        private static void stage1()
        {
            try
            {
                using (Stream manifestResourceStream = asm.GetManifestResourceStream("bytepress.lib.dll"))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        manifestResourceStream.CopyTo(memoryStream);
                        lib = Assembly.Load(memoryStream.ToArray());
                    }
                }
                using (Stream manifestResourceStream2 = asm.GetManifestResourceStream("data"))
                {
                    using (MemoryStream memoryStream2 = new MemoryStream())
                    {
                        manifestResourceStream2.CopyTo(memoryStream2);
                        byte[] array = Decompress(memoryStream2.ToArray(), 2);
                        if (array == null || array.Length == 0)
                        {
                            throw new Exception("Failed to decompress file");
                        }
                        decompressed = array;

                    }
                }

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
            }
        }
        private static void stage1custom(string libname)
        {
            try
            {
                using (Stream manifestResourceStream = asm.GetManifestResourceStream(libname))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        manifestResourceStream.CopyTo(memoryStream);
                        lib = Assembly.Load(memoryStream.ToArray());
                    }
                }
                using (Stream manifestResourceStream2 = asm.GetManifestResourceStream("data"))
                {
                    using (MemoryStream memoryStream2 = new MemoryStream())
                    {
                        manifestResourceStream2.CopyTo(memoryStream2);
                        string ac = libname.Replace(".dll", ".Main");
                        byte[] array = Decompresscustom(memoryStream2.ToArray(), 2, ac);
                        if (array == null || array.Length == 0)
                        {
                            throw new Exception("Failed to decompress file");
                        }
                        decompressed = array;

                    }
                }

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
            }
        }
    }
}
