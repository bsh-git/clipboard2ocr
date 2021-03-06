using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using GoogleVisionApi;

namespace clipboard2ocr
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
			apiKey = Program.ReadApiKey();

			AppendDebugMessage(String.Format("API key={0}", ApiKey));
			
        }

        private string apiKey;
		public string ApiKey { get { return apiKey; } }

		private string textFile;

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripTextBox2_Click(object sender, EventArgs e)
        {

        }

        private async void loadLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("load");
            DialogResult result;
            
            result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) {
                pictureBox1.ImageLocation = openFileDialog1.FileName;
                pictureBox1.Load();

				await CallVisionApiTask(new GoogleVisionApiSession(openFileDialog1.FileName, ApiKey));
            }

        }

       // private void CallVisionApi(String filename)
        //{
          //  CallVisionApiTask(filename);
        //}

        private async Task CallVisionApiTask(GoogleVisionApiSession sess)
        {
			await sess.PostAsync();
			var result = await  sess.GetResult();
			if (result == null) {
				// XXX
				AppendErrorMessage("API session not finished");
			}
			else if (result.Succeeded) {
                String text = result.GetText();

                foreach (var line in text.Split(new Char[] { '\n' }))
                {
                    textBox1.AppendText(line);
                    textBox1.AppendText("\r\n");
                }
			}
			else if (result.Error != null)
				AppendErrorMessage(String.Format("Http status={0} error code={1} status={2} message={3}",
												 result.HttpStatusCode,
												 result.Error.Code,
												 result.Error.Status,
												 result.Error.Message));
			else {
				AppendErrorMessage(String.Format("Http error: {0}", result.HttpStatusCode));
				AppendDebugMessage(result.HttpErrorMessage);
			}
		}

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("save as");
        }

		
        private async void LoadClipboardToolStripMenuItem(object sender, EventArgs e)
		{
			await LoadClipboard();
		}
			
        private async Task LoadClipboard()
        {
            //var source = System.Windows.Clipboard.GetImage();
            IDataObject data = Clipboard.GetDataObject();
            if (data.GetDataPresent(DataFormats.Bitmap))
            {
				Image image = (Image)data.GetData(DataFormats.Bitmap);
                pictureBox1.Image = image;
                using (MemoryStream ms = new MemoryStream())
                {
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
					await CallVisionApiTask(new GoogleVisionApiSession(ms.ToArray(), ApiKey));
                }
            }

        }

		



		private void AppendDiagnosticMessage(string tag, string msg)
		{
			foreach (var line in msg.Split(new Char[] {'\n'})) {
				textBox1.AppendText(tag);
				textBox1.AppendText(line);
				textBox1.AppendText("\r\n");
			}
		}

		public void AppendErrorMessage(string msg)
		{
			AppendDiagnosticMessage("[Error] ", msg);
		}
		
		public void AppendDebugMessage(string msg)
		{
			AppendDiagnosticMessage("[Debug] ", msg);
		}

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using(Form2 dialog = new Form2())
            {
				dialog.ShowSettingsDialog(this);
            }
        }

		public void UpdateApiKey(string newkey)
		{
			apiKey = newkey;
			Program.UpdateApiKey(newkey);
		}

		public void SaveTextToFile(string filename)
		{
			try {
				using (StreamWriter s = new StreamWriter(filename)) {
					s.Write(textBox1.Text);
					s.Close();
				}
			}
			catch (Exception e) {
				AppendErrorMessage(e.Message);
			}
		}

        private void saveTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textFile == null)
                saveTextAsToolStripMenuItem_Click(sender, e);
            else
                SaveTextToFile(textFile);
        }

        private void saveTextAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog2.ShowDialog() == DialogResult.OK) {
				textFile = openFileDialog2.FileName;
				SaveTextToFile(textFile);
			}
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await LoadClipboard();
        }
    }
}
