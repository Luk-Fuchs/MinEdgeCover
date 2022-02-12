using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Matching
{
    public static class Extensions
    {
        public static void Shuffle<T>(this List<T> list)
        {
            int n = list.Count;
            var random = new Random();
            for (int i = 0; i < list.Count; i++)
            {
                int k = random.Next(i, n);
                T value = list[k];
                list[k] = list[i];
                list[i] = value;
            }
        }
    }
}
