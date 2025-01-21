namespace F
{
    /// <summary>
    ///     读取key value 方式
    /// </summary>
    public interface IFSerializableKey
    {
        /// <summary>
        ///     自定义序列化
        /// </summary>
        /// <param name="serializable"></param>
        void Serialization(SerializableKey serializable);

        /// <summary>
        /// </summary>
        void Deserialization(SerializableKey serializable, string key);
    }
}