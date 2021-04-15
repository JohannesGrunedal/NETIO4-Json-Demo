# NETIO4-Json-Demo

Application
Simple demo application for testing NETIO 4 power switch using Json.

Author
© 2021 Johannes Grunedal (grunedal@gmail.com)

Environment
Built, run and tested with Visual Studio 2019
Target framework: .Net 5
Platform: x64

NOTE!
This application is tested with NETIO 4, minor changes may be needed to be run on other devices.
To keep demo simple and short, data validation and error handling is reduced to a minimum.

Getting started
 1. Enable read/write in web config M2M API Protocols -> JSON API -> Enable JSON API -> Enable READ-WRITE
 2. Enter username and password (default is netio).
 3. Click Save Changes.
 4. Note current NETIO IP address.
 5. Build and run this application.
 6. Enter IP address, user and password.
 7. Click Connect button.
 8. If successful, Info field is populated with current NETIO data (if not, check above settings).
 9. Set selected outputs to ON/OFF using the Control buttons.
10. Click Status button to read current NETIO putput status.

Run standalone
1. Create a new C#/WPF project.
2. Add NetIoDriver.cs to your project.
3. Include it: using NetIo;
4. Depending on your settings, one or more namespaces may have to be included (see using's below).
5. Init driver: netIoDriver = new NetIoDriver("192.168.11.250", netio, netio);
6. Test connection:
   var agent = netIoDriver.GetAgent();
   if ("Error" != agent.Model)   
     var model = agent.Model; // reads out all agent info
7. Set output, e.g. set output 2 to on:
   bool isOk = netIoDriver.SetState(NetIoDriver.Output.Output_2, NetIoDriver.Action.On);
8. Read output, e.g. read output 1:
   var output1 = netIoDriver.GetOutputState(NetIoDriver.Output.Output_1);
9. Enjoy!


![Screenshot_1](https://user-images.githubusercontent.com/25680930/114847324-e0d01a80-9ddd-11eb-9153-52e04ee76d74.jpg)
