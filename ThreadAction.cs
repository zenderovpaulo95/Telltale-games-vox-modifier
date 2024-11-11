using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Telltale_games_vox_modifier
{
    public delegate void ProgressHandler(int i);
    public delegate void ReportHandler(string report);

    public class ThreadAction
    {
        public event ProgressHandler Progress;
        public event ProgressHandler Maximum;
        public event ReportHandler Report;

        public void DoImport(object parameters)
        {
            List<string> param = parameters as List<string>;
            string inputDir = param[0];
            string outputDir = param[1];
            byte[] key = Methods.stringToKey(param[2]);
            bool needEncrypt = param[3].ToLower() == "true";

            DirectoryInfo di = new DirectoryInfo(inputDir);
            FileInfo[] fi = di.GetFiles("*.vox", SearchOption.TopDirectoryOnly);

            Maximum(fi.Length);

            for (int i = 0; i < fi.Length; i++)
            {
                if (File.Exists(fi[i].FullName.Remove(fi[i].FullName.Length - 3, 3) + "wav"))
                {
                    string result = Actions.Repack(fi[i], outputDir, key, needEncrypt);
                    Report(result);
                }

                Progress(i + 1);
            }
        }

        public void DoExport(object parameters)
        {
            List<string> param = parameters as List<string>;
            string inputDir = param[0];
            string outputDir = param[1];
            byte[] key = Methods.stringToKey(param[2]);
            bool needDecrypt = param[3].ToLower() == "true";

            DirectoryInfo di = new DirectoryInfo(inputDir);
            FileInfo[] fi = di.GetFiles("*.vox", SearchOption.TopDirectoryOnly);

            Maximum(fi.Length);

            for (int i = 0; i < fi.Length; i++)
            {
                string result = Actions.Unpack(fi[i], outputDir, key, needDecrypt);
                Report(result);
                Progress(i + 1);
            }
        }
    }
}
