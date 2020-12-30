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

using System.Management;
using System.Threading;
using System.Windows;

namespace IxiWatt
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly string devFeeConfirmationFileName = "devFeeConfirmation";

        private static Mutex _mutex = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "IxiWatt";
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                //app is already running! Exiting the application  
                Application.Current.Shutdown();
            }

            base.OnStartup(e);
        }

        public static string getHardwareId()
        {
            string id = "";
            using (var mbs = new ManagementObjectSearcher("Select ProcessorId From Win32_processor"))
            {
                ManagementObjectCollection mbsList = mbs.Get();
                foreach (ManagementObject mo in mbsList)
                {
                    id = mo["ProcessorId"].ToString();
                    break;
                }
            }
            return id;
        }
    }
}
