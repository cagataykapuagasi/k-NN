using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    public class Word //Text dosyalarının içindeki kelime hali.
    {
        public string name, className;
        public int count;
        public double tfIdf;

        public Word(string name, string className)
        {
            this.name = name;
            this.className = className;
            incrementCount();
        }

        public void incrementCount()
        {
            count++;
        }

        public void handleTfIdf() //if-idf hesabı
        {
            //http://bilgisayarkavramlari.sadievrenseker.com/2012/10/22/tf-idf/

            double tf = count * 1.0 / Data.totalWords.Count;
            double idf = Math.Log(3000 / Data.getFrequentByName(name), 10);

            tfIdf = tf * idf;
        }
    }
}
