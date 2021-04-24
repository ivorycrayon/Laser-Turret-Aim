﻿using System;
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
        public Stopwatch servoWatch { get; private set; }
        public Stopwatch idleVoiceLinesWatch { get; private set; }

        int soundIndex = 0;
        int fireIndex = 0;
        bool loopdone = true;
        bool laserSwitch = true;

        System.Media.SoundPlayer hello = new System.Media.SoundPlayer(@"C:\Users\Ivory.DESKTOP-J6TK9H0\Google Drive\programming\C\Arduino\Laser Turret Aim\portal\hellofriend.wav");
        System.Media.SoundPlayer iSeeYou = new System.Media.SoundPlayer(@"C:\Users\Ivory.DESKTOP-J6TK9H0\Google Drive\programming\C\Arduino\Laser Turret Aim\portal\iseeyou.wav");
        System.Media.SoundPlayer stillThere = new System.Media.SoundPlayer(@"C:\Users\Ivory.DESKTOP-J6TK9H0\Google Drive\programming\C\Arduino\Laser Turret Aim\portal\stillthere.wav");
        System.Media.SoundPlayer comeOver = new System.Media.SoundPlayer(@"C:\Users\Ivory.DESKTOP-J6TK9H0\Google Drive\programming\C\Arduino\Laser Turret Aim\portal\comehere.wav");
        System.Media.SoundPlayer deploy = new System.Media.SoundPlayer(@"C:\Users\Ivory.DESKTOP-J6TK9H0\Google Drive\programming\C\Arduino\Laser Turret Aim\portal\Turret_deploy.wav");
        System.Media.SoundPlayer disabled = new System.Media.SoundPlayer(@"C:\Users\Ivory.DESKTOP-J6TK9H0\Google Drive\programming\C\Arduino\Laser Turret Aim\portal\Turret_turret_disabled_4.wav");
        System.Media.SoundPlayer retire = new System.Media.SoundPlayer(@"C:\Users\Ivory.DESKTOP-J6TK9H0\Google Drive\programming\C\Arduino\Laser Turret Aim\portal\Turret_turret_retire_1.wav");
        System.Media.SoundPlayer retract = new System.Media.SoundPlayer(@"C:\Users\Ivory.DESKTOP-J6TK9H0\Google Drive\programming\C\Arduino\Laser Turret Aim\portal\Turret_retract.wav");
        System.Media.SoundPlayer anyoneThere = new System.Media.SoundPlayer(@"C:\Users\Ivory.DESKTOP-J6TK9H0\Google Drive\programming\C\Arduino\Laser Turret Aim\portal\Turret_turret_autosearch_5.wav");
        System.Media.SoundPlayer fire1 = new System.Media.SoundPlayer(@"C:\Users\Ivory.DESKTOP-J6TK9H0\Google Drive\programming\C\Arduino\Laser Turret Aim\portal\Turret_turret_fire_4x_01.wav");
        System.Media.SoundPlayer fire2 = new System.Media.SoundPlayer(@"C:\Users\Ivory.DESKTOP-J6TK9H0\Google Drive\programming\C\Arduino\Laser Turret Aim\portal\Turret_turret_fire_4x_02.wav");
        System.Media.SoundPlayer fire3 = new System.Media.SoundPlayer(@"C:\Users\Ivory.DESKTOP-J6TK9H0\Google Drive\programming\C\Arduino\Laser Turret Aim\portal\Turret_turret_fire_4x_03.wav");

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
        }

        private async void Form1_Shown(object sender, EventArgs e)
        {
            deploy.PlaySync();
            port.Write("ON");
            hello.Play();
            await Task.Delay(30);
            port.Write("X90Y90");
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            retract.PlaySync();
            disabled.PlaySync();
            port.Write("OFF");
            retire.PlaySync();
            port.Write("X90Y180");
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
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async void Form1_MouseMove(object sender, MouseEventArgs e)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            writeToPort(new Point(e.X, e.Y));

            idleVoiceLinesWatch = Stopwatch.StartNew();
        }

        public void writeToPort(Point coordinates)
        {
            int xcoord = (180 - coordinates.X / (Size.Width / 180));
            int ycoord = (180 - coordinates.Y / (Size.Height / 180));

            if (servoWatch.ElapsedMilliseconds > 15)
            {
                servoWatch = Stopwatch.StartNew();

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
