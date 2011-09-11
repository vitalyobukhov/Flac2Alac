using System;
using System.IO;
using System.Linq;
using System.Threading;
using Converter.Main;
using System.Diagnostics;

namespace ConsoleConverter
{
    class ConsoleConverter
    {
        private static Processor processor;
        private static ManualResetEvent processorFinishedEvent;

        private static object updateAvailabilityLock;
        private static bool updateAvailability;



        private static void Init(string[] args)
        {
            processor = new Processor
            {
                InputFilenames = args.Take(args.Length - 1).ToArray(),
                OutputDirectory = args.Last(),
                UpdateInterval = 250,
                Priority = ProcessPriorityClass.Normal
            };
            processor.PreparationCompleted += processor_PreparationCompleted;
            processor.PreparationCanceled += processor_PreparationCanceled;
            processor.ConversionCompleted += processor_ConversionCompleted;
            processor.ConversionCanceled += processor_ConversionCanceled;
            processor.ConversionUpdated += processor_ConversionUpdated;

            processorFinishedEvent = new ManualResetEvent(false);

            updateAvailabilityLock = new object();
            updateAvailability = true;

            Console.CancelKeyPress += Console_CancelKeyPress;
        }



        private static bool TryCaptureUpdateAvailability()
        {
            var result = false;

            lock (updateAvailabilityLock)
            {
                if (updateAvailability)
                {
                    updateAvailability = false;
                    result = true;
                }
            }

            return result;
        }

        private static bool TryFreeProcessorAvailability()
        {
            var result = false;

            lock (updateAvailabilityLock)
            {
                if (!updateAvailability)
                {
                    updateAvailability = true;
                    result = true;
                }
            }

            return result;
        }

        private static string GetProcessedFileCount(int proccessed, int total)
        {
            return string.Format("Files processed: {0} / {1}", proccessed, total);
        }

        private static string GetPercentage(ulong proccessed, ulong total)
        {
            return string.Format("Total progress: {0}%", Convert.ToByte((double)proccessed / total * 100));
        }

        private static void ClearProgressMessage()
        {
            var whitespace = new string(' ', Console.BufferWidth);
            Console.Write(whitespace);
            Console.Write(whitespace);
            Console.CursorTop = Console.CursorTop - 2;
        }

        private static void processor_ConversionUpdated(object sender, Converter.Main.EventArgs.ConversionUpdated e)
        {
            if (TryCaptureUpdateAvailability())
            {
                Console.WriteLine(GetProcessedFileCount(e.ProcessedFileCount, e.TotalFileCount));
                Console.WriteLine(GetPercentage(e.ProcessedSampleCount, e.TotalSampleCount));
                Console.CursorLeft = 0;
                Console.CursorTop = Console.CursorTop - 2;
                TryFreeProcessorAvailability();
            }
        }

        private static void processor_ConversionCanceled(object sender, Converter.Main.EventArgs.ConversionCanceled e)
        {
            ClearProgressMessage();
            Console.WriteLine("Conversion canceled.");
            Console.WriteLine("Processed file count: {0}", e.SuccessFileCount + e.FailureFileCount);
            Console.WriteLine("Successfully processed file count: {0}", e.SuccessFileCount);
            Console.WriteLine("Failed to process file count: {0}", e.FailureFileCount);
            processorFinishedEvent.Set();
        }

        private static void processor_ConversionCompleted(object sender, Converter.Main.EventArgs.ConversionCompleted e)
        {
            ClearProgressMessage();
            Console.WriteLine("Conversion completed.");
            Console.WriteLine("Processed file count: {0}", e.SuccessFileCount + e.FailureFileCount);
            Console.WriteLine("Successfully processed file count: {0}", e.SuccessFileCount);
            Console.WriteLine("Failed to process file count: {0}", e.FailureFileCount);
            processorFinishedEvent.Set();
        }

        private static void processor_PreparationCanceled(object sender, Converter.Main.EventArgs.PreparationCanceled e)
        {
            Console.WriteLine("Conversion canceled.");
            processorFinishedEvent.Set();
        }

        private static void processor_PreparationCompleted(object sender, Converter.Main.EventArgs.PreparationCompleted e)
        {
            Console.WriteLine("Done.");
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (processor.State == ProcessorState.Busy)
            {
                e.Cancel = true;
                processor.Cancel();
            }
        }

        private static bool ValidateArgs(string[] args)
        {
            var result = false;

            if (args == null || args.Length < 2)
            {
                goto EndValidateArgs;
            }

            if (!args.Take(args.Length - 1).All(p => File.Exists(p) || Directory.Exists(p)))
            {
                goto EndValidateArgs;
            }

            if (!Directory.Exists(args.Last()))
            {
                goto EndValidateArgs;
            }

            result = true;

        EndValidateArgs:
            return result;
        }



        private static void Main(string[] args)
        {
            if (ValidateArgs(args))
            {
                Init(args);

                Console.Write("Initializing. Please wait... ");

                processor.Start();

                processorFinishedEvent.WaitOne();
            }
            else
            {
                const string usageText =
                    "Usage: ConsoleConverter.exe <Input file and directory list> <Output directory>";
                Console.WriteLine(usageText);
            }
        }
    }
}
