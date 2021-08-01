using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using System.Threading.Tasks;
using System.Timers;

namespace Pick_n_Place_Wheel
{
    class GetSerialDevices
    {
        public static GetSerialDevices _getSerialDevices = new GetSerialDevices();

        private SerialPort _serialPort;
        string inputstring = "";
        private int con_try = 5;
        private string[] ports;

        public static System.Timers.Timer COMportQTimer;

        public static List<string> serialdevicelist = new List<string>();
        public List<string> stringstomatch = new List<string>();
        string queryString;
        public int characterstoread = 0;
        private int querycnt;
        private int porttoquery = 0;

        public async void getDevicesAsync(List<string> searchstrings, int bytestoread, string Qstr, Action callback)
        {
            stringstomatch = searchstrings;
            characterstoread = bytestoread;
            queryString = Qstr;
            Task<bool> process = new Task<bool>(startQueryProcess);
            process.Start();
            bool ret = await process;
            //close port
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                {
                    try
                    {
                        int tryturn = 0;
                        while (_serialPort.IsOpen && tryturn < con_try)
                        {
                            _serialPort.Close();
                            tryturn++;
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            callback();
            //callback

        }

        public bool getDevices(List<string> searchstrings, int bytestoread, string Qstr, Action callback)
        {
            getDevicesAsync(searchstrings, bytestoread, Qstr, callback);
            return true;
        }
        public bool startQueryProcess()
        {

            //get ports list
            getavailablePorts();
            //query ports based on specified string or strings
            currentportQueried = false;
            SetTimer();
            queryPorts();
            //return a list of matching serial devices and their matched ID
            return true;
        }

        private void switchserialport(string portname)
        {

            _serialPort = new SerialPort();
            _serialPort.PortName = portname;
            _serialPort.BaudRate = 250000;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
            _serialPort.DataReceived += DataReceivedHandler;

        }


        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {

            char[] inChar = new char[characterstoread];

            while (_serialPort.BytesToRead > 0)
            {
                try
                {
                    _serialPort.Read(inChar, 0, characterstoread);
                    inputstring = new string(inChar);


                }
                catch (Exception)
                {

                }
                foreach (string match in stringstomatch)
                {
                    if (inputstring.Contains(match))
                    {
                        string str = _serialPort.PortName + ";" + match;
                        if (!serialdevicelist.Contains(str))
                        {
                            serialdevicelist.Add(str);
                        }

                    }
                }

            }

        }

        List<string> portsqueried = new List<string>();
        private bool currentportQueried;

        private bool queryPorts()
        {
            
            int tryturn = 0;
            if (porttoquery < ports.Count())
            {

                //close port
                if (_serialPort != null)
                {
                    if (_serialPort.IsOpen)
                    {
                        tryturn = 0;
                        while (_serialPort.IsOpen && tryturn < con_try)
                        {
                            try
                            {
                                _serialPort.Close();
                            }
                            catch (Exception)
                            {


                            }
                            tryturn++;
                        }


                    }
                }
                //switch ports
                switchserialport(ports[porttoquery]);
                currentportQueried = false;
                //open port     
                tryturn = 0;
                while (!_serialPort.IsOpen && tryturn < con_try)
                {
                    try
                    {
                        _serialPort.Open();
                    }
                    catch (Exception)
                    {


                    }
                    tryturn++;
                }
                if (_serialPort.IsOpen)
                {
                    //enable timer
                    COMportQTimer.Enabled = true;
                    //countout = 0;
                    while (!currentportQueried)
                    {

                    }
                    if (porttoquery < ports.Count())
                    {
                        queryPorts();
                    }
                    else
                    {
                        porttoquery = 0;

                        COMportQTimer.Enabled = false;
                        //close port
                        if (_serialPort != null)
                        {
                            if (_serialPort.IsOpen)
                            {
                                tryturn = 0;
                                while (_serialPort.IsOpen && tryturn < con_try)
                                {
                                    try
                                    {
                                        _serialPort.Close();
                                    }
                                    catch (Exception)
                                    {


                                    }
                                    tryturn++;
                                }


                            }
                        }
                    }
                }
                else
                {
                    porttoquery += 1;
                    queryPorts();
                }



            
            }


            return true;

        }

        private void getavailablePorts()
        {
            /********************disconnect from connected serial device********************/
            if (_serialPort != null)
            {
                try
                {
                    int tryturn = 0;
                    while (_serialPort.IsOpen && tryturn < con_try)
                    {

                        _serialPort.Close();
                        tryturn++;
                    }
                }
                catch (Exception)
                {

                }
            }


            /*************************refresh connection variables***************************************/
            string[] portsrefresh = SerialPort.GetPortNames();
            portsrefresh = portsrefresh.Distinct().ToArray();
            Array.Resize<string>(ref ports, portsrefresh.Length);
            Array.Copy(portsrefresh, ports, portsrefresh.Length);
            Array.Clear(portsrefresh, 0, portsrefresh.Length);
        }


        private void COMportQTimerEvent(Object source, ElapsedEventArgs e)
        {
            if (_serialPort.IsOpen)
            {
                if (querycnt < con_try)
                {
                    try
                    {

                        querycnt++;
                        _serialPort.Write(queryString);
                        _serialPort.DtrEnable = true;
                        _serialPort.RtsEnable = true;

                    }
                    catch (Exception)
                    {

                    }
                }
                else if (querycnt == con_try)
                {
                    //set to next port
                    COMportQTimer.Enabled = false;
                    currentportQueried = true;
                    porttoquery += 1;
                    querycnt = 0;
                    //queryPorts();
                }

            }
        }

        private void SetTimer()
        {
            // Create a timer for com port query
            COMportQTimer = new System.Timers.Timer(200);
            COMportQTimer.AutoReset = true;
            COMportQTimer.Enabled = false;
            // Hook up the Elapsed event for the timer. 
            COMportQTimer.Elapsed += COMportQTimerEvent;

        }

    }
}
