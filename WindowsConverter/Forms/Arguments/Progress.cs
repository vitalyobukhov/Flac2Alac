using System.Collections.Generic;
using System.Windows.Forms;

namespace WindowsConverter.Forms.Arguments
{
    public class Progress
    {
        public IEnumerable<string> InputFilenames { get; set; }
        public string OutputDirectory { get; set; }
        public Form CallingForm { get; set; }
    }
}
