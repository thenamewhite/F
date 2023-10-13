using ExcelDataReader;
using F;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
// author  (hf) Date：2023/6/6 17:06:34
namespace F
{
    public class ExcelConfig
    {
        /// <summary>
        ///<param name="readDirectoryPath">读取文件路径</param>
        ///<param name="gengeneratedPath">生成cs 路径</param>
        ///<param name="configBytesPath">生成二进制文件路径</param>
        /// </summary>
        public static Dictionary<string, byte[]> Start(string readDirectoryPath, string gengeneratedPath, string configBytesPath = null)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //var path = "F:\\F2\\F2\\Config\\Test.xlsx";
            var classList = new Dictionary<string, byte[]>();
            //new List<(string, ByteStream)>();
            if (Directory.Exists(readDirectoryPath))
            {
                var files = Directory.GetFiles(readDirectoryPath, "*.xlsx", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var p = Path.GetFileName(file);
                    if (Path.GetFileName(file)[0] == '~')
                    {
                        continue;
                    }
                    var v = ReadFile(file, gengeneratedPath, configBytesPath);
                    classList.Add(v.Item1, v.Item2);
                }
            }
            else
            {
                Console.WriteLine($"{readDirectoryPath}，不是文件夹");
            }
            return classList;
        }

        private static (string, ByteStream) ReadFile(string filePath, string gengeneratedPath, string configBytesPath)
        {
            var index = 0;
            try
            {
                using FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var d = ExcelReaderFactory.CreateReader(fs);
                var row = d.RowCount;
                var classFiled = new List<string>();
                var filedExpalinFiled = new List<string>();
                var classTypeFiled = new List<string>();
                var splitsFiled = new List<string>();
                //var vlaueFiled = new List<object>();
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var vlaueFiled = new Dictionary<int, List<object>>();

                var filedLegnth = 0;

                while (d.Read())
                {

                    ///第一行是注释
                    if (index == 0)
                    {
                        //var ro = d.GetString(0);
                        //filedExpalinFiled.Add(ro);
                        for (int i = 0; i < d.FieldCount; i++)
                        {
                            var t = d.GetString(i);
                            if (!string.IsNullOrEmpty(t))
                            {
                                var ro = d.GetString(i);
                                filedExpalinFiled.Add(ro);
                            }
                        }

                    }
                    //第二行是函数生成的字段名称
                    else if (index == 1)
                    {

                        for (int i = 0; i < d.FieldCount; i++)
                        {
                            var t = d.GetString(i);
                            if (!string.IsNullOrEmpty(t))
                            {
                                //typeFiled.Add(t);
                                var funName = d.GetString(i) + "Filed";
                                classFiled.Add(funName);
                            }
                        }

                        //var funName = d.GetString(1) + "Filed";
                        //classFiled.Add(funName);
                    }
                    //生成的类型
                    else if (index == 3)
                    {

                        for (int i = 0; i < classFiled.Count; i++)
                        {
                            var t = d.GetString(i);
                            if (!string.IsNullOrEmpty(t))
                            {
                                classTypeFiled.Add(t);
                            }
                            //filedLegnth =;
                        }
                    }
                    else if (index == 2)
                    {
                        //用什么来分割
                        for (int i = 0; i < d.FieldCount; i++)
                        {
                            var t = d.GetString(i);
                            splitsFiled.Add(t);
                        }
                    }
                    else
                    {
                        if (d.FieldCount == 0 || d.GetValue(0) == null)
                        {
                            break;
                        }
                        var data = new List<object>();
                        vlaueFiled.Add(index - 2, data);
                        for (int i = 0; i < classTypeFiled.Count; i++)
                        {
                            //vlaueFiled.Add(index - 2)
                            data.Add(d.GetValue(i));
                        }
                    }

                    //后续都是值
                    //}
                    index++;
                }

                var dataClassName = fileName + "Data";
                fs.Close();
                fs.Dispose();



                //return default;
                return (fileName, CreateClass(fileName, dataClassName, gengeneratedPath, configBytesPath, classFiled, classTypeFiled, filedExpalinFiled, splitsFiled, vlaueFiled));
            }
            catch (Exception v)
            {
                Console.WriteLine($"  read :{v.StackTrace},row:{index + 1},{filePath}");
                throw new Exception($"  read :{v.StackTrace},row:{index + 1},{filePath}"); ;
            }

        }

