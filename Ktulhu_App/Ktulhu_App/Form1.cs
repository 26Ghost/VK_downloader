using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Net;
using System.IO;
using Microsoft.Win32;

namespace Ktulhu_App
{
    public partial class Form1 : Form
    {
        public int app_id = 2738973;
        public int count_user = 0;
        public int count_search = 0;
        //public List<string> inf = new List<string>();
        public List<string> to_down = new List<string>();
        public List<string> friends_inf = new List<string>();
        public string save_to_string;
        public string download_from_string;
        public int to_down_count = 0;
        public bool select_mode = false;
        public long bytes_down = 0;
        public string save_path;
        public string user = null;
        public Point lyrics_loc_id = new Point(0,0);
        public string access = null;
        public List<string> fail_to_download = new List<string>();
        float last_bytes_down_update=0;
        public SortOrder vl_sort_order = SortOrder.None;
        //public List<string> search_inf = new List<string>();
          
        //public List<string> friend_down_inf = new List<string>();
        public ImageList photos = new ImageList();

        public int search_count = 0;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Visible = false;
            toolStripStatusLabel3.Visible = false;

            toolStripProgressBar1.Visible = false;

            toolStripLabel1.Visible = false;
            toolStripLabel2.Visible = false;
            toolStripLabel3.Visible = false;
            notifyIcon1.Icon = SystemIcons.WinLogo;
            //notifyIcon1.ShowBalloonTip(1000,"Ахтунг","Ктулху Фхтагн",ToolTipIcon.Warning);
            webBrowser1.ScriptErrorsSuppressed = true;
            
