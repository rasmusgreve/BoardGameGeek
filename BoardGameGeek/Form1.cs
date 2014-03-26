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
            int id = 1, id_to = 2;
            int.TryParse(textBox1.Text, out id);
            int.TryParse(textBox2.Text, out id_to);
            for (int i = id; i < id_to; i++)
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                webClient.DownloadFileAsync(new Uri("http://www.boardgamegeek.com/xmlapi2/thing?id=" + i + "&stats=1&ratingcomments=1&pagesize=100&page=1"), @"xml/thing_" + i + ".txt", i);
                /*xd.Load("http://www.boardgamegeek.com/xmlapi2/thing?id=" + i + "&stats=1&ratingcomments=1&pagesize=100&page=1");
                xd.Save("xml/thing_" + i + ".xml");
                Console.WriteLine("Done with item: " + i);*/
            }
            MessageBox.Show("All started");
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            Console.WriteLine("Done with item: " + e.UserState);
        }
    }
}
