using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebScraper
{
    public partial class WebScraper : Form

    {
        public string inputString;
        private HttpClient _client;
        public List<string> list;
        public string data;
        public byte[] imageBytes;
        public string path;
        public string fileExtension;
        public string extensionSubstring;
        public int count = 0;
        public int fileCount = 0;

        public WebScraper()
        {
            InitializeComponent();
        }

        public  void textBox1_TextChanged(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                
                button1_Click_1(this, null);
                
            }
        }

        public async void button1_Click_1(object sender, EventArgs e)
        {

            inputString = textBox1.Text;
            listBox1.Items.Clear();
            try { 
            await CallURL(inputString).ConfigureAwait(true);
            foreach (var item in list)
            {
                listBox1.Items.Add(item);
            }

                textBox1.Clear();
            }
            catch {

                System.Windows.Forms.MessageBox.Show(@"Please use ""https:// at the start "" eg ""https://""www.test.com");
                

            }
        }

        public async void button2_Click(object sender, EventArgs e)
        {
            var folderBrowserDialog1 = new FolderBrowserDialog();


            DialogResult result = folderBrowserDialog1.ShowDialog();
            path = folderBrowserDialog1.SelectedPath;
            if (result == DialogResult.OK)
            {
                foreach (var i in list)
                {
                    try
                    {
                        ///
                        extensionSubstring = i;
                        int lastDot = extensionSubstring.LastIndexOf('.');
                        fileExtension = extensionSubstring.Substring(lastDot, 4);
                        Task<Task<byte[]>> urlBytes = Task.WhenAny(FetchImage(i));
                        Task<byte[]> processedBytes = await urlBytes;
                        byte[] imageBytes = await processedBytes;
                        fileCount++;
                        label1.Text = "Number of files downloaded: "+ fileCount;
                        label1.Update();

                        await GenerateImage(imageBytes);
                    }
                    catch
                    {

                    }
                }



            }
        }


       
        public async Task<byte[]> FetchImage(string url)
        {



            return await _client.GetByteArrayAsync(url).ConfigureAwait(false);

            
        }

        public async Task GenerateImage(byte[] imageBytes) { 

            string filename_initial = path + @"\image";
            string filename_current = filename_initial;
            while (File.Exists(filename_current))
            {
                count++;
                filename_current = Path.GetDirectoryName(filename_initial)
                                 + Path.DirectorySeparatorChar
                                 + Path.GetFileNameWithoutExtension(filename_initial)
                                 + count.ToString()
                                 + Path.GetExtension(fileExtension);
                //split på . sista värdet i url i samma for each där url läses in och lägg in i path.GetExtension
            }
            using (FileStream fs = File.Open(filename_current, FileMode.OpenOrCreate))
            {
                fs.Seek(0, SeekOrigin.End);
                await fs.WriteAsync(imageBytes, 0, imageBytes.Length);
            }


    }
        public async Task<List<string>> CallURL(String url)
        {
            _client = new HttpClient();
            HttpResponseMessage response = await _client.GetAsync(url)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode(); // 200 response
            string responseBody = await response.Content.ReadAsStringAsync();
            List<string> listValues = seperateValues(responseBody);
            return listValues;
        }

        public List<string> seperateValues(string s)
        {
            list = new List<string>();
            string regexImgSrc = @"<img[^>]*?src\s*=\s*[""']?([^'"" >]+?)[ '""][^>]*?>";
            MatchCollection collection =
                Regex.Matches(s, regexImgSrc, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match item in collection)
            {

                string reference = item.Groups[1].Value;
                list.Add(reference);
            }
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Contains("http://") || list[i].Contains("https://"))
                {
                    continue;
                }
                else
                {
                    list[i] = inputString + list[i];
                }
            }
            return list;

            
        }
        
       
    }
}

