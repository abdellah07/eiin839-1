using System;

namespace ExeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                string value = "";
                foreach (string arg in args)
                {
                    value += arg + " ";
                }
                Console.WriteLine("<HTML><BODY> Appel d'un executable extérieur avec les paramétres suivante : " + value + "</BODY></HTML>");
            }
            else
                Console.WriteLine("Aucun param a afficher");
        }
    }
}
