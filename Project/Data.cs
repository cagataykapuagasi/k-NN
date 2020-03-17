﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

namespace Project
{
    public class Data
    {
        List<string[]> positive = new List<string[]>();
        List<string[]> negative = new List<string[]>();
        List<string[]> neutral = new List<string[]>();
        List<string> stop_words = new List<string>();

        public Data()
        {
            init();
        }


        void init()
        {

            try //klasördeki verilerin okunması source: https://stackoverflow.com/questions/5840443/how-to-read-all-files-inside-particular-folder
            {
                stop_words.AddRange(File.ReadLines("stop-words.txt"));

                foreach (string file in Directory.EnumerateFiles("1"))
                {
                    var tokenizedStrings = tokenizer(File.ReadAllText(file).ToLower(new CultureInfo("tr-TR", false)));
                    var stopWordedStrings = stopWords(tokenizedStrings);
                    positive.Add(stopWordedStrings);
                    
                }
                foreach (string file in Directory.EnumerateFiles("2"))
                {
                    var tokenizedStrings = tokenizer(File.ReadAllText(file).ToLower(new CultureInfo("tr-TR", false)));
                    var stopWordedStrings = stopWords(tokenizedStrings);
                    negative.Add(stopWordedStrings);
                }
                foreach (string file in Directory.EnumerateFiles("3"))
                {
                    var tokenizedStrings = tokenizer(File.ReadAllText(file).ToLower(new CultureInfo("tr-TR", false)));
                    var stopWordedStrings = stopWords(tokenizedStrings);
                    neutral.Add(stopWordedStrings);
                }

                foreach (String i in positive[5])
                    Console.WriteLine(i);
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
                foreach(string s in stop_words)
                {
                    if(a == s)
                    {
                        newArray.Remove(a);
                        break;
                    }
                }
            }

            return newArray.ToArray();
        }
    }
}