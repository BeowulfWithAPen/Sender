using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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

    public struct VariableListEntry
    {
        public VariableListEntry(int index, string name, int type, byte arrayInfo) {
            Index = index;
            Name = name;
            Type = type;
            ArrayInfo = arrayInfo;
        }
        public int Index { get; set; }
        public string Name;
        public int Type;
        public byte ArrayInfo;
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
            byte[] demoBuff = new byte[1024];

            List<VariableListEntry> varList = new List<VariableListEntry>();
            varList.Add(new VariableListEntry(0, "a", 0, 0x00));

            IPAddress ipDemo = IPAddress.Parse("127.0.0.1");
            IPEndPoint EP2Demo = new IPEndPoint(ipDemo, 4420);
            Socket sockDemo = new Socket(ipDemo.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            Console.WriteLine("attempt to connect to Demohost\n");

            try
            {
                sockDemo.Connect(EP2Demo);
                Console.WriteLine("Connected to DemoHost Successfully\n");
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

            int msgSize = Initialize(demoBuff);
            byte[] msg0 = new byte[msgSize];
            Array.Copy(demoBuff, msg0, msgSize);
            sockDemo.Send(msg0);
            Thread.Sleep(500);

            msgSize = updateRequest(demoBuff, 0);
            byte[] msg1 = new byte[msgSize];
            Array.Copy(demoBuff, msg1, msgSize);
            sockDemo.Send(msg1);
            Thread.Sleep(500);

            msgSize = addVarUpdate(demoBuff, 0, 1, 1, "a", 1, varList);
            byte[] msg2 = new byte[msgSize];
            Array.Copy(demoBuff, msg2, msgSize);
            sockDemo.Send(msg2);
            Thread.Sleep(500);

            msgSize = openVarUpdatePage(demoBuff, 0, 0);
            byte[] msg3 = new byte[msgSize];
            Array.Copy(demoBuff, msg3, msgSize);
            sockDemo.Send(msg3);
            Thread.Sleep(500);

            msgSize = closeVarUpdatePage(demoBuff, 0, 0);
            byte[] msg4 = new byte[msgSize];
            Array.Copy(demoBuff, msg4, msgSize);
            sockDemo.Send(msg4);
            Thread.Sleep(500);

            msgSize = addDict(demoBuff, 0, varList[0]);
            byte[] msg5 = new byte[msgSize];
            Array.Copy(demoBuff, msg5, msgSize);
            sockDemo.Send(msg5);
            Thread.Sleep(500);

            msgSize = setVar(demoBuff, 0, 0, 1, msg5, varList[0]);
            byte[] msg6 = new byte[msgSize];
            Array.Copy(demoBuff, msg6, msgSize);
            sockDemo.Send(msg6);
            Thread.Sleep(500);

            msgSize = exitMsg(demoBuff);
            byte[] msg7 = new byte[msgSize];
            Array.Copy(demoBuff, msg7, msgSize);
            sockDemo.Send(msg7);
            Thread.Sleep(500);
        }

        public static int fillHeader(byte[] buffer, msg_Type msgType, int length)
        {
            switch (msgType)
            {
                case msg_Type.Initial: buffer[0] = 0x49; buffer[1] = 0x00; buffer[2] = 0x00; buffer[3] = 0x00; break;
                case msg_Type.AddDict: buffer[0] = 0x41; buffer[1] = 0x00; buffer[2] = 0x00; buffer[3] = 0x00; break;
                case msg_Type.AddVarUpdate: buffer[0] = 0x50; buffer[1] = 0x00; buffer[2] = 0x00; buffer[3] = 0x00; break;
                case msg_Type.OpenVarUpdate: buffer[0] = 0x4F; buffer[1] = 0x00; buffer[2] = 0x00; buffer[3] = 0x00; break;
                case msg_Type.CloseVarUpdate: buffer[0] = 0x43; buffer[1] = 0x00; buffer[2] = 0x00; buffer[3] = 0x00; break;
                case msg_Type.UpdateRequest: buffer[0] = 0x51; buffer[1] = 0x00; buffer[2] = 0x00; buffer[3] = 0x00; break;
                case msg_Type.SetVar: buffer[0] = 0x53; buffer[1] = 0x00; buffer[2] = 0x00; buffer[3] = 0x00; break;
                case msg_Type.Exit: buffer[0] = 0x45; buffer[1] = 0x00; buffer[2] = 0x00; buffer[3] = 0x00; break;
                default: break;
            }

            if (msgType == msg_Type.Initial) { return 1; }
            if (length > 1024) { return -1; }

            length = length - 8; // accounts for header & length 
            byte[] lenBytes = BitConverter.GetBytes(length);
            for (int i = 4; i < 8; i++)
            {
                buffer[i] = lenBytes[i - 4];
            }
            return 1;
        }

        public static int updateRequest(byte[] buffer, int listType)
        {
            int packetSize = 8;

            buffer[packetSize++] = (byte)listType;

            if (fillHeader(buffer, msg_Type.UpdateRequest, packetSize) < 0) { return -1; }

            return packetSize;
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
            buffer[0] = 0x49;

            for (int i = 1; i < 4; i++)
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
            byte[] isize = BitConverter.GetBytes(sizeof(int));
            buffer[10] = isize[0];

            //Spare & Trailer
            for (int i = 11; i < 15; i++)
            {
                buffer[i] = 0x00;
            }
            buffer[15] = 0x49;
            return 16;
        }

        public static int addVarUpdate(byte[] buffer, int listType, int pageId, int pageNameLen, string pageName, int numVars, List<VariableListEntry> variables)
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

            // Append Variables from variables...
            for (int i = 0; i < numVars; i++)
            {
                byte[] viByte = BitConverter.GetBytes(variables[i].Index);
                for (int x = 0; x < 4; x++) { buffer[packetSize++] = viByte[x]; }

                byte[] vtByte = BitConverter.GetBytes(variables[i].Type);
                for (int x = 0; x < 4; x++) { buffer[packetSize++] = vtByte[x]; }

                byte[] ndByte = BitConverter.GetBytes(1);
                for (int x = 0; x < 1; x++) { buffer[packetSize++] = ndByte[x]; }

                byte[] sdByte = BitConverter.GetBytes(1);
                for (int x = 0; x < 4; x++) { buffer[packetSize++] = sdByte[x]; }
                //for (int x = 0; x < numDimen; x++)
                //{
                //   byte[] sdByte = BitConverter.GetBytes(sizeDimen[x]);
                //    for (int y; y < 4; y++) { }
                //}

                string tempStr = variables[i].Name;
                int tempSze = tempStr.Length;
                byte[] nlByte = BitConverter.GetBytes(tempSze);
                for (int x = 0; x < 1; x++) { buffer[packetSize++] = nlByte[x]; }

                for (int x = 0; x < tempSze; x++) { buffer[packetSize++] = (byte)tempStr[x]; }

            }

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

        public static int addDict(byte[] buffer, int listType, VariableListEntry variable)
        {
            int packetSize = 8;

            // ERROR Handling
            //if (numDimen != sizeDimen.Length) { return -1; }
            if (variable.Name.Length > 15) { return -1; }

            // List Type starts at byte 8
            if (listType == 1)
            { buffer[packetSize++] = 0x01; }
            else
            { buffer[packetSize++] = 0x00; }

            // Variable Index starts at byte 9 
            byte[] vibyte = BitConverter.GetBytes(variable.Index);
            for (int i = 9; i < 13; i++) { buffer[i] = vibyte[i - 9]; packetSize++; }

            // Variable Type starts at byte 13
            byte[] vtbyte = BitConverter.GetBytes(variable.Type);
            for (int i = 13; i < 17; i++) { buffer[i] = vtbyte[i - 13]; packetSize++; }

            // Number of Dimensions
            byte[] ndByte = BitConverter.GetBytes(1);
            for (int x = 0; x < 1; x++) { buffer[packetSize++] = ndByte[x]; }

            byte[] sdByte = BitConverter.GetBytes(1);
            for (int x = 0; x < 4; x++) { buffer[packetSize++] = sdByte[x]; }

            //Variable Name (unknown start index)
            for (int i = 0; i < 1; i++)
            { buffer[packetSize++] = Convert.ToByte(variable.Name.Length); }

            //Variable Name (unknown start index)
            for (int i = 0; i < variable.Name.Length; i++)
            { buffer[packetSize++] = Convert.ToByte(variable.Name[i]); }

            if (fillHeader(buffer, msg_Type.AddDict, packetSize) < 0) { return -1; }

            return packetSize;
        }

        public static int setVar(byte[] buffer, int listType, int varIndex, int valueSize, byte[] newArrayData, VariableListEntry variable)
        {
            int packetSize = 8;

            // List Type
            if (listType == 1)
            { buffer[packetSize++] = 0x01; }
            else
            { buffer[packetSize++] = 0x00; }

            // Variable Index 
            byte[] viBytes = BitConverter.GetBytes(varIndex);
            for (int i = 0; i < 4; i++) { buffer[packetSize++] = viBytes[i]; }

            // Number of Dimensions
            byte[] ndByte = BitConverter.GetBytes(1);
            for (int x = 0; x < 1; x++) { buffer[packetSize++] = ndByte[x]; }

            byte[] sdByte = BitConverter.GetBytes(1);
            for (int x = 0; x < 4; x++) { buffer[packetSize++] = sdByte[x]; }

            // Value Size
            byte[] valsBytes = BitConverter.GetBytes(valueSize);
            for (int i = 0; i < 4; i++) { buffer[packetSize++] = valsBytes[i]; }

            // Value
            for (int x = 0; x < newArrayData.Length; x++)
            {
                buffer[packetSize++] = newArrayData[x];
            }

            if (fillHeader(buffer, msg_Type.SetVar, packetSize) < 0) { return -1; }

            return packetSize;
        }

        public static int exitMsg(byte[] buffer)
        {
            int packetSize = 8;

            if (fillHeader(buffer, msg_Type.Exit, packetSize) < 0) { return -1; }

            return packetSize;
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
   