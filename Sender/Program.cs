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
            IPAddress ipDemo    = IPAddress.Parse("127.0.0.1"); //demo address 
            IPAddress ipPhone   = IPAddress.Parse("127.0.0.1"); //phone address
            IPEndPoint EP2Demo  = new IPEndPoint(ipDemo, 4425);
            IPEndPoint EP2Phone = new IPEndPoint(ipPhone, 4420);

            Socket sockDemo = new Socket(ipDemo.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            Socket sockPhone = new Socket(ipPhone.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            try
            {
                sockDemo.Connect(EP2Demo);
                sockPhone.Connect(EP2Phone);
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