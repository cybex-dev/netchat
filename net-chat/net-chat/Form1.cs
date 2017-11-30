using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Net.NetworkInformation;

namespace net_chat
{
    public partial class Form1 : Form
    {
        static Form1 frm1;
        static IPAddress _IPADDRESS;
        static IPAddress _MultiIP;
        const int _PORT = 80;
        static Thread T;
        static bool bStop = false;
        //static List<string> _ipaddress_list = new List<string>();
        static string[] _files_to_send;

        public Form1()
        {
            InitializeComponent();
            frm1 = this;
        }

        public static IPAddress GetDefaultGateway()
        {
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                var card = item;
                if (card != null && card.Speed != -1 && item.OperationalStatus != OperationalStatus.Down && card.NetworkInterfaceType != NetworkInterfaceType.Loopback && card.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                {
                    UnicastIPAddressInformationCollection x = item.GetIPProperties().UnicastAddresses;
                    foreach (IPAddressInformation ipitem in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ipitem.IsDnsEligible)
                            return ipitem.Address;
                    }
                }
            }
            return null;
        }

        private void sendText(string _message)
        {
            sendMessage(_message, null, null);
        }

        //private void sendMessage(string _MESSAGE, Image img, string[] files)
        //{

        //    //handle image and files
        //    Socket _listener_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        //    IPAddress localip = _MultiIP;
        //    _listener_socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(localip));
        //    _listener_socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 1);
        //    IPEndPoint ipep = new IPEndPoint(localip, _PORT);
        //    _listener_socket.Connect(ipep);
        //    //send data to multicast group
        //    string message_to_send = _IPADDRESS.ToString() + "~m~" + _MESSAGE + "\n";
        //    byte[] bytes = new byte[message_to_send.Length * sizeof(char)];
        //    System.Buffer.BlockCopy(message_to_send.ToCharArray(), 0, bytes, 0, bytes.Length);
        //    _listener_socket.Send(bytes, bytes.Length, SocketFlags.None);

        //    _listener_socket.Close();
        //}

        private void sendMessage(string _MESSAGE, Image img, string[] files)
        {
            Socket _listener_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            foreach (IPAddress localIP in Dns.GetHostAddresses(Dns.GetHostName()).Where(i => i.AddressFamily == AddressFamily.InterNetwork))
            {
                //handle image and files                
                _listener_socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(_MultiIP/*, localIP*/));
                _listener_socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 1);
                //_listener_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                //_listener_socket.MulticastLoopback = true;
                _listener_socket.Connect(new IPEndPoint(_MultiIP, _PORT));
                //send data to multicast group
                string message_to_send = _IPADDRESS.ToString() + "~m~" + _MESSAGE + "\n";
                byte[] bytes = new byte[message_to_send.Length * sizeof(char)];
                System.Buffer.BlockCopy(message_to_send.ToCharArray(), 0, bytes, 0, bytes.Length);
                _listener_socket.Send(bytes, bytes.Length, SocketFlags.None);
            }
            _listener_socket.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //handle images and file
            //if (true)
            //{
            sendText(inputT_Msg.Text);
            //}
            inputT_Msg.Text = "Enter message to send";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _IPADDRESS = GetDefaultGateway();
            _MultiIP = IPAddress.Parse("224.0.0.3");
            Form1 frm1 = this;
            frm1.MaximizeBox = false;
            frm1.MinimizeBox = false;
            T = new Thread(new ThreadStart(recieveText));
            T.IsBackground = true;
            T.Start();
        }

        private void recieveText()
        {
            //initialise multicast group and bind to interface
            Socket _sender_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, _PORT);
            _sender_socket.Bind(ipep);
            IPAddress localip = _MultiIP;
            _sender_socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(localip, IPAddress.Any));

            //recieve data to multicast group
            //while (true)
            //{
            while (_sender_socket.IsBound && !bStop)
            {
                byte[] b = new byte[1024];
                _sender_socket.Receive(b);
                char[] chars = new char[b.Length / sizeof(char)];
                System.Buffer.BlockCopy(b, 0, chars, 0, b.Length);

                string _message = new string(chars).Trim();
                string ip = _message.Substring(0, _message.IndexOf("~"));
                _message = _message.Remove(0, _message.IndexOf("~") + 1);
                string _flag = _message.Substring(0, 1);
                _message = _message.Remove(0, _message.IndexOf("~") + 1);

                _message = _message.Replace("\0", string.Empty);

                handleData(ip, _flag, _message);
            }
        }

        private void handleData(string ip, string flag, string message/*, Image img, string[] files*/)
        {
            ChatPanel.position pos = ChatPanel.position.right;
            if (ip != _IPADDRESS.ToString())
            {
                switch (flag)
                {
                    case "m":
                        pos = ChatPanel.position.left;
                        break;

                    case "f":
                    default:
                        break;
                }
            }
            AddTextToChat(ip, message, pos);
        }

        void AddTextToChat(string ip, string message, ChatPanel.position pos)
        {
            chatPanel1.BeginInvoke((MethodInvoker)delegate () { chatPanel1.AddMessage(pos, ip, message); });
        }

        private void inputT_Msg_KeyDown(object sender, KeyEventArgs e)
        {
            if (inputT_Msg.Text == "")
                inputT_Msg.Text = "Enter message to send";
            else
                if (inputT_Msg.Text == "Enter message to send")
                inputT_Msg.Text = "";
            if (e.KeyCode == Keys.Return)
            {
                sendText(inputT_Msg.Text);
                inputT_Msg.Text = "Enter message to send";
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            _files_to_send = openFileDialog1.FileNames;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dlgResult = openFileDialog1.ShowDialog();
        }

        private void inputT_Msg_TextChanged(object sender, EventArgs e)
        {
            //Size = TextRenderer.MeasureText(_message, this.Font, new Size(this.Width, 0), TextFormatFlags.WordBreak),
        }
    }
}