        private static object Push(string filedType, string splitFiled, object v)
        {
            switch (filedType)
            {
                case "string":
                    v = v == null ? string.Empty : Convert.ToString(v).Replace("\\n", "\n");
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
                    if (string.IsNullOrEmpty(splitFiled))
                    {
                        v = v == null ? new string[0] : v.ToString().Split(',');
                    }
                    else
                    {
                        var vS = v == null ? new string[0] : v.ToString().Split(splitFiled);
                        for (int i = 0; i < vS.Length; i++)
                        {
                            vS[i] = vS[i].Replace("\\n", "\n");
                        }
                        v = vS;
                    }
                    break;
                ////处理语言文本，文本中本来就包含，用;来分割
                //case "string[]|;":
                //    v = v == null ? new string[0] : v.ToString().Split(';');
                //    break;
                case "int[]":
                case "uint[]":
                    var v2 = v == null ? new string[0] : Convert.ToString(v).Split(",");
                    var v3 = new int[v2.Length];
                    for (int j = 0; j < v2.Length; j++)
                    {
                        v3[j] = Convert.ToInt32(v2[j]);
                    }
                    v = v3;
                    break;
                case "float[]":
                    v2 = v == null ? new string[0] : v.ToString().Split(",");
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
                        var lengthArray2 = ((string)v).Split("|");
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
                        var lengthArray2 = ((string)v).Split("|");
                        var vUintArray2 = new string[lengthArray2.Length][];
                        for (int j = 0; j < lengthArray2.Length; j++)
                        {
                            var lengthArray = lengthArray2[j].Split(',');
                            vUintArray2[j] = new string[lengthArray.Length];
                            for (int k = 0; k < lengthArray.Length; k++)
                            {
                                vUintArray2[j][k] = lengthArray[k].Replace("\\n", "\n");
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
                    if (filedType != null)
                    {
                        if (filedType.IndexOf("[][]") > -1)
                        {
                            var className = filedType.Split("[][]")[0];
                            v = InstanceT.CreateInstance(className);
                        }
                        else if (filedType.IndexOf("[]") > -1)
                        {
                            filedType = filedType.Split("[]")[0];

                            if (v is string vS)
                            {
                                var data = ((string)v);
                                //if (d.GetString(i) != null)
                                //{
                                var lengthArray2 = vS.Split("|");
                                IFSerializable[] vUintArray2 = new IFSerializable[lengthArray2.Length];
                                for (int j = 0; j < vUintArray2.Length; j++)
                                {
                                    vUintArray2[j] = (IFSerializable)InstanceT.CreateInstance(filedType, lengthArray2[j].Split(",").ToArray());
                                }
                                v = vUintArray2;

                            }
                            else
                            {
                                v = Array.Empty<IFSerializable>();
                            }
                        }
                        else if (filedType.IndexOf("Enum") > -1)
                        {
                            v = v != null ? Convert.ToInt32(v) : 0;
                        }
                        else
                        {
                            if (v != null)
                            {
                                if (v is string vS)
                                {
                                    v = InstanceT.CreateInstance(filedType, vS.Split(",").ToArray());
                                }
                                else
                                {
                                    v = InstanceT.CreateInstance(filedType);
                                }
                            }
                            else
                            {
                                v = InstanceT.CreateInstance(filedType);
                            }
                        }
                    }
                    break;
            }
            return v;
        }


        private static ByteStream CreateClass(string className, string dataClassName, string savePath, string configBytesPath, List<string> classFiled, List<string> classFiledType, List<string> filedExpalin, List<string> splitFiled, Dictionary<int, List<object>> vlaueFileds)
        {
            var st = new StringBuilder();
            st.AppendLine("//This is the generated code, the modification is invalid");
            st.AppendLine("using F;");
            st.AppendLine("using System;");
            st.AppendLine("using System.Collections.Generic;");
            st.AppendLine($"public struct {dataClassName}" + " : IFSerializable");
            st.AppendLine("{");
            for (int i = 0; i < classFiled.Count; i++)
            {
                st.AppendLine("    /// <summary>");
                st.AppendLine("    ///" + filedExpalin[i]);
                st.AppendLine("    /// </summary>");
                st.AppendLine("    public" + " " + classFiledType[i] + " " + classFiled[i] + ";");
            }
            st.AppendLine("    public void Deserialization(Serializable serializable)");
            st.AppendLine("    {");
            for (int i = 0; i < classFiledType.Count; i++)
            {
                var v = classFiledType[i];
                switch (classFiledType[i])
                {
                    case "string":
                    case "int":
                    case "float":
                    case "long":
                    case "uint":
                    case "string[]":
                    case "int[]":
                    case "uint[]":
                    case "float[]":
                    case "int[][]":
                    case "uint[][]":
                    case "string[][]":
                        st.AppendLine("        serializable.Read(ref " + classFiled[i] + ");");
                        break;
                    default:
                        if (v.IndexOf("Enum") > -1)
                        {
                            st.AppendLine("        serializable.Read(ref " + classFiled[i] + ");");
                        }
                        else
                        {
                            st.AppendLine("        serializable.ReadSerializable(ref " + classFiled[i] + ");");
                        }
                        break;
                }
            }
            st.AppendLine("    }");
            st.AppendLine("    public void Serialization(Serializable serializable)");
            st.AppendLine("    {");
            st.AppendLine("    }");
            st.AppendLine("}");
            var key = 0;
            var count = classFiled.Count;
            //生成byte
            var serializable = new Serializable();
            var dict = new Dictionary<int, byte[]>();

            foreach (var value in vlaueFileds)
            {
                serializable.Bytes = default;
                key = Convert.ToInt32(value.Value[0]);
                dict.Add(key, new byte[0]);
                for (int i = 0; i < classFiledType.Count; i++)
                {
                    serializable.PushObj(Push(classFiledType[i], splitFiled.Count >= i ? splitFiled[i] : string.Empty, value.Value[i]));
                }
                dict[key] = serializable.Bytes;
            }

            st.AppendLine("\n");
            st.AppendLine($"public partial class {className} : IFSerializable");
            st.AppendLine("{");
            st.AppendLine($"    public static Dictionary<int, {dataClassName}> Data;");
            st.AppendLine($"    public static {dataClassName} Get(int sid)\n    {{\r\n        return Data[sid];\r\n    }}");
            st.AppendLine("    ///<summary>");
            st.AppendLine("    ///序列化完成");
            st.AppendLine("    ///<summary>");
            /// 序列化完成
            st.AppendLine($"    private Action DeserializationComplete;");
            st.AppendLine($"    public void Deserialization(Serializable se)\r\n    {{");
            st.AppendLine($"        se.ReadSerializable(ref Data);");
            st.AppendLine($"        DeserializationComplete?.Invoke();");
            st.AppendLine($"        DeserializationComplete=null;");
            st.AppendLine("    }");
            st.AppendLine("    public void Serialization(Serializable serializable)");
            st.AppendLine("    {");
            st.AppendLine("    }");
            st.AppendLine("}");
            var fileObj = File.CreateText($"{savePath}{className}.cs");
            fileObj.Write(st);
            fileObj.Close();
            FileIO.CreateFile($"{configBytesPath}{dataClassName}");
            return FileIO.WriteBytesDict($"{configBytesPath}{dataClassName}", dict);
        }



        private static void CreateClassConfig(string className, string dataClassName, string savePath)
        {
            //var st = new StringBuilder();
            //st.AppendLine("//This is the generated code, the modification is invalid");
            //st.AppendLine("using F;");
            //st.AppendLine("using System.Collections.Generic;");
            //st.AppendLine($"public static class {className}");
            //st.AppendLine("{");
            //st.AppendLine($"    public static Dictionary<int, {dataClassName}> Data;");
            //st.AppendLine($"    public static {dataClassName} Get(int sid)\n    {{\r\n        return Data[sid];\r\n    }}");
            //st.AppendLine($"    public static void Deserialization(Serializable se)\r\n    {{");
            //st.AppendLine($"        se.ReadSerializable(ref Data);");
            //st.AppendLine("    }");
            //st.AppendLine("}");
            //var fileObj = File.CreateText(savePath + className + ".cs");
            //fileObj.Write(st);
            //fileObj.Close();
        }
    }
}

