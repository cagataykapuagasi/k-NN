using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    public class AllWords
    {
        public string name;
        public int count;
        public double threshold;

        public AllWords(string name)
        {
            this.name = name;
            incrementCount();
        }

        public void incrementCount()
        {
            count++;
        }

        public void handleThreshold()
        {
            threshold = count * 1.0 / 3000;
        }
    }
}
