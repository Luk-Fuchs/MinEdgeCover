using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Matching.Tests
{
    class Plot
    {
        public static void CreateFigure( IEnumerable<double> yValue, IEnumerable<double> xValue = null, String title ="", String xLable ="", String yLable ="", String plottype = "bar", bool show = true)
        {
            var csvDataString = "";
            csvDataString += (xValue == null ? String.Join(",", Enumerable.Range(0, yValue.Count())) : String.Join(",", yValue)) + ";";
            csvDataString += String.Join(",", yValue);
            System.IO.File.WriteAllText(@"C:\Users\LFU\Desktop\tmp\values.csv", csvDataString);

            
            var csvParameterString = "";
            csvParameterString += "title," + title + ";";
            csvParameterString += "xLabel," + xLable + ";";
            csvParameterString += "yLabel," + yLable + ";";
            csvParameterString += "show," + show + ";";
            csvParameterString += "plottype," + plottype ;
            System.IO.File.WriteAllText(@"C:\Users\LFU\Desktop\tmp\parameters.csv", csvParameterString);
            RunPythonSkript();
        }

        private static void RunPythonSkript()
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"C:\Program Files (x86)\Microsoft Visual Studio\Shared\Python37_64\python.exe";//cmd is full path to python.exe
            start.Arguments = @"C:\Users\LFU\Documents\GitHub\MinEdgeCover\PythonPlots\plot.py";//args is path to .py file and any cmd line args
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.Write(result);
                }
            }
        }
    }
}
