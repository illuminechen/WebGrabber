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
using System.Diagnostics;
using Microsoft.Win32;
using System.Threading;

namespace WebGrabber
{
    public partial class Form1 : Form
    {
        int current = 0;
        int count = 0;
        Dictionary<string, string> result = new Dictionary<string, string>();
        List<compobj> FieldPat = new List<compobj>();
        List<string> urllist = new List<string>();
        public Form1()
        {
            InitializeComponent();
        }

        public class compobj
        {
            public string field;
            public string pattern;
            public string condition;
            public string attr;

            public string getContent(IElement tag)
            {
                if (condition.Trim() == "")
                {
                    if (attr == "")
                        return tag.TextContent.Replace("\n", "").Trim();
                    else
                    {
                        var attrContent = tag.GetAttribute(attr);
                        if (attr != null)
                            return attrContent.Replace("\n", "").Trim();
                        else
                            return " ";
                    }
                }
                else
                {
                    string[] conds = condition.Split('=');
                    if (tag.HasAttribute(conds[0]) && tag.GetAttribute(conds[0]) == conds[1])
                    {
                        if (attr == "")
                            return tag.TextContent.Replace("\n", "").Trim();
                        else
                            return tag.GetAttribute(attr).Replace("\n", "").Trim();
                    }
                    return " ";
                }
            }
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
                    string attr = dgvr.Cells[3].Value == null ? "" : dgvr.Cells[3].Value.ToString();
                    if (!string.IsNullOrWhiteSpace(field) && !string.IsNullOrWhiteSpace(pattern))
                        result.Add(new compobj() { field = field, pattern = pattern, condition = condition, attr = attr });
                }
            }
            return result;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0)
            {
                webBrowser1.Navigate("");
                FieldPat = getFieldPattern();
                richTextBox3.Text = string.Join("\t", FieldPat.Select(x => x.field)) + "\n";

                current = listBox1.SelectedIndices[0];
                count = listBox1.SelectedItems.Count;
                toolStripProgressBar1.Maximum = count;
                toolStripProgressBar1.Value = 0;
                urllist = listBox1.SelectedItems.Cast<string>().ToList().Select(x => x.ToString()).ToList();
                webBrowser1.Navigate(listBox1.SelectedItems[0].ToString());
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count > 0)
            {
                webBrowser1.Navigate("");
                FieldPat = getFieldPattern();
                richTextBox3.Text = string.Join("\t", FieldPat.Select(x => x.field)) + "\n";

                current = 0;
                count = listBox1.Items.Count;
                toolStripProgressBar1.Maximum = count;
                toolStripProgressBar1.Value = 0;

                urllist = listBox1.Items.Cast<string>().ToList().Select(x => x.ToString()).ToList();

                webBrowser1.Navigate(listBox1.Items[0].ToString());
            }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            var complist = getFieldPattern();

            SaveFileDialog sfd = new SaveFileDialog() { Filter = "文字|*.txt" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.Default))
                {
                    sw.Write(string.Join("\n", complist.Select(x => x.field + "\t" + x.pattern + "\t" + x.condition + "\t" + x.attr)));
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
                        if (line != "")
                            listBox1.Items.Add(line);
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            int BrowserVer, RegVal;

            // get the installed IE version
            using (WebBrowser Wb = new WebBrowser())
                BrowserVer = Wb.Version.Major;

            // set the appropriate IE version
            if (BrowserVer >= 11)
                RegVal = 11001;
            else if (BrowserVer == 10)
                RegVal = 10001;
            else if (BrowserVer == 9)
                RegVal = 9999;
            else if (BrowserVer == 8)
                RegVal = 8888;
            else
                RegVal = 7000;

            // set the actual key
            using (RegistryKey Key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", RegistryKeyPermissionCheck.ReadWriteSubTree))
                if (Key.GetValue(System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe") == null)
                    Key.SetValue(System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe", RegVal, RegistryValueKind.DWord);
        }

        private async Task DoAsync()
        {
            try
            {
                result.Clear();

                /*webBrowser1.Document.AttachEventHandler("onpropertychange", new EventHandler((ss, ee) => {
                }));*/
                var config = Configuration.Default.WithDefaultLoader();
                var context = BrowsingContext.New(config);
                string contenttext = webBrowser1.Document.Body.InnerHtml;
                var document = await context.OpenAsync(req => req.Content(contenttext));
                foreach (var comp in FieldPat)
                {
                    var tags = document.QuerySelectorAll(comp.pattern);
                    if (tags != null)
                    {
                        foreach (var tag in tags)
                        {
                            var content = comp.getContent(tag);
                            if (!result.ContainsKey(comp.field))
                            {
                                result.Add(comp.field, content);
                            }
                            else
                            {
                                result[comp.field] += "||" + content;
                            }
                        }
                    }
                }

                timer1.Enabled = false;
                timer1.Enabled = true;
            }
            catch (Exception ex)
            {

            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webBrowser1.Url.PathAndQuery == "blank") return;
            DoAsync();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (current >= count - 1)
            {
                webBrowser1.Navigate("");
            }
            else
            {
                current++;
                webBrowser1.Navigate(urllist[current]);
                listBox1.SelectedIndex = -1;
                listBox1.SelectedIndex = current;
            }
            richTextBox3.Invoke(new Action(() => { SetValue(result); }));
            timer1.Enabled = false;
        }
    }
}
