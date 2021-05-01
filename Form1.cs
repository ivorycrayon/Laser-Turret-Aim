using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Media;
using System.Speech.Recognition;
using System.Speech.Synthesis;

namespace Laser_Turret_Aim
{
    public partial class Form1 : Form
    {
        static System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();
        public Stopwatch servoWatch { get; private set; }
        public Stopwatch idleVoiceLinesWatch { get; private set; }

        SpeechRecognitionEngine _recognizer = new SpeechRecognitionEngine();
        SpeechSynthesizer voice = new SpeechSynthesizer();       

        int soundIndex = 0;
        int fireIndex = 0;
        bool loopdone = true;
        bool laserSwitch = true;
        string lastPos;
        int sensitivity = 10;

        System.Media.SoundPlayer hello = new System.Media.SoundPlayer(Laser_Turret_Aim.Properties.Resources.hellofriend);
        System.Media.SoundPlayer iSeeYou = new System.Media.SoundPlayer(Laser_Turret_Aim.Properties.Resources.iseeyou);
        System.Media.SoundPlayer stillThere = new System.Media.SoundPlayer(Laser_Turret_Aim.Properties.Resources.stillthere);
        System.Media.SoundPlayer comeOver = new System.Media.SoundPlayer(Laser_Turret_Aim.Properties.Resources.comehere);
        System.Media.SoundPlayer deploy = new System.Media.SoundPlayer(Laser_Turret_Aim.Properties.Resources.Turret_deploy);
        System.Media.SoundPlayer disabled = new System.Media.SoundPlayer(Laser_Turret_Aim.Properties.Resources.Turret_turret_disabled_4);
        System.Media.SoundPlayer retire = new System.Media.SoundPlayer(Laser_Turret_Aim.Properties.Resources.Turret_turret_retire_1);
        System.Media.SoundPlayer retract = new System.Media.SoundPlayer(Laser_Turret_Aim.Properties.Resources.Turret_retract);
        System.Media.SoundPlayer anyoneThere = new System.Media.SoundPlayer(Laser_Turret_Aim.Properties.Resources.Turret_turret_autosearch_5);
        System.Media.SoundPlayer fire1 = new System.Media.SoundPlayer(Laser_Turret_Aim.Properties.Resources.Turret_turret_fire_4x_01);
        System.Media.SoundPlayer fire2 = new System.Media.SoundPlayer(Laser_Turret_Aim.Properties.Resources.Turret_turret_fire_4x_02);
        System.Media.SoundPlayer fire3 = new System.Media.SoundPlayer(Laser_Turret_Aim.Properties.Resources.Turret_turret_fire_4x_03);

        List<SoundPlayer> idleSounds = new List<SoundPlayer>();
        List<SoundPlayer> fireSounds = new List<SoundPlayer>();

        List<int> anglesY = new List<int>();
        int angleX;

        public Form1()
        {
            InitializeComponent();
            InitializeTimer();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            servoWatch = Stopwatch.StartNew();
            idleVoiceLinesWatch = Stopwatch.StartNew();

            port.Open();

            _recognizer.SetInputToDefaultAudioDevice();

            _recognizer.LoadGrammarAsync(
                new Grammar(
                    new GrammarBuilder(
                        new Choices(new string[]{
                            "System on", "System off", "Move up", "Move down", "Move left", "Move right", "Sensitivity up", "Sensisitvity down", "Go loco" }))));

            _recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(Default_SpeechRecognized);
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);

            voice.SelectVoice("Microsoft Hazel Desktop");
        }
        private async void Default_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {           
            

            if (e.Result.Confidence > .80)
            {
                string speech = e.Result.Text;

                int xcoord = parseDataX(lastPos);
                int ycoord = parseDataY(lastPos);

                if (xcoord < 0) xcoord = 0;
                if (xcoord > 180) xcoord = 180;
                if (ycoord < 0) ycoord = 0;
                if (ycoord > 180) ycoord = 180;

                idleVoiceLinesWatch = Stopwatch.StartNew();

                switch (speech)
                {
                    case "System on":
                        On_Mode();
                        break;

                    case "System off":
                        Off_Mode();
                        break;

                    case "Move up":
                        lastPos = String.Format("X{0}Y{1}", xcoord, ycoord + sensitivity);
                        voice.SpeakAsync("Yes master");
                        port.Write(lastPos);
                        break;

                    case "Move down":
                        lastPos = String.Format("X{0}Y{1}", xcoord, ycoord - sensitivity);
                        voice.SpeakAsync("Yes master");
                        port.Write(lastPos);
                        break;

                    case "Move left":
                        lastPos = String.Format("X{0}Y{1}", xcoord + sensitivity, ycoord);
                        voice.SpeakAsync("Yes master");
                        port.Write(lastPos);
                        break;

                    case "Move right":
                        lastPos = String.Format("X{0}Y{1}", xcoord - sensitivity, ycoord);
                        voice.SpeakAsync("Yes master");
                        port.Write(lastPos);
                        break;

                    case "Sensitivity up":
                        sensitivity += 2;
                        voice.SpeakAsync($"Sensitivity is {sensitivity}");
                        break;

                    case "Sensitivity down":
                        if (sensitivity > 2) sensitivity -= 2;
                        voice.SpeakAsync($"Sensitivity is {sensitivity}");
                        break;

                    case "Go loco":
                        Go_Loco();

                        break;
                }
            }
            else
            {
                // ignore
            }            
        }

