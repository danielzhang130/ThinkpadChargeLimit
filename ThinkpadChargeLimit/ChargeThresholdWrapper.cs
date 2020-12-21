using System.Diagnostics;
using System.IO;

namespace ThinkpadChargeLimit
{
    public class ChargeThresholdWrapper
    {
        private static readonly string EXE_PATH = "ChargeThreshold.exe";
        private static readonly string STOP_AT = "Stop at ";

        internal bool HasLimit
        {
            get
            {
                return Limit != 100;
            }
        }

        public virtual int Limit
        {
            get
            {
                int _limit = 100;

                using (Process process = new Process())
                {
                    process.StartInfo.FileName = EXE_PATH;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.Arguments = "status";
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();

                    StreamReader reader = process.StandardOutput;
                    string output = reader.ReadToEnd();

                    int index = output.IndexOf(STOP_AT);
                    if (index != -1)
                    {
                        int index_percent = output.IndexOf('%', index + STOP_AT.Length);
                        string percentage = output.Substring(index + STOP_AT.Length, index_percent - index - STOP_AT.Length);
                        _limit = int.Parse(percentage);
                    }

                    process.WaitForExit();
                }

                return _limit;
            }

            set
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = EXE_PATH;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.Arguments = value == 100 ? "off" :  ("on " + value);
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    process.WaitForExit();
                }
            }
        }
    }
}