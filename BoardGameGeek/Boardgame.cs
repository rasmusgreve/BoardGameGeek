using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

namespace BoardGameGeek
{
    class Boardgame
    {
        private static StreamWriter _fileWriter;
        private static string[] _burstResult;
        const int RANGE = 200;
        const int BurstSize = 10;
        public static void Main()
        {
            /*
            var xd = new XmlDocument();
            xd.Load("http://www.boardgamegeek.com/xmlapi2/thing?id=" + 0 + "&stats=1&ratingcomments=1&pagesize=100&page=1");
            Boardgame b = ParseXml(xd);
            Console.WriteLine(b);
            Console.WriteLine("--------------------------");
            if (CSVForID(0) == null)
                Console.WriteLine("null");
            else
            {
                Console.WriteLine(CSVForID(0));
            }
            Console.ReadLine();
            */
            
            _burstResult = new string[BurstSize];
            var ts = new Thread[BurstSize];
            _fileWriter = File.CreateText("data" + string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}",
        DateTime.Now) + ".csv");
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 1; i <= RANGE; i+=BurstSize)
            {
                //break; //@@@@@@@ Do nothing! The file has been made
                //Start threads
                for (int j = i; j < i + BurstSize; j++)
                {
                    ts[j-i] = new Thread(FetchDataAsync);
                    ts[j - i].Start(new Tuple<int,int>(j,j-i));
                }
                //wait for completion
                foreach (var thread in ts)
                {
                    thread.Join();
                }
                //write to file
                foreach (var s in _burstResult)
                {
                    if (s == null) continue;
                    _fileWriter.WriteLine(s);
                }
                _fileWriter.Flush();
            }
            _fileWriter.Close();

            // id names
            StreamWriter ids = File.CreateText("linkIdNames.txt");
            var dicts = new Dictionary<int, string>[] { categoryNames, mechanicNames, familyNames, designerNames, artistNames, publisherNames };
            string[] headers = new string[]{"categories","mechanics","families","designers","artists","publishers"};
            for(int i = 0; i < dicts.Length; i++)
            {
                var dict = dicts[i];
                ids.WriteLine(headers[i]);
                foreach (var kv in dict.OrderBy(kv => kv.Key))
                {
                    ids.WriteLine(kv.Key + " = " + kv.Value);
                }
                ids.WriteLine("");
            }
            ids.Flush();
            ids.Close();

            Console.WriteLine("All done! " + sw.ElapsedMilliseconds/1000 + "s");
            Console.ReadLine();
        }

        public static void FetchDataAsync(Object o)
        {
            var v = (Tuple<int, int>)o;
            int i = v.Item1, p = v.Item2;
            string r = CSVForID(i);
            _burstResult[p] = r;
            if (r != null)
            {   
                Console.WriteLine("Id " + i + " done!");
            }
            else
            {
                Console.WriteLine("Id " + i + " skipped!");
            }
        }

        public static string CSVForID(int id)
        {
            try
            {
                var xd = new XmlDocument();
                xd.Load("http://www.boardgamegeek.com/xmlapi2/thing?id=" + id + "&stats=1&pagesize=100&page=1&type=boardgame");
                var game = ParseXml(xd);
                if (game == null) return null;
                return game.ToEmilCSV();
            }
            catch
            {
                return null;
            }
        }

        private static Dictionary<int, string> categoryNames = new Dictionary<int, string>();
        private static Dictionary<int, string> mechanicNames = new Dictionary<int, string>();
        private static Dictionary<int, string> familyNames = new Dictionary<int, string>();
        private static Dictionary<int, string> designerNames = new Dictionary<int, string>();
        private static Dictionary<int, string> artistNames = new Dictionary<int, string>();
        private static Dictionary<int, string> publisherNames = new Dictionary<int, string>();

        public static Boardgame ParseXml(XmlDocument d)
        {
            if (!d.FirstChild.NextSibling.HasChildNodes) return null;
            var result = new Boardgame();

            //Simple fields
            result.Id = int.Parse(getSimpleValue(d, "/items/item", "id"));
            result.Name = getSimpleValue(d, "/items/item/name[@type='primary']", "value");
            result.YearPublished = int.Parse(getSimpleValue(d, "/items/item/yearpublished", "value"));
            result.MinPlayers = int.Parse(getSimpleValue(d, "/items/item/minplayers", "value"));
            result.MaxPlayers = int.Parse(getSimpleValue(d, "/items/item/maxplayers", "value"));
            result.PlayingTime = int.Parse(getSimpleValue(d, "/items/item/playingtime", "value"));
            result.MinAge = int.Parse(getSimpleValue(d, "/items/item/minage", "value"));

            result.UsersRated = int.Parse(getSimpleValue(d, "/items/item/statistics/ratings/usersrated", "value"));
            result.Average = double.Parse(getSimpleValue(d, "/items/item/statistics/ratings/average", "value").Replace(".",","));
            result.StdDev = double.Parse(getSimpleValue(d, "/items/item/statistics/ratings/stddev", "value").Replace(".", ","));
            result.Owned = int.Parse(getSimpleValue(d, "/items/item/statistics/ratings/owned", "value"));
            result.Trading = int.Parse(getSimpleValue(d, "/items/item/statistics/ratings/trading", "value"));
            result.Wanting = int.Parse(getSimpleValue(d, "/items/item/statistics/ratings/wanting", "value"));
            result.Wishing = int.Parse(getSimpleValue(d, "/items/item/statistics/ratings/wishing", "value"));
            result.NumComments = int.Parse(getSimpleValue(d, "/items/item/statistics/ratings/numcomments", "value"));

            //Player count votes
            var numPlayersNodes = d.SelectNodes("/items/item/poll[@name='suggested_numplayers']/results/result");
            if (numPlayersNodes != null)
            {
                foreach (XmlNode node in numPlayersNodes)
                {
                    int numPlayers;
                    if (!int.TryParse(node.ParentNode.Attributes["numplayers"].Value, out numPlayers))
                        numPlayers = -1;

                    var votes = int.Parse(node.Attributes["numvotes"].Value);
                    if (node.Attributes["value"].Value.Equals("Best")){
                        result.NumPlayersBest.Add(numPlayers, votes);
                    }
                    else if (node.Attributes["value"].Value.Equals("Recommended")){
                        result.NumPlayersRecommended.Add(numPlayers, votes);
                    }
                    else{
                        result.NumPlayersNotRecommended.Add(numPlayers, votes);
                    }
                }
            }

            //Suggested player age
            var playerAgeNodes = d.SelectNodes("/items/item/poll[@name='suggested_playerage']/results/result");
            if (playerAgeNodes != null)
            {
                foreach (XmlNode node in playerAgeNodes)
                {
                    int age;
                    if (!int.TryParse(node.Attributes["value"].Value, out age))
                        age = -1;
                    var votes = int.Parse(node.Attributes["numvotes"].Value);
                    result.SuggestedPlayerAge.Add(age,votes);
                }
            }

            //Categories
            var categoriesNodes = d.SelectNodes("/items/item/link[@type='boardgamecategory']");
            if (categoriesNodes != null)
                foreach (XmlNode node in categoriesNodes)
                {
                    int id = int.Parse(node.Attributes["id"].Value);
                    string name = node.Attributes["value"].Value;
                    result.Categories.Add(id);
                    categoryNames[id] = name;
                }

            var mechanicsNodes = d.SelectNodes("/items/item/link[@type='boardgamemechanic']");
            if (mechanicsNodes != null)
                foreach (XmlNode node in mechanicsNodes)
                {
                    int id = int.Parse(node.Attributes["id"].Value);
                    string name = node.Attributes["value"].Value;
                    result.Mechanics.Add(id);
                    mechanicNames[id] = name;
                }
                    
            // board game family
            var familyNodes = d.SelectNodes("/items/item/link[@type='boardgamefamily']");
            if (familyNodes != null)
                foreach (XmlNode node in familyNodes)
                {
                    int id = int.Parse(node.Attributes["id"].Value);
                    string name = node.Attributes["value"].Value;
                    result.Families.Add(id);
                    familyNames[id] = name;
                }

            // designer
            var designerNodes = d.SelectNodes("/items/item/link[@type='boardgamedesigner']");
            if (designerNodes != null)
                foreach (XmlNode node in designerNodes)
                {
                    int id = int.Parse(node.Attributes["id"].Value);
                    string name = node.Attributes["value"].Value;
                    result.Designers.Add(id);
                    designerNames[id] = name;
                }

            // artist
            var artistNodes = d.SelectNodes("/items/item/link[@type='boardgameartist']");
            if (artistNodes != null)
                foreach (XmlNode node in artistNodes)
                {
                    int id = int.Parse(node.Attributes["id"].Value);
                    string name = node.Attributes["value"].Value;
                    result.Artists.Add(id);
                    artistNames[id] = name;
                }

            // publisher
            var publisherNodes = d.SelectNodes("/items/item/link[@type='boardgamepublisher']");
            if (publisherNodes != null)
                foreach (XmlNode node in publisherNodes)
                {
                    int id = int.Parse(node.Attributes["id"].Value);
                    string name = node.Attributes["value"].Value;
                    result.Publishers.Add(id);
                    publisherNames[id] = name;
                }

            return result;
        }

        private static string getSimpleValue(XmlDocument d, string xpath, string attribute)
        {
            var xmlNodeList = d.SelectNodes(xpath);
            return xmlNodeList != null ? xmlNodeList[0].Attributes[attribute].Value : null;
        }


        public int Id { get; set; }
        public string Name { get; set; }
        public int YearPublished { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public int PlayingTime { get; set; }
        public int MinAge { get; set; }
        public int UsersRated { get; set; }
        public double Average { get; set; }
        public double StdDev { get; set; }
        public int Owned { get; set; }
        public int Trading { get; set; }
        public int Wanting { get; set; }
        public int Wishing { get; set; }
        public int NumComments { get; set; }
        //numPlayers -> votes
        public Dictionary<int, int> NumPlayersBest { get; set; }
        public Dictionary<int, int> NumPlayersRecommended { get; set; }
        public Dictionary<int, int> NumPlayersNotRecommended { get; set; }
        //age -> votes
        public Dictionary<int, int> SuggestedPlayerAge { get; set; }
        public ISet<int> Categories { get; set; }
        public ISet<int> Mechanics { get; set; }
        public ISet<int> Families { get; set; }

        public ISet<int> Implementations { get; set; }
        public ISet<int> Designers { get; set; }
        public ISet<int> Artists { get; set; }
        public ISet<int> Publishers { get; set; }

        public Boardgame()
        {
            NumPlayersBest = new Dictionary<int, int>();
            NumPlayersRecommended = new Dictionary<int, int>();
            NumPlayersNotRecommended = new Dictionary<int, int>();
            SuggestedPlayerAge = new Dictionary<int, int>();
            Categories = new HashSet<int>();
            Mechanics = new HashSet<int>();
            Families = new HashSet<int>();

            Implementations = new HashSet<int>();
            Designers = new HashSet<int>();
            Artists = new HashSet<int>();
            Publishers = new HashSet<int>();
        }

        public string ToEmilCSV()
        {
            string space = ";";
            StringBuilder builder = new StringBuilder();
            builder.Append(Id + space);
            builder.Append(Name + space);
            builder.Append(YearPublished + space);
            builder.Append(MinPlayers + space);
            builder.Append(MaxPlayers + space);
            builder.Append(PlayingTime + space);
            builder.Append(MinAge + space);
            builder.Append(UsersRated + space);
            builder.Append(Average + space);
            builder.Append(StdDev + space);
            builder.Append(Owned + space);
            builder.Append(Trading + space);
            builder.Append(Wanting + space);
            builder.Append(Wishing + space);
            builder.Append(NumComments + space);
            builder.Append(Id + space);
            builder.Append(string.Join(",",NumPlayersBest.Select(kv => kv.Key+":"+kv.Value)) + space);
            builder.Append(string.Join(",", NumPlayersRecommended.Select(kv => kv.Key + ":" + kv.Value)) + space);
            builder.Append(string.Join(",", NumPlayersNotRecommended.Select(kv => kv.Key + ":" + kv.Value)) + space);
            builder.Append(string.Join(",", SuggestedPlayerAge.Select(kv => kv.Key + ":" + kv.Value)) + space);
            builder.Append(string.Join(",", Categories) + space);
            builder.Append(string.Join(",", Mechanics) + space);
            builder.Append(string.Join(",", Families) + space);
            builder.Append(string.Join(",", Implementations) + space);
            builder.Append(string.Join(",", Designers) + space);
            builder.Append(string.Join(",", Artists) + space);
            builder.Append(string.Join(",", Publishers) + space);

            return builder.ToString();
        }

        // missing newest values
        public string ToCSV()
        {
            return Id + ";" +
                "\"" + Name + "\";" +
                YearPublished + ";" +
                MinPlayers + ";" +
                MaxPlayers + ";" +
                PlayingTime + ";" +
                MinAge + ";" + 
                UsersRated  + ";" + 
                Average + ";" + 
                StdDev + ";" + 
                Owned + ";" + 
                Trading + ";" + 
                Wanting + ";" + 
                Wishing + ";" +
                NumComments + ";" + 
                DictCSV(NumPlayersBest) + ";" +
                DictCSV(NumPlayersRecommended) + ";" +
                DictCSV(NumPlayersNotRecommended) + ";" +
                DictCSV(SuggestedPlayerAge) + ";" +
                SetCSV(Categories) + ";" +
                SetCSV(Mechanics) + ";" +
                SetCSV(Families)
                ;
        }

        private static string DictCSV(Dictionary<int, int> dict)
        {
            return "{" + string.Join(";", dict.Select(v => v.Key + "=>" + v.Value)) + "}";
        }

        private static string SetCSV(ISet<int> set)
        {
            return "{" + string.Join(";", set) + "}";
        }

        public override string ToString()
        {
            return "[ Board game " +
                "\r\n\tId: " + Id +
                "\r\n\tName: " + Name +
                "\r\n\tYearPublished: " + YearPublished +
                "\r\n\tMinPlayers: " + MinPlayers +
                "\r\n\tMaxPlayers: " + MaxPlayers +
                "\r\n\tPlayingTime: " + PlayingTime +
                "\r\n\tMinAge: " + MinAge +
                "\r\n\tUsersRated : " + UsersRated  +
                "\r\n\tAverage: " + Average +
                "\r\n\tStdDev: " + StdDev +
                "\r\n\tOwned: " + Owned +
                "\r\n\tTrading: " + Trading +
                "\r\n\tWanting: " + Wanting +
                "\r\n\tWishing: " + Wishing +
                "\r\n\tNumComments: " + NumComments +
                "\r\n\tNumPlayersBest: " + string.Join(", ", NumPlayersBest) +
                "\r\n\tNumPlayersRecommended: " + string.Join(", ", NumPlayersRecommended) +
                "\r\n\tNumPlayersNotRecommended: " + string.Join(", ", NumPlayersNotRecommended) +
                "\r\n\tSuggestedPlayerAge: " + string.Join(", ", SuggestedPlayerAge) +
                "\r\n\tCategories: " + string.Join(", ", Categories) +
                "\r\n\tMechanics: " + string.Join(", ", Mechanics) +
                "\r\n\tFamilies: " + string.Join(", ", Families) +
                "\r\n]";
        }
    }
}
