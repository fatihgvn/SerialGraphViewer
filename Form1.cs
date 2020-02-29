using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SerialGraphViewer
{
    public partial class Form1 : Form
    {
        bool stopDraw = false;
        public Form1()
        {
            InitializeComponent();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            comlist.Items.Clear();
            comlist.Items.AddRange(SerialPort.GetPortNames());
            comlist.SelectedIndex = 0;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comlist.Items.AddRange(SerialPort.GetPortNames());
            comlist.SelectedIndex = 0;

            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart1.ChartAreas[0].AxisY.ScaleView.Zoomable = true;

            chart1.Series[0].XValueType = ChartValueType.Auto;
            chart1.Series[1].XValueType = ChartValueType.Auto;
            chart1.Series[2].XValueType = ChartValueType.Auto;
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "";

            chart1.Series[0].Points.AddY(0);
            chart1.Series[1].Points.AddY(0);
            chart1.Series[2].Points.AddY(0);

            chart1.Series[0].Enabled = false;
            chart1.Series[2].Enabled = false;

            chart1.MouseWheel += Chart1_MouseWheel;
        }

        private void Chart1_MouseWheel(object sender, MouseEventArgs e)
        {
            var chart = (Chart)sender;
            var xAxis = chart.ChartAreas[0].AxisX;
            var yAxis = chart.ChartAreas[0].AxisY;

            try
            {
                if (e.Delta < 0) // Scrolled down.
                {
                    xAxis.ScaleView.ZoomReset();
                    yAxis.ScaleView.ZoomReset();
                }
                else if (e.Delta > 0) // Scrolled up.
                {
                    var xMin = xAxis.ScaleView.ViewMinimum;
                    var xMax = xAxis.ScaleView.ViewMaximum;
                    var yMin = yAxis.ScaleView.ViewMinimum;
                    var yMax = yAxis.ScaleView.ViewMaximum;

                    var posXStart = xAxis.PixelPositionToValue(e.Location.X) - (xMax - xMin) / 4;
                    var posXFinish = xAxis.PixelPositionToValue(e.Location.X) + (xMax - xMin) / 4;
                    var posYStart = yAxis.PixelPositionToValue(e.Location.Y) - (yMax - yMin) / 4;
                    var posYFinish = yAxis.PixelPositionToValue(e.Location.Y) + (yMax - yMin) / 4;

                    xAxis.ScaleView.Zoom(posXStart, posXFinish);
                    yAxis.ScaleView.Zoom(posYStart, posYFinish);
                }
            }
            catch { }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!serialPort1.IsOpen)
                {
                    toolStripButton1.Image = Properties.Resources.close;

                    serialPort1.PortName = comlist.Text;
                    serialPort1.Open();
                }
                else
                {
                    toolStripButton1.Image = Properties.Resources.open;
                    this.Invoke(() =>
                    {
                        serialPort1.Close();
                    });
                }
            }
            catch (Exception)
            {

            }
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (stopDraw) return;

                SerialPort sp = (SerialPort)sender;
                //string indata = sp.ReadExisting();
                string indata = sp.ReadLine();
                double doubleData;
                Double.TryParse(indata, out doubleData);

                var match = Regex.Match(indata, @"^\-?\d+[\n\r]+?$", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    try
                    {
                        chart1.Invoke(() =>
                        {
                            chart1.Series[0].Points.AddY(doubleData);
                            chart1.Series[1].Points.AddY(doubleData);
                            chart1.Series[2].Points.AddY(doubleData);

                            if(sempler.Checked && chart1.Series[0].Points.Count > 301)
                            {
                                chart1.Series[0].Points.RemoveAt(0);
                                chart1.Series[1].Points.RemoveAt(0);
                                chart1.Series[2].Points.RemoveAt(0);

                                chart1.ChartAreas[0].RecalculateAxesScale();
                            }
                                
                        });

                    }
                    catch (Exception ex)
                    {
                        richTextBox1.Invoke(() => { richTextBox1.AppendText(ex.Message.ToString() + Environment.NewLine); });
                    }

                }
                richTextBox1.Invoke(() => { richTextBox1.AppendText(indata); });
            }
            catch (Exception)
            {

            }
        }


        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (autoScroll.Checked)
            {
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
            }
        }

        private void displaySinus_CheckedChanged(object sender, EventArgs e)
        {
            chart1.Series[0].Enabled = displaySinus.Checked;
        }

        private void displayLine_CheckedChanged(object sender, EventArgs e)
        {
            chart1.Series[1].Enabled = displayLine.Checked;
        }

        private void displaySquare_CheckedChanged(object sender, EventArgs e)
        {
            chart1.Series[2].Enabled = displaySquare.Checked;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (serialPort1.IsOpen)
                {
                    this.Invoke(() =>
                    {
                        serialPort1.Close();
                    });
                }
            }
            catch (Exception) { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            chart1.Series[2].Points.Clear();

            chart1.Series[0].Points.AddY(0);
            chart1.Series[1].Points.AddY(0);
            chart1.Series[2].Points.AddY(0);
        }

        private void sempler_CheckedChanged(object sender, EventArgs e)
        {
            if (sempler.Checked)
            {
                chart1.ChartAreas[0].AxisX.Maximum = 300;
            }
            else
            {
                chart1.ChartAreas[0].AxisX.Maximum = Double.NaN;
            }
        }

        private void saveImage_Click(object sender, EventArgs e)
        {
            stopDraw = true;
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Resim|*.png";
            save.FileName = "Chart.png";
            save.OverwritePrompt = true;
            save.CreatePrompt = true;

            if (save.ShowDialog() == DialogResult.OK)
            {
                chart1.SaveImage(save.FileName, ChartImageFormat.Png);
            }
            stopDraw = false;
        }
    }

    public static class ControlExtensions
    {
        public static void Invoke(this Control control, Action action)
        {
            if (control.InvokeRequired) control.Invoke(new MethodInvoker(action), null);
            else action.Invoke();
        }
    }
}
