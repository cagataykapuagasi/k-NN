using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    class Knn
    {
        int k = 0;
        //public List<Test> testData = Data.testData;
        public List<Object> list = null;

        public Knn(List<Object> list)
        {
            this.list = list;
            init();
        }

        void init()
        {
            foreach(Object i in list)
            {
                
            }
        }
    }
}
