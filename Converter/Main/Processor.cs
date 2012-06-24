using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Converter.Main.EventArgs;
using System.IO;
using Converter.Extensions;
using Converter.BitReader;
using Converter.FileFormats.Base;
using Converter.FileFormats;
using System.Diagnostics;
using Converter.Utility;

namespace Converter.Main
{
    public class Processor
    {
        private readonly System.Timers.Timer conversionUpdateTimer;

        private readonly int workerCount;
        private readonly Thread[] workers;

        private object stateLock;
        private ProcessorState state;

        private object successFileCountLock;
        private int successFileCount;

        private object failureFileCountLock;
        private int failureFileCount;

        private object totalFileCountLock;
        private int totalFileCount;

        private object validFileCountLock;
        private int validFileCount;

        private object totalSampleCountLock;
        private ulong totalSampleCount;

        private object sharedSampleCountLock;
        private ulong sharedSampleCount;

        private object workerSampleCountLock;
        private ulong[] workerSampleCount;

        private object updateIntervalLock;
        private int updateInterval;

        private object tasksLock;
        private Stack<ProcessorTask> tasks;

        private object finishedWorkerCountLock;
        private int finishedWorkerCount;

        private object inputFilenamesLock;
        private IEnumerable<string> inputFilenames;

        private object outputDirectoryLock;
        private string outputDirectory;

        private object priorityLock;
        private ProcessPriorityClass priority;

        public Processor()
        {
            stateLock = new object();
            successFileCountLock = new object();
            failureFileCountLock = new object();
            totalFileCountLock = new object();
            validFileCountLock = new object();
            totalSampleCountLock = new object();
            sharedSampleCountLock = new object();
            workerSampleCountLock = new object();
            updateIntervalLock = new object();
            tasksLock = new object();
            finishedWorkerCountLock = new object();
            inputFilenamesLock = new object();
            outputDirectoryLock = new object();
            priorityLock = new object();

            conversionUpdateTimer = new System.Timers.Timer();
            conversionUpdateTimer.Enabled = false;
            conversionUpdateTimer.Elapsed += conversionUpdateTimer_Elapsed;
            conversionUpdateTimer.AutoReset = true;

            workerCount = Environment.ProcessorCount;
            state = ProcessorState.Idle;
            updateInterval = 1000;
            workerSampleCount = new ulong[workerCount];
            workers = new Thread[workerCount];
            tasks = new Stack<ProcessorTask>();
            inputFilenames = null;
            outputDirectory = null;
            priority = ProcessPriorityClass.Normal;
        }



        public event EventHandler<PreparationCompleted> PreparationCompleted;

        public event EventHandler<PreparationCanceled> PreparationCanceled;

        public event EventHandler<ConversionUpdated> ConversionUpdated;

        public event EventHandler<ConversionCompleted> ConversionCompleted;

        public event EventHandler<ConversionCanceled> ConversionCanceled;



        public ProcessorState State
        {
            get
            {
                lock (stateLock)
                {
                    return state;
                }
            }
            private set
            {
                lock (stateLock)
                {
                    if (!(value == ProcessorState.Canceling && state == ProcessorState.Idle))
                    {
                        state = value;
                    }
                }
            }
        }

        public int UpdateInterval
        {
            get
            {
                lock (updateIntervalLock)
                {
                    return updateInterval;
                }
            }
            set
            {
                lock (stateLock)
                {
                    if (state != ProcessorState.Idle)
                    {
                        throw new InvalidOperationException("UpdateInterval can be changed only in idle state");
                    }

                    lock (updateIntervalLock)
                    {
                        if (value <= 0)
                        {
                            throw new ArgumentOutOfRangeException("value");
                        }

                        updateInterval = value;
                    }
                }
            }
        }

        public IEnumerable<string> InputFilenames 
        {
            get
            {
                lock (inputFilenamesLock)
                {
                    return inputFilenames;
                }
            }
            set
            {
                lock (inputFilenamesLock)
                {
                    if (State != ProcessorState.Idle)
                    {
                        throw new InvalidOperationException("InputFilenames can be changed only in idle state");
                    }

                    inputFilenames = value;
                }
            }
        }

        public string OutputDirectory 
        {
            get
            {
                lock (outputDirectoryLock)
                {
                    return outputDirectory;
                }
            }
            set
            {
                lock (outputDirectoryLock)
                {
                    if (State != ProcessorState.Idle)
                    {
                        throw new InvalidOperationException("OutputDirectory can be changed only in idle state");
                    }

                    outputDirectory = value;
                }
            }
        }

