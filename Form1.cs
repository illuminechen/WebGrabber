using AngleSharp;
using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebGrabber
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            for (var i = listBox1.SelectedIndices[listBox1.SelectedIndices.Count - 1]; i >= listBox1.SelectedIndices[0]; i--)
            {
                listBox1.Items.Remove(listBox1.SelectedIndices[i]);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.AddRange(richTextBox1.Text.Split('\n'));
            richTextBox1.Text = "";
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void DoOnce(string url)
        {
            IConfiguration config = Configuration.Default.WithDefaultLoader();
            IDocument doc = BrowsingContext.New(config).OpenAsync(url).Result;

            /*CSS Selector寫法*/
            IHtmlCollection<IElement> imgs = doc.QuerySelectorAll("div.movie_foto img:first-child");//取得圖片
            foreach (IElement img in imgs)
            {
                Console.WriteLine(img.GetAttribute("src"));
            }
            Console.ReadKey();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0)
            {
                toolStripProgressBar1.Maximum = listBox1.SelectedItems.Count;
                foreach (var item in listBox1.SelectedItems)
                {
                    try
                    {
                        DoOnce(item.ToString());
                        toolStripProgressBar1.Value++;
                    }
                    catch
                    {

                    }
                }
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count > 0)
            {
                toolStripProgressBar1.Maximum = listBox1.Items.Count;

                foreach (var item in listBox1.Items)
                {
                    try
                    {
                        DoOnce(item.ToString());
                        toolStripProgressBar1.Value++;
                    }
                    catch
                    {

                    }
                }
            }
        }
    }
}
