using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    class Program
    {
        static void Main(string[] args)
        {
            Data data = new Data();
            Knn knn = new Knn(Data.list);
            

            Console.Read();
        }
    }
}

