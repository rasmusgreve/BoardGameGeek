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
            String[][] rawData = CSVParser.ReadDataFile(file, ";", "?");

            Console.WriteLine("* Parsing data...");
            List<DataLine> data = DataLine.ParseFixed(rawData);

            Console.WriteLine("* Discretize numeric values");
            DiscretizeValues(data);

            Console.WriteLine("* Adding extra parameters");
            AddSpielNominee(data);

            Console.WriteLine("* Expanding arrays to boolean parameters...");
            List<DataLine> wekaData = DivideLists(data);

            Console.WriteLine("* Writing games to Weka CSV-file ("+outputfile+")...");
            WriteToFile(wekaData, outputfile);

            Console.WriteLine();
            Console.WriteLine("DONE");
            Console.ReadLine();
        }

        private static void AddSpielNominee(List<DataLine> data)
        {
            List<DataLine> nominees = DataLine.ParseInferred(CSVParser.ReadDataFile("spiel_des_jahres.csv", ";", null));

            foreach(DataLine game in data){
                bool isNom = nominees.Any(n => n.hashDoubles["game_id"].Equals(game.hashDoubles["id"]));
                game.hashBooleans["spiel_nominee"] = isNom;
            }
        }

        private static void DiscretizeValues(List<DataLine> data)
        {
            EqualDepthBin(data, "average_rating", 5);
            EqualDepthBin(data, "year_published", 10);
            EqualDepthBin(data, "min_players", 3);
            EqualDepthBin(data, "max_players", 5);
            EqualDepthBin(data, "playingtime", 7);
        }

        private static void EqualDepthBin(List<DataLine> data, string doubleLabel, int bins)
        {
            Console.WriteLine("\tBinning \"{0}\":",doubleLabel);

            var missing = data.Where(g => g.hashDoubles[doubleLabel] == 0.0);
            DataLine[] sorted = data.Where(g => g.hashDoubles[doubleLabel] != 0.0).OrderBy(g => g.hashDoubles[doubleLabel]).ToArray();

            // binning of games with real values
            int printedBin = 0;
            for (int i = 0; i < sorted.Length; i++)
            {
                int bin = 1 + (int)(i / (sorted.Length / (double)bins)); // 1 - bins

                if (printedBin < bin)
                {
                    if (printedBin > 0) Console.WriteLine(" to {0}", sorted[i - 1].hashDoubles[doubleLabel].ToString());
                    printedBin++;
                    Console.Write("\t\tBin {0}: {1}",bin,sorted[i].hashDoubles[doubleLabel].ToString());
                }

                sorted[i].hashStrings["eqdep(" + doubleLabel + ")"] = "("+bin+")";
            }
            Console.WriteLine(" to {0}", sorted[sorted.Length-1].hashDoubles[doubleLabel].ToString());

            // missing values
            foreach (DataLine d in missing)
            {
                d.hashStrings["eqdep(" + doubleLabel + ")"] = "?";
            }
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
