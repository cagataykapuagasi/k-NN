using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    class Results //result class
    {
        public int Tp, Fp, Fn;

        public void incrementTp()
        {
            Tp++;
        }
        public void incrementFp()
        {
            Fp++;
        }
        public void incrementFn()
        {
            Fn++;
        }
    }
    class Knn
    {
        int k = 1;
        int currentBeginIndex = 0, part = 300; //stratified 10-folds cross-validation için parça aralıkları. Başlangıç parça 1.

        public List<Column> list = null;
        public List<Column> testData = null;
        public List<Results> positiveResults = new List<Results>(); //sonuç çıktılarının biriktirildiği listeler.
        public List<Results> negativeResults = new List<Results>();
        public List<Results> neutralResults = new List<Results>();

        public Knn(List<Column> list)
        {
            this.list = list;
            init();
        }

        void init()
        {
            Console.WriteLine();
            for (int i = 0; i<10; i++) //knn algoritmasını dinamik olarak test ve train verilerini seçip değiştirerek çalıştırır
            { //http://bilgisayarkavramlari.sadievrenseker.com/2013/03/31/k-fold-cross-validation-k-katlamali-carpraz-dogrulama/
                Console.WriteLine();
                Console.Write("cross-validation: " + i);

                List<Column> test = list.GetRange(currentBeginIndex, 300);
                List<Column> train = new List<Column>(list);
                train.RemoveRange(currentBeginIndex, 300);
                knn(train, test);
                currentBeginIndex += 300;
                Console.Write(" Ok.");
            }

            performanceMesaure();
        }

        void knn(List<Column> train, List<Column> test) //knn
        {
            Results positiveResult = new Results();
            Results negativeResult = new Results();
            Results neutralResult = new Results();

            foreach (Column t in test)
            {
                if (t.words.Count > 0)
                {
                    Word Testw = t.words.First(); //k = 0
                    List<Word> trainWords = new List<Word>();
                    foreach (Column c in train)
                    {
                        Word w = c.words.Find(x => x.name == Testw.name);
                        if (w != null)
                        {
                            trainWords.Add(w);
                        }
                    }

                    if(trainWords.Count > 0)
                    {
                        trainWords.Sort((x, y) => y.tfIdf.CompareTo(x.tfIdf));
                        t.guessClassName = trainWords.First().className;
                    }
                  
                }

                if (t.className == "Positive")
                {
                    if (t.guessClassName == t.className)
                        positiveResult.incrementTp();
                    else
                        negativeResult.incrementFp();
                }
                else if (t.className == "Negative")
                {
                    if (t.guessClassName == t.className)
                        negativeResult.incrementFp();
                    else
                        negativeResult.incrementFn();
                }
                else
                {
                    if (t.guessClassName == t.className)
                        neutralResult.incrementFp();
                    else
                        neutralResult.incrementFn();
                }
            }

            positiveResults.Add(positiveResult);
            negativeResults.Add(negativeResult);
            neutralResults.Add(neutralResult);
        }


        void performanceMesaure() //sonuç ekranı çıktısı. 10 adet knn çıktısının ortalamaları alınıp çıktı olarak verilir.
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.Write("Writing result...");

            int class1Tp = 0, class1Fp = 0, class1Fn = 0, class2Tp = 0, class2Fp = 0, class2Fn = 0, class3Tp = 0, class3Fp = 0, class3Fn = 0;
            double class1Precision, class1Recall, class1FScore, class2Precision, class2Recall, class2FScore, class3Precision, class3Recall, class3FScore;

            foreach(Results r in positiveResults)
            {
                class1Tp += r.Tp;
                class1Fp += r.Fp;
                class1Fn += r.Fn;
            }
            class1Tp /= 10;
            class1Fp /= 10;
            class1Fn /= 10;
            class1Precision = class1Tp / (class1Tp + class1Fp);
            class1Recall = class1Tp / (class1Tp + class1Fn);
            class1FScore = 2 * (class1Precision * class1Recall) / (class1Precision + class1Recall);
            foreach (Results r in negativeResults)
            {
                class2Tp += r.Tp;
                class2Fp += r.Fp;
                class2Fn += r.Fn;
            }
            class2Tp /= 10;
            class2Fp /= 10;
            class2Fn /= 10;
            class2Precision = class2Tp / (class2Tp + class2Fp);
            class2Recall = class2Tp / (class2Tp + class2Fn);
            class2FScore = 2 * (class2Precision * class2Recall) / (class2Precision + class2Recall);
            foreach (Results r in neutralResults)
            {
                class3Tp += r.Tp;
                class3Fp += r.Fp;
                class3Fn += r.Fn;
            }
            class3Tp /= 10;
            class3Fp /= 10;
            class3Fn /= 10;
            class3Precision = class3Tp / (class3Tp + class3Fp);
            class3Recall = class3Tp / (class3Tp + class3Fn);
            class3FScore = 2 * (class3Precision * class3Recall) / (class3Precision + class3Recall);

            string row1 = "Sınıf1 " + "Sınıf2 " + "Sınıf3 " + "Macro Average " +"Micro Average",
                row2 = "Precision "+ class1Precision + " " + class2Precision + " " + class3Precision,
                row3 = "Recall " + class1Recall + " " + class2Recall + " " + class3Recall,
                row4 = "F-Score "+ class1FScore + " " + class2FScore + " " + class3FScore,
                row5 = "Tp Adedi " + class1Tp + " " + class2Tp + " " + class3Tp,
                row6 = "Fp Adedi "+ class1Fp + " " + class2Fp + " " + class3Fp,
                row7 = "Fn Adedi "+ class1Fn + " " + class2Fn + " " + class3Fn;

            Assembly asm = Assembly.GetExecutingAssembly();
            string path = Path.GetDirectoryName(asm.Location) + "/result.txt";

            using (var sw = new StreamWriter(path))
            {
                sw.WriteLine(row1);
                sw.WriteLine(row2);
                sw.WriteLine(row3);
                sw.WriteLine(row4);
                sw.WriteLine(row5);
                sw.WriteLine(row6);
                sw.WriteLine(row7);
            }

            Console.Write(" The result.txt is ready in Project/bin/Debug.");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("The process was finished.");
        }
    }
}
