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
        Stack<double> s = new Stack<double>();
        string beepSoundFileName = @"beep.wav";

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
                writer.Write(e.Buffer, 0, e.BytesRecorded);

                var buffer = new WaveBuffer(e.Buffer);
                // interpret as 16 bit floating point audio

                for (int index = 0; index < e.BytesRecorded; index += 2)
                {

                    short sample = (short)((e.Buffer[index + 1] << 8) |
                                            e.Buffer[index + 0]);
                    // to floating point
                    var sample32 = sample / 32768f;
                    //if (sam                           ple32 > 0.1 || sample32 < -0.1)
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
            StreamReader file = new StreamReader(path);
            int counter = 0;

            while ((tempLine = file.ReadLine()) != null)
            {
                string[] nums = tempLine.Split(delimetr);
                samples[counter] = Array.ConvertAll(nums, Double.Parse);
                counter++;
            }

        }

        double[][] framedSound;

        double[] h = new double[12]
        { 300, 517.33, 781.90, 1103.97, 1496.04, 1973.32, 2554.33, 3261.62, 4122.63, 5170.76, 6446.70, 8000};

        double MinOf(params double[] p)
        {
            double min = double.MaxValue;

            for (int i = 0; i < p.Length; i++)
            {
                if (p[i] < min)
                {
                    min = p[i];
                }
            }

            return min;
        }

        private void toolStripSplitButton1_ButtonClick(object sender, EventArgs e)
        {
            chart1.Series["Series1"].Points.Clear();
            textBox1.Text = "";

            int arrayLenth = nextPowerOf2(list.Count);
            double dw = sampleRate * 1.0 / arrayLenth * 1.0;
            double[] FurieOut = new double[arrayLenth];

            for (int i = list.Count - 1; i <= arrayLenth - 1; i++)
            {
                list.Add(0);
            }

            //entropia
            

            double hemming = 0;
            // змінив довжину фрейма з 512 на 128
            const int FrameSize = 128;
            // змінив знаходження кількості фреймів
            //int height = list.Count / (FrameSize / 2) - 1;
            int height = (list.Count / FrameSize) * 2 - 1;

            framedSound = new double[height][];

            k = 0;

            for (int i = 0; i < height; i++)
            {
                framedSound[i] = new double[FrameSize];

                for (int j = 0; j < FrameSize; j++)
                {
                    //framedSound[i][j] = list[k];

                    //if (i % 2 == 0)
                    //{
                    //    chart1.Series[0].Points.AddXY(k, framedSound[i][j] + 1);
                    //}
                    //else
                    //{
                    //    chart1.Series[1].Points.AddXY(k, framedSound[i][j] - 1);
                    //}
                    if (k == 0)
                    {
                        framedSound[i][j] = list[k];
                    }
                    else
                        framedSound[i][j] = list[k] - 0.97 * list[k - 1];
                    k++;
                }
                k -= FrameSize / 2;
            }

            chart1.Series["Series1"].Points.Clear();
            double[] entropia = new double[height];
            for (int i = 0; i < height; i++)
            {
                double E1 = 0;
                for (int j = 0; j < FrameSize; j++)
                {
                    E1 += framedSound[i][j] * Math.Log(Math.Abs(framedSound[i][j]), 2);
                }
                entropia[i] = E1;
                chart1.Series["Series1"].Points.AddY(E1);
            }
            int stop1 = 0;
            int slovo = 0;

            double[][][] newframedSound = new double[10][][];

            while (stop1 < height - 1)
            {
                if (Math.Abs(entropia[stop1]) < 0.01) { stop1++; }
                else
                {
                    int stop2 = 0;
                    int rozmir = 0;
                    int shym = 0;
                    while (stop2 < 5)
                    {
                        if (stop1 == height - 1) { break; }
                        stop2 = 0;
                        if (Math.Abs(entropia[stop1]) < 0.01)
                        {
                            for (int i = stop1; i < stop1 + 5; i++)
                            {
                                if (Math.Abs(entropia[stop1]) < 0.01) { stop2++; }
                            }
                        }
                        if (Math.Abs(entropia[stop1]) < 1) { shym++; }
                        rozmir++;
                        stop1++;
                    }
                    if ((rozmir > 8) && (rozmir != shym))
                    {
                        newframedSound[slovo] = new double[height][];
                        for (int i = stop1 - rozmir; i <= stop1; i++)
                        {
                            newframedSound[slovo][i] = new double[FrameSize];
                            for (int j = 0; j < FrameSize; j++)
                            {

                                newframedSound[slovo][i][j] = framedSound[i][j];
                            }
                        }
                        slovo++;
                    }
                }
            }

            textBox1.Text = slovo.ToString() + " ";

            for (int word = 0; word < slovo; word++)
            {
                //this.Refresh();
                //MessageBox.Show("Signal framed");
                //chart1.Series[0].Points.Clear();
                //chart1.Series[1].Points.Clear();
                for (int i = 0; i < height; i++)
                {
                    int starterPos = newframedSound[word][i].Length;
                    int newSubArrSize = nextPowerOf2(newframedSound[word][i].Length);
                    Array.Resize(ref newframedSound[word][i], newSubArrSize);

                    for (int j = starterPos; j < newSubArrSize; j++)
                    {
                        newframedSound[word][i][j] = 0;
                    }
                }

                double[][] framedSpectr = new double[height][];

                for (int i = 0; i < height; i++)
                {
                    framedSpectr[i] = new double[FrameSize];

                    for (int j = 0; j < FrameSize; j++)
                    {
                        hemming = 0.54 - 0.46 * Math.Cos(2 * Math.PI * j / (FrameSize - 1));
                        //framedSound[i][j] *= hemming;
                        newframedSound[word][i][j] *= hemming;
                    }
                    //FFTAnalysis(framedSound[i], framedSpectr[i], FrameSize, FrameSize);
                    FFTAnalysis(newframedSound[word][i], framedSpectr[i], FrameSize, FrameSize);

                    //if (i % 2 == 0)
                    //{
                    //    chart1.Series[0].Points.Add(framedSpectr[i]);
                    //}
                    //else
                    //{
                    //    chart1.Series[1].Points.Add(framedSpectr[i]);
                    //}
                }

                //this.Refresh();
                //MessageBox.Show("Spectr builted");
                //chart1.Series[0].Points.Clear();
                //chart1.Series[1].Points.Clear();

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

                double[] deltha = new double[filtersCount];
                double[] delthaDeltha = new double[filtersCount];

                for (int i = 0; i < filtersCount; i++)
                {
                    if (i < 2)
                    {
                        deltha[i] = Cosinusi[i + 2];
                    }
                    else if (i + 2 < filtersCount)
                    {
                        deltha[i] = Cosinusi[i + 2] - Cosinusi[i - 2];
                    }
                    else
                    {
                        deltha[i] = -Cosinusi[i - 2];
                    }
                }

                for (int i = 0; i < filtersCount; i++)
                {
                    if (i < 2)
                    {
                        delthaDeltha[i] = deltha[i + 2];
                    }
                    else if (i + 2 < filtersCount)
                    {

                        delthaDeltha[i] = deltha[i + 2] - deltha[i - 2];
                    }
                    else
                    {

                        delthaDeltha[i] = -deltha[i - 2];
                    }
                }

                if (radioButton2.Checked)
                {
                    using (StreamWriter sw = new StreamWriter(comboBox1.SelectedItem.ToString(), false, Encoding.Default))
                    {
                        for (int i = 0; i < filtersCount; i++)
                        {
                            if (i != filtersCount - 1)
                                sw.Write(Cosinusi[i].ToString() + " ");
                            else
                                sw.Write(Cosinusi[i].ToString());
                        }
                        sw.WriteLine();

                        for (int i = 0; i < filtersCount; i++)
                        {
                            if (i != filtersCount - 1)
                                sw.Write(deltha[i].ToString() + " ");
                            else
                                sw.Write(deltha[i].ToString());
                        }
                        sw.WriteLine();

                        for (int i = 0; i < filtersCount; i++)
                        {
                            if (i != filtersCount - 1)
                                sw.Write(delthaDeltha[i].ToString() + " ");
                            else
                                sw.Write(delthaDeltha[i].ToString());
                        }
                    }

                    System.Diagnostics.Process.Start("notepad.exe", comboBox1.SelectedItem.ToString());
                }

                if (radioButton1.Checked)
                {
                    double[] comparison = new double[3];
                    for (int WordsCounter = 0; WordsCounter < comparison.Length; WordsCounter++)
                    {

                        readFromFile(comboBox1.Items[WordsCounter].ToString());

                        double[] template = new double[30];
                        int iter1 = 0, iter2 = 0;
                        for (int i = 0; i < template.Length; i++)
                        {
                            template[i] = samples[iter1][iter2];

                            if (i == 9 || i == 19)
                            {
                                iter1++;
                            }

                            if (i != 9 && i != 19)
                            {
                                iter2++;
                            }
                            else
                            {
                                iter2 = 0;
                            }
                        }

                        double[] inputed = new double[30];
                        iter1 = 0;
                        iter2 = 0;
                        int iter3 = 0;

                        for (int i = 0; i < inputed.Length; i++)
                        {
                            if (i <= 9)
                            {
                                inputed[i] = Cosinusi[iter1];
                                iter1++;
                            }
                            else if (i <= 19)
                            {
                                inputed[i] = deltha[iter2];
                                iter2++;
                            }
                            else
                            {
                                inputed[i] = delthaDeltha[iter3];
                                iter3++;
                            }
                        }

                        int n = template.Length;
                        int m = inputed.Length;
                        double[,] d = new double[n, m];
                        double[,] D = new double[n, m];
                        Stack<double> w = new Stack<double>();

                        for (int i = 0; i < n; i++)
                        {
                            for (int j = 0; j < m; j++)
                            {
                                d[i, j] = Math.Abs(template[i] - inputed[j]);
                            }
                        }

                        D[0, 0] = d[0, 0];

                        for (int i = 1; i < n; i++)
                        {
                            D[i, 0] = d[i, 0] + D[i - 1, 0];
                        }

                        for (int j = 1; j < m; j++)
                        {
                            D[0, j] = d[0, j] + D[0, j - 1];
                        }

                        for (int i = 1; i < n; i++)
                        {
                            for (int j = 1; j < m; j++)
                            {
                                if (D[i - 1, j - 1] <= D[i - 1, j])
                                {
                                    if (D[i - 1, j - 1] <= D[i, j - 1])
                                    {
                                        D[i, j] = d[i, j] + D[i - 1, j - 1];
                                    }
                                    else
                                    {
                                        D[i, j] = d[i, j] + D[i, j - 1];
                                    }
                                }
                                else if (D[i - 1, j] <= D[i, j - 1])
                                {
                                    D[i, j] = d[i, j] + D[i - 1, j];
                                }
                                else
                                {
                                    D[i, j] = d[i, j] + D[i, j - 1];
                                }

                            }
                        }

                        int i1 = n - 1, j1 = m - 1;
                        double element = D[i1, j1];

                        w.Push(D[i1, j1]);

                        do
                        {
                            if (i1 > 0 && j1 > 0)
                            {
                                if (D[i1 - 1, j1 - 1] <= D[i1 - 1, j1])
                                {
                                    if (D[i1 - 1, j1 - 1] <= D[i1, j1 - 1])
                                    {
                                        i1--;
                                        j1--;
                                    }
                                    else
                                        j1--;
                                }
                                else if (D[i1 - 1, j1] <= D[i1, j1 - 1])
                                {
                                    i1--;
                                }
                                else
                                {
                                    j1--;
                                }
                            }
                            else if (i1 == 0)
                            {
                                j1--;
                            }
                            else
                            {
                                i1--;
                            }
                            w.Push(D[i1, j1]);
                        }
                        while (i1 != 0 || j1 != 0);

                        double sum = 0f;
                        foreach (double tempVar in w)
                        {
                            sum += tempVar;
                        }

                        comparison[WordsCounter] = sum /= w.Count;
                    }

                    if (comparison[0] == MinOf(comparison))
                    {
                        textBox1.Text += "Привіт";
                    }
                    else if (comparison[1] == MinOf(comparison))
                    {
                        textBox1.Text += "Вася";
                    }
                    else if (comparison[2] == MinOf(comparison))
                    {
                        textBox1.Text += "Знайди";
                    }
                }
            }
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