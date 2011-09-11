using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Converter.Main;
using WindowsConverter.Extensions;
using WindowsConverter.Forms.States;

namespace WindowsConverter.Forms
{
    public partial class Progress : Form
    {
        private Processor processor;

        private Form callingForm;

        private FormWindowState previousWindowState;
        private ProgressExitState exitState;



        public Progress(Arguments.Progress argument)
        {
            InitializeComponent();
            InitializeView(argument.CallingForm);
            InitializeProcessor(argument.InputFilenames, argument.OutputDirectory);
            InitializePriority();
        }

        private void InitializeProcessor(IEnumerable<string> inputFilenames, string outputDirectory)
        {
            processor = new Processor
            {
                InputFilenames = inputFilenames,
                OutputDirectory = outputDirectory,
                UpdateInterval = 250
            };

            processor.PreparationCompleted += (s, e) => this.TryInvoke(processor_PreparationCompleted, s, e);
            processor.PreparationCanceled += (s, e) => this.TryInvoke(processor_PreparationCanceled, s, e);
            processor.ConversionUpdated += (s, e) => this.TryInvoke(processor_ConversionUpdated, s, e);
            processor.ConversionCompleted += (s, e) => this.TryInvoke(processor_ConversionCompleted, s, e);
            processor.ConversionCanceled += (s, e) => this.TryInvoke(processor_ConversionCanceled, s, e);
        }

        private void InitializePriority()
        {
            var priorityData = EnumExtension.ToDataTable(typeof(ProcessPriorityClass),
                   EnumExtension.DefaultCaptionColumnName, EnumExtension.DefaultValueColumnName,
                   ProcessPriorityClassExtensions.ProcessPriorityClassAscendingSorter);

            cbxPriority.DisplayMember = EnumExtension.DefaultCaptionColumnName;
            cbxPriority.ValueMember = EnumExtension.DefaultValueColumnName;
            cbxPriority.DataSource = priorityData;
            cbxPriority.SelectedValue = (int)ProcessPriorityClass.Normal;

            tsmiPriority.DropDownItems.Clear();
            for (var i = priorityData.Rows.Count - 1; i >= 0; i--)
            {
                var item = new ToolStripMenuItem
                {
                    Text = (string)priorityData.Rows[i][EnumExtension.DefaultCaptionColumnName],
                    Tag = (int)priorityData.Rows[i][EnumExtension.DefaultValueColumnName]
                };
                item.Click += tsmiPrioritySubitem_Click;

                tsmiPriority.DropDownItems.Add(item);
            }
            
            ChangePriority(ProcessPriorityClass.Idle);
        }

        private void InitializeView(Form callingForm)
        {
            this.callingForm = callingForm;
            previousWindowState = FormWindowState.Normal;
            exitState = ProgressExitState.NotExiting;
        }



        private void ChangePriority(ProcessPriorityClass priority)
        {
            cbxPriority.SelectedValue = priority;

            for (var i = 0; i < tsmiPriority.DropDownItems.Count; i++)
            {
                var item = (ToolStripMenuItem)tsmiPriority.DropDownItems[i];
                item.Checked = (int)item.Tag == (int)priority;
            }

            processor.Priority = priority;
        }

        private void TryCancel()
        {
            if (processor.State == ProcessorState.Busy)
            {
                btnCancel.Enabled = false;
                processor.Cancel();
            }
        }

        private void TryShow()
        {
            Show();
            WindowState = FormWindowState.Normal;
            Progress_Resize(this, EventArgs.Empty);
        }

        private void processor_ConversionCanceled(object sender, Converter.Main.EventArgs.ConversionCanceled e)
        {
            lblProgress.Text = @"Canceled.";
            niProgress.Text = @"Conversion canceled";
            const string message = "Conversion canceled.\r\n" +
                "Successfully processed files: {0}\r\n" +
                "Failed processed files: {1}\r\n" +
                "Total files: {2}";
            this.ShowInfo(string.Format(message,
                    e.SuccessFileCount, e.FailureFileCount, e.SuccessFileCount + e.FailureFileCount),
                "Conversion canceled");
            Close();
        }

