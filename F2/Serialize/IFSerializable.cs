// author  (hf) Date：2023/6/30 10:27:20

using System.Runtime.CompilerServices;

[assembly: SuppressIldasm]

namespace F
{
    public interface IFSerializable
    {
        /// <summary>
        ///     自定义序列化
        /// </summary>
        /// <param name="serializable"></param>
        void Serialization(Serializable serializable);

        /// <summary>
        /// </summary>
        void Deserialization(Serializable serializable);
    }
}