﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Timers;
using System.Threading;

namespace PIDPeltier_Controller
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private SerialPort comDevice = new SerialPort();
        private DateTime localDate = DateTime.Now;
        private static System.Timers.Timer aTimer = new System.Timers.Timer();
        private static Mutex mut = new Mutex();
        private void printlineTimestamped(RichTextBox textbox, string line)
        {
            this.localDate = DateTime.Now;
            textbox.AppendText(localDate.ToString("HH:mm:ss.ff: ", System.Globalization.DateTimeFormatInfo.InvariantInfo));
            textbox.AppendText(line);
            textbox.AppendText(Environment.NewLine);
            textbox.ScrollToCaret();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            this.comboBox1.Items.AddRange(ports);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.comDevice.IsOpen)
            {
                try
                {
                    this.comDevice.Close();
                    this.label1.BackColor = System.Drawing.Color.Red;
                    this.label1.Text = "Closed";
                }
                catch (Exception exception)
                {
                    this.printlineTimestamped(this.richTextBox1, exception.ToString());
                }

            }
            else
            {
                this.comDevice.PortName = this.comboBox1.Text;
                this.comDevice.BaudRate = Int32.Parse(this.textBox1.Text);
                this.comDevice.ReadTimeout = 100;
                this.comDevice.ReadTimeout = 100;

                try
                {
                    this.comDevice.Open();
                    this.label1.BackColor = System.Drawing.Color.Green;
                    this.label1.Text = "Opened";
                }
                catch (Exception exception)
                {
                    this.printlineTimestamped(this.richTextBox1, exception.ToString());
                }

            }

        }
        private void PeriodicTempReadout(Object source, ElapsedEventArgs e)
        {
            if (this.comDevice.IsOpen)
            {
                try
                {
                    mut.WaitOne();
                    this.localDate = DateTime.Now;
                    textBox6.Text = localDate.ToString("HH:mm:ss.ff");

                    this.comDevice.WriteLine("CHIPT");
                    string line = this.comDevice.ReadLine();
                    textBox5.Text = line;

                    this.localDate = DateTime.Now;
                    textBox7.Text = localDate.ToString("HH:mm:ss.ff");

                    printlineTimestamped(richTextBox1, line);

                    mut.ReleaseMutex();
                }
                catch (Exception exception)
                {
                    this.printlineTimestamped(this.richTextBox1, exception.ToString());
                }
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                aTimer.Interval = Int32.Parse(textBox4.Text);
                aTimer.Elapsed += PeriodicTempReadout;
                aTimer.AutoReset = true;
                aTimer.Enabled = true;
            }
            else
            {
                aTimer.Enabled = false;
            }
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.Type == ScrollEventType.EndScroll)
            {
                this.textBox2.Text = (this.hScrollBar1.Value).ToString();

                if(checkBox1.Checked)
                {
                    try
                    {
                        string cmd = "PA15_" + hScrollBar1.Value.ToString().PadLeft(3, '0');
                        this.comDevice.WriteLine(cmd);
                        string line = this.comDevice.ReadLine();
                        printlineTimestamped(richTextBox1, line);
                    }
                    catch (Exception exception)
                    {
                        this.printlineTimestamped(this.richTextBox1, exception.ToString());
                    }
                }

            }
        }

        private void hScrollBar2_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.Type == ScrollEventType.EndScroll)
            {
                this.textBox3.Text = (this.hScrollBar2.Value).ToString();

                if (checkBox1.Checked)
                {
                    try
                    {
                        string cmd = "PB03_" + hScrollBar2.Value.ToString().PadLeft(3, '0');
                        this.comDevice.WriteLine(cmd);
                        string line = this.comDevice.ReadLine();
                        printlineTimestamped(richTextBox1, line);
                    }
                    catch (Exception exception)
                    {
                        this.printlineTimestamped(this.richTextBox1, exception.ToString());
                    }
                }

            }
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            this.hScrollBar1.Value = Int32.Parse(this.textBox2.Text);

            if (checkBox1.Checked)
            {
                try
                {
                    string cmd = "PA15_" + Int32.Parse(this.textBox2.Text).ToString().PadLeft(3, '0');
                    this.comDevice.WriteLine(cmd);
                    string line = this.comDevice.ReadLine();
                    printlineTimestamped(richTextBox1, line);
                }
                catch (Exception exception)
                {
                    this.printlineTimestamped(this.richTextBox1, exception.ToString());
                }
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            this.hScrollBar2.Value = Int32.Parse(this.textBox3.Text);

            if (checkBox1.Checked)
            {
                try
                {
                    string cmd = "PB03_" + Int32.Parse(this.textBox3.Text).ToString().PadLeft(3, '0');
                    this.comDevice.WriteLine(cmd);
                    string line = this.comDevice.ReadLine();
                    printlineTimestamped(richTextBox1, line);
                }
                catch (Exception exception)
                {
                    this.printlineTimestamped(this.richTextBox1, exception.ToString());
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                string cmd = "PA15_" + Int32.Parse(this.textBox2.Text).ToString().PadLeft(3, '0');
                this.comDevice.WriteLine(cmd);
                string line = this.comDevice.ReadLine();
                printlineTimestamped(richTextBox1, line);
            }
            catch (Exception exception)
            {
                this.printlineTimestamped(this.richTextBox1, exception.ToString());
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                string cmd = "PB03_" + Int32.Parse(this.textBox3.Text).ToString().PadLeft(3, '0');
                this.comDevice.WriteLine(cmd);
                string line = this.comDevice.ReadLine();
                printlineTimestamped(richTextBox1, line);
            }
            catch (Exception exception)
            {
                this.printlineTimestamped(this.richTextBox1, exception.ToString());
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (this.comDevice.IsOpen)
            {
                try
                {
                    mut.WaitOne();

                    this.localDate = DateTime.Now;
                    textBox6.Text = localDate.ToString("HH:mm:ss.ff");

                    this.comDevice.WriteLine("CHIPT");
                    string line = this.comDevice.ReadLine();
                    textBox5.Text = line;

                    this.localDate = DateTime.Now;
                    textBox7.Text = localDate.ToString("HH:mm:ss.ff");

                    printlineTimestamped(richTextBox1, line);

                    mut.ReleaseMutex();
                }
                catch (Exception exception)
                {
                    this.printlineTimestamped(this.richTextBox1, exception.ToString());
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                this.comDevice.WriteLine("LEDG_1");
                string line = this.comDevice.ReadLine();
                printlineTimestamped(richTextBox1, line);
            }
            catch (Exception exception)
            {
                this.printlineTimestamped(this.richTextBox1, exception.ToString());
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                this.comDevice.WriteLine("LEDG_0");
                string line = this.comDevice.ReadLine();
                printlineTimestamped(richTextBox1, line);
            }
            catch (Exception exception)
            {
                this.printlineTimestamped(this.richTextBox1, exception.ToString());
            }
        }
    }
}
