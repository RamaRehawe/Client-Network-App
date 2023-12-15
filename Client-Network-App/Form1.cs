using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using Client_Network_App.Utils;

namespace Client_Network_App
{
    public partial class winFormClient : Form
    {
        //Constants
        private const string CRLF = "\r\n";
        private const string LOCALHOST  = "127.0.0.1";
        private const int DEFAULT_PORT = 5000;

        //Fields
        private IPAddress _serverIpAddress;
        private int _port;
        private TcpClient _client;



        public winFormClient()
        {
            InitializeComponent();
            _serverIpAddress = GetIpAddress(_ipAddressTextBox.Text);
            _port = GetPort(_portTextBox.Text);
            _connectButton.Enabled = true;
            _disconnectButton.Enabled = false;
            _sendCommandButton.Enabled = false;
        }


        #region Event Handlre

        private void ConnectButtonHandler(object sender, EventArgs e)
        {
            try
            {
                _client = new TcpClient(_serverIpAddress.ToString(), _port);
                Thread t = new Thread(ProcessClientTransaction);
                t.IsBackground = true;
                t.Start(_client);

                _connectButton.Enabled = false;
                _disconnectButton.Enabled = true;
                _sendCommandButton.Enabled = true;

            }
            catch (Exception ex)
            {
                _statusTextBox.Text += CRLF + "Problem connecting to server.";
                _statusTextBox.Text += CRLF + ex.ToString();
            }
        }
        private void DisconnectedButonHandler(object sender, EventArgs e)
        {
            DisconnectFromServer();
        }

        private void SendCommandButonHandler(object sender, EventArgs e) 
        {
            try
            {
                if(_client.Connected)
                {
                    StreamWriter writer = new StreamWriter(_client.GetStream());
                    writer.WriteLine(_commandTextBox.Text);
                    writer.Flush();
                    _statusTextBox.Text += CRLF + "Command Sent to Server: " + _commandTextBox.Text;
                    _commandTextBox.Text = string.Empty;
                }

            }
            catch (Exception ex)
            {
                _statusTextBox.Text += CRLF + "Problem sending command to server...";
                _statusTextBox.Text += CRLF + ex.ToString();
            }
        }

        #endregion Event Handler

        private void ProcessClientTransaction(object tcpClient)
        {
            TcpClient client = (TcpClient) tcpClient;
            string input = string.Empty;
            StreamReader reader = null;
            StreamWriter writer = null;
            try
            {
                reader = new StreamReader(client.GetStream());
                writer = new StreamWriter(client.GetStream());

                // Tell the server we've connected
                writer.WriteLine("Hello from a client! Ready to do your bidding!");
                writer.Flush();


                while(client.Connected)
                {
                    input = reader.ReadLine(); // block here until we receive something from the server.
                    if(input == null)
                    {
                        DisconnectFromServer();
                    }
                    else
                    {
                        switch(input)
                        {
                            default:
                                {
                                    _statusTextBox.InvokeEx(stb => stb.Text += CRLF + "Recieved from server: " + input);
                                    break;
                                }
                        }
                    }
                }
            }catch(Exception ex)
            {
                _statusTextBox.InvokeEx(stb 
                    => stb.Text += CRLF + "Problem communicating with the server. Connection may have been intentionally disconnected.");
                _statusTextBox.InvokeEx(stb => stb.Text += CRLF +  ex.ToString());
            }
            _disconnectButton.InvokeEx(dcb => dcb.Enabled = false);
            _connectButton.InvokeEx(cb => cb.Enabled = true);
            _statusTextBox.InvokeEx(stb => stb.Text = string.Empty);
        } // End Process Transaction

        private void DisconnectFromServer()
        {
            try
            {
                _client.Close();
                _statusTextBox.InvokeEx(stb => stb.Text += CRLF + "Disconnected from the server!");
                _disconnectButton.InvokeEx(db => db.Enabled = false);
                _connectButton.InvokeEx(cb => cb.Enabled = true);
                _sendCommandButton.InvokeEx(scb => scb.Enabled = false);
                _statusTextBox.InvokeEx(stb => stb.Text = string.Empty);

            }
            catch(Exception ex)
            {
                _statusTextBox.InvokeEx(stb => stb.Text += CRLF + "Problem disconnecting from the server.");
                _statusTextBox.InvokeEx(stb => stb.Text += CRLF + ex.ToString());
            }
        }


        #region Utility Methods

        private IPAddress GetIpAddress(String ipAddress)
        {
            IPAddress address = IPAddress.Parse(LOCALHOST);
            try
            {
                if(!IPAddress.TryParse(ipAddress, out address))
                {
                    address = IPAddress.Parse(LOCALHOST);
                }


            }catch (Exception ex)
            {
                _statusTextBox.Text += CRLF + "Invalid IP address - Client will connect to: " + _serverIpAddress.ToString();
                _statusTextBox.Text += CRLF + ex.ToString();
            }

            return address;
        }


        private int GetPort(string serverPort)
        {

            int port = DEFAULT_PORT;

            try
            {
                if (!Int32.TryParse(serverPort, out port))
                {
                    port = DEFAULT_PORT;
                }
            }
            catch (Exception ex)
            {
                _statusTextBox.Text += CRLF + "Invalid port value - Client will connect to port: " + port.ToString();
                _statusTextBox.Text += CRLF + ex.ToString();
            }

            return port;

        }


        #endregion Utility Methods


    }
}
