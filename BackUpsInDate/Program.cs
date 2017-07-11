using System;

// OOPS - DON'T WANT .NET CORE FOR THIS AS GETS COMPLICATED FOR EXE
// AND DON'T WANT TO USE DLL VERSION YET ... JUST SIMPLE EXE VERSION
// - SO MAKING .NET FRAMEWORK VERSION INSTEAD!

namespace BackUpsInDate
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine($"0:{args[0]}");

            return 0;
        }
    }
}
