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
    public class Data
    {
        static public List<Column> list = new List<Column>();
        List<string> stop_words = new List<string>();
        Zemberek zemberek = new Zemberek(new TurkiyeTurkcesi());
        static public double Es; //Global E(s) değeri
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

                Console.WriteLine();
                Console.Write("Feature selection handling...");
                double s = 0;
                totalWords.ForEach(x => s += x.count);
                s = s * 1.0 / totalWords.Count;
                List<AllWords> newTotalWords = new List<AllWords>();
                foreach (AllWords a in totalWords)
                {
                    a.handleThreshold();
                    if (a.count > s)
                        newTotalWords.Add(a);
                }

                totalWords = newTotalWords;
                Console.Write(" Ok.");

                Console.WriteLine();
                Console.WriteLine();
                Console.Write("Calculating tf-idf...");
                foreach (Column o in list)
                {
                    foreach (Word w in o.words)
                        w.handleTfIdf();
                }
                Console.Write(" Ok.");
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("The data processing was finished.");
                Console.WriteLine();
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
                    column = new Column(stemmedStrings, "Positive");
                }
                else if (num == "2")
                {
                    column = new Column(stemmedStrings, "Negative");
                }
                else
                {
                    column = new Column(stemmedStrings, "Neutral");
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

        void writeOutputs() //Csv dosyasının yazılması.
        {
            Console.Write("Writing csv...");
            string column = "FILE,";
            totalWords.ForEach(x => column += x.name + ",");
            column += "Sınıf";
            List<string> rows = new List<string>();
            int i = 0;
            foreach (Column c in list)
            {
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
            string path = Path.GetDirectoryName(asm.Location) + "/csv.txt"; //bin/Debug

            using (var sw = new StreamWriter(path)) 
            {
                sw.WriteLine(column);
                foreach (string r in rows)
                {
                    sw.WriteLine(r);
                }
            }
            Console.Write(" The csv.txt is ready in Project/bin/Debug.");
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

        public static int getFrequentByName(string name) //Bir kelimenin kaç kez geçtiğini bulan fonksiyon.
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

        void handleStaticEs() //genel Es değerinin hesaplanması.
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