        private async void On_Mode()
        {            
            deploy.PlaySync();
            port.Write("ON"); 
            hello.Play();
            await Task.Delay(30);            
            port.Write("X90Y90");
            lastPos = "X90Y90";
        }

        private async void Off_Mode() //wtf how did i break this??
        {            
            retract.PlaySync();
            disabled.PlaySync();
            port.Write("OFF");
            retire.PlaySync();
            port.Write("X90Y180");
            lastPos = "X90Y180";
        }

        private async void Go_Loco()
        {
            var rand = new Random();

            for(int i = 0; i < 25; i++)
            {
                int xcoord = rand.Next(180);
                int ycoord = rand.Next(180);
                lastPos = String.Format("X{0}Y{1}", xcoord, ycoord);
                port.Write(lastPos);                
                await Task.Delay(85);
            }           

        }

        private async void Form1_Shown(object sender, EventArgs e)
        {
            On_Mode();
        }

        private async void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Off_Mode();
        }

        private async void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            fireSounds = new List<SoundPlayer>()
            {
                fire1,
                fire2,
                fire3
            };

            switch (e.Button)
            {
                case MouseButtons.Left:
                    fireSounds[fireIndex].Play();

                    if (fireIndex < 2)
                        fireIndex++;
                    else
                        fireIndex = 0;

                    break;

                case MouseButtons.Right:

                    if (laserSwitch)
                    {
                        await Task.Delay(25);
                        retract.Play();
                        await Task.Delay(25);
                        port.Write("OFF");

                        laserSwitch = false;
                    }
                    else
                    {
                        await Task.Delay(25);
                        deploy.Play();
                        await Task.Delay(25);
                        port.Write("ON");

                        laserSwitch = true;
                    }

                    break;

                case MouseButtons.Middle:

                    //flash laser mode maybe

                    break;
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async void Form1_MouseMove(object sender, MouseEventArgs e)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            writeToPort(new Point(e.X, e.Y));

            idleVoiceLinesWatch = Stopwatch.StartNew();
        }

        private int parseDataX(String data)
        {
            data = data.Remove(data.IndexOf("Y"));
            data = data.Remove(data.IndexOf("X"), 1);

            return int.Parse(data);
        }

        private int parseDataY(String data)
        {
            data = data.Remove(0, data.IndexOf("Y") + 1);

            return int.Parse(data);
        }

        public void writeToPort(Point coordinates)
        {
            int xcoord = (180 - coordinates.X / (Size.Width / 180));
            int ycoord = (180 - coordinates.Y / (Size.Height / 180));

            if (servoWatch.ElapsedMilliseconds > 15)
            {
                servoWatch = Stopwatch.StartNew();
                lastPos = String.Format("X{0}Y{1}", xcoord, ycoord);
                port.Write(String.Format("X{0}Y{1}", xcoord, ycoord));
            }
        }

        private void InitializeTimer()
        {
            timer1.Interval = 700;
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Enabled = true;
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            if (idleVoiceLinesWatch.ElapsedMilliseconds > 10000 && loopdone == true)
            {
                await idle();
            }
        }

        private async Task idle()
        {
            angleX = 0;
            string angle;

            loopdone = false;

            idleSounds = new List<SoundPlayer>()
            {
                stillThere,
                iSeeYou,
                anyoneThere,
                comeOver
            };

            anglesY = new List<int>()
            {
                110, 100, 90, 80, 70
            };

            port.Write("ON");
            laserSwitch = true;
            idleSounds.ElementAt(soundIndex).Play();

            if (soundIndex < 3)
            {
                soundIndex++;
            }
            else
            {
                soundIndex = 0;
            }

            foreach (int angleY in anglesY)
            {
                for (int i = 0; i < 180; i++) //0-180
                {
                    if (idleVoiceLinesWatch.ElapsedMilliseconds > 5000)
                    {
                        await Task.Delay(30); //35
                        angle = String.Format("X{0}Y{1}", angleX + i, angleY);
                        port.Write(angle);

                    }
                }
            }

            loopdone = true;
        }
    }
}
