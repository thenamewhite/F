//This is the generated code, the modification is invalid
using F;
public struct Test : IFSerializable
{
    /// <summary>
    ///枚举说明
    /// </summary>
    public TestEnum TestEnumFFiled;
    /// <summary>
    ///string 说明
    /// </summary>
    public string stringFiled;
    /// <summary>
    ///int 说明
    /// </summary>
    public int intFiled;
    /// <summary>
    ///stirng一维数组
    /// </summary>
    public string[] stringBBFiled;
    /// <summary>
    ///int一维数组
    /// </summary>
    public int[] INTAAFiled;
    /// <summary>
    ///string2维数组
    /// </summary>
    public string[][] stirngArray2Filed;
    /// <summary>
    ///int2维数组
    /// </summary>
    public int[][] intArray2Filed;
    public void Deserialization(Serializable serializable)
    {
        serializable.Read(ref TestEnumFFiled);
        serializable.Read(ref stringFiled);
        serializable.Read(ref intFiled);
        serializable.Read(ref stringBBFiled);
        serializable.Read(ref INTAAFiled);
        serializable.Read(ref stirngArray2Filed);
        serializable.Read(ref intArray2Filed);
    }
    public void Serialization(Serializable serializable)
    {
    }
}
