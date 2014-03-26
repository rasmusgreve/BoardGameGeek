using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BoardGameGeek
{
    class Boardgame
    {

        public static void Main()
        {
            var xd = new XmlDocument();
            xd.Load("http://www.boardgamegeek.com/xmlapi2/thing?id=" + 25292 + "&stats=1&ratingcomments=1&pagesize=100&page=1");
            Console.WriteLine(ParseXml(xd));
        }

        public static Boardgame ParseXml(XmlDocument d)
        {
            //if (!d.FirstChild.NextSibling.HasChildNodes) return null;
            var result = new Boardgame();

            //Simple fields
            result.Name = getSimpleValue(d, "/items/item/name[@type='primary']", "value");
            result.YearPublished = int.Parse(getSimpleValue(d, "/items/item/yearpublished", "value"));
            result.MinPlayers = int.Parse(getSimpleValue(d, "/items/item/minplayers", "value"));
            result.MaxPlayers = int.Parse(getSimpleValue(d, "/items/item/maxplayers", "value"));
            result.PlayingTime = int.Parse(getSimpleValue(d, "/items/item/playingtime", "value"));
            result.MinAge = int.Parse(getSimpleValue(d, "/items/item/minage", "value"));

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
                    result.Categories.Add(int.Parse(node.Attributes["id"].Value));

            var mechanicsNodes = d.SelectNodes("/items/item/link[@type='boardgamemechanic']");
            if (mechanicsNodes != null)
                foreach (XmlNode node in mechanicsNodes)
                    result.Mechanics.Add(int.Parse(node.Attributes["id"].Value));

            var familyNodes = d.SelectNodes("/items/item/link[@type='boardgamefamily']");
            if (familyNodes != null)
                foreach (XmlNode node in familyNodes)
                    result.Families.Add(int.Parse(node.Attributes["id"].Value));

            return result;
        }

        private static string getSimpleValue(XmlDocument d, string xpath, string attribute)
        {
            var xmlNodeList = d.SelectNodes(xpath);
            return xmlNodeList != null ? xmlNodeList[0].Attributes[attribute].Value : null;
        }



        public string Name { get; set; }
        public int YearPublished { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public int PlayingTime { get; set; }
        public int MinAge { get; set; }
        //numPlayers -> votes
        public Dictionary<int, int> NumPlayersBest { get; set; }
        public Dictionary<int, int> NumPlayersRecommended { get; set; }
        public Dictionary<int, int> NumPlayersNotRecommended { get; set; }
        //age -> votes
        public Dictionary<int, int> SuggestedPlayerAge { get; set; }
        public ISet<int> Categories { get; set; }
        public ISet<int> Mechanics { get; set; }
        public ISet<int> Families { get; set; }

        public Boardgame()
        {
            NumPlayersBest = new Dictionary<int, int>();
            NumPlayersRecommended = new Dictionary<int, int>();
            NumPlayersNotRecommended = new Dictionary<int, int>();
            SuggestedPlayerAge = new Dictionary<int, int>();
            Categories = new HashSet<int>();
            Mechanics = new HashSet<int>();
            Families = new HashSet<int>();
        }


        public override string ToString()
        {
            return "[ Board game " +
                "\r\n\tName: " + Name +
                "\r\n\tYearPublished: " + YearPublished +
                "\r\n\tMinPlayers: " + MinPlayers +
                "\r\n\tMaxPlayers: " + MaxPlayers +
                "\r\n\tPlayingTime: " + PlayingTime +
                "\r\n\tMinAge: " + MinAge +
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
