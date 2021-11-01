using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CASCLib;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WoWDataStats
{
    class Generator
    {
        public static LocaleFlags firstInstalledLocale = LocaleFlags.enUS;

        CASCConfig cascConfig;
        CASCHandler cascHandler;
        WowRootHandler wowRootHandler;
        CASCFolder rootFolder;

        Dictionary<string, int> fileTypeCount;
        Dictionary<string, FileInfo> fileSizes;
        List<FileInfo> fileInfos;

        public Generator(string installPath, string product, string listfilePath)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Initializing CASCLib.");
            Console.ResetColor();
            this.cascConfig = CASCConfig.LoadLocalStorageConfig(installPath, product);
            this.cascHandler = CASCHandler.OpenStorage(this.cascConfig);
            this.cascHandler.Root.LoadListFile(listfilePath);
            this.wowRootHandler = this.cascHandler.Root as WowRootHandler;
            this.rootFolder = this.wowRootHandler.SetFlags(firstInstalledLocale, false);

            // Begin analysis //
            Console.WriteLine("Starting Analysis.");
            this.fileTypeCount = new Dictionary<string, int>();
            this.fileSizes = new Dictionary<string, FileInfo>();
            this.fileInfos = new List<FileInfo>();

            int totalFiles = CASCFile.Files.Count;
            int counter = 0;
            float progress = 0;

            foreach (KeyValuePair<ulong, CASCFile> item in CASCFile.Files)
            {
                string filePath = item.Value.FullName;
                string fileExtension = Path.GetExtension(filePath);

                // Skip unknown files
                if (fileExtension == "") continue;
                if (this.fileTypeCount.ContainsKey(fileExtension))
                {
                    this.fileTypeCount[fileExtension]++;
                }
                else
                {
                    this.fileTypeCount.Add(fileExtension, 1);
                }

                long fileSize = 0;
                try
                {
                    if (this.cascHandler.FileExists(item.Value.Hash))
                        fileSize = item.Value.GetSize(this.cascHandler);
                }
                catch { }

                if (this.fileSizes.ContainsKey(fileExtension))
                {
                    this.fileSizes[fileExtension].size += fileSize;
                }
                else
                {
                    var info = new FileInfo { extension = fileExtension, size = fileSize };
                    this.fileInfos.Add(info);
                    this.fileSizes.Add(fileExtension, info);
                }

                counter++;
                progress = ((float)counter / (float)totalFiles) * 100f;
                if (counter % 1000 == 0)
                    Console.WriteLine(progress + "%");
            }

            List<FileInfo> sortedFiles = this.fileInfos.OrderBy(o => o.size).ToList();
            sortedFiles.Reverse();

            Console.WriteLine("Making Chart.");

            Chart chart = new Chart();
            chart.Size = new Size(2048, 1024);
            ChartArea CA = chart.ChartAreas.Add("A1");
            Series S1 = chart.Series.Add("S1");
            S1.ChartType = SeriesChartType.Pie;

            for (int i = 0; i < sortedFiles.Count; i++)
            {
                S1.Points.AddXY(i + 1, sortedFiles[i].size);
                S1.Points[i].LegendText = sortedFiles[i].extension + BytesToString(sortedFiles[i].size).PadLeft(20);
            }

            chart.Legends.Add(new Legend() { Name = "test" });

            /* Set chart color and other settings as required */
            chart.BackColor = Color.Transparent;
            CA.BackColor = chart.BackColor;
            CA.Area3DStyle.Enable3D = true;

            /*Assign AntiAliasing to Graphics style for smooth edges*/
            chart.AntiAliasing = AntiAliasingStyles.Graphics;

            /* Set the image path and save the image as PNG format*/
            string imageNameAndPath = Environment.CurrentDirectory + "/Image.png";
            chart.SaveImage(imageNameAndPath, ChartImageFormat.Png);

            Console.WriteLine("Done.");
        }

        class FileInfo
        {
            public string extension;
            public long size;
        }

        String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }
    }
}