            if (Registry.CurrentUser.OpenSubKey("Software\\Classes\\MIME\\Database\\Content type\\application/json") == null)
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey("Software\\Classes\\MIME\\Database\\Content Type\\application/json");
                key.SetValue("CLSID", "{25336920-03F9-11cf-8FD0-00AA00686F13}");
                key.SetValue("Encoding", 0x00080000);
            }
            webBrowser1.Navigate("http://api.vk.com/oauth/logout");
            while (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
                Application.DoEvents();

            webBrowser1.Navigate(String.Format("http://api.vk.com/oauth/authorize?client_id={0}&scope={1}&display=popup&response_type=token", app_id, 10));
            while (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
                Application.DoEvents();
            
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url.ToString().IndexOf("access_token") != -1)
            {
                webBrowser1.Hide();
                int begin = e.Url.ToString().IndexOf("&expires_in");
                int fe = e.Url.ToString().IndexOf("access_token=");
                int se = e.Url.ToString().IndexOf("&expires_in=");
                string str = e.Url.ToString().Substring(fe + 13, se - fe - 13);
                access = str;
                fe = e.Url.ToString().IndexOf("user_id=");
                string str2 = e.Url.ToString().Substring(fe + 8);
                user = str2;
                //audio(str, str2,listView1, inf);
                audio(str, str2, listView1, new List<string>());
                toolStripStatusLabel1.Text = "Всего записей: " + count_user.ToString();
                notifyIcon1.Text = "Ktulhu Audio Downloader \n" + "Всего записей: " + count_user.ToString();
                toolStripStatusLabel1.Visible = true;
            }
        }
        private void audio(string str, string str2, ListView lv, List<string> list_inf)
        {
            //checkedListBox1.Show();
            XmlDocument res = new XmlDocument();
            if (lv == listView2)
            {
                int sort_type = 2;
                if (radioButton1.Checked)
                    sort_type = 2;
                else if (radioButton2.Checked)
                    sort_type = 1;
                else if (radioButton3.Checked)
                    sort_type = 0;
                res.Load(String.Format("https://api.vkontakte.ru/method/audio.search.xml?access_token={0}&q={1}&sort={2}&count=300&lyrics={3}", access, textBox1.Text, sort_type, Convert.ToInt32(checkBox1.Checked)));
            }
            else //if (lv == listView1)
                res.Load(String.Format("https://api.vkontakte.ru/method/audio.get.xml?access_token={0}&uid={1}", str, str2));

            int fe, se, contrl_count, i = 0;


            string wtf = res.InnerXml.ToString();
            string wtf2 = null;
            if (wtf.IndexOf("<error_msg>Access denied: user deactivated</error_msg>") == -1)
            {
                while (wtf.Length > 93 && wtf.IndexOf("<audio>")!=-1)
                {
                    wtf2 = wtf.Substring(wtf.IndexOf("<audio>"), wtf.IndexOf("</audio>") - wtf.IndexOf("<audio>") + 8).Replace("&amp;#39;", "'");
                    if (wtf2.Length > "<audio></audio>".Length)
                    {
                        fe = wtf2.IndexOf("<artist>");
                        se = wtf2.IndexOf("</artist>");
                        list_inf.Add(wtf2.Substring(fe + 8, se - fe - 8));

                        fe = wtf2.IndexOf("<title>");
                        se = wtf2.IndexOf("</title>");
                        list_inf.Add(wtf2.Substring(fe + 7, se - fe - 7));

                        fe = wtf2.IndexOf("<duration>");
                        se = wtf2.IndexOf("</duration>");
                        list_inf.Add(wtf2.Substring(fe + 10, se - fe - 10));

                        fe = wtf2.IndexOf("<url>");
                        se = wtf2.IndexOf("</url>");
                        list_inf.Add(wtf2.Substring(fe + 5, se - fe - 5));

                        fe = wtf2.IndexOf("<lyrics_id>");
                        se = wtf2.IndexOf("</lyrics_id>");
                        if (fe != -1)
                            list_inf.Add(wtf2.Substring(fe + 11, se - fe - 11));
                        else
                            list_inf.Add(null);


                        i++;
                        list_inf.Add(i.ToString());
                    }
                    fe = wtf.IndexOf("<audio>");
                    se = wtf.IndexOf("</audio>");
                    wtf = wtf.Remove(fe, se - fe + 8);
                }
                contrl_count = i;
                int count = 0;
                for (i = 0; i < contrl_count * 6; i += 6)
                {
                    lv.Items.Add(new ListViewItem(new string[] { list_inf[i + 5], list_inf[i], list_inf[i + 1], (Convert.ToInt32(list_inf[i + 2]) / 3600).ToString().PadLeft(2, '0') + ":" + (Convert.ToInt32(list_inf[i + 2]) % 3600 / 60).ToString().PadLeft(2, '0') + ":" + ((Convert.ToInt32(list_inf[i + 2]) % 60) % 60).ToString().PadLeft(2, '0'), list_inf[i+3],list_inf[i+4] }));
                    //checkedListBox1.Items.AddRange(new object[] { (count + 1).ToString().PadRight(4) + inf[i].PadRight(40, '.') + inf[i + 1].PadRight(40, '.') + Convert.ToInt32(inf[i + 2]) / 60 + ":" + (Convert.ToInt32(inf[i + 2]) % 60).ToString().PadLeft(2, '0') });

                    //count_user++;
                    count++;
                }
                if (lv == listView1)
                    count_user = count;
                else if (lv == listView2)
                    count_search = count;

                lv.FullRowSelect = true;
                lv.Visible = true;
                lv.BringToFront();
            }
            else
                MessageBox.Show("Страница пользователя удалена или заблокирована", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Visible = false;
            toolStripStatusLabel3.Visible = false;
            listView1.Visible = false;
            toolStripProgressBar1.Visible = false;

            toolStripLabel1.Visible = false;
            toolStripLabel2.Visible = false;
            toolStripLabel3.Visible = false;

            //checkedListBox1.Hide();
            //checkedListBox1.Items.Clear();
            access = null;
            listView1.Items.Clear();
            listView2.Items.Clear();
            listView3.Items.Clear();
            friends_inf.Clear();

            count_user = 0;
            count_search = 0;

            to_down.Clear();
            friends_inf.Clear();
            save_to_string = null;
            download_from_string = null;
            to_down_count = 0;
            select_mode = false;
            bytes_down = 0;
            save_path = null;
            user = null;
            lyrics_loc_id = new Point(0, 0);
            access = null;
            fail_to_download.Clear();
            last_bytes_down_update = 0;
            vl_sort_order = SortOrder.None;
            //public List<string> search_inf = new List<string>();

            //public List<string> friend_down_inf = new List<string>();
            photos.Images.Clear();

            search_count = 0;
            for (int i = 3; i < tabControl1.TabPages.Count; i++)
                tabControl1.TabPages[i].Dispose();

            webBrowser1.Show();
            //inf.Clear();
            to_down.Clear();
            count_user = 0;
            webBrowser1.Navigate("http://api.vk.com/oauth/logout");
            while (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
                Application.DoEvents();
            webBrowser1.Navigate(String.Format("http://api.vk.com/oauth/authorize?client_id={0}&scope={1}&display=popup&response_type=token", app_id, 8));
            while (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
                Application.DoEvents();

        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            //Form1.ActiveForm.Close();
            //Form1.ActiveForm.ShowInTaskbar = false;
            Form1.ActiveForm.Hide();
            this.WindowState = FormWindowState.Minimized;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            
            //if ((count_user !=0 && tabControl1.SelectedIndex==0) || (count_search!=0 && tabControl1.SelectedIndex==1))
            ListView lv = (ListView)tabControl1.TabPages[tabControl1.SelectedIndex].Controls[0];

            if (tabControl1.SelectedIndex != 2 && lv.SelectedItems.Count!=0)
            {
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    save_path = folderBrowserDialog1.SelectedPath;
                    int count = 0;
                    //ListView lv = new ListView();
                    //List<string> list_inf = new List<string>();
                    //if (tabControl1.SelectedIndex == 0)
                    //{
                       // count = count_user;
                        //lv = listView1;
                        //list_inf = inf;
                    //}
                    //else if (tabControl1.SelectedIndex == 1)
                    //{
                        count = lv.Items.Count;
                        //lv = listView2;
                        //list_inf = search_inf;
                    //}
                    //else
                       // list_inf = friend_down_inf;

                    for (int i = 0; i < lv.Items.Count; i++)
                    {
                        //if (checkedListBox1.GetItemChecked(i))
                        if(lv.Items[i].Selected)
                        {
                           // to_down.Add(inf[(i + 1) * 6 - 6]);
                           // to_down.Add(inf[(i + 1) * 6 - 5]);
                            //to_down.Add(inf[(i + 1) * 6 - 3]); //uri

                            //to_down.Add(list_inf[Convert.ToInt32(lv.Items[i].SubItems[0].Text) * 6 - 6]);
                            //to_down.Add(list_inf[Convert.ToInt32(lv.Items[i].SubItems[0].Text) * 6 - 5]);
                            //to_down.Add(list_inf[Convert.ToInt32(lv.Items[i].SubItems[0].Text) * 6 - 3]);
                           
                            to_down.Add(lv.Items[i].SubItems[1].Text);
                            to_down.Add(lv.Items[i].SubItems[2].Text);
                            to_down.Add(lv.Items[i].SubItems[4].Text); 
                            to_down_count++;
                        }
                    }
                    if (to_down_count != 0)
                    {
                        download();
                        toolStripStatusLabel1.Visible = true;
                        toolStripStatusLabel3.Visible = true;

                        toolStripProgressBar1.Visible = true;

                        toolStripLabel1.Visible = true;
                        toolStripLabel2.Visible = true;
                        toolStripLabel3.Visible = true;
                    }
                    else
                        MessageBox.Show("Ни одна запись не выбрана", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            
                }
            }
            else
                MessageBox.Show("Ни одна запись не выбрана", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            
        }
        public void download()
        {
            toolStripButton2.Enabled = false;
            toolStripButton4.Enabled = false;
            toolStripButton5.Enabled = false;
            //toolStripButton6.Enabled = false;
            //checkedListBox1.Enabled = false;
            //listView1.Enabled = false;
            WebClient wc = new WebClient();
            wc.DownloadFileAsync(new Uri(to_down[2]), save_path.ToString() + @"\" + to_down[0] + ' ' + to_down[1] + ".mp3");            

            wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wc_DownloadProgressChanged);
            wc.DownloadFileCompleted += new AsyncCompletedEventHandler(wc_DownloadFileCompleted);
           
        }
        
        void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {


            if (bytes_down == last_bytes_down_update || bytes_down == 0)
            {
                notifyIcon1.ShowBalloonTip(2000, "Ошибка загрузки", to_down[0] + " " + to_down[1] + "\n файл не был загружен", ToolTipIcon.Error);
                fail_to_download.Add("-- " +to_down[0] + " " + to_down[1] + "\n");
            }
            to_down.RemoveAt(0);
            to_down.RemoveAt(0);
            to_down.RemoveAt(0);
            last_bytes_down_update = bytes_down;

            if (to_down.Count != 0)
                download();
            else
            {
                toolStripStatusLabel3.Text = (to_down_count - (to_down.Count + 1) / 3).ToString() + " из " + to_down_count.ToString() + " скачено";
                //MessageBox.Show("Загрузка окончена \n " + (Convert.ToDouble(bytes_down) / (2 * 1024 * 1024)).ToString() + "  Mb скачено" + to_down_count.ToString());
                if (bytes_down != 0)
                {
                    string failed = null;
                    if (fail_to_download.Count != 0)
                    {
                        failed += "\n Однако при загрузке слудующих файлов \nпроизошла ошибка, и они не были скачены: \n";
                        foreach (string str in fail_to_download)
                            failed += str;
                    }
                    notifyIcon1.ShowBalloonTip(3000, "Загрузка окончена", "Загружено " + (to_down_count-fail_to_download.Count).ToString() + " записей \n Скачено " + (Convert.ToDouble(bytes_down) / (2 * 1024 * 1024)).ToString().Substring(0, 6) + "  Mb"+failed, ToolTipIcon.Info);

                }
                to_down_count = 0;
                last_bytes_down_update = 0;
                fail_to_download.Clear();
                toolStripProgressBar1.Value = 0;
                bytes_down = 0;

                //toolStripStatusLabel1.Visible = false;

                toolStripStatusLabel3.Visible = false;

                toolStripProgressBar1.Visible = false;
                
                toolStripLabel1.Visible = false;
                toolStripLabel2.Visible = false;
                toolStripLabel3.Visible = false;
                for ( int i=0;i<listView1.Items.Count;i++)
                {
                    listView1.Items[i].Selected = false;
                    //checkedListBox1.SetItemChecked(items, false);
                }
                
                toolStripButton2.Enabled = true;
                toolStripButton4.Enabled = true;
                toolStripButton5.Enabled = true;
                //toolStripButton6.Enabled = true;
                //checkedListBox1.Enabled = true;
                listView1.Enabled = true;
            }
        }
        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            
            toolStripLabel1.Text = to_down[0].ToString() + " - " + to_down[1].ToString();
            toolStripLabel2.Text = e.BytesReceived.ToString() + "/" + e.TotalBytesToReceive + "  bytes downloaded";
            toolStripStatusLabel3.Text = (to_down_count - (to_down.Count + 1) / 3).ToString() + " из " + to_down_count.ToString() + " скачено";
            toolStripProgressBar1.Visible = true;
            toolStripProgressBar1.Value = e.ProgressPercentage;
            toolStripLabel3.Text = e.ProgressPercentage.ToString() + " %";

            if (e.ProgressPercentage == 100)
                bytes_down += e.TotalBytesToReceive;
                
            
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex != 2)
            {
                tabControl1.SelectedTab.Controls[0].Select();
                ListView lv = (ListView)tabControl1.TabPages[tabControl1.SelectedIndex].Controls[0];
                for (int i = 0; i < lv.Items.Count; i++)
                {
                    //checkedListBox1.SetItemChecked(i, true);
                    lv.Items[i].Selected = true;
                }
            }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex != 2)
            {
                ListView lv = (ListView)tabControl1.TabPages[tabControl1.SelectedIndex].Controls[0];
                for (int i = 0; i < lv.Items.Count; i++)
                {
                    //checkedListBox1.SetItemChecked(i, true);
                    lv.Items[i].Selected = false;
                }
            }
            
        }


        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Написал Частов Антон на досуге ;)");
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Visible = false;
            toolStripStatusLabel3.Visible = false;
            toolStripProgressBar1.Visible = false;
            toolStripLabel1.Visible = false;
            toolStripLabel2.Visible = false;
            toolStripLabel3.Visible = false;
            listView1.Hide();
            //checkedListBox1.Hide();
            listView1.Items.Clear();
            //checkedListBox1.Items.Clear();
            //inf.Clear();
            to_down.Clear();
            count_user = 0;

