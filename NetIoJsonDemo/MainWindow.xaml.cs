/// <summary>
/// Application       NETIO 4 Json Demo Application
/// Author            Johannes Grunedal (grunedal@gmail.com)
/// Environment       Built, run and tested with Visual Studio 2019
///                   Target framework: .Net 5
///                   Platform: x64
/// Version           0.1 (2021-03-17)
/// 
/// NOTE!             This application is tested with NETIO 4, minor changes may be needed to be run on other devices.
///                   To keep demo simple and short, data validation and error handling is reduced to a minimum.
/// 
/// Getting started                   
///                    1. Enable read/write in web config
///                       M2M API Protocols -> JSON API -> Enable JSON API -> Enable READ-WRITE
///                    2. Enter username and password (default is netio).
///                    3. Click Save Changes.
///                    4. Note current NETIO IP address.
///                    5. Build and run this application.
///                    6. Enter IP address, user and password.
///                    7. Click Connect button.
///                    8. If successful, Info field is populated with current NETIO data (if not, check above settings).
///                    9. Set selected outputs to ON/OFF using the Control buttons.
///                   10. Click Status button to read current NETIO putput status.
///                   
/// Run standalone
///                   1. Create a new C#/WPF project.
///                   2. Add NetIoDriver.cs to your project.
///                   3. Include it: using NetIo;
///                   4. Depending on your settings, one or more namespaces may have to be included (see using's below).
///                   5. Init driver: 
///                      netIoDriver = new NetIoDriver("192.168.11.250", netio, netio);
///                   6. Test connection:
///                      var agent = netIoDriver.GetAgent();
///                      if ("Error" != agent.Model)                      
///                         var model = agent.Model; // reads out all agent info
///                   7. Set output, e.g. set output 2 to on:
///                      bool isOk = netIoDriver.SetState(NetIoDriver.Output.Output_2, NetIoDriver.Action.On);
///                   8. Read output, e.g. read output 1:
///                      var output1 = netIoDriver.GetOutputState(NetIoDriver.Output.Output_1);
///                   9. Enjoy!
///                      
/// 
/// NETIO 4 Json Demo Driver.
/// Copyright © Johannes Grunedal 2021 (grunedal@gmail.com). All rights reserved.
/// Redistribution and use in source and binary forms, with or without modification, 
/// are permitted provided that the following conditions are met:
/// 
///  - Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
///  - Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer 
///    in the documentation and/or other materials provided with the distribution.
///  - Neither the name of Django nor the names of its contributors may be used to endorse or promote products derived from this 
///    software without specific prior written permission.
///    
/// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS “AS IS” AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, 
/// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT SHALL 
/// THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
/// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
/// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
/// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
/// </summary>
namespace NetIoJsonDemo
{
    using NetIo;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // variables
        NetIoDriver netIoDriver;

        // constants
        const string version = "0.1 (2021-03-24)";

        // functions
        /// <summary>
        /// Application start.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            setup();
        }

        /// <summary>
        /// Main setup.
        /// </summary>
        private void setup()
        {
            guiVersion.Content = version;
            enableControls(false);
        }

        /// <summary>
        /// Try to connect/communicate with NETIO device.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onConnectClick(object sender, RoutedEventArgs e)
        {
            // init driver
            netIoDriver = new NetIoDriver(guiIpAddress.Text, guiUsername.Text, guiPassword.Text);

            // test communication with NETIO, read out Agent info
            var agent = netIoDriver.GetAgent();

            // show result 
            if ("Error" != agent.Model)
            {
                enableControls(true);
                guiModel.Text = agent.Model;
                guiModelVersion.Text = agent.Version;
                guiJsonVersion.Text = agent.JSONVer;
                guiDeviceName.Text = agent.DeviceName;
                guiOemId.Text = agent.OemID.ToString();
                guiSerialNumber.Text = agent.SerialNumber;
                guiNoOfOutputs.Text = agent.NumOutputs.ToString();
            }
            else
            {
                MessageBox.Show($"Could not find/connect to NETIO device.\r\nMake sure IP address, username and/or password is correct.", "Connect error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Action button x clicked, set selected output and action.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void actionButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // get clicked button
                var button = ((Button)sender);

                // convert to output and action      
                var output = button.ButtonToOutput();
                var action = button.ButtonToAction();

                // set NETIO output and action        
                if (!netIoDriver.SetState(output, action))
                    throw new Exception($"Failed to set output: {Enum.GetName(output)} and action = {Enum.GetName(action)}");

                // wait for relays to set
                Thread.Sleep(666);

                // update gui with set/toggled output(s)
                updateGuiStatus();
            }
            catch
            {
                MessageBox.Show("Button click failed");
            }
        }

        /// <summary>
        /// On status button click. Show current output status info.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onStatusButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var stateList = netIoDriver.GetState();
                var status = "Current output status:\r\n\r\n";

                foreach (var state in stateList)
                {
                    status += $"ID: {state.ID}\r\n";
                    status += $"Name: {state.Name}\r\n";
                    status += $"Action: {state.Action.EnumToString()}\r\n";
                    status += $"State: {state.State.EnumToString()}\r\n\r\n";
                }

                MessageBox.Show(status, "NETIO 4 Status");
            }
            catch
            {
                MessageBox.Show("Failed to get output status!");
            }
        }

        /// <summary>
        /// Update current NETIO output status in GUI.
        /// </summary>
        private void updateGuiStatus()
        {
            var output = netIoDriver.GetState();

            rbStatusOutput_1.IsChecked = NetIoDriver.OutputStatus.On == output[0].State;
            rbStatusOutput_2.IsChecked = NetIoDriver.OutputStatus.On == output[1].State;
            rbStatusOutput_3.IsChecked = NetIoDriver.OutputStatus.On == output[2].State;
            rbStatusOutput_4.IsChecked = NetIoDriver.OutputStatus.On == output[3].State;
        }

        /// <summary>
        /// Show/hide selected GUI controls.
        /// </summary>
        /// <param name="isEnabled"></param>
        private void enableControls(bool isEnabled)
        {
            // show/hide 'info' controls         
            List<TextBox> textboxes = new List<TextBox>();
            getAllChildren<TextBox>(guiInfoGrid, textboxes);
            textboxes.ForEach(textbox => textbox.IsEnabled = isEnabled);

            // show/hide 'control' controls
            List<Button> buttons = new List<Button>();
            getAllChildren<Button>(guiControlGrid, buttons);
            buttons.ForEach(button => button.IsEnabled = isEnabled);

            List<RadioButton> radiobuttons = new List<RadioButton>();
            getAllChildren<RadioButton>(guiControlGrid, radiobuttons);
            radiobuttons.ForEach(radiobutton => radiobutton.IsEnabled = isEnabled);
        }

        private static void getAllChildren<T>(DependencyObject parent, List<T> collection) where T : DependencyObject
        {
            IEnumerable children = LogicalTreeHelper.GetChildren(parent);

            foreach (object child in children)
            {
                if (child is DependencyObject)
                {
                    DependencyObject dependencyObject = child as DependencyObject;
                    if (child is T)
                        collection.Add(child as T);

                    getAllChildren(dependencyObject, collection);
                }
            }
        }

        private void onInfoClick(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show($"Version {version}\r\n\r\nBy Johannes Grunedal 2021\r\ngrunedal@gmail.com", "NETIO 4 Json Demo Test Application", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
