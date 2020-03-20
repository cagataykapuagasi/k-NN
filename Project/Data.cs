using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using net.zemberek.erisim;
using net.zemberek.tr.yapi;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;

namespace Project
{
    //public class Column
    //{
    //    public List<Object> list = new List<Object>();
    //    public string className;
    //    public Column()
    //    {

    //    }
    //}
    //public class Test
    //{
    //   public List<string> list = new List<string>();
    //   public string className;
    //   public string guess;

    //    public string Guess
    //    {
    //        get
    //        {
    //            return guess;
    //        }
    //        set
    //        {
    //            guess = value;
    //        }
    //    }
    //   public Test(string[] list, string className)
    //   {
    //        this.list.AddRange(list);
    //        this.className = className;
    //   }
    //}
    //public class Object
    //{
    //    public string name;
    //    public int count, positiveCount, negativeCount, neutralCount;
    //    public double MutualInfo, Es, tfIdf;

    //    public Object(string name)
    //    {
    //        this.name = name;
    //        incrementCount();
    //    }

    //    public void incrementCount()
    //    {
    //        count++;
    //    }

    //    public void incrementPositiveCount()
    //    {
    //        positiveCount++;
    //    }

    //    public void incrementNegativeCount()
    //    {
    //        negativeCount++;
    //    }

    //    public void incrementNeutralCount()
    //    {
    //        neutralCount++;
    //    }

    //    public void handleMutualInformation() //Feature selection via Mutual Information
    //    {
    //        double first = positiveCount * 1.0 / count, second = negativeCount * 1.0 / count, third = neutralCount * 1.0 / count, firstLog = 0, secondLog = 0, thirdLog = 0;
    //        int all = positiveCount + negativeCount + neutralCount;

    //        if (first != 0) //0 ise logaritma alma
    //        {
    //            firstLog = Math.Log((positiveCount * count) / (756.0 * all), 2);

    //        }
    //        if (second != 0) 
    //        {
    //            secondLog = Math.Log((negativeCount * count) / (1287.0 * all), 2);
    //        }
    //        if (third != 0)
    //        {
    //            thirdLog = Math.Log((neutralCount * count) / (957.0 * all), 2);
    //        }


    //        MutualInfo = (first * firstLog) + (second * secondLog) + (third * thirdLog);
    //    }


    //    public void handleEs()
    //    {
    //        double first = positiveCount * 1.0 / count, second = negativeCount * 1.0 / count, third = neutralCount * 1.0 / count, firstLog = 0, secondLog = 0, thirdLog = 0;

    //        if (first != 0)  //0 ise logaritma alma
    //        {
    //            firstLog = Math.Log(first, 2);
    //        }
    //        if (second != 0)
    //        {
    //            secondLog = Math.Log(second, 2);
    //        }
    //        if (third != 0)
    //        {
    //            thirdLog = Math.Log(third, 2);
    //        }

    //        Es = -first * firstLog - second * secondLog - third * thirdLog;
    //    }
    //}

    public class Data
    {
        static public List<Column> list = new List<Column>();
        //List<Object> negative = new List<Object>();
        //List<Object> neutral = new List<Object>();
        List<string> stop_words = new List<string>();
        //static public List<Test> testData = new List<Test>();
        Zemberek zemberek = new Zemberek(new TurkiyeTurkcesi());
        static public double Es; //Global E(s) değeri
        //static public int totalWordsCount;
        static public List<AllWords> totalWords = new List<AllWords>();
        
        public Data()
        {
            init();
            writeOutputs();
            handleStaticEs();
        }


        void init()
        {
            
            try //klasördeki verilerin okunması source: https://stackoverflow.com/questions/5840443/how-to-read-all-files-inside-particular-folder
            {
                Console.WriteLine("Processing...");
                stop_words.AddRange(File.ReadLines("stop-words.txt"));

                
                readAndHandle("1");
                readAndHandle("2");
                readAndHandle("3");

                double s = 0;
                totalWords.ForEach(x => s += x.count);
                s = s * 1.0 / totalWords.Count;
                List<AllWords> newTotalWords = new List<AllWords>();
                foreach (AllWords a in totalWords)
                {
                    a.handleThreshold();
                    //Console.WriteLine(a.threshold);
                    if (a.count > s)
                        newTotalWords.Add(a);
                }

                totalWords = newTotalWords;

                //double averageMI = 0.0;
                foreach (Column o in list)
                {
                    foreach (Word w in o.words)
                        w.handleTfIdf();
                    //Console.WriteLine(o.words[0].count);
                    //o.handleMutualInformation();
                    //o.handleEs();
                    //averageMI += o.MutualInfo;
                }


               
                //List<Object> newList = new List<Object>();
                //averageMI = averageMI / list.Count;

                //foreach(Object o in list)  //MI ı ortalamadan düşük olan wordler temizlendi
                //{
                //    if(o.MutualInfo > averageMI)
                //    {
                //        newList.Add(o);
                //    }
                //}

                //newList.Sort((x, y) => y.MutualInfo.CompareTo(x.MutualInfo)); //MI a göre büyükten küçüğe sıralama
                //list = newList;
                Console.WriteLine(list.Count);
                

                Console.WriteLine();
                Console.WriteLine("The data processing was finished.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Hata: " + e.Message);

                Environment.Exit(0);  //hata olma durumunda işlemlerin devam etmemesi için
            }
        }

