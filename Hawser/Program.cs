using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hawser {
    class Program {
        static void Main(string[] args) {
            if (args.Length > 0) {
                switch (args[1].Trim().ToLower()) {
                    case "-export": {
                        WAD _load = new WAD(args[0]);
                        _load.Export("strings.txt");
                        break;
                    }

                    case "-import": {
                        WAD _load = new WAD(args[0]);
                        _load.Import(args[2]);
                        break;
                    }
                }
            } else {
                Console.WriteLine("Usage:\n  Hawser.exe <WAD> -export\n  Hawser.exe <WAD> -import <STRINGS>");
            }
        }
    }
}
