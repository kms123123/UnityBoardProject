using Unity.Collections;
using Unity.Networking.Transport;

public class NetWelcome : NetMessage
{
    public int assignedTeam { set; get; }

    public NetWelcome()
    {
        Code = OpCode.WELCOME;
    }

    public NetWelcome(DataStreamReader reader)
    {
        Code = OpCode.WELCOME;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(assignedTeam);
    }

    public override void Deserialize(DataStreamReader reader)
    {
        assignedTeam = reader.ReadInt();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_WELCOME?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_WELCOME?.Invoke(this, cnn);
    }
}
