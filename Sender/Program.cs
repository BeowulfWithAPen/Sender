using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class SocketClient
{
    public static int Main(String[] args)
    {
        StartISSClient();
        return 0;
    }

    public static int Initialize(byte[] msg)
    {
        //  Header                  Byte Order  Floating Point          Byte  Spare                   Trailer
        //{ 0x49, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x49 };

        float ftype = 1;
        short shtype = 1;
        byte[] fbyte = BitConverter.GetBytes(ftype);
        byte[] shbyte = BitConverter.GetBytes(shtype);

        //Header
        msg[0]  = 0x49;

        for (int i = 1; i < 4 ; i++)
        {
            msg[i] = 0x00;
        }

        //Byte Order
        for (int i = 4; i < 6; i++)
        {
            msg[i] = shbyte[i - 4];
        }

        //floating point
        for (int i = 6; i < 10; i++)
        {
            msg[i] = fbyte[i - 6];
        }

        //Integer Size
        byte[] isize= BitConverter.GetBytes(sizeof(int));
        msg[10]  = isize[0];

        //Spare & Trailer
        for (int i = 11; i < 15; i++)
        {
            msg[i] = 0x00;
        }
        msg[15] = 0x49;
        return 16;
    }

    public static int addDict(byte[] msg, int listType, int varIndex, int VarType, int numDimen, int sizeDimen, string varName)
    {
        int packetSize = 4;
        
        //Header
        msg[0] = 0x41;
        for (int i = 1; i < 4; i++)
        {
            msg[i] = 0x00;
        }

        //VarName
        return packetSize;
    }

    public static void StartISSClient()
    {
        byte[] bytes = new byte[1024];

        try
        {
            // Connect to a Remote server
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 4420);

            // Create a TCP/IP  socket.
            Socket sender = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect the socket to the remote endpoint. Catch any errors.
            try
            {
                // Connect to Remote EndPoint
                sender.Connect(remoteEP);

                Console.WriteLine("Socket connected to {0}",
                    sender.RemoteEndPoint.ToString());
                //byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");

                // Send the data through the socket.
                byte[] msg = new byte[30];
                int msgSize = Initialize(msg);
                byte[] endOfFile = Encoding.ASCII.GetBytes("<EOF>");
                for (int i = 0; i < endOfFile.Length; i++)
                {
                    msg[msgSize + i] = endOfFile[i];
                }
                int bytesSent = sender.Send(msg);

                // Receive the response from the remote device.
                int bytesRec = sender.Receive(bytes);
                Console.WriteLine("Echoed test = {0}",
                    Encoding.ASCII.GetString(bytes, 0, bytesRec));

                // Release the socket.
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();

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