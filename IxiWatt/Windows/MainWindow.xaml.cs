// Copyright (C) 2017-2020 Ixian OU
// This file is part of IxiWatt - www.github.com/ProjectIxian/IxiWatt
//
// IxiWatt is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// IxiWatt is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with IxiWatt.  If not, see <https://www.gnu.org/licenses/>.

using Base58Check;
using IXICore;
using IxiWatt.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace IxiWatt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string devAddress = "4GrwLBJBxEjQSnkDipfCnCKvdp4hUNCWDo6yJFGtexwhYPJygojxGZoqcGUdTzLHe";
        bool miningForDev = false;
        bool miningForDevInitializing = false;
        DateTime startedRunning = DateTime.Now;

        int baseAcc = 0;
        int baseRej = 0;
        int baseBlocks = 0;

        string settingsFileName = "settings.cfg";
        Dictionary<string, string> settings = new Dictionary<string, string>();
        Process minerProcess = null;
        bool starting = false;

        const int devMineInterval = 12120; // 202 minutes
        const int devMineTime = 60; // 1 minute

        public MainWindow()
        {
            Loaded += OnLoaded;
            InitializeComponent();
            cbPoolSelect.SelectedIndex = 0;
            cbHasher.SelectedIndex = 0;
            tbLog.Text = "Log:\n";
            bStart.Content = FindResource("toggle_disabled");
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            string hasher = identifyGPU();
            setHasherCheckBox(hasher);
            loadSettings();
            checkDevFeeConfirmation();
        }

        private void setHasherCheckBox(string hasher)
        {
            switch (hasher)
            {
                case "AMD":
                    cbHasher.SelectedIndex = 1;
                    break;

                case "NVIDIA":
                    cbHasher.SelectedIndex = 2;
                    break;

                default:
                    cbHasher.SelectedIndex = 0;
                    break;
            }
        }

        private void checkDevFeeConfirmation()
        {

            if (File.Exists(App.devFeeConfirmationFileName))
            {
                if (File.ReadAllText(App.devFeeConfirmationFileName) == App.getHardwareId())
                {
                    return;
                }
                File.Delete(App.devFeeConfirmationFileName);
            }

            AboutWindow about = new AboutWindow();
            about.Owner = this;
            about.ShowDialog();

            DevFeeConfirmationWindow window = new DevFeeConfirmationWindow();
            window.Owner = this;
            window.ShowDialog();

            if (window.accepted)
            {
                File.WriteAllText(App.devFeeConfirmationFileName, App.getHardwareId());
            }
            else
            {
                this.Close();
            }
        }

        private void loadSettings()
        {
            if (!File.Exists(settingsFileName))
            {
                return;
            }
            string[] lines = File.ReadAllLines(settingsFileName);
            foreach (string line in lines)
            {
                string[] option = line.Split('=');
                if (option.Length < 2)
                {
                    continue;
                }
                string key = option[0].Trim(new char[] { ' ', '\t', '\r', '\n' });
                string value = option[1].Trim(new char[] { ' ', '\t', '\r', '\n' });

                if (key.StartsWith(";"))
                {
                    continue;
                }

                settings.Add(key, value);
            }

            if (settings.ContainsKey("PoolURL"))
            {
                tbPoolURL.Text = settings["PoolURL"];
            }
            if (settings.ContainsKey("WalletAddress"))
            {
                tbWalletAddress.Text = settings["WalletAddress"];
            }
            if (settings.ContainsKey("Hasher"))
            {
                setHasherCheckBox(settings["Hasher"]);
            }
            if (settings.ContainsKey("Intensity"))
            {
                sIntensity.Value = Double.Parse(settings["Intensity"]);
            }
        }

        private void saveSettings()
        {
            string[] lines = new string[settings.Count];
            int i = 0;
            foreach (var setting in settings)
            {
                lines[i] = setting.Key + " = " + setting.Value;
                i++;
            }
            File.WriteAllLines(settingsFileName, lines);
        }

        private void fetchIxiMiner()
        {
            if (Directory.Exists("miner"))
            {
                if (Directory.Exists(Path.Combine("miner", "miner")) && File.Exists(Path.Combine("miner", "miner", "iximiner.exe")))
                {
                    log("Ixi Miner already available, not downloading...");
                    return;
                }
                Directory.Delete("miner", true);
            }

            log("Fetching Ixi Miner...");
            using (var client = new WebClient())
            {
                Directory.CreateDirectory("miner");
                client.DownloadFile("https://github.com/bogdanadnan/iximiner/releases/download/v0.2.0alpha/iximiner_v0.2.0_08.08.2019_windows_10.zip", "miner/iximiner08082019Win.zip");
                log("Unpacking Ixi Miner...");
                ZipFile.ExtractToDirectory("miner/iximiner08082019Win.zip", "miner");
                Directory.Move("miner/iximiner_v0.2.0_08.08.2019_windows_10", "miner/miner");
                File.Delete("miner/iximiner08082019Win.zip");
            }
        }

        private string identifyGPU()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DisplayConfiguration"))
            {
                foreach (ManagementObject mo in searcher.Get())
                {
                    foreach (PropertyData property in mo.Properties)
                    {
                        if (property.Name == "Description")
                        {
                            string graphicsCard = property.Value.ToString().ToLower();
                            log("Found GPU: " + graphicsCard);
                            if (graphicsCard.Contains("nvidia"))
                            {
                                return "NVIDIA";
                            }
                            if (graphicsCard.Contains("amd") || graphicsCard.Contains("advanced micro devices"))
                            {
                                return "AMD";
                            }
                        }
                    }
                }
            }
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                foreach (ManagementObject mo in searcher.Get())
                {
                    foreach (PropertyData property in mo.Properties)
                    {
                        if (property.Name == "Name")
                        {
                            string graphicsCard = property.Value.ToString().ToLower();
                            log("Found GPU: " + graphicsCard);
                            if (graphicsCard.Contains("nvidia"))
                            {
                                return "NVIDIA";
                            }
                            if (graphicsCard.Contains("amd") || graphicsCard.Contains("advanced micro devices"))
                            {
                                return "AMD";
                            }
                        }
                    }
                }
            }
            return null;
        }

        private void startIxiMiner(string pool, string wallet, string hasher, string intensity)
        {
            if (minerProcess != null && !minerProcess.HasExited)
            {
                log("Ixi Miner already running...");
                return;
            }
            string gpuDriver = null;
            switch (hasher)
            {
                case "AMD":
                    gpuDriver = "OPENCL";
                    break;

                case "NVIDIA":
                    gpuDriver = "CUDA";
                    break;
            }
            log("Starting Ixi Miner using driver '" + gpuDriver + "'...");
            minerProcess = new Process();
            minerProcess.StartInfo.WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "miner", "miner");
            minerProcess.StartInfo.FileName = Path.Combine(Directory.GetCurrentDirectory(), "miner", "miner", "iximiner.exe");
            string extraParams = " --cpu-intensity " + intensity + " --gpu-intensity 0";
            if (gpuDriver != null)
            {
                extraParams = " --force-gpu-optimization " + gpuDriver + " --cpu-intensity 0 --gpu-intensity " + intensity;
            }
            minerProcess.StartInfo.Arguments = "--mode miner --pool " + pool + " --wallet " + wallet + extraParams;
            minerProcess.StartInfo.UseShellExecute = false;
            minerProcess.StartInfo.RedirectStandardOutput = true;
            minerProcess.StartInfo.CreateNoWindow = true;

            minerProcess.OutputDataReceived += minerOutput;

            log("Start " + minerProcess.StartInfo.FileName + " " + minerProcess.StartInfo.Arguments);

            try
            {
                minerProcess.Start();
                if (!minerProcess.HasExited)
                {
                    minerProcess.BeginOutputReadLine();
                }
                startedRunning = DateTime.Now;
            }
            catch (Exception e)
            {
                log("Exception occured while starting miner process: " + e);
            }
        }

        private void stopIxiMiner()
        {
            if (minerProcess == null)
            {
                log("Ixi Miner already stopped...");
                return;
            }
            try
            {
                minerProcess.Kill();
                minerProcess.Dispose();
            }
            catch (Exception)
            {

            }

            this.Dispatcher.Invoke(() =>
            {
                tbHashRate.Text = "Hash Rate: 0kH/s";
            });

            minerProcess = null;
            log("Ixi Miner stopped");
        }

        private void minerOutput(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (outLine.Data == null)
            {
                return;
            }
            try
            {
                log(outLine.Data);
                string[] out_split = outLine.Data.Split('|');
                if (out_split.Length <= 5)
                {
                    return;
                }

                int hrIndex = 1;
                int accIndex = out_split.Length - 4;
                int rejIndex = out_split.Length - 3;
                int solvedBlocksIndex = out_split.Length - 2;

                if (out_split[solvedBlocksIndex].Trim().StartsWith("Block"))
                {
                    if (miningForDev)
                    {
                        if (miningForDevInitializing)
                        {
                            log("Mining for dev initialized.");
                            miningForDevInitializing = false;
                            startedRunning = DateTime.Now;
                        }
                    }
                    return;
                }

                int acc = Int32.Parse(out_split[accIndex].Trim());
                int rej = Int32.Parse(out_split[rejIndex].Trim());
                int blocks = Int32.Parse(out_split[solvedBlocksIndex].Trim());

                this.Dispatcher.Invoke(() =>
                {
                    tbHashRate.Text = "Hash Rate: " + out_split[hrIndex].Trim() + "H/s";
                    tbAcceptedShares.Text = "Acc: " + (baseAcc + acc);
                    tbRejectedShares.Text = "Rej: " + (baseRej + rej);
                    tbBlocks.Text = "Blocks: " + (baseBlocks + blocks);
                });

                if (miningForDev)
                {
                    if ((DateTime.Now - startedRunning).TotalSeconds > devMineTime)
                    {
                        log("Switching to mining for user.");
                        baseAcc += acc;
                        baseRej += rej;
                        baseBlocks += blocks;
                        miningForDev = false;
                        miningForDevInitializing = false;

                        stopIxiMiner();
                        startIxiMiner(settings["PoolURL"], settings["WalletAddress"], settings["Hasher"], settings["Intensity"]);
                    }
                }
                else
                {
                    if ((DateTime.Now - startedRunning).TotalSeconds > devMineInterval)
                    {
                        log("Switching to mining for dev.");
                        baseAcc += acc;
                        baseRej += rej;
                        baseBlocks += blocks;
                        miningForDev = true;
                        miningForDevInitializing = true;

                        stopIxiMiner();
                        startIxiMiner(settings["PoolURL"], devAddress, settings["Hasher"], settings["Intensity"]);
                    }
                }
            }
            catch (Exception e)
            {
                log(e.ToString());
            }
        }

        private void PoolSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null || e.AddedItems.Count == 0)
            {
                return;
            }
            System.Windows.Controls.ComboBoxItem cbi = (System.Windows.Controls.ComboBoxItem)e.AddedItems[0];
            if ((string)cbi.Content == "Custom" || cbi.Content == null)
            {
                tbPoolURL.Text = "";
            }
            else
            {
                tbPoolURL.Text = (string)cbi.Content;
            }
        }

        private void log(string text)
        {
            this.Dispatcher.Invoke(() =>
            {
                tbLog.AppendText(text + "\n");
                tbLog.ScrollToEnd();
            });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            stopIxiMiner();
            base.OnClosing(e);
        }

        private void intensity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            tbIntensity.Text = "Mining Intensity: " + e.NewValue + "%";
        }

        private void enableControls()
        {
            bStart.Content = FindResource("toggle_off");
            lMiningStatus.Content = "Mining is OFF";
            cbPoolSelect.IsEnabled = true;
            tbPoolURL.IsEnabled = true;
            tbWalletAddress.IsEnabled = true;
            cbHasher.IsEnabled = true;
            sIntensity.IsEnabled = true;
        }

        private void disableControls()
        {
            bStart.Content = FindResource("toggle_on");
            lMiningStatus.Content = "Mining is ON";
            cbPoolSelect.IsEnabled = false;
            tbPoolURL.IsEnabled = false;
            tbWalletAddress.IsEnabled = false;
            cbHasher.IsEnabled = false;
            sIntensity.IsEnabled = false;
        }

        private void bStart_Click(object sender, RoutedEventArgs e)
        {
            if (starting)
            {
                return;
            }
            if ((string)lMiningStatus.Content == "Mining is ON")
            {
                stopIxiMiner();
                enableControls();
            }
            else
            {
                if (tbPoolURL.Text == "")
                {
                    log("Please enter Pool URL.");
                    bStart.Content = FindResource("toggle_disabled");
                    lMiningStatus.Content = "Mining is OFF";
                    return;
                }
                if (tbWalletAddress.Text == "")
                {
                    log("Please enter IxiCash Wallet Address.");
                    bStart.Content = FindResource("toggle_disabled");
                    lMiningStatus.Content = "Mining is OFF";
                    return;
                }
                var t = this;
                string poolUrl = tbPoolURL.Text;
                string walletAddress = tbWalletAddress.Text;
                string hasher = (string)((System.Windows.Controls.ComboBoxItem)cbHasher.SelectedItem).Content;
                string intensity = sIntensity.Value.ToString();

                if (!Address.validateChecksum(Base58CheckEncoding.DecodePlain(walletAddress)))
                {
                    log("Invalid IxiCash Wallet Address.");
                    bStart.Content = FindResource("toggle_disabled");
                    lMiningStatus.Content = "Mining is OFF";
                    return;
                }

                settings["PoolURL"] = poolUrl;
                settings["WalletAddress"] = walletAddress;
                settings["Hasher"] = hasher;
                settings["Intensity"] = intensity;
                saveSettings();

                new Thread(() => {
                    if (starting)
                    {
                        return;
                    }
                    starting = true;
                    try
                    {
                        t.Dispatcher.Invoke(() =>
                        {
                            disableControls();
                        });
                        fetchIxiMiner();
                        startIxiMiner(poolUrl, walletAddress, hasher, intensity);
                    }
                    catch (Exception ex)
                    {
                        log("Exception occured while starting the miner: " + ex.ToString());
                        t.Dispatcher.Invoke(() =>
                        {
                            enableControls();
                        });
                    }
                    starting = false;
                }).Start();
            }
        }

        private void bAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow about = new AboutWindow();
            about.Owner = this;
            about.ShowDialog();
        }
    }
}
