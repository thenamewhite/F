//This is the generated code, the modification is invalid
using F;
using System.Collections.Generic;
public static class TestConfig
{
    public static Dictionary<int, Test> Data;
    public static Test Get(int sid)
    {
        return Data[sid];
    }
    public static void Deserialization(Serializable se)
    {
        se.ReadSerializable(ref Data);
    }
}
