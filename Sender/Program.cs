using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
//Important Note: I have assumed that Message Header and 'Message Length' var does not count towards the Message Body Length Variable

namespace IOS_Middle_Man
{

    public enum msg_Type
    {
        Initial = 1,
        AddDict = 2,
        AddVarUpdate = 3,
        OpenVarUpdate = 4,
        CloseVarUpdate = 5,
        UpdateRequest = 6,
        SetVar = 7,
        Exit = 8,
    }

    public class SocketClient
    {
        public static int Main(String[] args)
        {
            StartIOSClient(0);
            return 0;
        }

        public static void StartIOSClient(int debug)
        {
            try
            {
                byte[] demoBuff = new byte[1024];
                byte[] phoneBuff = new byte[1024];

                TcpListener listen = new TcpListener(System.Net.IPAddress.Any,4420);
                listen.Start();

                while (true)
                {
                    TcpClient client = listen.AcceptTcpClient();
                    NetworkStream netStream = client.GetStream();

                    netStream.Read(phoneBuff, 0, 1024);
                    GetMsg(phoneBuff);
                }

                /*IPAddress ipDemo = IPAddress.Parse("127.0.0.1"); //demo address 
                IPAddress ipPhone = IPAddress.Parse("192.168.254.171"); //phone address

                IPEndPoint EP2Demo = new IPEndPoint(ipDemo, 4420);
                IPEndPoint EP2Phone = new IPEndPoint(ipPhone, 4425);

                Socket sockDemo = new Socket(ipDemo.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                Socket sockPhone = new Socket(ipPhone.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                Console.WriteLine("attempt to connect...\n");

                try
                {
                    sockDemo.Connect(EP2Demo);
                    sockPhone.Connect(EP2Phone);
                    Console.WriteLine("No crash yet\n");

                    //sockDemo.Receive(demoBuff);
                    //ExtractMsg(demoBuff);
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
*/
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static int ExtractMsg(byte[] inBuffer)
        {
            switch (inBuffer[0])
            {
                case 0x4B:
                    ackMsg(inBuffer);
                    break;
                case 0x43:
                    ConNewVar(inBuffer);
                    break;
                case 0x50:
                    AddVarUpdateCon(inBuffer);
                    break;
                case 0x55:
                    VarUpdate(inBuffer);
                    break;
                case 0x53:
                    Supplemental(inBuffer);
                    break;
            }
            return 0;
        }

        public static void returnStatus(int statusCode, int pageID, int varIndex)
        {
            switch (statusCode)
            {
                case 0:
                    Console.WriteLine("Variable pn page " + pageID + " index " + varIndex + " Added Successfully\n");
                    break;
                case 1:
                    Console.WriteLine("Variable on page " + pageID + " index " + varIndex + " already on update list\n");
                    break;
                case 2:
                    Console.WriteLine("Variable on page " + pageID + " index " + varIndex + " not found\n");
                    break;
                case 3:
                    Console.WriteLine("page " + pageID + " index " + varIndex + ", variable type is incorrect\n");
                    break;
                case 4:
                    Console.WriteLine("Variable on page " + pageID + " index " + varIndex + ": Number of Array dimensions incorrect\n");
                    break;
                case 5:
                    Console.WriteLine("Variable on page " + pageID + " index " + varIndex + ": Incorrect dimension size\n");
                    break;
                case 6:
                    Console.WriteLine("Communication Server Error\n");
                    break;
                case 7:
                    Console.WriteLine("Index Mismatch\n");
                    break;
            }

            return;
        }

        public static int ackMsg(byte[] inBuffer)
        {
            return 0;
        }

        public static int ConNewVar(byte[] inBuffer)
        {
            int packetSize = 4;
            int msgLen = BitConverter.ToInt32(inBuffer, (packetSize = packetSize + 4));
            int listType = (byte)BitConverter.ToChar(inBuffer, (packetSize = packetSize + 1));
            int varIndex = BitConverter.ToInt32(inBuffer, (packetSize = packetSize + 4));
            int status = (int)BitConverter.ToChar(inBuffer, (packetSize = packetSize + 1));
            Console.WriteLine("----- Confirm New Variable Message Recieved ------\n");
            returnStatus(status, 0, varIndex);

            return packetSize;
        }

        public static int AddVarUpdateCon(byte[] inBuffer)
        {
            int packetSize = 4;
            int varIndex, varStatus;
            int msgLen = BitConverter.ToInt32(inBuffer, (packetSize = packetSize + 4));
            int listType = (int)BitConverter.ToChar(inBuffer, (packetSize = packetSize + 4));
            int pageID = BitConverter.ToInt32(inBuffer, (packetSize = packetSize + 1));
            int numVars = BitConverter.ToInt32(inBuffer, (packetSize = packetSize + 4));
            Console.WriteLine("----- Add Variable UpdatePage Confirm (7.4) -----\n");
            for (int i = 0; i < numVars; i++)
            {
                varIndex = BitConverter.ToInt32(inBuffer, (packetSize = packetSize + 4));
                varStatus = (int)BitConverter.ToChar(inBuffer, (packetSize = packetSize + 1));
                returnStatus(varStatus, pageID, varIndex);
            }
            return packetSize;
        }

        public static int VarUpdate(byte[] inBuffer)
        {
            int packetSize = 0;
            int msgSize = BitConverter.ToInt32(inBuffer, (packetSize = packetSize + 4));
            int listType = BitConverter.ToChar(inBuffer, (packetSize = packetSize + 1));
            int numVars = BitConverter.ToInt32(inBuffer, (packetSize = packetSize + 4));
            for (int i = 0; i < numVars; i++)
            {
                int varIndex = BitConverter.ToInt32(inBuffer, (packetSize = packetSize + 4));
                // Add length check on Variables List later
                int varSize = BitConverter.ToInt32(inBuffer, (packetSize = packetSize + 4));
                //updateEntry
                for (int x = 0; x < varSize; x++)
                {
                    Console.WriteLine(BitConverter.ToChar(inBuffer, packetSize = packetSize + 1));
                }
                //Array.Copy(inBuffer, (packetSize = packetSize + 4), variables[varIndex].ArrayInfo, 0, varSize);
                packetSize = packetSize + varSize;
            }
            return packetSize;
        }

        public static int Supplemental(byte[] inBuffer)
        {
            Console.WriteLine("----- Supplemental Message Receieved -----\n");
            return 0;
        }

        public static int GetMsg(byte[] inBuffer)
        {
            byte[] header = new byte[4];
            Array.Copy(inBuffer, 0, header, 0, 4);
            switch (header[0])
            {
                case 0x49: Console.WriteLine("Initial received from phone\n"); break;
                case 0x41: Console.WriteLine("AddDict received from phone\n"); break;
                case 0x50: Console.WriteLine("AddVarUpdate received from phone\n"); break;
                case 0x4F: Console.WriteLine("OpenVarUpdate received from phone\n"); break;
                case 0x43: Console.WriteLine("CloseVarUpdate received from phone\n"); break;
                case 0x51: Console.WriteLine("UpdateRequest received from phone\n"); break;
                case 0x53: Console.WriteLine("SetVar received from phone\n"); break;
                case 0x45: Console.WriteLine("Exit Msg received from phone\n"); break;
                default: Console.WriteLine("Error in Header \n"); break;
            }

            return 0;
        }
    }

}
   