        void readAndHandle(string num) //dosyayı okuyup gerekli işlemlerden sonra frekans sayılarını hesaplayıp listeye ekler.
        {
            Console.WriteLine();
            Console.Write("Reading folder "+num+"...");

            foreach (string file in Directory.EnumerateFiles(num))
            {
                var tokenizedStrings = tokenizer(File.ReadAllText(file).ToLower(new CultureInfo("tr-TR", false)));
                var stopWordedStrings = stopWords(tokenizedStrings);
                var stemmedStrings = stemming(stopWordedStrings);

                Column column;

                if (num == "1") //klasör isimlerine göre başka sınıf frekanslarını arttırır
                {
                    column = new Column(stemmedStrings, "1");
                }
                else if (num == "2")
                {
                    column = new Column(stemmedStrings, "1");
                }
                else
                {
                    column = new Column(stemmedStrings, "1");
                }

                list.Add(column);

                foreach (string i in stemmedStrings) //handle edilmiş dizimizi alıp frekansları hesaplar
                {
                    AllWords a = totalWords.Find(x => x.name == i);
                    if(a == null)
                    {
                        a = new AllWords(i);
                        totalWords.Add(a);
                    } else
                    {
                        a.incrementCount();
                    }
                }
            }

            Console.Write(" Ok.");
            Console.WriteLine();
        }

        void writeOutputs()
        {
            Console.WriteLine("Writing csv..." + totalWords.Count);
            //totalWords.RemoveRange(0, 11000);

            string column = "FILE,";
            totalWords.ForEach(x => column += x.name + ",");
            column += "Sınıf";
            List<string> rows = new List<string>();
            int i = 0;
            foreach (Column c in list)
            {
                //Console.WriteLine(i);

                string row = "Kayıt" + i + ",";
                foreach(AllWords x in totalWords)
                {
                    Word word = c.words.Find(y => y.name == x.name);
                    if (word == null)
                    {
                        row += 0 + ",";
                    }
                    else
                    {
                        row += word.tfIdf + ",";
                    }
                }
                row += i;
                rows.Add(row);
                i++;
            }

            Assembly asm = Assembly.GetExecutingAssembly();
            string path = Path.GetDirectoryName(asm.Location) + "/csv.txt";

            string allText = column + Environment.NewLine;
            File.WriteAllText(path, column);

            using (var sw = new StreamWriter(path))
            {
                
                    sw.WriteLine(column);
                foreach (string r in rows)
                {
                    sw.WriteLine(r);
                }
            }

            //foreach (string r in rows)
            //{
            //    allText += r + Environment.NewLine;
            //    Console.WriteLine(r);
            //}

           

            
            //File.WriteAllText(path, allText);

            Console.Write("Ok.");

        }

        string[] tokenizer(string value) //cümleyi kelimelere parçalama
        {
            return value.Split(null);
        }

        string[] stopWords(string[] array) //dosyadaki stopwordlerde geçen elemanın silinmesi
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

        string[] stemming(string[] array) //zemberek kullanılarak kök haline getirme
        {
            List<string> newArray = new List<string>(array);

            foreach (string i in array)
            {
               
                var stems = zemberek.kelimeCozumle(i);
                if (stems.Length > 0)
                {
                    newArray.Add(stems[0].kok().icerik());
                }
             
            }


            return newArray.ToArray();
        }

        public static int getFrequentByName(string name)
        {
            int count = 0;
            foreach(Column c in list)
            {
                Word w = c.words.Find(x => x.name == name);
                if (w != null)
                    count++;
            }

            return count;
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
