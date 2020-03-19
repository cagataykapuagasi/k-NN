using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    public class Column
    {
        public List<Word> words = new List<Word>();
        public string className;

        public Column(string[] _words, string className)
        {
            this.className = className;

            List<string> wordsList = new List<string>();
            wordsList.AddRange(_words);

            foreach (string i in _words)
            {
                Word word = words.Find(x => x.name == i);
                if (word == null)
                {
                   word = new Word(i);
                   words.Add(word);
                } else
                {
                    word.incrementCount();
                }

            }

            //foreach(Word w in words)
            //{
            //    w.handleTfIdf();
            //}


            words.Sort((x, y) => y.count.CompareTo(x.count));
        }
    }
}
