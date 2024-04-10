namespace F
{
    /// <summary>
    /// 读取key value 方式
    /// </summary>
    public interface IFSerializableKey
    {
        /// <summary>
        /// 自定义序列化
        /// </summary>
        /// <param name="serializable"></param>

        void Serialization(Serializable serializable);

        /// <summary>
        /// 
        /// </summary>
        void Deserialization(Serializable serializable, string key);
    }
}

