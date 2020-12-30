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

using System.Windows;

namespace IxiWatt.Windows
{
    /// <summary>
    /// Interaction logic for DevFeeConfirmationWindow.xaml
    /// </summary>
    public partial class DevFeeConfirmationWindow : Window
    {
        public bool accepted { get; private set; } = false;
        public DevFeeConfirmationWindow()
        {
            InitializeComponent();
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            accepted = true;
            this.Close();
        }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            accepted = false;
            this.Close();
        }
    }
}
