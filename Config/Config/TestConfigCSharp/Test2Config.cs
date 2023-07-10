//This is the generated code, the modification is invalid
using F;
using System.Collections.Generic;
public static class Test2Config
{
    public static Dictionary<int, Test2> Data;
    public static Test2 Get(int sid)
    {
        return Data[sid];
    }
    public static void Deserialization(Serializable se)
    {
        se.ReadSerializable(ref Data);
    }
}