        public ProcessPriorityClass Priority
        {
            get
            {
                lock (priorityLock)
                {
                    return priority;
                }
            }
            set
            {
                lock (priorityLock)
                {
                    priority = value;
                }
            }
        }



        private void Work(object workerStartArgument)
        {
            int consoleUpdateInterval = UpdateInterval;
            var workerNumber = (int)workerStartArgument;

            var consoleBuffer = new char[QAAC.ConsoleBufferSize];

            ProcessorTask task;
            while((task = GetTask()) != null)
            {
                if (State == ProcessorState.Canceling)
                {
                    do
                    {
                        FailureFileCount++;
                        SharedSampleCount += task.SampleCount;
                    }
                    while ((task = GetTask()) != null);
                    break;
                }

                var qaacStartInfo = new ProcessStartInfo
                {
                    Arguments = QAAC.ArgumentsTemplate
                        .Replace(QAAC.InputArgument, task.InputFilename)
                        .Replace(QAAC.OutputArgument, task.OutputFilename),
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    FileName = QAAC.ModuleFilename,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardError = true
                };

                Process qaacProcess;
                try
                {
                    qaacProcess = Process.Start(qaacStartInfo);
                }
                catch
                {
                    SharedSampleCount += task.SampleCount;
                    FailureFileCount++;
                    continue;
                }

                SetWorkerSampleCount(workerNumber, 0);

                while (!qaacProcess.HasExited)
                {
                    if (State == ProcessorState.Canceling)
                    {
                        qaacProcess.Kill();
                        qaacProcess.WaitForExit();
                        qaacProcess = null;
                        break;
                    }

                    qaacProcess.PriorityClass = Priority;

                    var consoleTextLength = qaacProcess.StandardError.Read(consoleBuffer, 0, QAAC.ConsoleBufferSize);
                    var consoleText = new string(consoleBuffer, 0, consoleTextLength);

                    var consoleMatches = QAAC.ConsoleRegex.Matches(consoleText);

                    if (consoleMatches.Count > 0)
                    {
                        var consoleMatch = consoleMatches[consoleMatches.Count - 1];
                        SetWorkerSampleCount(workerNumber,
                            Convert.ToUInt64(consoleMatch.Groups[QAAC.ConsoleRegexProcessedGroup].Value));
                    }

                    qaacProcess.WaitForExit(consoleUpdateInterval);
                }

                if (qaacProcess != null && qaacProcess.ExitCode == 0)
                {
                    SuccessFileCount++;
                }
                else
                {
                    FailureFileCount++;

                    if (qaacProcess == null)
                    {
                        try
                        {
                            File.Delete(string.Concat(task.OutputFilename, QAAC.TemporaryFileExtension));
                        }
                        catch
                        { }
                    }
                }

                SetWorkerSampleCount(workerNumber, 0);
                SharedSampleCount += task.SampleCount;
            }

            AddFinishedWorkerCount();
        }

        private bool Prepare()
        {
            successFileCount = 0;
            failureFileCount = 0;
            totalFileCount = 0;
            validFileCount = 0;
            totalSampleCount = 0;
            sharedSampleCount = 0;

            for (var i = 0; i < workerCount; i++)
            {
                workerSampleCount[i] = 0;
                workers[i] = new Thread(Work);
            }

            ClearFinishedWorkerCount();
            ClearTasks();

            conversionUpdateTimer.Interval = UpdateInterval;

            var validInputFilenames = Utility.Path.GetValidPaths(InputFilenames);
            failureFileCount += (InputFilenames.Count() - validInputFilenames.Count());

            var inputOutputFilenameMaps = Utility.Path.
                GetInputOutputFilenameMaps(validInputFilenames,
                FileFormatExtensions.GetAllFileFormatFilters(),
                outputDirectory);

            totalFileCount = inputOutputFilenameMaps.Count() + failureFileCount;

            for (var i = 0; i < inputOutputFilenameMaps.Count(); i++)
            {
                var filenameMap = inputOutputFilenameMaps.Skip(i).First();
                var inputFilename = filenameMap.Key;

                var fileFormat = inputFilename.GetFileFormatFromFilename();
                var inputFilenameExtension = fileFormat.GetFileExtensionFromFileFormat();
                var outputFilename = string.Concat(filenameMap.Value.Substring(0,
                        filenameMap.Value.Length - inputFilenameExtension.Length),
                    QAAC.OutputFileExtension);

                try
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputFilename));
                }
                catch
                {
                    failureFileCount++;
                    continue;
                }

