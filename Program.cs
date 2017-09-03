using System;

namespace Gomoku{
    class Program{
        static void Main(string[] args){
            Initialize();
            Board b = new Board(15, true, false);
            b.StartLoop();
            Console.Clear();
        }

        public static void Initialize(){
            Console.Clear();
            try{
                Console.CursorSize = 100;
            } catch(System.PlatformNotSupportedException){

            }
            Console.CancelKeyPress += delegate {
                ExitGame();
            };
        }

        public static void ExitGame(){
            Console.Clear();
            Environment.Exit(0);
        }
    }
}
