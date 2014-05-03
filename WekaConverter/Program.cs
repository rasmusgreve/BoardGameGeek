using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataMiningIndividual;
using DataMining;
using System.IO;

namespace WekaConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            String file = args.Length > 0 ? args[0] : "data_w_right_ratings2014-05-02.csv";
            String outputfile = file.Split('.')[0] + "-weka.csv";

            Console.WriteLine("* Loading CSV-file ("+file+")...");
            String[][] rawData = CSVParser.ReadDataFile(file, ";", "null");

            Console.WriteLine("* Parsing data...");
            List<DataLine> data = DataLine.ParseFixed(rawData);

            Console.WriteLine("* Expanding arrays to boolean parameters...");
            List<DataLine> wekaData = DivideLists(data);

            Console.WriteLine("* Writing games to Weka CSV-file ("+outputfile+")...");
            WriteToFile(wekaData, outputfile);

            Console.WriteLine();
            Console.WriteLine("DONE");
            Console.ReadLine();
        }

        private static void WriteToFile(List<DataLine> wekaData, string outputfile)
        {
            char separator = ',';
            StreamWriter writer = File.CreateText(outputfile);

            // top line
            StringBuilder builder = new StringBuilder();
            wekaData[0].hashStrings.Keys.ForEach(k => builder.Append(k + separator));
            wekaData[0].hashDates.Keys.ForEach(k => builder.Append(k + separator));
            wekaData[0].hashDoubles.Keys.ForEach(k => builder.Append(k + separator));
            wekaData[0].hashBooleans.Keys.ForEach(k => builder.Append(k + separator));
            builder.Length--;
            writer.WriteLine(builder.ToString());

            // actual data
            foreach (DataLine d in wekaData)
            {
                builder = new StringBuilder();
                d.hashStrings.Keys.ForEach(k => builder.Append("\"" + (d.hashStrings[k] == null ? "null" : d.hashStrings[k].Replace(separator, '-').Replace('"', '-').Replace('\'', '-')) + "\"" + separator));
                d.hashDates.Keys.ForEach(k => builder.Append(d.hashDates[k].ToString() + separator));
                d.hashDoubles.Keys.ForEach(k => builder.Append(((double)d.hashDoubles[k]).ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + separator));
                d.hashBooleans.Keys.ForEach((k, i) => builder.Append(((bool)d.hashBooleans[k] ? "\"T\"" : "\"F\"") + separator));

                builder.Length--;

                writer.WriteLine(builder.ToString());
            }
            writer.Flush();
            writer.Close();
        }

        private static List<DataLine> DivideLists(List<DataLine> data)
        {
            List<DataLine> result = new List<DataLine>(data.Count);
            for (int i = 0; i < data.Count; i++) result.Add(new DataLine());

            // create lists for gathering values
            Dictionary<string, HashSet<string>> stringArrayValues = new Dictionary<string, HashSet<string>>();
            foreach (string key in data[0].hashStringArrays.Keys)
                stringArrayValues[key] = new HashSet<string>();

            // gather values
            for (int i = 0; i < data.Count; i++)
            {
                foreach (KeyValuePair<string,string[]> kv in data[i].hashStringArrays)
                {
                    foreach (string s in kv.Value) stringArrayValues[kv.Key].Add(s);
                }
            }

            // make all these values into boolean parameters in new datalines
            for (int i = 0; i < data.Count; i++)
            {
                DataLine oldDataLine = data[i];
                DataLine newDataLine = result[i];

                // add existing boolean parameters
                newDataLine.hashBooleans = new Dictionary<string, bool?>(oldDataLine.hashBooleans);

                // create new parameters
                foreach (KeyValuePair<string, HashSet<string>> kv in stringArrayValues)
                {
                    foreach (string value in kv.Value)
                    {
                        newDataLine.hashBooleans[kv.Key + " " + value] = oldDataLine.hashStringArrays[kv.Key].Contains(value);
                    }
                }

                // add all other parameters from original
                newDataLine.hashDates = new Dictionary<string, DateTime?>(oldDataLine.hashDates);
                newDataLine.hashDoubles = new Dictionary<string, double?>(oldDataLine.hashDoubles);
                newDataLine.hashStrings = new Dictionary<string, string>(oldDataLine.hashStrings);
            }

            return result;
        }
    }
}
