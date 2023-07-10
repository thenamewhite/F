using F;
using System;
using System.Collections.Generic;
using System.Text;

// author  (hf) Date：2023/6/27 19:05:25

public struct TestDefin : IFSerializable
{

    public int Rewad;

    public int Num;

    public void Deserialization(Serializable serializable)
    {
        serializable.Read(ref Rewad);
        serializable.Read(ref Num);
    }

    public void Serialization(Serializable serializable)
    {

    }
}
