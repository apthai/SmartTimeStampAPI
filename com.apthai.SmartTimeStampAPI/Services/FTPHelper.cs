using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace com.apthai.SmartTimeStampAPI.Services
{
    public class FTPHelper
    {
        private Encoding ASCII = Encoding.ASCII;
        private static int BLOCK_SIZE = 0x200;
        private byte[] buffer = new byte[BLOCK_SIZE];
        private int bytes;
        private Socket clientSocket;
        private bool debug = false;
        private bool logined = false;
        private string mes;
        private string remoteHost = "192.168.0.65";
        private string remotePass = "support";
        private string remotePath = ".";
        private int remotePort = 0x15;
        private string remoteUser = "onetoone";
        private string reply;
        private int retValue;
        public  FTPHelper()
        {
            //remoteHost = SettingServiceProvider.FTP_Host;
            //remotePass = SettingServiceProvider.FTP_Password;
            //remoteUser = SettingServiceProvider.FTP_UserName;
        }

        public void chdir(string dirName)
        {
            if (!dirName.Equals("."))
            {
                if (!this.logined)
                {
                    this.login();
                }
                this.sendCommand("CWD " + dirName);
                if (this.retValue != 250)
                {
                    throw new IOException(this.reply.Substring(4));
                }
                this.remotePath = dirName;
                Console.WriteLine("Current directory is " + this.remotePath);
            }
        }

        private void cleanup()
        {
            if (this.clientSocket != null)
            {
                this.clientSocket.Close();
                this.clientSocket = null;
            }
            this.logined = false;
        }

        public void close()
        {
            if (this.clientSocket != null)
            {
                this.sendCommand("QUIT");
            }
            this.cleanup();
            Console.WriteLine("Closing...");
        }

        private Socket createDataSocket()
        {
            this.sendCommand("PASV");
            if (this.retValue != 0xe3)
            {
                throw new IOException(this.reply.Substring(4));
            }
            int index = this.reply.IndexOf('(');
            int num2 = this.reply.IndexOf(')');
            string str = this.reply.Substring(index + 1, (num2 - index) - 1);
            int[] numArray = new int[6];
            int length = str.Length;
            int num4 = 0;
            string s = "";
            for (int i = 0; (i < length) && (num4 <= 6); i++)
            {
                char c = char.Parse(str.Substring(i, 1));
                if (char.IsDigit(c))
                {
                    s = s + c;
                }
                else if (c != ',')
                {
                    throw new IOException("Malformed PASV reply: " + this.reply);
                }
                if ((c == ',') || ((i + 1) == length))
                {
                    try
                    {
                        numArray[num4++] = int.Parse(s);
                        s = "";
                    }
                    catch (Exception)
                    {
                        throw new IOException("Malformed PASV reply: " + this.reply);
                    }
                }
            }
            string hostName = string.Concat(new object[] { numArray[0], ".", numArray[1], ".", numArray[2], ".", numArray[3] });
            int port = (numArray[4] << 8) + numArray[5];
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint remoteEP = new IPEndPoint(Dns.Resolve(hostName).AddressList[0], port);
            try
            {
                socket.Connect(remoteEP);
            }
            catch (Exception)
            {
                throw new IOException("Can't connect to remote server");
            }
            return socket;
        }

        public void deleteRemoteFile(string fileName)
        {
            if (!this.logined)
            {
                this.login();
            }
            this.sendCommand("DELE " + fileName);
            if (this.retValue != 250)
            {
                throw new IOException(this.reply.Substring(4));
            }
        }

        public void download(string remFileName)
        {
            this.download(remFileName, "", false);
        }

        public void download(string remFileName, bool resume)
        {
            this.download(remFileName, "", resume);
        }

        public void download(string remFileName, string locFileName)
        {
            this.download(remFileName, locFileName, false);
        }

        public void download(string remFileName, string locFileName, bool resume)
        {
            if (!this.logined)
            {
                this.login();
            }
            this.setBinaryMode(true);
            Console.WriteLine("Downloading file " + remFileName + " from " + this.remoteHost + "/" + this.remotePath);
            if (locFileName.Equals(""))
            {
                locFileName = remFileName;
            }
            if (!System.IO.File.Exists(locFileName))
            {
                System.IO.File.Create(locFileName).Close();
            }
            FileStream stream2 = new FileStream(locFileName, FileMode.Open);
            Socket socket = this.createDataSocket();
            long offset = 0L;
            if (resume)
            {
                offset = stream2.Length;
                if (offset > 0L)
                {
                    this.sendCommand("REST " + offset);
                    if (this.retValue != 350)
                    {
                        offset = 0L;
                    }
                }
                if (offset > 0L)
                {
                    if (this.debug)
                    {
                        Console.WriteLine("seeking to " + offset);
                    }
                    Console.WriteLine("new pos=" + stream2.Seek(offset, SeekOrigin.Begin));
                }
            }
            this.sendCommand("RETR " + remFileName);
            if ((this.retValue != 150) && (this.retValue != 0x7d))
            {
                throw new IOException(this.reply.Substring(4));
            }
            do
            {
                this.bytes = socket.Receive(this.buffer, this.buffer.Length, SocketFlags.None);
                stream2.Write(this.buffer, 0, this.bytes);
            }
            while (this.bytes > 0);
            stream2.Close();
            if (socket.Connected)
            {
                socket.Close();
            }
            Console.WriteLine("");
            this.readReply();
            if ((this.retValue != 0xe2) && (this.retValue != 250))
            {
                throw new IOException(this.reply.Substring(4));
            }
        }

        public string[] getFileList(string mask)
        {
            int num;
            if (!this.logined)
            {
                this.login();
            }
            Socket socket = this.createDataSocket();
            this.sendCommand("NLST " + mask);
            if ((this.retValue != 150) && (this.retValue != 0x7d))
            {
                throw new IOException(this.reply.Substring(4));
            }
            this.mes = "";
            do
            {
                num = socket.Receive(this.buffer, this.buffer.Length, SocketFlags.None);
                this.mes = this.mes + this.ASCII.GetString(this.buffer, 0, num);
            }
            while (num >= this.buffer.Length);
            char[] separator = new char[] { '\n' };
            string[] strArray = this.mes.Split(separator);
            socket.Close();
            this.readReply();
            if (this.retValue != 0xe2)
            {
                throw new IOException(this.reply.Substring(4));
            }
            return strArray;
        }

        public long getFileSize(string fileName)
        {
            if (!this.logined)
            {
                this.login();
            }
            this.sendCommand("SIZE " + fileName);
            if (this.retValue != 0xd5)
            {
                throw new IOException(this.reply.Substring(4));
            }
            return long.Parse(this.reply.Substring(4));
        }

        public string getRemoteHost()
        {
            return this.remoteHost;
        }

        public string getRemotePath()
        {
            return this.remotePath;
        }

        public int getRemotePort()
        {
            return this.remotePort;
        }

        public void login()
        {
            this.clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint remoteEP = new IPEndPoint(Dns.Resolve(this.remoteHost).AddressList[0], this.remotePort);
            try
            {
                this.clientSocket.Connect(remoteEP);
            }
            catch (Exception)
            {
                throw new IOException("Couldn't connect to remote server");
            }
            this.readReply();
            if (this.retValue != 220)
            {
                this.close();
                throw new IOException(this.reply.Substring(4));
            }
            if (this.debug)
            {
                Console.WriteLine("USER " + this.remoteUser);
            }
            this.sendCommand("USER " + this.remoteUser);
            if ((this.retValue != 0x14b) && (this.retValue != 230))
            {
                this.cleanup();
                throw new IOException(this.reply.Substring(4));
            }
            if (this.retValue != 230)
            {
                if (this.debug)
                {
                    Console.WriteLine("PASS xxx");
                }
                this.sendCommand("PASS " + this.remotePass);
                if ((this.retValue != 230) && (this.retValue != 0xca))
                {
                    this.cleanup();
                    throw new IOException(this.reply.Substring(4));
                }
            }
            this.logined = true;
            Console.WriteLine("Connected to " + this.remoteHost);
            this.chdir(this.remotePath);
        }

        public void mkdir(string dirName)
        {
            if (!this.logined)
            {
                this.login();
            }
            this.sendCommand("MKD " + dirName);
            if (this.retValue != 250)
            {
                throw new IOException(this.reply.Substring(4));
            }
        }

        private string readLine()
        {
            do
            {
                this.bytes = this.clientSocket.Receive(this.buffer, this.buffer.Length, SocketFlags.None);
                this.mes = this.mes + this.ASCII.GetString(this.buffer, 0, this.bytes);
            }
            while (this.bytes >= this.buffer.Length);
            char[] separator = new char[] { '\n' };
            string[] strArray = this.mes.Split(separator);
            if (this.mes.Length > 2)
            {
                this.mes = strArray[strArray.Length - 2];
            }
            else
            {
                this.mes = strArray[0];
            }
            if (!this.mes.Substring(3, 1).Equals(" "))
            {
                return this.readLine();
            }
            if (this.debug)
            {
                for (int i = 0; i < (strArray.Length - 1); i++)
                {
                    Console.WriteLine(strArray[i]);
                }
            }
            return this.mes;
        }

        private void readReply()
        {
            this.mes = "";
            this.reply = this.readLine();
            this.retValue = int.Parse(this.reply.Substring(0, 3));
        }

        public void renameRemoteFile(string oldFileName, string newFileName)
        {
            if (!this.logined)
            {
                this.login();
            }
            this.sendCommand("RNFR " + oldFileName);
            if (this.retValue != 350)
            {
                throw new IOException(this.reply.Substring(4));
            }
            this.sendCommand("RNTO " + newFileName);
            if (this.retValue != 250)
            {
                throw new IOException(this.reply.Substring(4));
            }
        }

        public void rmdir(string dirName)
        {
            if (!this.logined)
            {
                this.login();
            }
            this.sendCommand("RMD " + dirName);
            if (this.retValue != 250)
            {
                throw new IOException(this.reply.Substring(4));
            }
        }

        private void sendCommand(string command)
        {
            byte[] bytes = Encoding.ASCII.GetBytes((command + "\r\n").ToCharArray());
            this.clientSocket.Send(bytes, bytes.Length, SocketFlags.None);
            this.readReply();
        }

        public void setBinaryMode(bool mode)
        {
            if (mode)
            {
                this.sendCommand("TYPE I");
            }
            else
            {
                this.sendCommand("TYPE A");
            }
            if (this.retValue != 200)
            {
                throw new IOException(this.reply.Substring(4));
            }
        }

        public void setDebug(bool debug)
        {
            this.debug = debug;
        }

        public void setRemoteHost(string remoteHost)
        {
            this.remoteHost = remoteHost;
        }

        public void setRemotePass(string remotePass)
        {
            this.remotePass = remotePass;
        }

        public void setRemotePath(string remotePath)
        {
            this.remotePath = remotePath;
        }

        public void setRemotePort(int remotePort)
        {
            this.remotePort = remotePort;
        }

        public void setRemoteUser(string remoteUser)
        {
            this.remoteUser = remoteUser;
        }

        public void upload(string fileName)
        {
            this.upload(fileName, false);
        }

        public void upload(string fileName, bool resume)
        {
            if (!this.logined)
            {
                this.login();
            }
            Socket socket = this.createDataSocket();
            long offset = 0L;
            if (resume)
            {
                try
                {
                    this.setBinaryMode(true);
                    offset = this.getFileSize(fileName);
                }
                catch (Exception)
                {
                    offset = 0L;
                }
            }
            if (offset > 0L)
            {
                this.sendCommand("REST " + offset);
                if (this.retValue != 350)
                {
                    offset = 0L;
                }
            }
            this.sendCommand("STOR " + Path.GetFileName(fileName));
            if ((this.retValue != 0x7d) && (this.retValue != 150))
            {
                throw new IOException(this.reply.Substring(4));
            }
            FileStream stream = new FileStream(fileName, FileMode.Open);
            if (offset != 0L)
            {
                if (this.debug)
                {
                    Console.WriteLine("seeking to " + offset);
                }
                stream.Seek(offset, SeekOrigin.Begin);
            }
            Console.WriteLine("Uploading file " + fileName + " to " + this.remotePath);
            while ((this.bytes = stream.Read(this.buffer, 0, this.buffer.Length)) > 0)
            {
                socket.Send(this.buffer, this.bytes, SocketFlags.None);
            }
            stream.Close();
            Console.WriteLine("");
            if (socket.Connected)
            {
                socket.Close();
            }
            this.readReply();
            if ((this.retValue != 0xe2) && (this.retValue != 250))
            {
                throw new IOException(this.reply.Substring(4));
            }
        }
    }
}