        private void processor_ConversionCompleted(object sender, Converter.Main.EventArgs.ConversionCompleted e)
        {
            lblProgress.Text = @"Completed.";
            niProgress.Text = @"Conversion completed";
            const string message = "Conversion completed.\r\n" +
                "Successfully processed files: {0}\r\n" +
                "Failed processed files: {1}\r\n" +
                "Total files: {2}";
            this.ShowInfo(string.Format(message,
                    e.SuccessFileCount, e.FailureFileCount, e.SuccessFileCount + e.FailureFileCount),
                "Conversion completed");
            Close();
        }

        private void processor_ConversionUpdated(object sender, Converter.Main.EventArgs.ConversionUpdated e)
        {
            const string iconMessage = "{0}% completed";
            const string labelMessage = "{0} of {1} files are processed.";
            niProgress.Text = string.Format(iconMessage, 
                Convert.ToInt32(Convert.ToDecimal(e.ProcessedSampleCount) / (e.TotalSampleCount == 0 ? e.ProcessedSampleCount : e.TotalSampleCount) * 100));
            lblProgress.Text = string.Format(labelMessage, e.ProcessedFileCount, e.TotalFileCount);
            pbProgress.Value = e.TotalSampleCount > Int32.MaxValue ?
                Convert.ToInt32(e.ProcessedSampleCount / (ulong.MaxValue / int.MaxValue)) :
                (int)e.ProcessedSampleCount;
        }

        private void processor_PreparationCanceled(object sender, Converter.Main.EventArgs.PreparationCanceled e)
        {
            niProgress.Text = @"Conversion canceled";
            lblProgress.Text = @"Canceled.";
            this.ShowInfo("Conversion was canceled.", "Canceled");
            Close();
        }

        private void processor_PreparationCompleted(object sender, Converter.Main.EventArgs.PreparationCompleted e)
        {
            niProgress.Text = @"Initialization completed.";
            lblProgress.Text = @"Initialization completed.";
            pbProgress.Style = ProgressBarStyle.Continuous;
            pbProgress.Maximum = e.TotalSampleCount > int.MaxValue ? int.MaxValue : (int)e.TotalSampleCount;
        }

        private void Progress_Shown(object sender, EventArgs e)
        {
            processor.Start();
        }

        private void Progress_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (exitState == ProgressExitState.NotExiting)
            {
                switch (processor.State)
                {
                    case ProcessorState.Busy:
                        if (this.ShowYesNoQuestion("Cancel conversion?", "Cancelation"))
                        {
                            exitState = ProgressExitState.Exiting;
                            btnCancel_Click(sender, e);
                        }
                        e.Cancel = true;
                        break;

                    case ProcessorState.Canceling:
                        e.Cancel = true;
                        break;

                    case ProcessorState.Idle:
                        e.Cancel = false;
                        break;
                }
            }
        }

        private void Progress_FormClosed(object sender, FormClosedEventArgs e)
        {
            switch (exitState)
            {
                case ProgressExitState.Exiting:
                    exitState = ProgressExitState.Exited;
                    callingForm.Close();
                    break;

                case ProgressExitState.NotExiting:
                    callingForm.Show();
                    break;
            }
        }

        private void Progress_Resize(object sender, EventArgs e)
        {
            switch (WindowState)
            {
                case FormWindowState.Normal:
                    if (previousWindowState == FormWindowState.Minimized)
                    {
                        niProgress.Visible = false;
                        Show();
                        previousWindowState = WindowState;
                    }
                    break;

                case FormWindowState.Minimized:
                    if (previousWindowState == FormWindowState.Normal)
                    {
                        niProgress.Visible = true;
                        Hide();
                        previousWindowState = WindowState;
                    }
                    break;
            }
        }

        private void niProgress_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TryShow();
            }
        }

        private void cbxPriority_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cbxPriority.DataSource != null && !string.IsNullOrEmpty(cbxPriority.ValueMember))
            {
                ChangePriority((ProcessPriorityClass)(int)cbxPriority.SelectedValue);
            }
        }

        private void tsmiPrioritySubitem_Click(object sender, EventArgs e)
        {
            ChangePriority((ProcessPriorityClass)((ToolStripMenuItem)sender).Tag);
        }

        private void tsmiCancel_Click(object sender, EventArgs e)
        {
            TryShow();
            TryCancel();
        }

        private void tsmiExit_Click(object sender, EventArgs e)
        {
            TryShow();
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            TryCancel();
        }
    }
}
