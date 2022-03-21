using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class SocketClient
{
    public static int Main(String[] args)
    {
        StartIOSClient();
        return 0;
    }

    public static int Initialize(byte[] buffer)
    {
        //  Header                  Byte Order  Floating Point          Byte  Spare                   Trailer
        //{ 0x49, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x49 };

        float ftype = 1;
        short shtype = 1;
        byte[] fbyte = BitConverter.GetBytes(ftype);
        byte[] shbyte = BitConverter.GetBytes(shtype);

        //Header
        buffer[0]  = 0x49;

        for (int i = 1; i < 4 ; i++)
        {
            buffer[i] = 0x00;
        }

        //Byte Order
        for (int i = 4; i < 6; i++)
        {
            buffer[i] = shbyte[i - 4];
        }

        //floating point
        for (int i = 6; i < 10; i++)
        {
            buffer[i] = fbyte[i - 6];
        }

        //Integer Size
        byte[] isize= BitConverter.GetBytes(sizeof(int));
        buffer[10]  = isize[0];

        //Spare & Trailer
        for (int i = 11; i < 15; i++)
        {
            buffer[i] = 0x00;
        }
        buffer[15] = 0x49;
        return 16;
    }

    public static int addDict(byte[] buffer, int listType, int varIndex, int VarType, int numDimen, int sizeDimen, string varName)
    {
        int packetSize  = 4;
        int varNameSize = 0;
        
        //Header starts at byte 0
        buffer[0] = 0x41;
        for (int i = 1; i < 4; i++)
        {
            buffer[i] = 0x00;
        }

        //List Type starts at byte 8


        //Variable Index starts at byte 9 
        byte[] vibyte = BitConverter.GetBytes(varIndex);
        for (int i = 9; i < 13; i++) { buffer[i] = vibyte[i-9]; }

        //Variable Type starts at byte 13

        //# of Dimensions starts at byte 17

        //Dimension Size starts at byte 18

        //Vriable Name Length()

        //Variable Name (unknown start index)
        while () { }

        //Variable Name Length (unknown start index)

        //Message Size starts at byte 4
        byte[] bsize = BitConverter.GetBytes(packetSize);
        for (int i = 4; i < 8; i++) { buffer[i] = bsize[i-4]; }

        return packetSize;
    }

    public static int addVarUpdate()
    {
        int packetSize = 0;

        return packetSize;
    }

    public static int openVarUpdatePage()
    {
        int packetSize = 0;

        return packetSize;
    }

    public static int closeVarUpdatePage()
    {
        int packetSize = 0;

        return packetSize;
    }

    public static int updateRequest(byte[] buffer)
    {
        buffer[0] = 0x55; buffer[1] = 0x00; buffer[2] = 0x00; buffer[3] = 0x00;
        return 9;
    }
    public static int setVar(byte[] buffer)
    {
        int packetSize = 4;
        buffer[0] = 0x53; buffer[1] = 0x00; buffer[2] = 0x00; buffer[3] = 0x00;  

        return packetSize;
    }

    public static int exitMsg(byte[] buffer)
    {
        buffer[0] = 0x45; 
        
        for (int i=1; i < 8; i++)
        {
            buffer[i] = 0x00;
        }
        return 8;
    }

    public static void StartIOSClient()
    {
        byte[] bytes = new byte[2048];

        try
        {
            // Connect to a Remote server
            //IPHostEntry host    = Dns.GetHostEntry("localhost");
            IPAddress ip        = IPAddress.Parse("127.0.0.1");//host.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ip, 4420);

            // Create a TCP/IP  socket.
            Socket sender = new Socket(ip.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect the socket to the remote endpoint. Catch any errors.
            try
            {
                Console.WriteLine("attempt to connect to {0}",
                    ip.ToString());

                // Connect to Remote EndPoint
                sender.Connect(remoteEP);

                Console.WriteLine("Socket connected to {0}",
                    sender.RemoteEndPoint.ToString());
                //byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");

                // Send the data through the socket.
                byte[] msg = new byte[16];

                int msgSize = Initialize(msg);
                int bytesSent = sender.Send(msg);

                // Release the socket.
                //exitMsg(msg);
                //sender.Send(msg); 
                //sender.Shutdown(SocketShutdown.Both);
                //sender.Close();

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

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}