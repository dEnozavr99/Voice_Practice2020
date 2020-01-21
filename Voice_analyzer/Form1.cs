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


namespace Voice_analyzer
{
    public partial class Form1 : Form
    {
        int sampleRate = 44100;

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
                timer1.Enabled = true;
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
            timer1.Enabled = false;
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

        private void fft(double[] input, double[] output)
        {
            for (int i = 0; i <= input.Length - 1; i++)
            {
                double sum = 0;
                
                for (int j = 0; j <= input.Length - 1; j++)
                {
                    sum = input[j] * Math.Pow(Math.E, (-TwoPi * i * j) / input.Length);
                }
                output[i] = sum;
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

        //
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

            double hemming = (0.54 - 0.46 * Math.Cos((2 * 3.14 * Math.Sqrt(FurieOut.Length)) / (FurieOut.Length - 1)));
            for (int i = 0; i <= arrayLenth - 1; i++)
            {
                list[i] *= hemming;
            }
            //FFTAnalysis(list.ToArray(), FurieOut, list.Count - 1, FurieOut.Length - 1);
            fft(list.ToArray(), FurieOut);
            int zeroInd = Array.IndexOf(FurieOut, 0);
            FurieOut = FurieOut.Where((val, idx) => idx != zeroInd).ToArray();
            double[] frequencyes = new double[FurieOut.Length];
            //for (int i = 0; i <= FurieOut.Length / 2; i++)
            //{
            //    frequencyes[i] = dw * i;
            //    chart1.Series["Series1"].Points.AddXY(frequencyes[i], FurieOut[i]);
            //}
            int ind = 0;
            frequencyes[ind] = dw * ind;
            while (frequencyes[ind] <= sampleRate / 2)
            {
                chart1.Series["Series1"].Points.AddXY(frequencyes[ind], FurieOut[ind]);
                ind++;
                frequencyes[ind] = dw * ind;
            }
            listFreq = frequencyes.ToList();
        }

        int milisecounds = 0;

        private void timer1_Tick(object sender, EventArgs e)
        {
            milisecounds++;
            toolStripStatusLabel2.Text = milisecounds.ToString();
        }

        

        //private double Hm(int n, int N)
        //{
        //    return 0.54 - 0.46 * Math.Cos((2 * Math.PI * n) / (N - 1));
        //}

        //public void Hemming(int intervalCount, int size)
        //{
        //    int t = 0;
        //    for (int i = 0; i <= intervalCount - 1; i++)
        //    {
        //        t = 0;
        //        for (int j = size * i / intervalCount; j <= size * (i + 1) / intervalCount - 1; j++)
        //        {
        //            Amplitude[j] *= Hm(t, size / intervalCount);
        //            t++;
        //        }
        //    }
        //}

    }
}
