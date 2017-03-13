// Copyright SCIEX 2017. All rights reserved.

namespace TeamCityToConfluence
{
    class Program
    {
        static void Main(string[] args)
        {
            var runner = new Runner(args);
            runner.Run();
        }
    }
}