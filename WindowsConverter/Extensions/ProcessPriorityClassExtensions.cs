using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsConverter.Extensions
{
    static class ProcessPriorityClassExtensions
    {
        public static int ProcessPriorityClassAscendingSorter(int key)
        {
            var result = Int32.MaxValue;

            switch (key)
            {
                case 32:
                    result = 2;
                    break;

                case 64:
                    result = 0;
                    break;

                case 128:
                    result = 4;
                    break;

                case 256:
                    result = 5;
                    break;

                case 16384:
                    result = 1;
                    break;

                case 32768:
                    result = 3;
                    break;
            }

            return result;
        }
    }
}
