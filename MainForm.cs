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
        private BlockingCollection<string> comCommandQueue;
        private Thread tempMeasThread;
        private Thread comControlThread;
        private int tempMeas_Timeout;

        private enum CmdRetItems { TWO_RETURN_ITEMS, THREE_RETURN_ITEMS };
        private delegate void SafeCallDelegate(string echo, string value, string status);
        private delegate void SafeCallDelegateTer(string temperature);
        private delegate void SafeCallDelegateUncheckCheckbox();
        public MainForm()
        {
            InitializeComponent();
        }

        private void popUpExceptionWindow(string message)
        {
            ExceptionForm form2 = new ExceptionForm();
            form2.SetLabelText(message);
            form2.ShowDialog();
        }

        private void uncheckCheckbox2_routine()
        {
            if (textBox5.InvokeRequired)
            {
                var dlg = new SafeCallDelegateUncheckCheckbox(uncheckCheckbox2_routine);
                checkBox2.Invoke(dlg, new object[] {});
            }
            else
            {
                if(checkBox2.Checked)
                {
                    checkBox2.Checked = false;
                }
            }
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

                comCommandQueue.Add("RD_TER1");
                comCommandQueue.Add("RD_TER2");
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
                        string echo = "Echo word error";
                        string value = "Value word error";
                        string status = "Status word error";
                        try
                        {
                            comDevice.WriteLine(command);
                            echo = comDevice.ReadLine();
                            value = comDevice.ReadLine();
                            status = comDevice.ReadLine();
                        }
                        catch (Exception ex)
                        {
                            tempMeasTerminator.Add(true);
                            while (comCommandQueue.TryTake(out _)) { }
                            popUpExceptionWindow(ex.ToString());
                            var checkboxThreadParams = new System.Threading.ThreadStart(delegate { uncheckCheckbox2_routine(); });
                            var checkboxThread = new Thread(checkboxThreadParams);
                            checkboxThread.Start();
                            checkboxThread.Join();
                            continue;
                        }
                        finally
                        {
                            var threadParams = new System.Threading.ThreadStart(delegate { WriteRichBoxSafe(echo, value, status); });
                            var thread = new Thread(threadParams);
                            thread.Start();
                        }

                        switch (command)
                        {
                            case "RD_TER1":
                                {
                                    ThreadStart sas;
                                    var tp = new System.Threading.ThreadStart(delegate { WriteTER1LabelSafe(value); });
                                    var t = new Thread(tp);
                                    t.Start();
                                }
                                break;
                            case "RD_TER2":
                                {
                                    var tp = new System.Threading.ThreadStart(delegate { WriteTER2LabelSafe(value); });
                                    var t = new Thread(tp);
                                    t.Start();
                                }
                                break;
                            default:
                                // code block
                                break;
                        }
                    }
                }
            }
        }
        private void WriteTER1LabelSafe(string temperature)
        {
            if(textBox5.InvokeRequired)
            {
                var dlg = new SafeCallDelegateTer(WriteTER1LabelSafe);
                textBox5.Invoke(dlg, new object[] { temperature });
            }
            else
            {
                textBox5.Text = temperature;
            }
        }

        private void WriteTER2LabelSafe(string temperature)
        {
            if (textBox5.InvokeRequired)
            {
                var dlg = new SafeCallDelegateTer(WriteTER2LabelSafe);
                textBox8.Invoke(dlg, new object[] { temperature });
            }
            else
            {
                textBox8.Text = temperature;
            }
        }

        private void WriteRichBoxSafe(string echo, string value, string status)
        {
            if (richTextBox1.InvokeRequired)
            {
                var dlg = new SafeCallDelegate(WriteRichBoxSafe);
                richTextBox1.Invoke(dlg, new object[] { echo, value, status });
            }
            else
            {
                DateTime commandTime = DateTime.Now;
                string strCommandTime = commandTime.ToString("RX: HH:mm:ss.ff: ", System.Globalization.DateTimeFormatInfo.InvariantInfo);

                richTextBox1.AppendText(strCommandTime);
                richTextBox1.AppendText(echo);
                richTextBox1.AppendText(Environment.NewLine);
                richTextBox1.AppendText(strCommandTime);
                richTextBox1.AppendText(value);
                richTextBox1.AppendText(Environment.NewLine);
                richTextBox1.AppendText(strCommandTime);
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
                if (lines > 299)
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

            comControlThread = new Thread(comControlRoutine);
            comCommandQueue = new BlockingCollection<string>();

            tempMeasTerminator = new BlockingCollection<bool>();
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
            if (lines > 399)
            {
                textbox.Clear();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
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
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

            if (checkBox2.Checked)
            {
                while (tempMeasTerminator.TryTake(out _)) { }
                tempMeasThread = new Thread(tempMeas_routine);

                int requiredTimemout = Int32.Parse(textBox4.Text);
                if (requiredTimemout < 300 || requiredTimemout > 3000)
                {
                    textBox4.Text = "500";
                    requiredTimemout = 500;
                }
                tempMeas_Timeout = requiredTimemout;
                tempMeasThread.Start();
            }
            else
            {
                tempMeasTerminator.Add(true);
            }
        }
        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.Type == ScrollEventType.EndScroll)
            {
                this.textBox2.Text = (this.hScrollBar1.Value).ToString();
            }
        }
        private void hScrollBar2_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.Type == ScrollEventType.EndScroll)
            {
                this.textBox3.Text = (this.hScrollBar2.Value).ToString();
            }
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            this.hScrollBar1.Value = Int32.Parse(this.textBox2.Text);
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            this.hScrollBar2.Value = Int32.Parse(this.textBox3.Text);
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
                comCommandQueue.Add("RD_TER1");
                comCommandQueue.Add("RD_TER2");
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
