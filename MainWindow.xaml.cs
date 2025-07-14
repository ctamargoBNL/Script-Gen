using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Windows;

namespace RampScriptTool
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs(out int start, out int end, out int step,
                                out int waitRamp, out int measures, out int ampsPerSec))
                return;

            txtOutput.Text = BuildScript(start, end, step, waitRamp, measures, ampsPerSec, out int totalMeasures);
            txtTotalMeasures.Text = totalMeasures.ToString();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtOutput.Text))
            {
                MessageBox.Show("Nothing to save. Generate script first.");
                return;
            }

            var dlg = new SaveFileDialog { Filter = "Text Files (*.txt)|*.txt" };
            if (dlg.ShowDialog() == true)
                File.WriteAllText(dlg.FileName, txtOutput.Text);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private bool ValidateInputs(out int start, out int end, out int step,
                                   out int waitRamp, out int measures, out int ampsPerSec)
        {
            start = end = step = waitRamp = measures = ampsPerSec = 0;

            try
            {
                start = int.Parse(txtStart.Text);
                end = int.Parse(txtEnd.Text);
                step = int.Parse(txtStep.Text);
                waitRamp = int.Parse(txtWaitRamp.Text);
                measures = int.Parse(txtMeasures.Text);
                ampsPerSec = int.Parse(txtAmpsPerSec.Text);

                if (step == 0)
                    throw new Exception("Step interval cannot be zero.");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid input: " + ex.Message);
                return false;
            }
        }

        private string BuildScript(int start, int end, int step,
                                   int waitRamp, int measures, int ampsPerSec,
                                   out int totalMeasures)
        {
            var sb = new StringBuilder();
            int prev = 0;
            int curr = start;
            totalMeasures = 0;

            // Adjust step sign if needed
            if (start > end && step > 0) step = -step;
            if (start < end && step < 0) step = -step;

            while ((step > 0 && curr <= end) || (step < 0 && curr >= end))
            {
                sb.AppendLine($"Ramp,{prev},{curr},{ampsPerSec},0.2,0.2");
                sb.AppendLine($"wait,{waitRamp}");

                for (int i = 0; i < measures; i++)
                {
                    sb.AppendLine("Measure");
                    sb.AppendLine("wait,3");
                }

                totalMeasures += measures;
                prev = curr;
                curr += step;
            }

            return sb.ToString();
        }
    }
}

