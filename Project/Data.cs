using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using net.zemberek.erisim;
using net.zemberek.tr.yapi;
using System.Text.RegularExpressions;

namespace Project
{
    class Object
    {
        public string name;
        public int count, positiveCount, negativeCount, neutralCount;
        public double Es;

        public Object(string name, int frequent)
        {
            this.name = name;
            incrementCount();
        }

        public void incrementCount()
        {
            count++;
        }

        public void incrementPositiveCount()
        {
            positiveCount++;
        }

        public void incrementNegativeCount()
        {
            negativeCount++;
        }

        public void incrementNeutralCount()
        {
            neutralCount++;
        }

        public void handleEs() //Child ın kendi Es değerini hesaplaması
        {
            double first = positiveCount * 1.0 / count, second = negativeCount * 1.0 / count,third = neutralCount * 1.0 / count, firstLog = 0, secondLog = 0, thirdLog = 0;

            if (first != 0) //0 ise logaritma alma
            {
                firstLog = Math.Log(first, 2);
            }
            if (second != 0) 
            {
                secondLog = Math.Log(second, 2);

            }
            if (third != 0)
            {
                thirdLog = Math.Log(second, 2);

            }


            Es = -first * firstLog - second * secondLog - third * thirdLog;
        }

    }

    public class Data
    {
        List<string[]> positive = new List<string[]>();
        List<string[]> negative = new List<string[]>();
        List<string[]> neutral = new List<string[]>();
        List<string> stop_words = new List<string>();
        Zemberek zemberek = new Zemberek(new TurkiyeTurkcesi());
        static public double Es; //Global E(s) değeri

        public Data()
        {
            init();
            handleStaticEs();
        }


        void init()
        {
            
            var suggestions = zemberek.kelimeCozumle("denemeler");
            if(suggestions.Length > 0)
            Console.WriteLine(suggestions[0].kok().icerik());

            Console.WriteLine(suggestions[0]);
            Console.WriteLine(suggestions[1]);
            Console.WriteLine(suggestions[2]);

            
            try //klasördeki verilerin okunması source: https://stackoverflow.com/questions/5840443/how-to-read-all-files-inside-particular-folder
            {
                stop_words.AddRange(File.ReadLines("stop-words.txt"));

                foreach (string file in Directory.EnumerateFiles("1"))
                {
                    var tokenizedStrings = tokenizer(File.ReadAllText(file).ToLower(new CultureInfo("tr-TR", false))); //tokenizer işlemi için lowercase yapıp boşluklardan itibaren bölmek.
                    var stopWordedStrings = stopWords(tokenizedStrings); //Stopwords lerle eşleşen stringleri çıkartmak
                    var stemmedStrings = stemming(stopWordedStrings);
                  
                    positive.Add(stemmedStrings); //handle olmuş datayı listeye eklemek
                }
                foreach (string file in Directory.EnumerateFiles("2"))
                {
                    var tokenizedStrings = tokenizer(File.ReadAllText(file).ToLower(new CultureInfo("tr-TR", false)));
                    var stopWordedStrings = stopWords(tokenizedStrings);
                    var stemmedStrings = stemming(stopWordedStrings);

                    negative.Add(stemmedStrings);
                }
                foreach (string file in Directory.EnumerateFiles("3"))
                {
                    var tokenizedStrings = tokenizer(File.ReadAllText(file).ToLower(new CultureInfo("tr-TR", false)));
                    var stopWordedStrings = stopWords(tokenizedStrings);
                    var stemmedStrings = stemming(stopWordedStrings);

                    neutral.Add(stemmedStrings);
                }

                
            }
            catch (Exception e)
            {
                Console.WriteLine("Hata: " + e.Message);

                Environment.Exit(0);  //hata olma durumunda işlemlerin devam etmemesi için
            }
        }


        string[] tokenizer(String value)
        {
            return value.Split(null);
        }

        string[] stopWords(String[] array)
        {
            List<string> newArray = new List<string>(array);

            foreach (string a in array)
            {
                foreach (string s in stop_words)
                {
                    if (a == s)
                    {
                        newArray.Remove(a);
                        break;
                    }
                }
            }

            return newArray.ToArray();
        }

        string[] stemming(String[] array)
        {
            List<string> newArray = new List<string>(array);

            foreach (string i in array)
            {
               
                var stems = zemberek.kelimeCozumle(i);
                if (stems.Length > 0)
                {
                    Console.Write("ilk " + i + "-");

                    newArray.Add(stems[0].kok().icerik());
                }

                if (stems.Length > 0)
                    Console.WriteLine("sonra " + stems[0].kok().icerik());
            }


            return newArray.ToArray();
        }

        void handleStaticEs()
        {
            double positiveCount = 756, negativeCount = 1287, neutralCount = 957, count = 3000;

            double firstLog = Math.Log(positiveCount /count, 2); // 2 tabanında logaritma alımı
            double secondLog = Math.Log(negativeCount / count, 2);
            double thirdLog = Math.Log(neutralCount / count, 2);

            Es = -(positiveCount / count) * firstLog -
                  (negativeCount / count) * secondLog -
                  (neutralCount / count) * thirdLog; //E(S) değerinin hesaplanması
        }
    }
}