                if (State == ProcessorState.Canceling)
                {
                    return false;
                }

                Stream fileStream;
                try
                {
                    fileStream = File.OpenRead(inputFilename);
                }
                catch
                {
                    failureFileCount++;
                    continue;
                }

                Reader bitReader;
                try
                {
                    bitReader = new Reader(fileStream);
                }
                catch
                {
                    fileStream.Dispose();
                    failureFileCount++;
                    continue;
                }

                ulong sampleCount;
                ISampleCounterBase<IHeaderParserBase<IHeaderBase>, IHeaderBase> sampleCounter;

                switch (fileFormat)
                {
                    case FileFormat.Flac:
                        var flacHeaderParser = new FileFormats.Flac.HeaderParser(bitReader);
                        sampleCounter = new FileFormats.Flac.SampleCounter(flacHeaderParser);

                        try
                        {
                            sampleCount = sampleCounter.GetSampleCount();
                        }
                        catch
                        {
                            sampleCounter.Dispose();
                            failureFileCount++;
                            continue;
                        }

                        sampleCounter.Dispose();
                        break;

                    case FileFormat.Wave:
                        var waveHeaderParser = new FileFormats.Wave.HeaderParser(bitReader);

                        FileFormats.Wave.Headers.HeaderBase waveHeader;
                        try
                        {
                            waveHeader = waveHeaderParser.Parse();

                            if (waveHeader.Format.FormatCode != FileFormats.Wave.Headers.Chunks.FormatCode.PCM &&
                                waveHeader.Format.FormatCode != FileFormats.Wave.Headers.Chunks.FormatCode.IEEEFloat)
                            {
                                throw new InvalidDataException("Only PCM and IEEEFloat formats are supported");
                            }
                        }
                        catch
                        {
                            waveHeaderParser.Dispose();
                            failureFileCount++;
                            continue;
                        }

                        sampleCounter = new FileFormats.Wave.SampleCounter(waveHeaderParser);
                        try
                        {
                            sampleCount = sampleCounter.GetSampleCount();
                        }
                        catch
                        {
                            sampleCounter.Dispose();
                            failureFileCount++;
                            continue;
                        }

                        sampleCounter.Dispose();
                        break;

                    default:
                        bitReader.Dispose();
                        continue;
                }

                totalSampleCount += sampleCount;

                AddTask(new ProcessorTask
                {
                    InputFilename = filenameMap.Key,
                    OutputFilename = outputFilename,
                    SampleCount = sampleCount
                });

