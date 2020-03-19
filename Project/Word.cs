using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    public class Word
    {
        public string name;
        public int count;
        public double tfIdf;

        public Word(string name)
        {
            this.name = name;
            incrementCount();
        }

        public void incrementCount()
        {
            count++;
        }

        public void handleTfIdf()
        {
            double tf = count * 1.0 / Data.totalWords.Count;
            double idf = Math.Log(3000 / Data.getFrequentByName(name), 10);

            tfIdf = tf * idf;

            //Console.WriteLine(@"tf: {0} idf: {1} tfidf: {2}",tf,idf, tfIdf);
        }
    }
}
