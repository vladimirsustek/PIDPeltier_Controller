using System;
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
        private System.Timers.Timer aTimer;

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
            comDevice.ReadTimeout = 1000;
            comDevice.WriteTimeout = 1000;

            if (checkBox1.Checked)
            {
                checkBox1.Checked = false;
            }
            if(checkBox2.Checked)
            {
                checkBox2.Checked = false;
            }

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
                    this.localDate = DateTime.Now;
                    textBox6.Text = localDate.ToString("HH:mm:ss.ff");

                    this.comDevice.WriteLine("RD_TER1");
                    string cmd_echo = this.comDevice.ReadLine();
                    string value = this.comDevice.ReadLine();
                    string ok = this.comDevice.ReadLine();

                    //printlineTimestamped(richTextBox1, cmd_echo);
                    if (checkBox3.Checked)
                    {
                        printlineTimestamped(richTextBox1, value);
                    }
                    //printlineTimestamped(richTextBox1, ok);

                    Thread.Sleep(50);

                    if (0 == String.Compare(ok, "CMD_OK"))
                    {
                        textBox5.Text = value;
                    }

                    this.comDevice.WriteLine("RD_TER2");
                    cmd_echo = this.comDevice.ReadLine();
                    value = this.comDevice.ReadLine();
                    ok = this.comDevice.ReadLine();

                    //printlineTimestamped(richTextBox1, cmd_echo);
                    if (checkBox4.Checked)
                    {
                        printlineTimestamped(richTextBox1, value);
                    }
                    //printlineTimestamped(richTextBox1, ok);

                    if (0 == String.Compare(ok, "CMD_OK"))
                    {
                        textBox8.Text = value;
                    }

                    Thread.Sleep(50);

                    this.comDevice.WriteLine("ST_SWDG");
                    cmd_echo = this.comDevice.ReadLine();
                    //printlineTimestamped(richTextBox1, cmd_echo);
                    ok = this.comDevice.ReadLine();
                    //printlineTimestamped(richTextBox1, ok);

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
                        string cmd = "ST_PWM1_" + Int32.Parse(this.textBox2.Text).ToString().PadLeft(5, '0');
                        this.comDevice.WriteLine(cmd);
                        string line = this.comDevice.ReadLine();
                        printlineTimestamped(richTextBox1, line);
                        line = this.comDevice.ReadLine();
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
                        string cmd = "ST_PWM2_" + Int32.Parse(this.textBox3.Text).ToString().PadLeft(5, '0');
                        this.comDevice.WriteLine(cmd);
                        string line = this.comDevice.ReadLine();
                        printlineTimestamped(richTextBox1, line);
                        line = this.comDevice.ReadLine();
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
                    string cmd = "ST_PWM1_" + Int32.Parse(this.textBox3.Text).ToString().PadLeft(5, '0');
                    this.comDevice.WriteLine(cmd);
                    string line = this.comDevice.ReadLine();
                    printlineTimestamped(richTextBox1, line);
                    line = this.comDevice.ReadLine();
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
                        string cmd = "ST_PWM2_" + Int32.Parse(this.textBox3.Text).ToString().PadLeft(5, '0');
                        this.comDevice.WriteLine(cmd);
                        string line = this.comDevice.ReadLine();
                        printlineTimestamped(richTextBox1, line);
                        line = this.comDevice.ReadLine();
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
                string cmd = "ST_PWM1_" + Int32.Parse(this.textBox2.Text).ToString().PadLeft(5, '0');
                this.comDevice.WriteLine(cmd);
                string line = this.comDevice.ReadLine();
                printlineTimestamped(richTextBox1, line);
                line = this.comDevice.ReadLine();
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
                string cmd = "ST_PWM2_" + Int32.Parse(this.textBox3.Text).ToString().PadLeft(5, '0');
                this.comDevice.WriteLine(cmd);
                string line = this.comDevice.ReadLine();
                printlineTimestamped(richTextBox1, line);
                line = this.comDevice.ReadLine();
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
                    this.localDate = DateTime.Now;
                    textBox6.Text = localDate.ToString("HH:mm:ss.ff");

                    this.comDevice.WriteLine("RD_TER1");
                    string cmd_echo = this.comDevice.ReadLine();
                    string value = this.comDevice.ReadLine();
                    string ok = this.comDevice.ReadLine();

                    //printlineTimestamped(richTextBox1, cmd_echo);
                    if (checkBox3.Checked)
                    {
                        printlineTimestamped(richTextBox1, value);
                    }
                    //printlineTimestamped(richTextBox1, ok);

                    Thread.Sleep(50);

                    if (0 == String.Compare(ok, "CMD_OK"))
                    {
                        textBox5.Text = value;
                    }

                    this.comDevice.WriteLine("RD_TER2");
                    cmd_echo = this.comDevice.ReadLine();
                    value = this.comDevice.ReadLine();
                    ok = this.comDevice.ReadLine();

                    //printlineTimestamped(richTextBox1, cmd_echo);
                    if (checkBox4.Checked)
                    {
                        printlineTimestamped(richTextBox1, value);
                    }
                    //printlineTimestamped(richTextBox1, ok);

                    if (0 == String.Compare(ok, "CMD_OK"))
                    {
                        textBox8.Text = value;
                    }

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
                this.comDevice.WriteLine("ST_LEDR_1");
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
                this.comDevice.WriteLine("ST_LEDR_0");
                string line = this.comDevice.ReadLine();
                printlineTimestamped(richTextBox1, line);
            }
            catch (Exception exception)
            {
                this.printlineTimestamped(this.richTextBox1, exception.ToString());
            }
        }
        private void comboBox1_Clicked(object sender, EventArgs e)
        {

            comboBox1.Items.Clear();
            comboBox1.ResetText();

            string[] ports = SerialPort.GetPortNames();
            this.comboBox1.Items.AddRange(ports);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                this.comDevice.WriteLine("EN_PWM1_1");
                string line = this.comDevice.ReadLine();
                printlineTimestamped(richTextBox1, line);
                line = this.comDevice.ReadLine();
                printlineTimestamped(richTextBox1, line);
            }
            catch (Exception exception)
            {
                this.printlineTimestamped(this.richTextBox1, exception.ToString());
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                this.comDevice.WriteLine("EN_PWM1_0");
                string line = this.comDevice.ReadLine();
                printlineTimestamped(richTextBox1, line);
                line = this.comDevice.ReadLine();
                printlineTimestamped(richTextBox1, line);
            }
            catch (Exception exception)
            {
                this.printlineTimestamped(this.richTextBox1, exception.ToString());
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                this.comDevice.WriteLine("EN_PWM2_1");
                string line = this.comDevice.ReadLine();
                printlineTimestamped(richTextBox1, line);
                line = this.comDevice.ReadLine();
                printlineTimestamped(richTextBox1, line);
            }
            catch (Exception exception)
            {
                this.printlineTimestamped(this.richTextBox1, exception.ToString());
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                this.comDevice.WriteLine("EN_PWM2_0");
                string line = this.comDevice.ReadLine();
                printlineTimestamped(richTextBox1, line);
                line = this.comDevice.ReadLine();
                printlineTimestamped(richTextBox1, line);
            }
            catch (Exception exception)
            {
                this.printlineTimestamped(this.richTextBox1, exception.ToString());
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                this.comDevice.WriteLine("ST_SWDG");
                string line = this.comDevice.ReadLine();
                printlineTimestamped(richTextBox1, line);
                line = this.comDevice.ReadLine();
                printlineTimestamped(richTextBox1, line);
            }
            catch (Exception exception)
            {
                this.printlineTimestamped(this.richTextBox1, exception.ToString());
            }
        }
    }


}
