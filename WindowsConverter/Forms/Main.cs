using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WindowsConverter.Extensions;
using System.IO;
using CodeProject.Dialog;

namespace WindowsConverter.Forms
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }



        private void Main_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Effect == DragDropEffects.Link)
            {
                e.Effect = DragDropEffects.None;

                var filenames = (IEnumerable<string>)e.Data.GetData("FileDrop");

                fdbOutputDirectory.SelectedPath = Path.GetDirectoryName(filenames.First());

                if (DlgBox.ShowDialog(fdbOutputDirectory, this) == DialogResult.OK)
                {
                    var progressForm = new Progress(new Arguments.Progress
                    {
                        InputFilenames = filenames,
                        OutputDirectory = fdbOutputDirectory.SelectedPath,
                        CallingForm = this
                    });

                    progressForm.Show();
                    Hide();
                }
            }
        }

        private void Main_DragEnter(object sender, DragEventArgs e)
        {
            if (Visible)
            {
                if (e.Data.GetFormats().Contains("FileDrop") &&
                    (e.Data.GetData("FileDrop") as IEnumerable<string>) != null)
                {
                    e.Effect = DragDropEffects.Link;
                }
                else
                {
                    this.ShowInfo("Dropped content does not includes files.", "Please provide files");
                }
            }
        }
    }
}
