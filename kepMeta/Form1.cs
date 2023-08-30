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

namespace kepMeta
{
    public partial class Form1 : Form
    {
        private string eleres;
        private List<string> filePaths ;
        private List<string> negativak;
        private string[] imageArray;
        private string talalt;
        Bitmap bmp;
        Bitmap bmp1;
        Bitmap bmp2;
        Bitmap bmp3;
        bool egyezik;
        bool egyedi;
        public Form1()
        {
            InitializeComponent();
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
                kepkivetel();
                kepEgyezesViszgalat();
                MessageBox.Show("Kész a müvelet");
            }
        }
        public void kepkivetel()
        {
            negativak = filePaths;
            for (int i = 0; i < negativak.Count(); i++)
            {
                using (Image tesztkep = new Bitmap(negativak[i])) {
                    try
                    {
                        //van metaadat
                        var userComment = Encoding.UTF8.GetString(tesztkep.GetPropertyItem(0x9286).Value);
                        if (userComment != null)
                        {
                            //a megadott comment van törlés
                            negativak.RemoveAt(i);
                        }

                    }
                    catch
                    {

                    }
                }
            }
        }
        public void kepEgyezesViszgalat()
        {
            if (negativak.Count > 0)
            {
                for (int i = 0; i < negativak.Count; i++)
                {
                    //optimalizálás
                    using ( bmp = new Bitmap(negativak[i])) {
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
                                listBox1.Items.Add(negativak[i] + "|" + filePaths[j]);
                                listBox1.Refresh();
                                egyedi = false;
                            }
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
                }
                listBox1.SelectedIndexChanged += new EventHandler(listBox1_SelectedIndexChanged);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                talalt = listBox1.SelectedItem.ToString();
                string[] subs = talalt.Split('|');
                if (File.Exists(subs[0]) && File.Exists(subs[1]))
                {
                    pictureBox1.Image = Image.FromFile(subs[0]);
                    pictureBox2.Image = Image.FromFile(subs[1]);
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
            if (File.Exists(mappa))
            {

            }
            else {
                Directory.CreateDirectory(mappa);
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
                    File.Delete(subs[0]);
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
                    File.Delete(subs[1]);
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
    }
}
