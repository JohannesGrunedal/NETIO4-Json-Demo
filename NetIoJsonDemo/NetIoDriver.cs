namespace NetIo
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Windows;
    using System.Collections.Generic;
    using System.Linq;
    using static NetIo.NetIoDriver;

    /// <summary>
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
    public class NetIoDriver
    {
        // variables        
        readonly string ipAddress;
        readonly string user;
        readonly string password;
        readonly string url;

        // enums
        /// <summary>
        /// NETIO 4 outputs.
        /// </summary>
        public enum Output
        {
            Error, Output_1, Output_2, Output_3, Output_4, Output_All,
        }

        /// <summary>
        /// Output status.
        /// </summary>
        public enum OutputStatus
        {
            Off, On,
        }

        /// <summary>
        /// Output action.
        /// </summary>
        public enum Action
        {
            Off,      // turn OFF output
            On,       // turn ON output
            ShortOff, // short OFF delay (restart)
            ShortOn,  // short ON delay
            Toggle,   // toggle(invert the state)
            None,     // no change to this one output
            Ignore,   // ignored (return value from reading the tag)
        }

        /// <summary>
        /// Setup NETIO driver.
        /// </summary>
        /// <param name="ipAddress">NETIO ip address</param>
        /// <param name="user">json api username</param>
        /// <param name="password">json api password</param>
        public NetIoDriver(string ipAddress, string user, string password)
        {
            this.ipAddress = ipAddress;
            this.user = user;
            this.password = password;
            this.url = $"http://{this.ipAddress}/netio.json";
        }

        /// <summary>
        /// Get NETIO Agent object.
        /// </summary>
        /// <returns></returns>
        public Agent GetAgent()
        {
            return getNetIoState().Agent;
        }

        /// <summary>
        /// Get all NETIO output states.
        /// </summary>
        /// <returns></returns>
        public List<OutputFull> GetState()
        {
            return getNetIoState().Outputs;
        }

        /// <summary>
        /// Get all NETIO data.
        /// </summary>
        /// <returns></returns>
        private NetIoState getNetIoState()
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = "application/json";
                request.Credentials = new NetworkCredential(user, password);
                request.Method = "GET";
                request.Timeout = 2000;

                var response = request.GetResponse();

                using var reader = new System.IO.StreamReader(response.GetResponseStream());
                string responseBody = reader.ReadToEnd();
                return System.Text.Json.JsonSerializer.Deserialize<NetIoState>(responseBody);
            }
            catch
            {
                return new NetIoState();
            }
        }

        /// <summary>
        /// Set new NETIO state.
        /// </summary>
        /// <param name="output">selected output</param>
        /// <param name="action">selected action</param>
        /// <returns></returns>
        public bool SetState(Output output, Action action)
        {
            try
            {
                // format set data 
                NetIoSet netIoSet = new NetIoSet
                {
                    Outputs = new List<NetIo.Output>()
                };

                if (Output.Output_All == output)
                {
                    netIoSet.Outputs.Add(new NetIo.Output() { ID = Output.Output_1, Action = action, });
                    netIoSet.Outputs.Add(new NetIo.Output() { ID = Output.Output_2, Action = action, });
                    netIoSet.Outputs.Add(new NetIo.Output() { ID = Output.Output_3, Action = action, });
                    netIoSet.Outputs.Add(new NetIo.Output() { ID = Output.Output_4, Action = action, });
                }
                else
                {
                    netIoSet.Outputs.Add(new NetIo.Output() { ID = output, Action = action, });
                }

                // convert to json
                var jsonData = System.Text.Json.JsonSerializer.Serialize<NetIoSet>(netIoSet);

                // convert to bytes
                ASCIIEncoding encoding = new ASCIIEncoding();
                Byte[] jsonByteData = encoding.GetBytes(jsonData);

                // setup
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Credentials = new System.Net.NetworkCredential(user, password);
                request.ContentType = "application/json";
                request.Method = "POST";
                request.ContentLength = jsonByteData.Length;
                request.Timeout = 2000;

                // post data
                using (Stream requestStream = request.GetRequestStream())
                    requestStream.Write(jsonByteData);

                // read response
                using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                return (HttpStatusCode.OK == response.StatusCode);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Get selected output data.
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public OutputFull GetOutputState(NetIoDriver.Output output)
        {
            try
            {
                return getNetIoState().Outputs[(int)output - 1];
            }
            catch
            {
                return new OutputFull();
            }
        }
    }

    /// <summary>
    /// NETIO Json root.
    /// </summary>
    public class NetIoState
    {
        public Agent Agent { get; set; } = new Agent();

        public List<OutputFull> Outputs { get; set; } = new List<OutputFull>();
    }

    /// <summary>
    /// NETIO defiend 'Agent' object.
    /// </summary>
    public class Agent
    {
        public string Model { get; set; } = "Error";
        public string Version { get; set; } = "Error";
        public string JSONVer { get; set; } = "Error";
        public string DeviceName { get; set; } = "Error";
        public int VendorID { get; set; } = -1;
        public int OemID { get; set; } = -1;
        public string SerialNumber { get; set; } = "Error";
        public int Uptime { get; set; } = -1;
        public DateTime Time { get; set; } = DateTime.MinValue;
        public int NumOutputs { get; set; } = -1;
    }

    /// <summary>
    /// NETIO Json output object.
    /// </summary>
    public class Output
    {
        public NetIoDriver.Output ID { get; set; } = NetIoDriver.Output.Error;
        public NetIoDriver.Action Action { get; set; } = NetIoDriver.Action.None;
    }

    /// <summary>
    /// NETIO full Json output object.
    /// </summary>
    public class OutputFull : Output
    {
        public string Name { get; set; } = "NA";
        public OutputStatus State { get; set; } = OutputStatus.Off;
        public int Delay { get; set; } = -1;
    }

    /// <summary>
    /// Object for setting new output value(s).
    /// </summary>
    public class NetIoSet
    {
        public List<Output> Outputs { get; set; } = new List<Output>();
    }
}
