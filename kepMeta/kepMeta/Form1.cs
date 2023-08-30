using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Threading;

namespace kepMeta
{
    public partial class Form1 : Form
    {
        private string eleres;
        private List<string> filePaths;
        private List<string> negativak;
        private string[] imageArray;
        private string talalt;
        string filename;
        Bitmap bmp;
        Bitmap bmp1;
        Bitmap bmp2;
        Bitmap bmp3;
        bool egyezik;
        bool egyedi;
        BackgroundWorker bw = new BackgroundWorker();
        public Form1()
        {
            InitializeComponent();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = false;
            bw.DoWork += kepkivetel;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                //jpg és png képek megtalálása kezdet
                eleres = folderBrowserDialog1.SelectedPath;
                imageArray = Directory.GetFiles(eleres, "*.jpg");
                imageArray = imageArray.Concat(Directory.GetFiles(eleres, "*.png")).ToArray();
                filePaths = imageArray.OfType<string>().ToList();
                //jpg és png képek megtalálása vége
                folderCheckd(eleres);
                // kepkivetel();
                try
                {
                    //kepEgyezesViszgalat();
                    bw.RunWorkerAsync();
                }
                catch (OutOfMemoryException ex)
                {
                    ErrorList(eleres, ex.Message);
                    DeleteButtonOn();
                    GC.Collect();
                    MessageBox.Show("Hiba! Elfogyott a memoria.");
                }
            }
        }
        public void kepkivetel(object sender, EventArgs e)
        {
            negativak = new List<string>(filePaths);
            button4.Invoke((MethodInvoker)delegate {
                button4.Visible = true;
            });
            for (int i = 0; i < negativak.Count(); i++)
            {
                try
                {
                    using (Bitmap tesztkep = new Bitmap(negativak[i]))
                    {
                        //van metaadat
                        string userComment = Encoding.UTF8.GetString(tesztkep.GetPropertyItem(0x9286).Value);
                        if (!string.IsNullOrEmpty(userComment))
                        {
                            //a megadott comment van törlés
                            negativak.RemoveAt(i);
                            i--;
                        }
                    }
                }
                catch
                {

                }
                if (bw.WorkerSupportsCancellation == true)
                {
                    bw.CancelAsync();
                    break;
                }
            }
            progressBar1.Invoke((MethodInvoker)delegate {
                progressBar1.Maximum = negativak.Count;
            });
            if (bw.WorkerSupportsCancellation == false) {
                kepEgyezesViszgalat();
            }
        }
        public void kepEgyezesViszgalat()
        {
            if (negativak.Count > 0)
            {
                listBox1.SelectedIndexChanged += new EventHandler(listBox1_SelectedIndexChanged);
                for (int i = 0; i < negativak.Count; i++)
                {
                    //optimalizálás
                    using (bmp = new Bitmap(negativak[i])) {
                        bmp1 = new Bitmap(bmp, 16, 16);
                    }
                    egyedi = true;
                    for (int j = 0; j < filePaths.Count; j++)
                    {
                        if (negativak[i] != filePaths[j])
                        {
                            egyezik = true;
                            //optimalizálás
                            using (bmp2 = new Bitmap(filePaths[j]))
                            {
                                bmp3 = new Bitmap(bmp2, 16, 16);
                            }
                            for (int x = 0; x < 16; x++)
                            {
                                for (int y = 0; y < 16; y++)
                                {
                                    if (bmp1.GetPixel(x, y).ToString() != bmp3.GetPixel(x, y).ToString())
                                    {
                                        egyezik = false;
                                        break;
                                    }
                                }
                                if (!egyezik)
                                {
                                    break;
                                }
                            }
                            if (egyezik)
                            {
                                //kép találat listához hozzá adni
                                string[] negativpaths = negativak[i].Split('\\');
                                string[] filePathsName = filePaths[j].Split('\\');
                                listBox1.Invoke((MethodInvoker)delegate {
                                    listBox1.Items.Add(negativpaths.Last() + "|" + filePathsName.Last());
                                });
                                using (StreamWriter writer = new StreamWriter(filename, true))
                                {
                                    writer.Write(negativpaths.Last() + " | " + filePathsName.Last() + "\r");
                                }
                                egyedi = false;
                            }
                        }
                        if (bw.WorkerSupportsCancellation == true)
                        {
                            bw.CancelAsync();
                            break;
                        }
                    }
                    if (egyedi)
                    {
                        //nincs egyezés semmivel mehet a meta adat
                        using (bmp = new Bitmap(negativak[i]))
                        {
                            PropertyItem propItem = bmp.PropertyItems[0];
                            propItem.Id = 0x9286;  // this is called 'UserComment'
                            propItem.Type = 2;
                            propItem.Value = System.Text.Encoding.UTF8.GetBytes("Checked\0");
                            propItem.Len = propItem.Value.Length;
                            bmp.SetPropertyItem(propItem);
                            string name = Path.GetFileName(negativak[i]);
                            string mappa = eleres + @"\Checked\";
                            try
                            {
                                bmp.Save(mappa + name);
                            }
                            catch
                            {

                            }
                        }
                    }
                    progressBar1.Invoke((MethodInvoker)delegate {
                        progressBar1.Value += 1;
                    });
                    if (bw.WorkerSupportsCancellation == true)
                    {
                        break;
                    }
                }
            }
            button4.Invoke((MethodInvoker)delegate {
                button4.Visible = false;
            });
            bw.WorkerSupportsCancellation = true;
            DeleteButtonOn();
            MessageBox.Show("Kész a müvelet");
        }
        public void DeleteButtonOn()
        {
            button2.Invoke((MethodInvoker)delegate {
                button2.Visible = true;
            });
            button3.Invoke((MethodInvoker)delegate {
                button3.Visible = true;
            });
            button4.Invoke((MethodInvoker)delegate {
                button4.Visible = false;
            });
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                talalt = listBox1.SelectedItem.ToString();
                string[] subs = talalt.Split('|');
                if (File.Exists(eleres + "\\" + subs[0]) && File.Exists(eleres + "\\" + subs[1]))
                {
                    try
                    {
                        pictureBox1.Image = Image.FromFile(eleres + "\\" + subs[0]);
                        pictureBox2.Image = Image.FromFile(eleres + "\\" + subs[1]);
                    }
                    catch
                    {
                        listBox1.Items.Remove(listBox1.SelectedItem);
                        pictureBox1.Image = null;
                        pictureBox2.Image = null;
                        listBox1.Refresh();
                    }
                }
                else
                {
                    listBox1.Items.Remove(listBox1.SelectedItem);
                    listBox1.Refresh();
                }
            }
        }

        private void folderCheckd(string path)
        {
            string mappa = path + @"\Checked";
            if (!File.Exists(mappa))
            {
                Directory.CreateDirectory(mappa);
            }
            filename = mappa + @"\find.txt";
            if (!File.Exists(filename))
            {
                using (StreamWriter sw = File.CreateText(filename)) { };
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1) {
                talalt = listBox1.SelectedItem.ToString();
                string[] subs = talalt.Split('|');
                pictureBox1.Image.Dispose();
                listBox1.Items.Remove(listBox1.SelectedItem);
                try
                {
                    File.Delete(eleres + "\\" + subs[0]);
                }
                catch 
                {
                    MessageBox.Show("Hiba! A törlés nem sikerült");
                }
                pictureBox1.Image = null;
                pictureBox2.Image = null;
                pictureBox1.Refresh();
                pictureBox2.Refresh();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                talalt = listBox1.SelectedItem.ToString();
                string[] subs = talalt.Split('|');
                pictureBox2.Image.Dispose();
                listBox1.Items.Remove(listBox1.SelectedItem);
                try
                {
                    File.Delete(eleres + "\\" + subs[1]);
                }
                catch
                {
                    MessageBox.Show("Hiba! A törlés nem sikerült");
                }
                pictureBox1.Image = null;
                pictureBox2.Image = null;
                pictureBox1.Refresh();
                pictureBox2.Refresh();
            }
        }
        public void ErrorList(string path,string error) {
            string errorName = path + @"\Checked\error.txt";
            if (!File.Exists(errorName))
            {
                using (StreamWriter sw = File.CreateText(errorName)) {
                    sw.Write(error);
                };
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            bw.WorkerSupportsCancellation = true;
            DeleteButtonOn();
        }
    }
}