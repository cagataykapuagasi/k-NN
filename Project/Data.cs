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
    public class Object
    {
        public string name;
        public int count, positiveCount, negativeCount, neutralCount;
        public double MutualInfo;

        public Object(string name)
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

        public void handleMutualInformation() //Feature selection via Mutual Information
        {
            double first = positiveCount * 1.0 / count, second = negativeCount * 1.0 / count, third = neutralCount * 1.0 / count, firstLog = 0, secondLog = 0, thirdLog = 0;
            int all = positiveCount + negativeCount + neutralCount;

            if (first != 0) //0 ise logaritma alma
            {
                firstLog = Math.Log((positiveCount * count) / (756.0 * all), 2);

            }
            if (second != 0) 
            {
                secondLog = Math.Log((negativeCount * count) / (1287.0 * all), 2);
            }
            if (third != 0)
            {
                thirdLog = Math.Log((neutralCount * count) / (957.0 * all), 2);
            }


            MutualInfo = (first * firstLog) + (second * secondLog) + (third * thirdLog);
        }
    }

    public class Data
    {
        public List<Object> list = new List<Object>();
        //List<Object> negative = new List<Object>();
        //List<Object> neutral = new List<Object>();
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
            
            try //klasördeki verilerin okunması source: https://stackoverflow.com/questions/5840443/how-to-read-all-files-inside-particular-folder
            {
                Console.WriteLine("Processing...");
                stop_words.AddRange(File.ReadLines("stop-words.txt"));

                
                readAndHandle("1");
                readAndHandle("2");
                readAndHandle("3");

                foreach (Object o in list)
                {
                    o.handleMutualInformation();
                    //Console.WriteLine(o.name + "-" + o.MutualInfo);
                }

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
            Console.WriteLine("Reading folder "+num+"...");

            foreach (string file in Directory.EnumerateFiles(num))
            {
                var tokenizedStrings = tokenizer(File.ReadAllText(file).ToLower(new CultureInfo("tr-TR", false)));
                var stopWordedStrings = stopWords(tokenizedStrings);
                var stemmedStrings = stemming(stopWordedStrings);

                foreach (string i in stemmedStrings) //handle edilmiş dizimizi alıp frekansları hesaplar
                {
                    Object obj = list.Find(x => x.name == i);
                    bool isNew =  false;

                    if (obj != null)
                    {
                        obj.incrementCount();
                    }
                    else
                    {
                        obj = new Object(i);
                        isNew = true;
                    }

                    if(num == "1") //klasör isimlerine göre başka sınıf frekanslarını arttırır
                    {
                        obj.incrementPositiveCount();
                    } else if(num == "2")
                    {
                        obj.incrementNegativeCount();
                    } else
                    {
                        obj.incrementNeutralCount();
                    }

                    if(isNew)
                    {
                        list.Add(obj);
                    }

                }
            }

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
