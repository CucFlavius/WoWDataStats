using System;

namespace WoWDataStats
{
    class Program
    {
        static void Main(string[] args)
        {
            string installPath = @"D:\Games\WoW PTR\";
            string product = "wow"; // wow (release), wowt (ptr)
            string listfilePath = @"C:\Users\cg3\Downloads\listfile (16).csv";

            Generator generator = new Generator(installPath, product, listfilePath);
        }
    }
}
