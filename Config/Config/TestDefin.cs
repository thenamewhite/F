using F;
using System;
using System.Collections.Generic;
using System.Text;
// author  (hf) Date：2023/6/27 19:05:25

public struct TestDefin : IFSerializable
{

    public int Rewad;

    public int Num;
    public string Text;

    public TestEnum TestEnum;

    public void Deserialization(Serializable serializable)
    {
        serializable.Read(ref Rewad);
        serializable.Read(ref Num);
        serializable.Read(ref Text);
        serializable.Read(ref TestEnum);

    }

    public void Serialization(Serializable serializable)
    {
        serializable.Push(Rewad);
        serializable.Push(Num);
        serializable.Push(Text);
        serializable.Push(TestEnum);
    }

#if DEBUG
    public TestDefin(string a, string b, string c, string testEnum)
    {
        Rewad = Convert.ToInt32(a);
        Num = Convert.ToInt32(b);
        Text = c;
        TestEnum = (TestEnum)Convert.ToInt32(testEnum);
    }
#endif
}