            webBrowser1.Navigate(String.Format("http://api.vk.com/oauth/authorize?client_id={0}&scope={1}&display=popup&response_type=token", app_id, 8));
            while (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
                Application.DoEvents();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            //if (this.WindowState == FormWindowState.Minimized)
            //{
            //    this.WindowState = FormWindowState.Normal;
            //    this.Show();
            //    this.BringToFront();
            //}
            //else
            //{
            //    this.Hide();
            //    this.WindowState = FormWindowState.Minimized;
            //}
        }

        private void закрытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void развернутьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
                this.Show();
            }
            else
            {
                this.WindowState = FormWindowState.Minimized;
                this.Hide();
            }
        }

        private void текстToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //ListView lv = new ListView();
            List<string> list_inf = new List<string>();
            //if (tabControl1.SelectedIndex == 0)
            //{
            //    lv = listView1;
            //    list_inf = inf;
            //}
            //else if (tabControl1.SelectedIndex == 1)
            //{
            //    lv = listView2;
            //    list_inf = search_inf;
            //}
            
            ListView lv = (ListView)tabControl1.TabPages[tabControl1.SelectedIndex].Controls[0];
            XmlDocument res = new XmlDocument();
            //res.Load(String.Format("https://api.vkontakte.ru/method/audio.getLyrics.xml?access_token={0}&lyrics_id={1}", access, list_inf[Convert.ToInt32(lv.GetItemAt(lyrics_loc_id.X, lyrics_loc_id.Y).SubItems[0].Text) * 6 - 2]));
            if (lv.GetItemAt(lyrics_loc_id.X, lyrics_loc_id.Y).SubItems[5] != null)
                res.Load(String.Format("https://api.vkontakte.ru/method/audio.getLyrics.xml?access_token={0}&lyrics_id={1}", access, lv.GetItemAt(lyrics_loc_id.X, lyrics_loc_id.Y).SubItems[5].Text));
           
            string wtf = res.InnerXml.ToString();
        

            if (wtf.IndexOf("</text>") != -1)
            {
                wtf = wtf.Substring(wtf.IndexOf("<text>") + 6, wtf.IndexOf("</text>") - wtf.IndexOf("<text>") - 6);

                //Form2 f2 = new Form2(wtf.Replace("&amp;#39;", "'").Replace("&amp;#33;","!"), inf[(listView1.GetItemAt(lyrics_loc_id.X, lyrics_loc_id.Y).Index + 1) * 6 - 6] + " - " + inf[(listView1.GetItemAt(lyrics_loc_id.X, lyrics_loc_id.Y).Index + 1) * 6 - 5]);
                Form2 f2 = new Form2(wtf.Replace("&amp;#39;", "'").Replace("&amp;#33;", "!"), lv.GetItemAt(lyrics_loc_id.X, lyrics_loc_id.Y).SubItems[1].Text + " - " + lv.GetItemAt(lyrics_loc_id.X, lyrics_loc_id.Y).SubItems[2].Text);

                f2.Show();
            }
            else MessageBox.Show( "К сожалению текст отсутствует",":(", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void скачатьToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.Item.Selected)
                e.Item.ForeColor=Color.Chartreuse;
            else
                e.Item.ForeColor = Color.Black;
        }

        private void listView1_MouseClick_1(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip2.Show(e.X + Form1.ActiveForm.Location.X + listView1.Location.X+tabControl1.Location.X, e.Y + Form1.ActiveForm.Location.Y + listView1.Location.Y + 20+tabControl1.Location.Y);
                lyrics_loc_id = e.Location;
            }
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            //теперь универсальный сортировщик, lv2 можн удалить к хуям
            ListView lv = (ListView)tabControl1.TabPages[tabControl1.SelectedIndex].Controls[0];
            int sort_type = 1;
            lv.BeginUpdate();
            lv.Columns[0].Text = "№";
            lv.Columns[1].Text = "Исполнитель"; 
            lv.Columns[2].Text = "Название";
            lv.Columns[3].Text = "Длительность";

            if (vl_sort_order == SortOrder.None)
            {
                sort_type = 1;
                vl_sort_order = SortOrder.Ascending;
                lv.Columns[e.Column].Text += "   ↑"; //▲";
            }
            else if (vl_sort_order == SortOrder.Ascending)
            {
                sort_type = -1;
                vl_sort_order = SortOrder.Descending;
                lv.Columns[e.Column].Text += "   ↓"; //▼";
            }
            else if (vl_sort_order == SortOrder.Descending)
            {
                sort_type = 1;
                vl_sort_order = SortOrder.Ascending;
                lv.Columns[e.Column].Text += "   ↑";
            }
            //if (listView1.Columns[e.Column].Text.Substring(listView1.Columns[e.Column].Text.Length) == ' ')
             //listView1.Columns[e.Column].Text=
            lv.ListViewItemSorter = new ListViewItemComparer(e.Column, sort_type);
            lv.EndUpdate();
        }

        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            if(notifyIcon1.BalloonTipText!="Поиск")
                System.Diagnostics.Process.Start("explorer.exe", save_path);
        }

        private void button_search_Click(object sender, EventArgs e)
        {
            if (access!=null&&textBox1.Text!=null)
            {
                tabControl1.SelectedIndex = 1;
                XmlDocument res = new XmlDocument();
                //res.Load(String.Format("https://api.vkontakte.ru/method/audio.search.xml?access_token={0}&q={1}&sort=2", access, textBox1.Text));
                int sort_type = 2;
                if (radioButton1.Checked)
                    sort_type = 2;
                else if (radioButton2.Checked)
                    sort_type = 1;
                else if (radioButton3.Checked)
                    sort_type = 0;
                res.Load(String.Format("https://api.vkontakte.ru/method/audio.search.xml?access_token={0}&q={1}&sort={2}&count=300&lyrics={3}", access, textBox1.Text, sort_type, Convert.ToInt32(checkBox1.Checked)));

                string wtf = res.InnerXml.ToString();

                wtf = wtf.Remove(0, wtf.IndexOf("<count>") + "<count>".Length);
                wtf = wtf.Remove(wtf.IndexOf("</count>"));
                //wtf = wtf.Substring(fe, se - fe + 8);
                //wtf = wtf.Remove(fe, se - fe + 8);
                //MessageBox.Show(wtf);
                if (wtf == "0")
                    notifyIcon1.ShowBalloonTip(1000, "Поиск", "Ничего не найдено", ToolTipIcon.Error);
                search_count = Convert.ToInt32(wtf);
                //search_inf.Clear();
                listView2.Items.Clear();
                tabControl1.SelectedTab.Text = "Поиск: " + textBox1.Text;
                audio(access, user, listView2, new List<string>());
            }
            else
            {
                MessageBox.Show("Сначала авторизируйтесь!", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void listView2_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip2.Show(e.X + Form1.ActiveForm.Location.X + listView1.Location.X + tabControl1.Location.X, e.Y + Form1.ActiveForm.Location.Y + listView1.Location.Y + 20 + tabControl1.Location.Y);
                lyrics_loc_id = e.Location;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (panel1.Visible == false)
            {
                panel1.Visible = true;
                panel1.BringToFront();
            }
            else panel1.Visible = false;
        }

        private void panel1_new_MouseLeave(object sender, EventArgs e)
        {
            //panel1.Visible = false;
            //panel1.BackColor = Color.Blue;
            //MessageBox.Show("exit");
        }

        private void panel1_MouseEnter(object sender, EventArgs e)
        {
            //panel1.BackColor = Color.Beige;
            //panel1.MouseLeave += new EventHandler(panel1_new_MouseLeave);
            //MessageBox.Show("enter");
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
 
        }

        private void listView2_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int sort_type = 1;
            listView2.BeginUpdate();
            listView2.Columns[0].Text = "№";
            listView2.Columns[1].Text = "Исполнитель";
            listView2.Columns[2].Text = "Название";
            listView2.Columns[3].Text = "Длительность";

            if (vl_sort_order == SortOrder.None)
            {
                sort_type = 1;
                vl_sort_order = SortOrder.Ascending;
                listView2.Columns[e.Column].Text += "   ▲";
            }
            else if (vl_sort_order == SortOrder.Ascending)
            {
                sort_type = -1;
                vl_sort_order = SortOrder.Descending;
                listView2.Columns[e.Column].Text += "   ▼";
            }
            else if (vl_sort_order == SortOrder.Descending)
            {
                sort_type = 1;
                vl_sort_order = SortOrder.Ascending;
                listView2.Columns[e.Column].Text += "   ▲";
            }
            //if (listView1.Columns[e.Column].Text.Substring(listView1.Columns[e.Column].Text.Length) == ' ')
            //listView1.Columns[e.Column].Text=
            this.listView2.ListViewItemSorter = new ListViewItemComparer(e.Column, sort_type);
            listView2.EndUpdate();
        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }


        private void tabPage3_Enter(object sender, EventArgs e)
        {
            if (listView3.Items.Count == 0)
            {
                XmlDocument res = new XmlDocument();
                res.Load(string.Format("https://api.vkontakte.ru/method/friends.get.xml?access_token={0}&fields={1}", access, "uid,first_name,last_name,nickname,photo"));

                string wtf = res.InnerXml.ToString().Substring(res.InnerXml.ToString().IndexOf("<user>"));
                wtf = wtf.Remove(wtf.IndexOf("</response>"));

                string wtf2 = null;
                int fe, se;
                string name = null;

                while (wtf.Length > 0)
                {
                    wtf2 = wtf.Substring(wtf.IndexOf("<user>"), wtf.IndexOf("</user>") - wtf.IndexOf("<user>") + "</user>".Length);
                    fe = wtf2.IndexOf("<uid>");
                    se = wtf2.IndexOf("</uid>");
                    friends_inf.Add(wtf.Substring(fe + 5, se - fe - 5));

                    fe = wtf2.IndexOf("<first_name>");
                    se = wtf2.IndexOf("</first_name>");
                    name = wtf2.Substring(fe + "<first_name>".Length, se - fe - "<first_name>".Length) + " ";

                    fe = wtf2.IndexOf("<last_name>");
                    se = wtf2.IndexOf("</last_name>");
                    name += wtf2.Substring(fe + "<last_name>".Length, se - fe - "<last_name>".Length);
                    friends_inf.Add(name);

                    fe = wtf2.IndexOf("<photo>");
                    se = wtf2.IndexOf("</photo>");
                    friends_inf.Add(wtf.Substring(fe + "<photo>".Length, se - fe - "<photo>".Length));
                    //online нахрен
                    //photos.ImageSize = new Size(50, 50);
                    //photos.Images.Add(Image.FromStream(WebRequest.Create(wtf.Substring(fe + "<photo>".Length, se - fe - "<photo>".Length)).GetResponse().GetResponseStream()));
                    fe = wtf.IndexOf("<user>");
                    se = wtf.IndexOf("</user>");
                    wtf = wtf.Remove(fe, se - fe + 7);

                    //res.Load(String.Format("https://api.vkontakte.ru/method/audio.getCount.xml?access_token={0}&oid={1}",access, friends_inf[friends_inf.Count - 3]));
                    //listView3.Items.Add(new ListViewItem(new string[] { friends_inf[friends_inf.Count - 2], res.InnerXml.ToString().Substring(res.InnerXml.IndexOf("<response>") + "<response>".Length, res.InnerXml.IndexOf("</response>")-res.InnerXml.IndexOf("<response>") - "<response>".Length) },photos.Images.Count-1 ));
                }
                listView3.SmallImageList = photos;
                listView3.BeginUpdate();
                if (MessageBox.Show("Загрузить изображение контактов\nи инф. о кол-ве записей пользователей?\nЭто займет некоторое время", " ", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    photo_load();
                else
                    friends_load();

                listView3.EndUpdate();
            }
        }
        private void friends_load()
        {
            if (listView3.Columns.Count==2)
             listView3.Columns.RemoveAt(1);
            //XmlDocument res = new XmlDocument();
            for (int i = 0; i < friends_inf.Count; i += 3)
            {
                //res.Load(String.Format("https://api.vkontakte.ru/method/audio.getCount.xml?access_token={0}&oid={1}", access, friends_inf[i]));
                listView3.Items.Add(new ListViewItem(new string[] { friends_inf[i + 1]}));
            }
        }
        private void photo_load()
        {
            if (listView3.Columns.Count==1)
                listView3.Columns.Add("Количесво записей", 60);

            XmlDocument res = new XmlDocument();
            photos.ImageSize = new Size(35, 35);
            photos.ColorDepth = ColorDepth.Depth32Bit;
            panel2.Visible = true;
            panel2.BringToFront();
            label2.Refresh(); 
            for (int i = 0; i < friends_inf.Count; i+=3)
            {
                
                photos.Images.Add(Image.FromStream(WebRequest.Create(friends_inf[i+2]).GetResponse().GetResponseStream()));
                res.Load(String.Format("https://api.vkontakte.ru/method/audio.getCount.xml?access_token={0}&oid={1}",access, friends_inf[i]));
                listView3.Items.Add(new ListViewItem(new string[] { friends_inf[i+1], res.InnerXml.ToString().Substring(res.InnerXml.IndexOf("<response>") + "<response>".Length, res.InnerXml.IndexOf("</response>") - res.InnerXml.IndexOf("<response>") - "<response>".Length) }, photos.Images.Count-1));
                progressBar1.Value = 100 * i / friends_inf.Count;
            }
            panel2.Visible = false;
        }

        private void listView3_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (listView3.GetItemAt(e.X, e.Y).Index!=0)
                    if (listView3.GetItemAt(e.X, e.Y).Text == listView3.Items[listView3.GetItemAt(e.X, e.Y).Index - 1].Text)
                        for (int i = listView3.GetItemAt(e.X, e.Y).Index - 1; i < listView3.Items.Find(listView3.GetItemAt(e.X, e.Y).Text, false).Count() + listView3.GetItemAt(e.X, e.Y).Index + 1; i++)
                        {
                            if (tabControl1.TabPages["TabPage" + listView3.GetItemAt(e.X, e.Y).Text] != null)
                                if (tabControl1.TabPages["TabPage" + listView3.GetItemAt(e.X, e.Y).Text].Text == listView3.GetItemAt(e.X, e.Y).Text)
                                    tabControl1.TabPages["TabPage" + listView3.GetItemAt(e.X, e.Y).Text].Text += " " + (i - listView3.GetItemAt(e.X, e.Y).Index + 1).ToString();
                            friends_inf[friends_inf.IndexOf(listView3.GetItemAt(e.X, e.Y).Text)] += " " + (i - listView3.GetItemAt(e.X, e.Y).Index + 1).ToString();
                            listView3.Items[i].Text += " " + (i - listView3.GetItemAt(e.X, e.Y).Index + 1).ToString();
                        }
                tabControl1.TabPages.Add("TabPage" + listView3.GetItemAt(e.X, e.Y).Text, listView3.GetItemAt(e.X, e.Y).Text);
                tabControl1.TabPages[tabControl1.TabPages.Count-1].MouseClick += new MouseEventHandler(TabPage_MouseClick);
                ColumnHeader ch1 = new ColumnHeader();
                ch1.Text = "№";
                ColumnHeader ch3 = new ColumnHeader();
                ch3.Text = "Название";
                ch3.Width = 170;
                ColumnHeader ch2 = new ColumnHeader();
                ch2.Text = "Исполнитель";
                ch2.Width = 170;
                ColumnHeader ch4 = new ColumnHeader();
                ch4.Text = "Длительность";
                
                ListView lv = new ListView();
                lv.BackColor = System.Drawing.SystemColors.InactiveBorder;
                lv.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { ch1,ch2,ch3,ch4});
                lv.Dock = System.Windows.Forms.DockStyle.Fill;
                lv.Location = new System.Drawing.Point(3, 3);
                lv.Name = "ListView" + listView3.GetItemAt(e.X, e.Y).Text;
                lv.Size = new System.Drawing.Size(564, 307);
                lv.TabIndex = 0;
                lv.UseCompatibleStateImageBehavior = false;
                lv.View = System.Windows.Forms.View.Details;
                lv.HideSelection = false;
                lv.MouseClick += new MouseEventHandler(listView1_MouseClick_1);
                lv.ColumnClick += new ColumnClickEventHandler(listView1_ColumnClick);
                tabControl1.TabPages["TabPage" + listView3.GetItemAt(e.X, e.Y).Text].Controls.Add(lv);
                //audio(access, friends_inf[(listView3.GetItemAt(e.X, e.Y).Index) *3], (ListView)tabControl1.TabPages["TabPage" + listView3.GetItemAt(e.X, e.Y).Text].Controls[0], new List<string>());
                audio(access,  friends_inf[friends_inf.IndexOf(listView3.GetItemAt(e.X, e.Y).Text)-1], (ListView)tabControl1.TabPages["TabPage" + listView3.GetItemAt(e.X, e.Y).Text].Controls[0], new List<string>());
                
            }
        }

        void TabPage_MouseClick(object sender, MouseEventArgs e)
        {
            //throw new NotImplementedException();
            if (MouseButtons == MouseButtons.Right)
                tabControl1.TabPages[tabControl1.SelectedIndex].Dispose();
        }

        private void tabControl1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                for (int i = 3; i < tabControl1.TabCount; i++)
                {
                    Rectangle rect = tabControl1.GetTabRect(i);
                    if (rect.Contains(e.Location))
                    {
                        //MessageBox.Show(i.ToString());
                        tabControl1.SelectedIndex = i;
                        contextMenuStrip3.Show(e.X+this.Location.X+tabControl1.Location.X, e.Y+this.Location.Y+tabControl1.Location.Y);
                        
                    }
                }
            }
        }

        private void закрытьToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab.Dispose();
        }
       
    }
}
