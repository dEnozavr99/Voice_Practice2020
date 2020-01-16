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

        List<float> list = new List<float>();
        List<short> list1 = new List<short>();

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
                float max = 0;

                var buffer = new WaveBuffer(e.Buffer);
                // interpret as 32 bit floating point audio
                for (int index = 0; index < e.BytesRecorded; index += 2)
                {
                    short sample = (short)((e.Buffer[index + 1] << 8) |
                                            e.Buffer[index + 0]);
                    // to floating point
                    var sample32 = sample / 32768f;
                    list.Add(sample32);
                    // absolute value 
                    if (sample32 < 0) sample32 = -sample32;
                    // is this the max value?
                    if (sample32 > max) max = sample32;
                }
                //list.Add(max);
            }
        }

        //stop recording 
        void StopRecording()
        {
            MessageBox.Show("Stop recording");
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
                list.Clear();
                chart1.Series["Series1"].Points.Clear();
                MessageBox.Show("Start recording");
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
                waveIn.WaveFormat = new WaveFormat(8000, 1);
                //initialithing object WaveFileWritter
                writer = new WaveFileWriter(outputFileName, waveIn.WaveFormat);
                //begin of the record
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
                isRecording = false;
                StopRecording();
            }
            
        }

        SoundPlayer Player = null;

        private void button3_Click(object sender, EventArgs e)
        {
            chart1.Series["Series1"].Points.Clear();
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


        }

        void OnDataAvailable(object sender, WaveInEventArgs args)
        {
            //if (isRecording)
            //{
            //    writer.Write(args.Buffer, 0, args.BytesRecorded);
            //}

            
            //for (int index = 0; index < args.BytesRecorded / 4; index++)
            //{
            //    var sample = buffer.FloatBuffer[index];

            //    // absolute value 
            //    if (sample < 0) sample = -sample;
            //    // is this the max value?
            //    if (sample > max) max = sample;
            //}

        }
    }
}
