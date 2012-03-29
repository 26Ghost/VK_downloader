using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

namespace Ktulhu_App
{
    [Serializable]
    public partial class Form2 : Form
    {
        public Form2(string text,string zag)
        {           
            InitializeComponent();
            linkLabel1.Text = zag;
            richTextBox1.Text = text;
            this.Text="Lyrics: "+zag;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(richTextBox1.Text.Replace("\n", "\r\n"), TextDataFormat.UnicodeText);
            //Clipboard.SetData(DataFormats., richTextBox1.Text);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            fontDialog1.ShowDialog();
            richTextBox1.Font = fontDialog1.Font;
            //richTextBox1.Font.Bold=fontDialog1.Font.Bold
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "txt files(*.txt)|*.txt";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.FileName = this.Text.Replace(':', '-');
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Stream ms = saveFileDialog1.OpenFile();
                if (ms != null)
                {
                    ms.Write(UTF8Encoding.UTF8.GetBytes(richTextBox1.Text.Replace("\n", "\r\n")), 0, UTF8Encoding.UTF8.GetBytes(richTextBox1.Text).Length);
                    ms.Close();
                }
            }
        }

    }
}
