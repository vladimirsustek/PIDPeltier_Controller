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
using System.Threading;
using System.Collections.Concurrent;

namespace PIDPeltier_Controller
{
    public partial class MainForm : Form
    {
        private SerialPort comDevice;
        private DateTime localDate = DateTime.Now;
        private BlockingCollection<bool> tempMeasTerminator;
        private BlockingCollection<bool> comCommandTerminator;
        private BlockingCollection<string> comCommandQueue;
        private Thread tempMeasThread;
        private Thread comControlThread;
        private int tempMeas_Timeout;

        private enum CmdRetItems { TWO_RETURN_ITEMS, THREE_RETURN_ITEMS };
        private delegate void SafeCallDelegate(string command, CmdRetItems rItems);
        public MainForm()
        {
            InitializeComponent();
        }
        private void tempMeas_routine()
        {
            while (true)
            {
                bool terminateLoop = false;

                if (true == tempMeasTerminator.TryTake(out terminateLoop, tempMeas_Timeout) && terminateLoop)
                {
                    break;
                }
            }
        }
        private void comControlRoutine()
        {
            while (true)
            {
                string command;

                if (true == comCommandQueue.TryTake(out command, -1))
                {
                    if (command == "CLOSE")
                    {
                        break;
                    }
                    else
                    {
                        var threadParams = new System.Threading.ThreadStart(delegate { WriteRichBoxSafe(command, CmdRetItems.THREE_RETURN_ITEMS); });
                        var thread = new Thread(threadParams);
                        thread.Start();
                    }
                }
            }
        }

        private void WriteRichBoxSafe(string text, CmdRetItems ret_items)
        {
            if (richTextBox1.InvokeRequired)
            {
                var dlg = new SafeCallDelegate(WriteRichBoxSafe);
                richTextBox1.Invoke(dlg, new object[] { text, ret_items });
            }
            else
            {
                if (!comDevice.IsOpen)
                {
                    richTextBox1.AppendText("No COM available");
                    richTextBox1.AppendText(Environment.NewLine);
                    return;
                }
                comDevice.WriteLine(text);

                string echo = comDevice.ReadLine();
                string value = comDevice.ReadLine();
                string status = comDevice.ReadLine();

                DateTime commandTime = DateTime.Now;

                richTextBox1.AppendText(commandTime.ToString("RX: HH:mm:ss.ff: ", System.Globalization.DateTimeFormatInfo.InvariantInfo));
                richTextBox1.AppendText(echo);
                richTextBox1.AppendText(Environment.NewLine);
                richTextBox1.AppendText(commandTime.ToString("RX: HH:mm:ss.ff: ", System.Globalization.DateTimeFormatInfo.InvariantInfo));
                richTextBox1.AppendText(value);
                richTextBox1.AppendText(Environment.NewLine);
                richTextBox1.AppendText(commandTime.ToString("RX: HH:mm:ss.ff: ", System.Globalization.DateTimeFormatInfo.InvariantInfo));
                richTextBox1.AppendText(status);
                richTextBox1.AppendText(Environment.NewLine);
                richTextBox1.ScrollToCaret();

                int len = richTextBox1.TextLength;
                int lines;
                if (len > 0)
                {
                    lines = richTextBox1.GetLineFromCharIndex(len - 1) + 1;
                }
                else
                {
                    lines = 0;
                }
                if (lines > 999)
                {
                    richTextBox1.Clear();
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (comDevice.IsOpen)
            {
                comDevice.Close();
            }
            tempMeasTerminator.Add(true);
            comCommandQueue.Add("CLOSE");
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            this.comboBox1.Items.AddRange(ports);

            comDevice = new SerialPort();
            localDate = DateTime.Now;

            tempMeasThread = new Thread(tempMeas_routine);
            tempMeasTerminator = new BlockingCollection<bool>();
            tempMeas_Timeout = 1000;

            comControlThread = new Thread(comControlRoutine);
            comCommandQueue = new BlockingCollection<string>();

            comCommandTerminator = new BlockingCollection<bool>();


            tempMeasThread.Start();
            comControlThread.Start();
        }
        private void printlineTimestamped(RichTextBox textbox, string line)
        {
            this.localDate = DateTime.Now;
            textbox.AppendText(localDate.ToString("HH:mm:ss.ff: ", System.Globalization.DateTimeFormatInfo.InvariantInfo));
            textbox.AppendText(line);
            textbox.AppendText(Environment.NewLine);
            textbox.ScrollToCaret();
            int len = textbox.TextLength;
            int lines;
            if (len > 0)
            {
                lines = textbox.GetLineFromCharIndex(len - 1) + 1;
            }
            else
            {
                lines = 0;
            }
            if (lines > 999)
            {
                textbox.Clear();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                checkBox1.Checked = false;
            }
            if(checkBox2.Checked)
            {
                checkBox2.Checked = false;
            }

            if(comboBox1.Text == "")
            {
                return;
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
                this.comDevice.ReadTimeout = 750;
                this.comDevice.ReadTimeout = 750;

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
        private void PeriodicTempReadout(Object source, int e)
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
                    else
                    {
                        textBox5.Text = "Error";
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
                    else
                    {
                        textBox5.Text = "Error";
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
            /*
            if (checkBox2.Checked)
            {
                if(!TimerInitialized)
                {
                    aTimer.Interval = Int32.Parse(textBox4.Text);
                    aTimer.Elapsed += PeriodicTempReadout;
                    aTimer.AutoReset = true;
                    TimerInitialized = true;
                }

                aTimer.Enabled = true;
            }
            else
            {
                aTimer.Enabled = false;
            }*/
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
                    string echo = this.comDevice.ReadLine();
                    string status = this.comDevice.ReadLine();
                    printlineTimestamped(richTextBox1, echo);
                    printlineTimestamped(richTextBox1, status);
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
                        string echo = this.comDevice.ReadLine();
                        string status = this.comDevice.ReadLine();
                        printlineTimestamped(richTextBox1, echo);
                        printlineTimestamped(richTextBox1, status);
                }
                    catch (Exception exception)
                    {
                        this.printlineTimestamped(this.richTextBox1, exception.ToString());
                    }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string cmd = "ST_PWM1_" + Int32.Parse(this.textBox2.Text).ToString().PadLeft(5, '0');
            comCommandQueue.Add(cmd);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string cmd = "ST_PWM2_" + Int32.Parse(this.textBox3.Text).ToString().PadLeft(5, '0');
            comCommandQueue.Add(cmd);
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (this.comDevice.IsOpen)
            {
                try
                {
                    /*
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
                    */
                    comCommandQueue.Add("RD_TER1");
                    comCommandQueue.Add("RD_TER2");


                }
                catch (Exception exception)
                {
                    this.printlineTimestamped(this.richTextBox1, exception.ToString());
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            comCommandQueue.Add("ST_LEDR_1");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            comCommandQueue.Add("ST_LEDR_0");
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
            comCommandQueue.Add("EN_PWM1_1");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            comCommandQueue.Add("EN_PWM1_0");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            comCommandQueue.Add("EN_PWM2_1");
        }

        private void button10_Click(object sender, EventArgs e)
        {
            comCommandQueue.Add("EN_PWM2_0");
        }

        private void button11_Click(object sender, EventArgs e)
        {
            comCommandQueue.Add("ST_SWDG");
        }

        private void button12_Click(object sender, EventArgs e)
        {
            this.richTextBox1.Clear();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            comCommandQueue.Add("ST_LEDR_1");
        }
    }
}
