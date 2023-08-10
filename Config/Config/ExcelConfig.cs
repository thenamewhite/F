using ExcelDataReader;
using F;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
// author  (hf) Date：2023/6/6 17:06:34
namespace F
{
    public class ExcelConfig
    {
        /// <summary>
        /// as
        /// </summary>
        ///<param name="readDirectoryPath">读取文件路径</param>
        ///<param name="gengeneratedPath">生成cs 路径</param>
        public static void Start(string readDirectoryPath, string gengeneratedPath)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //var path = "F:\\F2\\F2\\Config\\Test.xlsx";

            if (Directory.Exists(readDirectoryPath))
            {
                var files = Directory.GetFiles(readDirectoryPath, "*.xlsx", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    ReadFile(file, gengeneratedPath);

                }
            }
            else
            {
                Console.WriteLine($"{readDirectoryPath}，不是文件夹");
            }
            return;

        }

        private static void ReadFile(string filePath, string gengeneratedPath)
        {
            try
            {
                using FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var d = ExcelReaderFactory.CreateReader(fs);
                var row = d.RowCount;
                var classFiled = new List<string>();
                var filedExpalinFiled = new List<string>();
                var typeFiled = new List<string>();
                var vlaueFiled = new List<object>();
                var index = 0;
                while (d.Read())
                {
                    for (int i = 0; i < d.FieldCount; i++)
                    {
                        ///第一行是注释
                        if (index == 0)
                        {
                            var ro = d.GetString(i);
                            filedExpalinFiled.Add(ro);
                        }
                        //第二行是函数生成的字段名称
                        else if (index == 1)
                        {
                            var funName = d.GetString(i) + "Filed";
                            classFiled.Add(funName);
                        }
                        //生成的类型
                        else if (index == 2)
                        {
                            typeFiled.Add(d.GetString(i));
                        }
                        //后续都是值
                        else
                        {
                            var v = d.GetValue(i);
                            if (i != 0)
                            {
                                var filedType = typeFiled[i];
                                switch (filedType)
                                {
                                    case "string":
                                        v = v == null ? string.Empty : Convert.ToString(v);
                                        break;
                                    case "int":
                                        v = v == null ? 0 : Convert.ToInt32(v);
                                        break;
                                    case "float":
                                        v = v == null ? 0f : Convert.ToSingle(v);
                                        break;
                                    case "long":
                                        v = v == null ? 0L : Convert.ToInt64(v);
                                        break;
                                    case "uint":
                                        v = v == null ? 0U : Convert.ToUInt32(v);
                                        break;
                                    case "string[]":
                                        v = v == null ? new string[0] : d.GetString(i).Split(',');
                                        break;
                                    case "int[]":
                                    case "uint[]":
                                        var v2 = v == null ? new string[0] : d.GetString(i).Split(",");
                                        var v3 = new int[v2.Length];
                                        for (int j = 0; j < v2.Length; j++)
                                        {
                                            v3[j] = Convert.ToInt32(v2[j]);
                                        }
                                        v = v3;
                                        break;
                                    case "float[]":
                                        v2 = v == null ? new string[0] : d.GetString(i).Split(",");
                                        var vfloat = new float[v2.Length];
                                        for (int j = 0; j < v2.Length; j++)
                                        {
                                            vfloat[j] = Convert.ToSingle(v2[j]);
                                        }
                                        v = vfloat;
                                        break;
                                    case "int[][]":
                                    case "uint[][]":
                                        if (v != null)
                                        {
                                            var lengthArray2 = d.GetString(i).Split("|");
                                            var vUintArray2 = new int[lengthArray2.Length][];
                                            for (int j = 0; j < lengthArray2.Length; j++)
                                            {
                                                var lengthArray = lengthArray2[j].Split(',');
                                                vUintArray2[j] = new int[lengthArray.Length];
                                                for (int k = 0; k < lengthArray.Length; k++)
                                                {
                                                    vUintArray2[j][k] = Convert.ToInt32(lengthArray[k]);
                                                }
                                            }
                                            v = vUintArray2;
                                        }
                                        else
                                        {
                                            v = new int[0][];
                                        }
                                        break;
                                    case "string[][]":
                                        if (v != null)
                                        {
                                            var lengthArray2 = d.GetString(i).Split("|");
                                            var vUintArray2 = new string[lengthArray2.Length][];
                                            for (int j = 0; j < lengthArray2.Length; j++)
                                            {
                                                var lengthArray = lengthArray2[j].Split(',');
                                                vUintArray2[j] = new string[lengthArray.Length];
                                                for (int k = 0; k < lengthArray.Length; k++)
                                                {
                                                    vUintArray2[j][k] = lengthArray[k];
                                                }
                                            }
                                            v = vUintArray2;
                                        }
                                        else
                                        {
                                            v = new string[0][];
                                        }
                                        break;
                                    default:
                                        if (filedType.IndexOf("[][]") > -1)
                                        {
                                            var className = filedType.Split("[][]")[0];
                                            v = InstanceT.CreateInstance(className);
                                        }
                                        else if (filedType.IndexOf("[]") > -1)
                                        {

                                        }
                                        else
                                        {
                                            if (v != null)
                                            {
                                                v = InstanceT.CreateInstance(filedType, ((string)v).Split(",").ToArray());
                                            }
                                            else
                                            {
                                                v = InstanceT.CreateInstance(filedType);
                                            }
                                        }
                                        break;
                                }
                            }
                            vlaueFiled.Add(v);
                        }
                    }
                    index++;
                }
                CreateClass(Path.GetFileNameWithoutExtension(filePath), gengeneratedPath, classFiled, typeFiled, filedExpalinFiled, vlaueFiled);
            }
            catch (Exception)
            {

                throw;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        private static void CreateClass(string className, string savePath, List<string> classFiled, List<string> classFiledType, List<string> filedExpalin, List<object> vlaueFiled)
        {
            var st = new StringBuilder();
            st.AppendLine("//This is the generated code, the modification is invalid");
            st.AppendLine("using F;");
            st.AppendLine($"public struct {className}" + " : IFSerializable");
            st.AppendLine("{");
            for (int i = 1; i < classFiled.Count; i++)
            {
                st.AppendLine("    /// <summary>");
                st.AppendLine("    ///" + filedExpalin[i]);
                st.AppendLine("    /// </summary>");
                st.AppendLine("    public" + " " + classFiledType[i] + " " + classFiled[i] + ";");
            }
            st.AppendLine("    public void Deserialization(Serializable serializable)");
            st.AppendLine("    {");
            for (int i = 1; i < classFiled.Count; i++)
            {
                if (vlaueFiled[i] as IFSerializable != null)
                {
                    st.AppendLine("        serializable.ReadSerializable(ref " + classFiled[i] + ");");
                }
                else
                {
                    st.AppendLine("        serializable.Read(ref " + classFiled[i] + ");");
                }

            }
            st.AppendLine("    }");
            st.AppendLine("    public void Serialization(Serializable serializable)");
            st.AppendLine("    {");

            //for (int i = 1; i < classFiled.Count; i++)
            //{
            //    st.AppendLine("        serializable.Push(intFiled);");
            //}
            st.AppendLine("    }");

            st.AppendLine("}");
            var fileObj = File.CreateText(savePath + className + ".cs");
            fileObj.Write(st);
            fileObj.Close();
            var index = 0;
            var key = 0;
            var count = classFiled.Count;
            //生成byte
            var serializable = new Serializable();
            var dict = new Dictionary<int, byte[]>();
            for (int i = 0; i < vlaueFiled.Count; i++)
            {
                var v = vlaueFiled[i];
                if (index == 0)
                {
                    //serializable.Push(className);
                    key = ((int)Convert.ToUInt32(v));
                    dict.Add(key, new byte[0]);
                    index++;
                    continue;
                }
                index++;
                if (v != null)
                {
                    serializable.PushObj(v);
                }
                if (index == count)
                {
                    dict[key] = serializable.Bytes;
                    serializable.Bytes = new ByteStream();
                    index = 0;
                }
            }
            FileIO.WriteBytesDict($"{savePath}{className}", dict);
            CreateClassConfig(className, savePath);

        }

        private static void CreateClassConfig(string className, string savePath)
        {
            var st = new StringBuilder();
            st.AppendLine("//This is the generated code, the modification is invalid");
            st.AppendLine("using F;");
            st.AppendLine("using System.Collections.Generic;");
            st.AppendLine($"public static class {className}Config");
            st.AppendLine("{");
            st.AppendLine($"    public static Dictionary<int, {className}> Data;");
            st.AppendLine($"    public static {className} Get(int sid)\n    {{\r\n        return Data[sid];\r\n    }}");
            st.AppendLine($"    public static void Deserialization(Serializable se)\r\n    {{");
            //st.AppendLine($"        Data = new Dictionary<int, {className}>();");
            //st.AppendLine($"        var length = se.Read<int>();\r\n        while (length > 0)\r\n        {{\r\n            var key = se.Read<int>();\r\n            var toClass = new {className}();\r\n            toClass.Deserialization(se);\r\n            Data.Add(key, toClass);\r\n            length--;\r\n        }}\n    }}");
            st.AppendLine($"        se.ReadSerializable(ref Data);");
            st.AppendLine("    }");
            st.AppendLine("}");
            var fileObj = File.CreateText(savePath + className + "Config" + ".cs");
            fileObj.Write(st);
            fileObj.Close();
        }
    }
}

