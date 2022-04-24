namespace IOS_Middle_Man
{
    public static class ExtractMessage
{

    public static int ExtractMsg(byte[] inBuffer, List<VariableListEntry> variables)
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
}
}