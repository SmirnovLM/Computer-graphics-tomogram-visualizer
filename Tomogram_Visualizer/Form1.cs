using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tomogram_Visualizer
{
    public partial class Form1 : Form
    {
        enum MODE
        {
            Quads,
            Texture2D,
            QuadStrip
        }
        private MODE mode = MODE.Quads;

        private Bin bin;
        private View view;

        private bool loaded = false;
        private bool needReload = false;
        private int currentLayer = 0; // переменная, хранящая номер слоя для визуализации

        private int frameCount;
        private DateTime NextFPSUpdate = DateTime.Now.AddSeconds(1);

        private int min;
        private int width;

        public Form1() { InitializeComponent(); }

        void Application_Idle(object sender, EventArgs e)
        {
            while(glControl1.IsIdle)
            {
                displayFPS();
                glControl1.Invalidate();
            }
        }

        void displayFPS()
        {
            if (DateTime.Now >= NextFPSUpdate)
            {
                this.Text = String.Format("CT Visualizer (fps={0})", frameCount);
                NextFPSUpdate = DateTime.Now.AddSeconds(1);
                frameCount = 0;
            }
            frameCount++;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Application.Idle += Application_Idle;

            bin = new Bin();
            view = new View();

            radioButton1.Checked = true;

            min = trackBar2.Value;
            width = trackBar3.Value;
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string str = dialog.FileName;
                bin.ReadBIN(str);
                view.SetupView(glControl1.Width, glControl1.Height);
                loaded = true;
                glControl1.Invalidate();
                trackBar1.Maximum = Bin.Z - 1;

                view.find(currentLayer);
                trackBar2.Minimum = view.GetMinn();
                trackBar2.Maximum = view.GetMaxx();

                label5.Text = view.GetMinn().ToString();
                label6.Text = view.GetMaxx().ToString();
            }
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (loaded)
            {
                switch (mode) 
                {
                    case MODE.Quads:
                        view.DrawQuads(currentLayer, min, width);
                        glControl1.SwapBuffers();
                        break;

                    case MODE.Texture2D:
                        if (needReload)
                        {
                            view.generateTextureImage(currentLayer, min, width);
                            view.Load2DTexture();
                            needReload = false;
                        }
                        view.DrawQuads(currentLayer, min, width);
                        glControl1.SwapBuffers();
                        break;

                    case MODE.QuadStrip:
                        view.DrawQuadsStrip(currentLayer, min, width);
                        glControl1.SwapBuffers();
                        break;
                }
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            currentLayer = trackBar1.Value;
            needReload = true;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            min = trackBar2.Value;
            needReload = true;
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            width = trackBar3.Value;
            needReload = true;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            mode = MODE.Quads;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            mode = MODE.Texture2D;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            mode = MODE.QuadStrip;
        }

        
    }
}
