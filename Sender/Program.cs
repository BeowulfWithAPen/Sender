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

    public static int fillHeader(byte[] buffer, msg_Type msgType, int length)
    {
        switch (msgType)
        {
            case msg_Type.Initial        : buffer[0] = 0x49; buffer[1] = 0x00; buffer[2] = 0x00; buffer[3] = 0x00; break;
            case msg_Type.AddDict        : buffer[0] = 0x41; buffer[1] = 0x00; buffer[2] = 0x00; buffer[3] = 0x00; break;
            case msg_Type.AddVarUpdate   : buffer[0] = 0x50; buffer[1] = 0x00; buffer[2] = 0x00; buffer[3] = 0x00; break;
            case msg_Type.OpenVarUpdate  : buffer[0] = 0x4F; buffer[1] = 0x00; buffer[2] = 0x00; buffer[3] = 0x00; break;
            case msg_Type.CloseVarUpdate : buffer[0] = 0x43; buffer[1] = 0x00; buffer[2] = 0x00; buffer[3] = 0x00; break;
            case msg_Type.UpdateRequest  : buffer[0] = 0x55; buffer[1] = 0x00; buffer[2] = 0x00; buffer[3] = 0x00; break;
            case msg_Type.SetVar         : buffer[0] = 0x53; buffer[1] = 0x00; buffer[2] = 0x00; buffer[3] = 0x00; break;
            case msg_Type.Exit           : buffer[0] = 0x45; buffer[1] = 0x00; buffer[2] = 0x00; buffer[3] = 0x00; break;
            default: break;
        }

        if (msgType == msg_Type.Initial) { return 1;  }
        if (length > 1024)               { return -1; }

        length = length - 8; // accounts for header & length 
        byte[] lenBytes = BitConverter.GetBytes(length);
        for (int i = 4; i < 8; i++)
        {
            buffer[i] = lenBytes[i - 4];
        }
        return 1;
    }

    public static int Initialize(byte[] buffer)
    {
        int packetSize = 16;
        float ftype = 1;
        short shtype = 1;
        byte[] fbyte = BitConverter.GetBytes(ftype);
        byte[] shbyte = BitConverter.GetBytes(shtype);

        //Byte Order
        for (int i = 4; i < 6; i++)
        { buffer[i] = shbyte[i - 4]; }

        //floating point
        for (int i = 6; i < 10; i++)
        { buffer[i] = fbyte[i - 6]; }

        //Integer Size
        byte[] isize = BitConverter.GetBytes(sizeof(int));
        buffer[10] = isize[0];

        //Spare & Trailer
        for (int i = 11; i < 15; i++)
        { buffer[i] = 0x00; }
        buffer[15] = 0x49;

        if (fillHeader(buffer, msg_Type.Initial, 0) < 0) { return -1; }

        return packetSize;
    }

    public static int addDict(byte[] buffer, int listType, int varIndex, int VarType, int numDimen, int[] sizeDimen, string varName)
    {
        int packetSize  = 8;

       // ERROR Handling
        if (numDimen != sizeDimen.Length) { return -1; }
        if (varName.Length > 15)          { return -1; }

       // List Type starts at byte 8
        if (listType == 1)
            { buffer[packetSize++] = 0x01; }
        else
            { buffer[packetSize++] = 0x00; }

       // Variable Index starts at byte 9 
        byte[] vibyte = BitConverter.GetBytes(varIndex);
        for (int i = 9; i < 13; i++) { buffer[i] = vibyte[i-9]; packetSize++; }

       // Variable Type starts at byte 13
        byte[] vtbyte = BitConverter.GetBytes(varIndex);
        for (int i = 13; i < 17; i++) { buffer[i] = vtbyte[i - 13]; packetSize++;  }

       // # of Dimensions starts at byte 17
        byte[] numDBytes = BitConverter.GetBytes(numDimen);
        buffer[packetSize++] = numDBytes[0];

       // Dimension Size starts at byte 18
        for (int i = 0; i < numDimen; i++) {
            byte[] dimenBytes = BitConverter.GetBytes(sizeDimen[i]);
            for (int x = 0; x < numDimen; x++) { buffer[packetSize++] = dimenBytes[x]; }
        }

       //Variable Name Length(unknown start index)
        buffer[packetSize++] = (byte) varName.Length;

       //Variable Name (unknown start index)
        for (int i = 0; i < varName.Length; i++)
        { buffer[packetSize++] = (byte)varName[i]; }

        if (fillHeader(buffer, msg_Type.AddDict, packetSize) < 0) { return -1; }  

        return packetSize;
    }

    public static int addVarUpdate(byte[] buffer, int listType, int pageId, int pageNameLen, string pageName, int numVars)
    {
        int packetSize = 8;

       // List Type
        if (listType == 1)
            { buffer[packetSize++] = 0x01; }
        else
            { buffer[packetSize++] = 0x00; }

       // Page ID
        byte[] pidBytes = BitConverter.GetBytes(pageId);
        for (int i = 0; i < 4; i++) { buffer[packetSize++] = pidBytes[i]; }

       // Page Name Length
        byte[] pnlBytes = BitConverter.GetBytes(pageNameLen);
        for (int i = 0; i < 4; i++) { buffer[packetSize++] = pnlBytes[i]; }

       // Page Name
        for (int i = 0; i < pageName.Length; i++)
        { buffer[packetSize++] = (byte)pageName[i]; }

       // Number of Variables
        byte[] nvBytes = BitConverter.GetBytes(numVars);
        for (int i = 0; i < 4; i++) { buffer[packetSize++] = nvBytes[i]; } 

        if (fillHeader(buffer, msg_Type.AddVarUpdate, packetSize) < 0) { return -1; }

        return packetSize;
    }

    public static int openVarUpdatePage(byte[] buffer, int listType, int VarUpdatePageID)
    {
        int packetSize = 8;
        
       // List Type
        if (listType == 1)
            { buffer[packetSize++] = 0x01; }
        else
            { buffer[packetSize++] = 0x00; }

       // Variable Update Page ID
        byte[] vuBytes = BitConverter.GetBytes(VarUpdatePageID);
        for (int i = 0; i < 4; i++) { buffer[packetSize++] = vuBytes[i]; }

        if (fillHeader(buffer, msg_Type.OpenVarUpdate, packetSize) < 0) { return -1; }

        return packetSize;
    }

    public static int closeVarUpdatePage(byte[] buffer, int listType, int VarUpdatePageID)
    {
        int packetSize = 8;

       // List Type
        if (listType == 1)
            { buffer[packetSize++] = 0x01; }
        else
            { buffer[packetSize++] = 0x00; }

       // Variable Update Page ID
        byte[] vuBytes = BitConverter.GetBytes(VarUpdatePageID);
        for (int i = 0; i < 4; i++) { buffer[packetSize++] = vuBytes[i]; }

        if (fillHeader(buffer, msg_Type.CloseVarUpdate, packetSize) < 0) { return -1; }

        return packetSize;
    }

    public static int updateRequest(byte[] buffer, int listType)
    {
        int packetSize = 8;

        buffer[packetSize++] = (byte) listType;

        if (fillHeader(buffer, msg_Type.UpdateRequest, packetSize) < 0) { return -1; }

        return packetSize;
    }

    public static int setVar(byte[] buffer, int listType, int varIndex, int numDimen, int[] dimen, int valueSize, int value)
    {
        int packetSize = 0;
        
        if (numDimen > 15) { return -1; }

       // List Type
        if (listType == 1)
            { buffer[packetSize++] = 0x01; }
        else
            { buffer[packetSize++] = 0x00; }

       // Variable Index 
        byte[] viBytes = BitConverter.GetBytes(varIndex);
        for (int i = 0; i < 4; i++) { buffer[packetSize++] = viBytes[i]; }

       // Number of Dimensions
        byte[] numDBytes = BitConverter.GetBytes(numDimen);
        buffer[packetSize++] = numDBytes[0];

       // Dimensions
        for (int i = 0; i < numDimen; i++)
        {
            byte[] dimenBytes = BitConverter.GetBytes(dimen[i]);
            for (int x = 0; x < 4; x++) { buffer[x + packetSize] = numDBytes[x]; }
            packetSize += 4;
        }

       // Value Size
        byte[] valsBytes = BitConverter.GetBytes(valueSize);
        for (int i = 0; i < 4; i++) { buffer[packetSize++] = valsBytes[i]; }
        
       // Value
        

        if (fillHeader(buffer, msg_Type.SetVar, packetSize) < 0) { return -1; }

        return packetSize;
    }

    public static int exitMsg(byte[] buffer)
    {
        int packetSize = 8;

        if (fillHeader(buffer, msg_Type.Exit, packetSize) < 0) { return -1; }

        return packetSize;
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

                byte[] buff_o1 = new byte[16];
                byte[] buff_o2 = new byte[40];
                int msgSize = Initialize(buff_o1);

                //Array.Copy(buff_out, buff, msgSize);
                int bytesSent = sender.Send(buff_o1);

                int[] dimensions = { 1 };

                msgSize   = addDict(buff_o2, 0, 0, 0, 1, dimensions, "testChar"); 
                bytesSent = sender.Send(buff_o2);

                if (debug == 1)
                {
                    for (int i = 0; i < msgSize; i++)
                    {
                        Console.WriteLine(buff_o1);
                    }
                }

                //msgSize   = exitMsg(buff_out);
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