using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using NAudio.Wave;
using NAudio.FileFormats;
using NAudio.CoreAudioApi;
using NAudio;
using System.Media;
using System.Linq;

namespace Voice_analyzer
{
    public partial class Form1 : Form
    {
        int sampleRate = 16000;

        double TwoPi = Math.PI * 2;
        //recording thread
        WaveIn waveIn;
        //record to file class
        WaveFileWriter writer;
        //Name of the recording file
        string outputFileName = "test.wav";
        //active recording flasg
        bool isRecording = false;
        //array for amplitude
        float[] amplitude;

        string beepSoundFileName = @"C:\Users\Admin\Desktop\Learning\Pract\Voice\Voice_analyzer\Voice_analyzer\bin\Debug\beep.wav";

        List<double> list = new List<double>();
        List<double> listFreq = new List<double>();

        int it = 0;
        

        int k = 0;
        public Form1()
        {
            InitializeComponent();
        }

        //getting data form incoming buffer
        void waveIn_DataAvaible(object sender, WaveInEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<WaveInEventArgs>(waveIn_DataAvaible), sender, e); 
            }
            else
            {
                //writting incoming data into a file
                writer.WriteData(e.Buffer, 0, e.BytesRecorded);
                

                var buffer = new WaveBuffer(e.Buffer);
                // interpret as 16 bit floating point audio
                
                for (int index = 0; index < e.BytesRecorded; index += 2)
                {
                    
                    short sample = (short)((e.Buffer[index + 1] << 8) |
                                            e.Buffer[index + 0]);
                    // to floating point
                    var sample32 = sample / 32768f;
                    if (sample32 > 0.1 || sample32 < -0.1)
                        list.Add(sample32);
                    
                }
                
            }
        }

        //stop recording 
        void StopRecording()
        {
            //MessageBox.Show("Stop recording");
            waveIn.StopRecording();
        }

        //finishing writting down

        private void waveIn_RecordingStopped(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler(waveIn_RecordingStopped), sender, e);
            }
            else
            {
                waveIn.Dispose();
                waveIn = null;
                writer.Close();
                writer = null;
            }
        }

        //start recording by button pressed event
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (Player != null)
                {
                    Player.Stop();
                    Player.Dispose();
                    Player = null;
                }
                Player = new SoundPlayer(beepSoundFileName);


                list.Clear();
                chart1.Series["Series1"].Points.Clear();
                //MessageBox.Show("Start recording");
                isRecording = true;
                waveIn = new WaveIn();
                //if we have a default sound recording hardware
                //notebook`s micro has number 0
                waveIn.DeviceNumber = 0;

                //add a function to event DataAvaible , appeared when there are some incoming data
                waveIn.DataAvailable += waveIn_DataAvaible;
                //add a function for ending record
                waveIn.RecordingStopped += new EventHandler<StoppedEventArgs>(waveIn_RecordingStopped);
                //wav-file format setting parametrs dyskr frequency and chanels count
                waveIn.WaveFormat = new WaveFormat(sampleRate, 1);
                //initialithing object WaveFileWritter
                writer = new WaveFileWriter(outputFileName, waveIn.WaveFormat);
                //begin of the record
                Player.Play();
                waveIn.StartRecording();
                
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
    
            if (waveIn != null)
            {
                if (Player != null)
                {
                    Player.Stop();
                    Player.Dispose();
                    Player = null;
                }
                Player = new SoundPlayer(beepSoundFileName);

                isRecording = false;
                StopRecording();
                Player.Play();
            }
            
        }
        //create player, which will play a wave file
        SoundPlayer Player = null;

        //playback recorded audio
        private void button3_Click(object sender, EventArgs e)
        {
            
            chart1.Series["Series1"].Points.Clear();
            toolStripStatusLabel1.Text = list.Count.ToString();
            foreach (var i in list)
            {
                chart1.Series["Series1"].Points.AddY(i);
            }

            if (Player != null)
            {
                Player.Stop();
                Player.Dispose();
                Player = null;
            }

            Player = new SoundPlayer(outputFileName);
            Player.Play();
            chart1.SaveImage("testSound.bmp", System.Windows.Forms.DataVisualization.Charting.ChartImageFormat.Bmp);
            comboBox1.SelectedIndex = 0;
        }

        
        //method of fast furie transform
        void FFTAnalysis(double[] AVal, double[] FTvl, int Nvl, int Nft)
        {
            int i, j, n, m, Mmax, Istp;
            double Tmpr, Tmpi, Wtmp, Theta;
            double Wpr, Wpi, Wr, Wi;
            double[] Tmvl;

            n = Nvl * 2; Tmvl = new double[n];

            for (i = 0; i < n; i += 2)
            {
                Tmvl[i] = 0;
                Tmvl[i + 1] = AVal[i / 2];
            }

            i = 1; j = 1;
            while (i < n)
            {
                if (j > i)
                {
                    Tmpr = Tmvl[i]; Tmvl[i] = Tmvl[j]; Tmvl[j] = Tmpr;
                    Tmpr = Tmvl[i + 1]; Tmvl[i + 1] = Tmvl[j + 1]; Tmvl[j + 1] = Tmpr;
                }
                i = i + 2; m = Nvl;
                while ((m >= 2) && (j > m))
                {
                    j = j - m; m = m >> 1;
                }
                j = j + m;
            }

            Mmax = 2;
            while (n > Mmax)
            {
                Theta = -TwoPi / Mmax; Wpi = Math.Sin(Theta);
                Wtmp = Math.Sin(Theta / 2); Wpr = Wtmp * Wtmp * 2;
                Istp = Mmax * 2; Wr = 1; Wi = 0; m = 1;

                while (m < Mmax)
                {
                    i = m; m = m + 2; Tmpr = Wr; Tmpi = Wi;
                    Wr = Wr - Tmpr * Wpr - Tmpi * Wpi;
                    Wi = Wi + Tmpr * Wpi - Tmpi * Wpr;

                    while (i < n)
                    {
                        j = i + Mmax;
                        Tmpr = Wr * Tmvl[j] - Wi * Tmvl[j - 1];
                        Tmpi = Wi * Tmvl[j] + Wr * Tmvl[j - 1];

                        Tmvl[j] = Tmvl[i] - Tmpr; Tmvl[j - 1] = Tmvl[i - 1] - Tmpi;
                        Tmvl[i] = Tmvl[i] + Tmpr; Tmvl[i - 1] = Tmvl[i - 1] + Tmpi;
                        i = i + Istp;
                    }
                }

                Mmax = Istp;
            }

            for (i = 0; i < Nft; i++)
            {
                j = i * 2; FTvl[i] = 2 * Math.Sqrt(Math.Pow(Tmvl[j], 2) + Math.Pow(Tmvl[j + 1], 2)) / Nvl;
            }
        }

       

        //method for finding next power of two from n
        private int nextPowerOf2(int n)
        {
            int count = 0;

            if (n > 0 && (n & (n - 1)) == 0)
                return n;

            while (n != 0)
            {
                n >>= 1;
                count += 1;
            }

            return 1 << count;
        }

        double[][] samples = new double[3][];

        void readFromFile(string path)
        {
            char delimetr = ' ';
            string tempLine = "";
            StreamReader file = new System.IO.StreamReader(path);
            int counter = 0;

            while((tempLine = file.ReadLine()) != null)
            {
                string[] nums = tempLine.Split(delimetr);
                samples[counter] = Array.ConvertAll<string, double>(nums, Double.Parse);
                counter++;
            }

        }

        //
        double[][] framedSound;
        double[] h = new double[12] 
        { 300, 517.33, 781.90, 1103.97, 1496.04, 1973.32, 2554.33, 3261.62, 4122.63, 5170.76, 6446.70, 8000};

        private void toolStripSplitButton1_ButtonClick(object sender, EventArgs e)
        {
            chart1.Series["Series1"].Points.Clear();

            int arrayLenth = nextPowerOf2(list.Count);
            double dw = sampleRate * 1.0 / arrayLenth * 1.0;
            double[] FurieOut = new double[arrayLenth];
            
            for (int i = list.Count - 1; i <= arrayLenth - 1; i++)
            {
                list.Add(0);
            }


           
            double hemming = 0;

            //FFTAnalysis(list.ToArray(), FurieOut, list.Count - 1, FurieOut.Length - 1);

            //for (int i = 0; i <= FurieOut.Length - 1; i++)
            //{
            //    hemming = (0.54 - 0.46 * Math.Cos((2 * 3.14 * i) / (FurieOut.Length - 1)));
            //    FurieOut[i] *= hemming;
            //}

            //int height = 63;
            //int FrameSize = (2 * list.Count) / (height + 1);

            int FrameSize = 512;
            int height = list.Count / (FrameSize / 2) - 1;

            framedSound = new double[height][];
            for (int i = 0; i < height; i++)
            {
                framedSound[i] = new double[FrameSize];
                for (int j = 0; j < FrameSize; j++)
                {
                    framedSound[i][j] = list[k];
                    k++;
                }
                k -= 256;
            }

            double[][] framedSpectr = new double[height][];
            for (int i = 0; i < height; i++)
            {
                framedSpectr[i] = new double[FrameSize];
               
                for (int j = 0; j < FrameSize; j++)
                {
                    hemming = 0.54 - 0.46 * Math.Cos(2 * Math.PI * j / (FrameSize - 1));
                    framedSound[i][j] *= hemming;
                }
                FFTAnalysis(framedSound[i], framedSpectr[i], FrameSize, FrameSize);
            }

            int[] f = new int[h.Length];
            for (int i = 0; i < h.Length; i++)
            {
                f[i] = (int)Math.Floor((FrameSize + 1) * h[i] / sampleRate);
            }

            int filtersCount = 10;
            double[,] Hmk = new double[filtersCount, FrameSize];

            for (int m = 1; m <= filtersCount; m++)
            {
                for (int k1 = 0; k1 < FrameSize; k1++)
                {
                    if (k1 <= f[m] && k1 >= f[m - 1])
                    {
                        Hmk[m - 1, k1] = (k1 - f[m - 1]) / (f[m] - f[m - 1]);
                    }
                    else if (k1 > f[m] && k1 <= f[m + 1])
                    {
                        Hmk[m - 1, k1] = (f[m + 1] - k1) / (f[m + 1] - f[m]);
                    }
                    else
                    {
                        Hmk[m - 1, k1] = 0;
                    }
                }
            }


            double[] s = new double[filtersCount];

           
            for (int i1 = 0; i1 < filtersCount; i1++)
            {
                double sum = 0;
                for (int j1 = 0; j1 < FrameSize; j1++)
                {
                     sum += Math.Pow(Math.Abs(framedSound[i1][j1]), 2) * Hmk[i1, j1];
                }
                s[i1] = Math.Log(sum);            
            }

            double[] Cosinusi = new double[filtersCount];

            for (int l = 0; l < filtersCount; l++)
            {
                double sum = 0;

                for (int m = 0; m < filtersCount; m++)
                {
                    sum = s[m] * Math.Cos(Math.PI * l * (m + 0.5) / filtersCount);
                }
                Cosinusi[l] = sum;
            }

            using (StreamWriter sw = new StreamWriter(comboBox1.SelectedItem.ToString(), true, Encoding.Default))
            {
                for (int i = 0; i < Cosinusi.Length; i++)
                {
                    string tmpstr = Cosinusi[i].ToString() + " ";
                    sw.Write(tmpstr);
                }
                sw.WriteLine();
            }
            //bool test = true;
            readFromFile(@"C:\Users\Admin\Desktop\Learning\Pract\Voice\Voice_analyzer\sample.txt");

            double min = 100000;
            int vid = -1;
            for (int i = 0; i < 3; i++)
            {
                double sum = 0;
                for (int j = 0; j < filtersCount; j++)
                {
                   sum += Math.Pow((samples[i][j] - Cosinusi[j]),2);
                }
               sum = Math.Sqrt(sum);
                if (min>sum) { vid = i; min = sum; }
            }

            if (vid == 0) { textBox1.Text = "Привiт"; }
           else if (vid == 1) { textBox1.Text = "Вася"; }
           else if (vid == 2) { textBox1.Text = "Знайди"; }
        }

    }
}
/*
 * нормування тривалості слова
 * 1 берем що всі слова із запасом 1с і ділимо на 100
 * взяти тестовий сигнал/ записати тестовий сигнал
 * спитати Юрія Степановича Мочульського
 * вибрати розмір фреймів 16(+/-8)мс
*/