using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Vypinac
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //this.MaximumSize = this.Size;
            //this.MinimumSize = this.Size;
        }

        private bool aktivni;
        private DateTime cas;
        private bool za = true;
        private bool zhasnuto;
        private Point zacatek;

        private const int HWND_BROADCAST = 0xFFFF;
        private const int SC_MONITORPOWER = 0xF170;
        private const int WM_SYSCOMMAND = 0x112;

        private const int MONITOR_ON = -1;
        private const int MONITOR_OFF = 2;
        private const int MONITOR_STANBY = 1;

        [DllImport("user32.dll")]
        private static extern int SendMessage(int hWnd, int hMsg, int wParam, int lParam);

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Start();
            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.CustomFormat = "HH:mm:ss";
            dateTimePicker1.Value = new DateTime(1800, 1, 1, 0, 1, 0);
            radioButton1.Checked = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (za)
            {
                cas = DateTime.Now.AddHours(dateTimePicker1.Value.Hour).AddMinutes(dateTimePicker1.Value.Minute).AddSeconds(dateTimePicker1.Value.Second);
            }
            else
            {
                if (dateTimePicker1.Value < DateTime.Now)
                {
                    MessageBox.Show("Nelze zadat čas z minulosti.", "Tohle ale ne!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }
                cas = dateTimePicker1.Value;
            }
            aktivni = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            aktivni = false;
            label1.Text = "Nenastaveno...";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            za = !za;
            if (za)
            {
                dateTimePicker1.CustomFormat = "HH:mm:ss";
                dateTimePicker1.Value = new DateTime(1800, 1, 1, 0, 1, 0);
                if (radioButton1.Checked) label2.Text = "Vypnout za:";
                else label2.Text = "Zhasnout za:";
            }
            else
            {
                dateTimePicker1.CustomFormat = "dd.MM.  HH:mm:ss";
                if (radioButton1.Checked) label2.Text = "Vypnout v:";
                else label2.Text = "Zhasnout v:";
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!aktivni) return;
            if (radioButton1.Checked) label1.Text = String.Format("Vypnu se za: {0}h {1}m {2}s", (int)cas.Subtract(DateTime.Now).TotalHours, cas.Subtract(DateTime.Now).Minutes, cas.Subtract(DateTime.Now).Seconds);
            else label1.Text = String.Format("Zhasnu za: {0}h {1}m {2}s", (int)cas.Subtract(DateTime.Now).TotalHours, cas.Subtract(DateTime.Now).Minutes, cas.Subtract(DateTime.Now).Seconds);
            if (cas < DateTime.Now)
                if (radioButton1.Checked)
                    Process.Start("shutdown", "/s /t 0");
                else
                {
                    aktivni = false;
                    label1.Text = "Nenastaveno...";
                    SendMessage(-1, WM_SYSCOMMAND, SC_MONITORPOWER, MONITOR_OFF); 
                    Thread.Sleep(1000);
                    this.WindowState = FormWindowState.Maximized;
                    //Thread.Sleep(5000);
                    zacatek = Cursor.Position;
                    zhasnuto = true;
                }

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked) 
                if (za) label2.Text = "Vypnout za:"; 
                else label2.Text = "Vypnout v:";
            else 
                if (za) label2.Text = "Zhasnout za:"; 
                else label2.Text = "Zhasnout v:";
        }


        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {  
            if (zhasnuto && (Math.Abs(zacatek.X - Cursor.Position.X) > 3 || Math.Abs(zacatek.Y - Cursor.Position.Y) > 3))
            {
                this.WindowState = FormWindowState.Normal;
                Thread.Sleep(500);
                SendMessage(HWND_BROADCAST, WM_SYSCOMMAND, SC_MONITORPOWER, MONITOR_ON);
                zhasnuto = false;
            }
        }


    }
}