                validFileCount++;
            }

            PreparationCompleted(this,
                new PreparationCompleted
                {
                    TotalFileCount = validFileCount,
                    TotalSampleCount = totalSampleCount
                });

            return true;
        }

        private void ValidateStart()
        {
            if (!QAAC.CheckDependencies())
            {
                State = ProcessorState.Idle;
                throw new FileNotFoundException("QAAC dependencies weren't found");
            }

            if (InputFilenames == null)
            {
                State = ProcessorState.Idle;
                throw new ArgumentNullException("InputFilenames");
            }

            if (OutputDirectory == null)
            {
                State = ProcessorState.Idle;
                throw new ArgumentNullException("OutputDirectory");
            }

            if (!Directory.Exists(OutputDirectory))
            {
                State = ProcessorState.Idle;
                throw new IOException("OutputDirectory does not exists");
            }
        }

        public void Start()
        {
            if (State == ProcessorState.Idle)
            {
                State = ProcessorState.Busy;

                ValidateStart();

                if (Prepare())
                {
                    for (var i = 0; i < workers.Length; i++)
                    {
                        workers[i].Start(i);
                    }

                    conversionUpdateTimer.Start();
                }
                else
                {
                    State = ProcessorState.Idle;
                    PreparationCanceled(this, new PreparationCanceled());
                }
            }
            else
            {
                throw new InvalidOperationException("Processor can be launched only from idle state.");
            }
        }

        public void Cancel()
        {
            if (State == ProcessorState.Busy)
            {
                //conversionUpdateTimer.Stop();
                State = ProcessorState.Canceling;
            }
            else
            {
                throw new InvalidOperationException("Processor can be canceled only from busy state.");
            }
        }

        //public void Terminate()
        //{
        //    if (State == ProcessorState.Busy || State == ProcessorState.Canceling)
        //    {
        //        conversionUpdateTimer.Stop();

        //        foreach (var worker in workers)
        //        {
        //            worker.Abort();
        //        }

        //        State = ProcessorState.Idle;
        //    }
        //    else
        //    {
        //        throw new InvalidOperationException("Processor can be canceled only from busy or canceling state.");
        //    }
        //}

        private ulong GetWorkerSampleCount(int workerNumber)
        {
            lock (workerSampleCountLock)
            {
                return workerSampleCount[workerNumber];
            }
        }

        private void SetWorkerSampleCount(int workerNumber, ulong sampleCount)
        {
            lock (workerSampleCountLock)
            {
                workerSampleCount[workerNumber] = sampleCount;
            }
        }

        private ProcessorTask GetTask()
        {
            lock (tasksLock)
            {
                return tasks.Count == 0 ? null : tasks.Pop();
            }
        }

        private void AddTask(ProcessorTask task)
        {
            lock (tasksLock)
            {
                tasks.Push(task);
            }
        }

        private void ClearTasks()
        {
            lock (tasksLock)
            {
                tasks.Clear();
            }
        }

        private void AddFinishedWorkerCount()
        {
            lock (finishedWorkerCountLock)
            {
                finishedWorkerCount++;

                if (finishedWorkerCount == workerCount)
                {
                    conversionUpdateTimer.Stop();

                    var previousState = State;
                    State = ProcessorState.Idle;

                    if (previousState == ProcessorState.Busy)
                    {
                        ConversionCompleted(this,
                            new ConversionCompleted
                            {
                                FailureFileCount = FailureFileCount,
                                SuccessFileCount = SuccessFileCount
                            });
                    }
                    else
                    {
                        ConversionCanceled(this,
                            new ConversionCanceled
                            {
                                FailureFileCount = FailureFileCount,
                                SuccessFileCount = SuccessFileCount
                            });
                    }
                }
            }
        }

        private void ClearFinishedWorkerCount()
        {
            lock (finishedWorkerCountLock)
            {
                finishedWorkerCount = 0;
            }
        }

        private void conversionUpdateTimer_Elapsed(object sender, System.EventArgs e)
        {
            var processedFileCount = SuccessFileCount + FailureFileCount - (TotalFileCount - ValidFileCount);
            ulong processedSampleCount = SharedSampleCount;
            for (var i = 0; i < workerCount; i++)
            {
                processedSampleCount += GetWorkerSampleCount(i);
            }

            ConversionUpdated(this,
                new ConversionUpdated
                {
                    TotalFileCount = validFileCount,
                    ProcessedFileCount = processedFileCount,
                    TotalSampleCount = TotalSampleCount,
                    ProcessedSampleCount = processedSampleCount
                });
        }


        private int SuccessFileCount
        {
            get
            {
                lock (successFileCountLock)
                {
                    return successFileCount;
                }
            }
            set
            {
                lock (successFileCountLock)
                {
                    successFileCount = value;
                }
            }
        }

        private int FailureFileCount
        {
            get
            {
                lock (failureFileCountLock)
                {
                    return failureFileCount;
                }
            }
            set
            {
                lock (failureFileCountLock)
                {
                    failureFileCount = value;
                }
            }
        }

        private int TotalFileCount
        {
            get
            {
                lock (totalFileCountLock)
                {
                    return totalFileCount;
                }
            }
            set
            {
                lock (totalFileCountLock)
                {
                    totalFileCount = value;
                }
            }
        }

        private int ValidFileCount
        {
            get
            {
                lock (validFileCountLock)
                {
                    return validFileCount;
                }
            }
            set
            {
                lock (validFileCountLock)
                {
                    validFileCount = value;
                }
            }
        }

        private ulong TotalSampleCount
        {
            get
            {
                lock (totalSampleCountLock)
                {
                    return totalSampleCount;
                }
            }
            set
            {
                lock (totalSampleCountLock)
                {
                    totalSampleCount = value;
                }
            }
        }

        private ulong SharedSampleCount
        {
            get
            {
                lock (sharedSampleCountLock)
                {
                    return sharedSampleCount;
                }
            }
            set
            {
                lock (sharedSampleCountLock)
                {
                    sharedSampleCount = value;
                }
            }
        }
    }
}
