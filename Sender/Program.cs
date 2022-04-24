using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
//Important Note: I have assumed that Message Header and 'Message Length' var does not count towards the Message Body Length Variable

public enum msg_Type
{
    Initial        = 1,
    AddDict        = 2,
    AddVarUpdate   = 3,
    OpenVarUpdate  = 4,
    CloseVarUpdate = 5,
    UpdateRequest  = 6,
    SetVar         = 7,
    Exit           = 8,
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
            IPAddress ip        = IPAddress.Parse("127.0.0.1");//host.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ip, 4420);

            Socket sender = new Socket(ip.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            try
            {
                if (debug == 1) 
                {
                    Console.WriteLine("Attempt to connect to {0}",
                        sender.RemoteEndPoint.ToString());
                    sender.Connect(remoteEP);
                }
                else { sender.Connect(remoteEP); }

                //bytesSent = sender.Send(buff_out); 

                // Release the socket
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