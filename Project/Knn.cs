using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    class Results
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
        public List<Results> positiveResults = new List<Results>();
        public List<Results> negativeResults = new List<Results>();
        public List<Results> neutralResults = new List<Results>();

        public Knn(List<Column> list)
        {
            this.list = list;
            init();
        }

        void init()
        {
            for(int i = 0; i<10; i++)
            {
                List<Column> test = list.GetRange(currentBeginIndex, 300);
                List<Column> train = new List<Column>(list);
                train.RemoveRange(currentBeginIndex, 300);
                knn(train, test);
                currentBeginIndex += 300;
            }
            performanceMesaure();
        }

        void knn(List<Column> train, List<Column> test)
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
                //Console.WriteLine(t.guessClassName+ " Doğrusu:" + t.className);
            }

            positiveResults.Add(positiveResult);
            negativeResults.Add(negativeResult);
            neutralResults.Add(neutralResult);
        }


        void performanceMesaure()
        {
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
                row2 = "Precision",
                row3 = "Recall",
                row4 = "F-Score",
                row5 = "Tp Adedi",
                row6 = "Fp Adedi",
                row7 = "Fn Adedi";
        }
    }
}
