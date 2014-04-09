using System;
using System.Collections.Generic;
using System.IO;

namespace DataMiningIndividual
{
    class CSVParser
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>A dictionary from link type(string) to (dictionary of id (int) to name (string))</returns>
        public static Dictionary<string, Dictionary<int, string>> ReadLinkFile(string fileName)
        {
            var result = new Dictionary<string, Dictionary<int, string>>();
            Dictionary<int, string> curDict = null;
            string curHead = null;

            StreamReader bufRdr = File.OpenText(fileName);
            string line = null;
            
            while ((line = bufRdr.ReadLine()) != null)
            {
                if (line.Trim().Equals("")) continue;
                string[] arr = line.Split(new char[] { '=' }, StringSplitOptions.None);
                if (arr.Length == 1) //Read heading
                {
                    if (curDict != null) result.Add(curHead,curDict);
                    curDict = new Dictionary<int, string>();
                    curHead = line.Trim();
                    continue;
                }
                var key = int.Parse(arr[0].Trim());
                var val = arr[1].Trim();
                curDict.Add(key, val);
            }
            bufRdr.Close();
            return result;
        }

        	    /**
	     * The read method reads in a csv file as a two dimensional string array.
	     * This method is utilizes the string.split method for splitting each line of the data file.
	     * @param csvFile File to load
	     * @param seperationChar Character used to seperate entries
	     * @param nullValue What to insert in case of missing values
	     * @return Data file content as a 2D string array
	     * @throws IOException
	     */
	    public static string[][] ReadDataFile(string csvFile, string seperationChar, string nullValue)
	    {
		    List<string[]> lines = new List<string[]>();

            StreamReader bufRdr = File.OpenText(csvFile);
		    //BufferedReader bufRdr = new BufferedReader(new FileReader(new File(csvFile)));
		    // read the header
            string line = null;// bufRdr.ReadLine(); KEEP HEADER
		
		    while ((line = bufRdr.ReadLine()) != null) {
			    string[] arr = line.Split(new char[]{seperationChar[0]},StringSplitOptions.None);
                string[] result = new string[lines.Count > 0 ? lines[0].Length : arr.Length-1]; // semi-colon at the end
			
			    for(int i = 0; i < result.Length; i++) 
			    {
                    if (i >= arr.Length || arr[i].Equals("") || arr[i].Equals("-"))
                    {
                        result[i] = nullValue;
                    }
                    else
                    {
                        result[i] = arr[i];
                    }
			    }			
			    lines.Add(result);
		    }
		
		    //string[][] ret = new string[lines.Count][];
		    bufRdr.Close();
            return lines.ToArray();
		    //return lines.toArray(ret);
	    }
    }
}
