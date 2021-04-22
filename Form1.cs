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

namespace Laser_Turret_Aim
{
    public partial class Form1 : Form
    {
        static System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();
        public Stopwatch watch { get; private set; }
        public Stopwatch watch2 { get; private set; }
        public Stopwatch portalWatch { get; private set; }
        public Stopwatch waitWatch { get; private set; }

        int sound = 0;        

        System.Media.SoundPlayer hello = new System.Media.SoundPlayer(@"C:\Users\Ivory.DESKTOP-J6TK9H0\Google Drive\programming\C\Arduino\Laser Turret Aim\portal\hellofriend.wav");
        System.Media.SoundPlayer iSeeYou = new System.Media.SoundPlayer(@"C:\Users\Ivory.DESKTOP-J6TK9H0\Google Drive\programming\C\Arduino\Laser Turret Aim\portal\iseeyou.wav");
        System.Media.SoundPlayer stillThere = new System.Media.SoundPlayer(@"C:\Users\Ivory.DESKTOP-J6TK9H0\Google Drive\programming\C\Arduino\Laser Turret Aim\portal\stillthere.wav");
        System.Media.SoundPlayer comeOver = new System.Media.SoundPlayer(@"C:\Users\Ivory.DESKTOP-J6TK9H0\Google Drive\programming\C\Arduino\Laser Turret Aim\portal\comehere.wav");
        System.Media.SoundPlayer deploy = new System.Media.SoundPlayer(@"C:\Users\Ivory.DESKTOP-J6TK9H0\Google Drive\programming\C\Arduino\Laser Turret Aim\portal\Turret_deploy.wav");
        System.Media.SoundPlayer disabled = new System.Media.SoundPlayer(@"C:\Users\Ivory.DESKTOP-J6TK9H0\Google Drive\programming\C\Arduino\Laser Turret Aim\portal\Turret_turret_disabled_4.wav");
        System.Media.SoundPlayer retire = new System.Media.SoundPlayer(@"C:\Users\Ivory.DESKTOP-J6TK9H0\Google Drive\programming\C\Arduino\Laser Turret Aim\portal\Turret_turret_retire_1.wav");


        public Form1()
        {
            InitializeComponent();
            InitializeTimer();
            waitWatch = Stopwatch.StartNew();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            watch = Stopwatch.StartNew();
            watch2 = Stopwatch.StartNew();
            portalWatch = Stopwatch.StartNew();
            

            port.Open();
            port.Write("X90Y90");
        }     

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            writeToPort(new Point(e.X, e.Y));            

            waitWatch = Stopwatch.StartNew();
        }
        private void InitializeTimer()
        {
            timer1.Interval = 1000;
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Enabled = true;            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(waitWatch.ElapsedMilliseconds > 5000)
            {              
                int anglex = 0;
                int angley = 90;
                string angle;

                for(int i = 0; i < 180; i++)
                {
                    angle = String.Format("X{0}Y{1}", anglex + i, angley);

                    port.Write(angle);
                    System.Threading.Thread.Sleep(35);                
                }

                System.Threading.Thread.Sleep(100);

            }

            if (waitWatch.ElapsedMilliseconds > 25000)
            {
                stillThere.Play();
                waitWatch = Stopwatch.StartNew();
            }

            if (portalWatch.ElapsedMilliseconds > 10000 && sound == 0)
            {
                portalWatch = Stopwatch.StartNew();

                iSeeYou.Play();
                sound += 1;
            }

            else if (portalWatch.ElapsedMilliseconds > 10000 && sound == 1)
            {
                portalWatch = Stopwatch.StartNew();

                comeOver.Play();
                sound -= 1;
            }
        }

        public void writeToPort(Point coordinates)
        {                      
            int xcoord = (180 - coordinates.X / (Size.Width / 180));
            int ycoord = (180 - coordinates.Y / (Size.Height / 180));

            if (watch.ElapsedMilliseconds > 15)
            {
                watch = Stopwatch.StartNew();

                port.Write(String.Format("X{0}Y{1}", xcoord, ycoord));
            }                        
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {            
            disabled.PlaySync();
            port.Write("X90Y90");
            retire.PlaySync();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            deploy.PlaySync();
            hello.Play();
        }
    }
}
