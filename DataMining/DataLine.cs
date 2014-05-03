using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMiningIndividual
{

    public class DataLine
    {

        public const int CATEGORY           = 1000000;
        public const int MECHANIC           = 2000000;
        public const int BOARDGAMEFAMILY    = 3000000;
        public const int DESIGNER           = 4000000;
        public const int ARTIST             = 5000000;
        public const int PUBLISHER          = 6000000;

        // Dictionaries for my 5 possible datatypes, the key is column header
        // and value is the current DataLine's value for it
        public Dictionary<string, DateTime?> hashDates;
        public Dictionary<string, string> hashStrings;
        public Dictionary<string, string[]> hashStringArrays;
        public Dictionary<string, double[]> hashDoubleArrays;
        public Dictionary<string, bool?> hashBooleans;
        public Dictionary<string, double?> hashDoubles;

        /// <summary>
        /// Parses the given data array without any knowledge of the
        /// types of each column by testing for types. If no type has
        /// a success rate of more than 66% it is considered a string.
        /// </summary>
        /// <param name="data">A two-dimensional (jagged) array of strings, containing
        /// the names of columns as the first element and then DataLine content in all
        /// following.</param>
        /// <returns>A list of DataLines with matching keys and types.</returns>
        public static List<DataLine> ParseInferred(string[][] data)
        {
            List<DataLine> result = data.Skip(1).Select(d => new DataLine()).ToList();

            // for each column
            for (int i = 0; i < data[0].Length; i++)
            {
                // try and parse all lines' value as all possible types (except for string)
                List<double?> matchesDouble = data.Skip(1).Select(d => ParseDouble(d[i])).ToList();
                List<bool?> matchesBool = data.Skip(1).Select(d => ParseBoolean(d[i])).ToList();
                List<DateTime?> matchesDate = data.Skip(1).Select(d => ParseDate(d[i])).ToList();
                List<string[]> matchesArray = data.Skip(1).Select(d => ParseStringArray(d[i])).ToList();

                // count matches
                List<int> matches = new int[]{matchesDouble.Count(d => d != null),
                    matchesBool.Count(d => d != null),
                    matchesDate.Count(d => d != null),
                    matchesArray.Count(d => d != null && d.Length > 1)}.ToList();

                // what is it?
                int maxIndex = matches.FindIndex(m => matches.Max() == m);
                if (matches[maxIndex] >= (2.0 / 3.0) * (data.Length - 1)) // 2/3 confidence required to not just be string
                {
                    switch (maxIndex)
                    {
                        case 0: { result.ForEach((d, j) => d.hashDoubles[data[0][i]] = matchesDouble[j]); break; }
                        case 1: { result.ForEach((d, j) => d.hashBooleans[data[0][i]] = matchesBool[j]); break; }
                        case 2: { result.ForEach((d, j) => d.hashDates[data[0][i]] = matchesDate[j]); break; }
                        case 3: { result.ForEach((d, j) => d.hashStringArrays[data[0][i]] = matchesArray[j]); break; }
                    }
                }
                else
                {
                    result.ForEach((d, j) => d.hashStrings[data[0][i]] = ParseString(data[1 + j][i]));
                }
            }

            return result;
        }

        /// <summary>
        /// Parses all the data with the types (defined by human) for the BoardGameGeek data
        /// </summary>
        /// <param name="data">A two-dimensional (jagged) array of strings, containing
        /// the names of columns (as the first element) and then DataLine content in all
        /// following.</param>
        /// <returns>A list of DataLines with matching keys and types.</returns>
        public static List<DataLine> ParseFixed(string[][] data)
        {
            Console.WriteLine("Parsing " + data.Length + " lines");
            return data.Skip(1).Select(d => DataLine.ParseFixed(d,data[0])).ToList();
        }

        public static List<DataLine> ParseHistorical(string[][] data)
        {
            return data.Skip(1).Select(d => DataLine.ParseHistorical(d, data[0])).ToList();
        }

        public static DataLine ParseHistorical(string[] data, string[] names)
        {
            DataLine result = ParseFixed(data, names);
            for (int i = 26; i <= 30; i++)
            {
                string name = names[i];
                string[] splitted = data[i].Split(',');

                double[] doubleArray = new double[6];
                for (int j = 0; j < doubleArray.Length; j++)
                {
                    bool success = double.TryParse(splitted[j], 
                        System.Globalization.NumberStyles.AllowDecimalPoint, 
                        System.Globalization.NumberFormatInfo.InvariantInfo,
                        out doubleArray[j]);
                    if (!success) doubleArray[j] = 999999.0; // Hardcoded knowledge that the rank will be missing often
                }

                result.hashDoubleArrays[name] = doubleArray;
            }

            return result;
        }

        /// <summary>
        /// Parses the data with the types (defined by human) for the BoardGameGeek data
        /// </summary>
        /// <param name="data">One line of data to be parsed.</param>
        /// <param name="names">Names (keys) for each column.</param>
        /// <returns>A DateLine with the parsed content.</returns>
        public static DataLine ParseFixed(string[] data, string[] names)
        {
            DataLine result = new DataLine();
            result.hashDoubles[names[0]] = double.Parse(data[0], System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo); 			 //id
            result.hashStrings[names[1]] = data[1];             //name
            //result.hashStrings[names[2]] = "year_published " + data[2];    //year_published
            result.hashDoubles[names[2]] = double.Parse(data[2]);    //year_published
            result.hashDoubles[names[3]] = double.Parse(data[3], System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo);     //min_players
            //result.hashStrings[names[3]] = "min_players " + data[3];     //min_players
            result.hashDoubles[names[4]] = double.Parse(data[4], System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo);     //max_players
            //result.hashStrings[names[4]] = "max_players " + data[4];       //max_players
            //result.hashStrings[names[5]] = "playingtime " + data[5];         //playingtime
            result.hashDoubles[names[5]] = double.Parse(data[5], System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo);         //playingtime
            result.hashDoubles[names[6]] = double.Parse(data[6], System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo);             //min_age
            result.hashDoubles[names[7]] = double.Parse(data[7], System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo);             //users_rated
            result.hashDoubles[names[8]] = double.Parse(data[8], System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo);       //average_rating
            //result.hashStrings[names[8]] = "average_rating " + data[8];       //average_rating
            result.hashDoubles[names[9]] = double.Parse(data[9], System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo);             //rating_stddev
            result.hashDoubles[names[10]] = double.Parse(data[10], System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo);           //num_owned
            result.hashDoubles[names[11]] = double.Parse(data[11], System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo);           //num_trading
            result.hashDoubles[names[12]] = double.Parse(data[12], System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo);           //num_wanting
            result.hashDoubles[names[13]] = double.Parse(data[13], System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo);           //num_wishing
            result.hashDoubles[names[14]] = double.Parse(data[14], System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo);           //num_comments
            //result.hashDoubles[names[15]] = ParseDouble(data[15]);          //num_players_best
            //result.hashDoubles[names[16]] = ParseDouble(data[16]);          //num_players_rec
            //result.hashDoubles[names[17]] = ParseDouble(data[17]);          //num_players_notrec
            //result.hashDoubles[names[18]] = Double.Parse(data[18]);         //suggested_age
            result.hashStringArrays[names[19]] = ParseStringArray(data[19]);//, CATEGORY); //categories
            result.hashStringArrays[names[20]] = ParseStringArray(data[20]);//, MECHANIC); //mechanics
            //result.hashStringArrays[names[21]] = ParseStringArray(data[21]);//, BOARDGAMEFAMILY); //boardgamefamilies
            //result.hashStringArrays[names[22]] = ParseStringArray(data[22]); //implementation_of
            //result.hashStringArrays[names[23]] = ParseStringArray(data[23]);//, DESIGNER); //designers
            //result.hashStringArrays[names[24]] = ParseStringArray(data[24]);//, ARTIST); //artists
            //result.hashStringArrays[names[25]] = ParseStringArray(data[25]);//, PUBLISHER); //publishers

            
            return result;
	    }

        public static Dictionary<string, Dictionary<int, string>> linkDictionary;

        public static string IDtoLabel(string idString)
        {
            try
            {
                int id = int.Parse(idString);
                if (id < MECHANIC) return linkDictionary["categories"][id - CATEGORY];
                if (id < BOARDGAMEFAMILY) return linkDictionary["mechanics"][id - MECHANIC];
                if (id < DESIGNER) return linkDictionary["families"][id - BOARDGAMEFAMILY];
                if (id < ARTIST) return linkDictionary["designers"][id - DESIGNER];
                if (id < PUBLISHER) return linkDictionary["artists"][id - ARTIST];
                return linkDictionary["publishers"][id - PUBLISHER];
            }
            catch (Exception)
            {
                return idString;
            }
        }

        /// <summary>
        /// Tries to parse the input as a double.
        /// </summary>
        /// <param name="input">The string to be parsed.</param>
        /// <returns>A parsed double or null if unable to parse.</returns>
	    private static double? ParseDouble(string input) {
            if (input == null) return null;

		    input = ParseString(input);//.Replace(",",".");
		    try{
                return double.Parse(input, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo);
		    }catch(FormatException){
			    // maybe mixed with text
			    String[] split = input.Split(new char[]{' '},StringSplitOptions.RemoveEmptyEntries);
			    if(split.Length == 1) return null; // no difference

			    double? result = null;
			    foreach(String s in split){
				    double? cur = ParseDouble(s);
				    if(cur != null){
					    if(result == null){
						    result = cur;
					    }else{
						    return null; // ambiguity
					    }
				    }
				    return cur; // worst case, same as before
			    }
			    return null;
		    }
	    }

        /// <summary>
        /// Tries to parse the input as a date.
        /// </summary>
        /// <param name="input">The string to be parsed.</param>
        /// <returns>A parsed date or null if unable to parse (or ambigous).</returns>
	    private static DateTime? ParseDate(string input) {
            if (input == null) return null;

		    // split into separate infos
		    string[] rawInfos = input.Split(new char[]{'\\','.','-',' ','/'},StringSplitOptions.RemoveEmptyEntries);
		    //System.out.println(Arrays.toString(rawInfos));
		    List<string> infos = new List<string>();
		
		    //connected numbers
		    if(rawInfos.Length == 1 && input.Length == 6){
			    return ParseDate(input.Substring(0,2)+" "
						    +input.Substring(2,2)+" "
						    +input.Substring(4,2));
		    }
		
		    foreach(String s in rawInfos){
			    if(s.Length > 0) infos.Add(RemoveBeginningZeros(ParseString(s)));
		    }
		
		    // check each if it can be day,month,year
		    int?[][] matches = new int?[infos.Count][];
		
		    for(int i = 0; i < matches.Length; i++){
                matches[i] = new int?[3];
			    matches[i][0] = canBeDay(infos[i]);
			    matches[i][1] = canBeMonth(infos[i]);
			    matches[i][2] = canBeYear(infos[i]);
			    //System.out.println(i+": "+Arrays.toString(matches[i]));
		    }
		
		    // remove values that can not be anything
		    List<int?[]> nonEmpty = new List<int?[]>();
		    List<List<int>> indices = new List<List<int>>();

		    foreach(int?[] arr in matches){
			    List<int> ind = GetContentIndices(arr,-1);
			    if(ind.Count > 0){
				    nonEmpty.Add(arr);
				    indices.Add(ind);
			    }
		    }
		
		    // 2 values are too little, 4 are too many
		    if(nonEmpty.Count < 3){
			    //Console.WriteLine("Error: "+nonEmpty.Count+" values are too little");
			    return null;
		    }
		
		    // gather values
		    int[] result = new int[3]; // day,month,year
		    int found = 0;
		
		    while(found < 3){
			    // find one with single value
			    int indexOfOner = -1;
			    for(int i = 0; i < indices.Count; i++){
				    if(indices[i].Count == 1)
					    indexOfOner = i;
			    }
			    if(indexOfOner == -1){
				    //Console.WriteLine("Error: Not a single one was one value");
				    return null;
			    }
			
			    // put the value
			    int pos = indices[indexOfOner][0];
			    result[pos] = (int)nonEmpty[indexOfOner][pos];
			
			    // remove from options
			    foreach(List<int> arr in indices){
				    arr.Remove(pos);
			    }
			
			    // repeat
			    found++;
		    }
	
		    // assemble!
            try
            {
                return new DateTime(result[2], result[1], result[0]);
            }catch(ArgumentOutOfRangeException){
                return null;
            }
	    }
	
	    private static String RemoveBeginningZeros(String number){
		    if(number.StartsWith("0")){
			    return RemoveBeginningZeros(number.Substring(1,number.Length-1));
		    }
		    return number;
	    }

        /// <summary>
        /// Finds the indice numbers where there is non-null content in the array.
        /// </summary>
        /// <param name="arr">The array to look in.</param>
        /// <param name="excludeIndex">An index that should be skipped.</param>
        /// <returns>A list with the indice numbers where you will find content in the array.</returns>
	    private static List<int> GetContentIndices(int?[] arr, int excludeIndex){
		    List<int> result = new List<int>();
		    for(int i = 0; i < arr.Length; i++){
			    if(i != excludeIndex && arr[i] != null) result.Add(i);
		    }
		    return result;
	    }
	
	    private static int? canBeDay(string s){
		    s = ParseString(s);
		    int? number = ParseInteger(s);
	
		    if(number == null && (s.EndsWith("st") || s.EndsWith("nd") || s.EndsWith("rd") || s.EndsWith("th"))){
			    number = ParseInteger(s.Substring(0, s.Length-2));
		    }
		
		    if(number != null && number <= 31) return number;
		    return null; // number above 31
	    }
	
	    private static int? canBeMonth(string s){
		    s = ParseString(s);
		    int? number = ParseInteger(s);
		    if(number != null && number <= 12) return number;
		
		    string[] months = {"jan","feb","mar","apr","may","jun","jul","aug","sep","oct","nov","dec"};
		    for(int i = 0; i < months.Length; i++){
			    if(s.Contains(months[i])) return i+1;
		    }
		    return null;
	    }
	
	    private static int? canBeYear(string s){
		    int? number = ParseInteger(s);
		    if(number == null || number < 0 || number > 3000) return null;
				    // 2015
		    if(number < 15) return number+2000;
		    if(number < 100) return number+1900;
		    return number;
	    }

        /// <summary>
        /// Tries to parse the input as a boolean.
        /// </summary>
        /// <param name="input">The string to be parsed.</param>
        /// <returns>A parsed boolean or null if unable to parse.</returns>
	    private static bool? ParseBoolean(string input) {
            if (input == null) return null;

		    input = ParseString(input);
		    string[] positive = {"yes","ja","si","da","oui","shi","affirmative","positive","true","yea"};
		    string[] negative = {"no","nej","nein","bushi","negative","false"};
		    bool pos = false;
		    bool neg = false;
		
		    foreach(string s in positive){
			    if(input.Contains(s)) pos = true;
		    }
		    foreach(string s in negative){
			    if(input.Contains(s)) neg = true;
		    }
		
		    if(pos & !neg) return true;
		    if(neg & !pos) return false;
		
		    return null;
	    }

        private static string[] ParseStringArray(string input, int numberToAddToValue)
        {
            if (input == null) return null;

            string[] output = ParseStringArray(input);
            for (int i = 0; i < output.Length;i++)
            {
                try
                {
                    int v = int.Parse(output[i]);
                    v += numberToAddToValue;
                    output[i] = v.ToString();   
                }
                catch (FormatException){ /* do nothing */ }
            }
            return output;
        }

	    private static string[] ParseStringArray(string input) {
            if (input == null) return null;

		    string[] output;
		    if(input.Contains(",")){
			    output = input.Split(',');
		    }else{
			    output = input.Split(' ');
		    }
            //TODO: Remember that parse string has been commented out for performance
		    //for(int i = 0; i < output.Length; i++) output[i] = ParseString(output[i]);
		    return output;
	    }

        /// <summary>
        /// Tries to parse the input as an integer.
        /// </summary>
        /// <param name="input">The string to be parsed.</param>
        /// <returns>A parsed integer or null if unable to parse.</returns>
	    private static int? ParseInteger(string input) {
		    input = ParseString(input);

            try
            {
                return int.Parse(input);
            }
            catch (FormatException)
            {
                // maybe mixed with text
                string[] split = input.Split(' ');
                if (split.Length == 1) return null; // no difference

                int? result = null;
                foreach (string s in split)
                {
                    int? cur = ParseInteger(s);
                    if (cur != null)
                    {
                        if (result == null)
                        {
                            result = cur;
                        }
                        else
                        {
                            return null; // ambiguity
                        }
                    }
                    return cur; // worst case, same as before
                }
                return null;
            }
            catch (OverflowException)
            {
                return null;
            }
	    }

        /// <summary>
        /// Parses the string, making it lowercase and removes beginning
        /// and trailing whitespace.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The resulting string.</returns>
	    private static string ParseString(string input, string prefix = "") {
            if (input == null) return null;
		    // try and make strings uniform
		
		    // all lower
            input = input.ToLower();
		
		    // remove extra space
            input = input.Trim();
		
		    return prefix + input;
	    }

        /// <summary>
        /// Just creates all Dictionaries.
        /// </summary>
        public DataLine()
        {
            hashDates = new Dictionary<string, DateTime?>();
            hashStrings = new Dictionary<string, string>();
            hashStringArrays = new Dictionary<string, string[]>();
            hashDoubleArrays = new Dictionary<string, double[]>();
            hashBooleans = new Dictionary<string, bool?>();
            hashDoubles = new Dictionary<string, double?>();
        } 

	    public override string ToString() {
            StringBuilder builder = new StringBuilder();
            builder.Append("[");
            hashStrings.Keys.ForEach(k => builder.Append("\"" + k + "\"" + ": " + (hashStrings[k] ?? "null") + ",")); // hashStringArrays[k].Aggregate("",(agg,cur) => agg + ", "+ (cur ?? "null"))
            hashStringArrays.Keys.ForEach(k => builder.Append("\"" + k + "\"" + ": [" + (hashStringArrays[k] == null ? "null" : string.Join(";",hashStringArrays[k])) + "],"));
            hashDoubleArrays.Keys.ForEach(k => builder.Append("\"" + k + "\"" + ": [" + (hashDoubleArrays[k] == null ? "null" : string.Join(";", hashDoubleArrays[k])) + "],"));
            hashDates.Keys.ForEach(k => builder.Append("\"" + k + "\"" + ": " + hashDates[k] + ","));
            hashDoubles.Keys.ForEach(k => builder.Append("\"" + k + "\"" + ": " + ((double)hashDoubles[k]).ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + ","));
            hashBooleans.Keys.ForEach(k => builder.Append("\"" + k + "\"" + ": " + hashBooleans[k] + ","));

            builder.Append("]");
            return builder.ToString();
	    }

    }

}
