using AngleSharp;
using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngleSharp.Io;
using Flurl.Http;

namespace WebGrabber
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        class compobj
        {
            public string field;
            public string pattern;
            public string condition;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndices.Count > 0)
                for (int i = listBox1.SelectedItems.Count - 1; i >= 0; i--)
                {
                    listBox1.Items.Remove(listBox1.SelectedItems[i]);
                }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.AddRange(richTextBox1.Text.Split('\n'));
            richTextBox1.Text = "";
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (listBox1.SelectedItems.Count > 0)
            //{
            //    webBrowser1.Navigate(listBox1.SelectedItems[0].ToString());
            //}
            //else
            //{
            //    webBrowser1.Navigate("");
            //}
        }

        private async void DoAsync(List<string> urllist, List<compobj> FieldPat)
        {
            int index = 0;
            foreach (var item in urllist)
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                try
                {

                    var resp = await item.ToString().WithTimeout(5).GetAsync().ReceiveString();

                    var config = Configuration.Default.WithDefaultLoader();
                    var context = BrowsingContext.New(config);

                    var document = await context.OpenAsync(req => req.Content(resp));

                    /*CSS Selector寫法*/
                    foreach (var comp in FieldPat)
                    {
                        IElement tag = document.QuerySelector(comp.pattern);
                        if (tag != null)
                        {
                            if (comp.condition.Trim() == "")
                            {
                                if (!result.ContainsKey(comp.field))
                                    result.Add(comp.field, tag.TextContent.Replace("\n", "").Trim());
                            }
                            else
                            {
                                string[] conds = comp.condition.Split('=');
                                if (tag.HasAttribute(conds[0]) && tag.GetAttribute(conds[0]) == conds[1])
                                {
                                    if (!result.ContainsKey(comp.field))
                                        result.Add(comp.field, tag.TextContent.Replace("\n", "").Trim());
                                }
                                else
                                {
                                    if (!result.ContainsKey(comp.field))
                                        result.Add(comp.field, " ");
                                }
                            }
                        }
                    }
                }
                catch
                {

                }
                finally
                {
                    richTextBox3.Invoke(new Action(() => { SetValue(result); }));
                }

            }



        }

        private void SetValue(Dictionary<string, string> dict)
        {
            List<compobj> patterns = getFieldPattern();
            List<string> words = new List<string>();
            foreach (string name in patterns.Select(x => x.field).ToList())
            {
                words.Add(dict.ContainsKey(name) ? dict[name] : "");
            }
            richTextBox3.Text += string.Join("\t", words) + "\n";
            toolStripProgressBar1.Value++;
        }

        private List<compobj> getFieldPattern()
        {
            List<compobj> result = new List<compobj>();
            foreach (DataGridViewRow dgvr in dataGridView1.Rows)
            {
                if (dgvr.Cells[0].Value != null && dgvr.Cells[1].Value != null)
                {
                    string field = dgvr.Cells[0].Value.ToString();
                    string pattern = dgvr.Cells[1].Value.ToString();
                    string condition = dgvr.Cells[2].Value == null ? "" : dgvr.Cells[2].Value.ToString();
                    if (!string.IsNullOrWhiteSpace(field) && !string.IsNullOrWhiteSpace(pattern))
                        result.Add(new compobj() { field = field, pattern = pattern, condition = condition });
                }
            }
            return result;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0)
            {
                var complist = getFieldPattern();
                richTextBox3.Text = string.Join("\t", complist.Select(x => x.field)) + "\n";
                toolStripProgressBar1.Maximum = listBox1.SelectedItems.Count;
                toolStripProgressBar1.Value = 0;
                DoAsync(listBox1.SelectedItems.Cast<object>().Select(x => x.ToString()).ToList(), complist);
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count > 0)
            {
                var complist = getFieldPattern();
                richTextBox3.Text = string.Join("\t", complist.Select(x => x.field)) + "\n";
                toolStripProgressBar1.Maximum = listBox1.Items.Count;
                toolStripProgressBar1.Value = 0;
                DoAsync(listBox1.Items.Cast<object>().Select(x => x.ToString()).ToList(), complist);
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //richTextBox2.Text = webBrowser1.DocumentText;
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            var complist = getFieldPattern();

            SaveFileDialog sfd = new SaveFileDialog() { Filter = "文字|*.txt" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.Default))
                {
                    sw.Write(string.Join("\n", complist.Select(x => x.field + "\t" + x.pattern + "\t" + x.condition)));
                }
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "文字|*.txt" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                using (StreamReader sr = new StreamReader(ofd.FileName, Encoding.Default))
                {
                    string str = sr.ReadToEnd();


                    dataGridView1.Rows.Clear();

                    foreach (string line in str.Split('\n'))
                    {
                        var cells = line.Split('\t');
                        dataGridView1.Rows.Add(cells);
                    }
                }
            }
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog() { Filter = "文字|*.txt" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.Default))
                {
                    sw.Write(string.Join("\n", listBox1.Items.Cast<object>().ToList().Select(x => x.ToString())));
                }
            }
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "文字|*.txt" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                using (StreamReader sr = new StreamReader(ofd.FileName, Encoding.Default))
                {
                    string str = sr.ReadToEnd();
                    listBox1.Items.Clear();

                    foreach (string line in str.Split('\n'))
                    {
                        listBox1.Items.Add(line);
                    }
                }
            }
        }
    }
}
