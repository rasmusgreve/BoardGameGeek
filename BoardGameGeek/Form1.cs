using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Xml;

namespace BoardGameGeek
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            XmlDocument xd = new XmlDocument();
            string id = textBox1.Text;
            xd.Load("http://www.boardgamegeek.com/xmlapi2/thing?id="+id+"&stats=1&ratingcomments=1&pagesize=100&page=1");
            foreach (XmlNode n in xd.ChildNodes)
            {
                if (n.Name.Equals("items"))
                {
                    foreach (XmlNode nn in n.ChildNodes)
                    {
                        foreach (XmlNode nnn in nn.ChildNodes)
                        {
                            if (nnn.Attributes["value"] != null)
                                Console.WriteLine(nnn.Name + " - " + nnn.Attributes["value"].Value);
                            else
                                Console.WriteLine(nnn.Name);
                        }
                    }
                }
            }
            
            
        }
    }
}
