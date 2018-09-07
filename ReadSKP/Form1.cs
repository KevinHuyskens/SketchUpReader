using System;
using System.Windows.Forms;

namespace ReadSKP
{
    public partial class Form1 : Form
    {
        private SketchUpReader skpReader = new SketchUpReader();
        private UdpClient client = new UdpClient();

        public Form1()
        {
            InitializeComponent();
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);
        }

        void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                textBox1.AppendText("Reading SKP ... \n");
                try
                {
                    if (!checkBox1.Checked)
                    {
                        skpReader.loadSkp(file);
                        textBox1.AppendText("Vertice count: " + skpReader.verticeCount + "\n");
                    }
                    else
                    {
                        var newPath = @"C:\Users\huyskke\Desktop\NewSKP.skp";
                        if (SketchUpReader.ReformatModel(file, "2017", newPath))
                        {
                            MessageBox.Show("Loading: " + newPath);
                            skpReader.loadSkp(newPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }
    }
}
