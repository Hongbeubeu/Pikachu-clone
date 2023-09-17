//----------------------------------------------
// SA: Google Spreadsheet Loader
// Copyright © 2014 SuperAshley Entertainment
//----------------------------------------------

using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SuperAshley.GoogleSpreadSheet
{
    public class SASheetDataClassGenerator
    {
        const string FIELD_FORMAT = "\tpublic {0} {1};\n";

        public static void GenerateClass(string name, ValueRange cellList)
        {
            var className = cellList.Values[0][0].ToString();
            Type elementType = Assembly.GetExecutingAssembly().GetType(className);
            if (elementType == null)
            {
                //			className = name.Replace(" ", "");
                string classData = string.Empty;
                classData =
                    "using UnityEngine;\nusing System.Collections.Generic;\n\n[System.Serializable]\npublic class " +
                    className + " {\n";
                for (int col = 0; col < cellList.Values[1].Count; col++)
                {
                    string fieldName = cellList.Values[1][col].ToString();
                    if (!fieldName.Contains("_")) continue;
                    string filedType = fieldName.Substring(0, fieldName.IndexOf('_'));
                    string fieldName2 = fieldName.Substring(fieldName.IndexOf('_') + 1);
                    if (filedType == "non")
                    {
                        continue;
                    }

                    classData += string.Format(FIELD_FORMAT, GetTypeName(filedType.Trim()), fieldName2);
                }

                classData += "}\n\n";
                string dataClassName = className + "Data";

                classData += "public class " + dataClassName + " : ScriptableObject {\n\tpublic List<" + className +
                             "> " + className[0].ToString().ToLower() + className.Substring(1) + "s = new List<" +
                             className + ">();\n}\n";

                StreamWriter writer = File.CreateText(Application.dataPath +
                                                      SASettings.ScriptFolder.Replace("Assets", "") + "/" +
                                                      dataClassName + ".cs");
                writer.WriteLine(classData);
                writer.Close();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private static string GetTypeName(string _data)
        {
            string typeName = "";
            List<string> annotationList = new List<string>(_data.Split('.'));
            if (annotationList.Count == 1)
            {
                typeName = GetPrimitiveTypeName(annotationList[0]);
            }
            else
            {
                typeName = GetComplicatedTypeName(annotationList);
                //if (annotationList[0] == "li")
                //{
                //	string elementType = string.Empty;
                //	if (annotationList[1].Length == 1)
                //	{
                //		elementType = GetPrimitiveTypeName(annotationList[1]);
                //	}
                //	else if (annotationList[1].StartsWith("p"))
                //	{
                //		int first = annotationList[1].IndexOf('<');
                //		int last = annotationList[1].IndexOf('>');
                //		string insideStr = annotationList[1].Substring(first + 1, last - first - 1);
                //		string[] elementList = insideStr.Split(',');
                //		string keyType = GetPrimitiveTypeName(elementList[0]).Capitalize();
                //		string valueType = GetPrimitiveTypeName(elementList[1]).Capitalize();
                //		elementType = "Pair" + keyType + valueType;
                //	}
                //	else if (annotationList[1] == "li")
                //	{

                //	}
                //	typeName = "List<" + elementType + ">";
                //}
            }

            return typeName;
        }

        private static string GetComplicatedTypeName(List<string> annotationList)
        {
            string typeName = string.Empty;
            if (annotationList == null || annotationList.Count == 0)
            {
                return string.Empty;
            }

            if (annotationList.Count == 1)
            {
                return GetPrimitiveTypeName(annotationList[0]);
            }

            List<string> subAnnotationList = new List<string>(annotationList);
            subAnnotationList.RemoveAt(0);

            if (annotationList[0] == "li")
            {
                string elementType = GetComplicatedTypeName(subAnnotationList);
                typeName = "List<" + elementType + ">";
            }
            else if (annotationList[0].StartsWith("p"))
            {
                int first = annotationList[1].IndexOf('<');
                int last = annotationList[1].IndexOf('>');
                string insideStr = annotationList[1].Substring(first + 1, last - first - 1);
                string[] elementList = insideStr.Split(',');
                string keyType = GetPrimitiveTypeName(elementList[0]).Capitalize();
                string valueType = GetPrimitiveTypeName(elementList[1]).Capitalize();
                typeName = "Pair" + keyType + valueType;
            }

            return typeName;
        }

        private static string GetPrimitiveTypeName(string _data)
        {
            string typeName = string.Empty;
            if (_data == "n")
            {
                typeName = "int";
            }
            else if (_data == "l")
            {
                typeName = "long";
            }
            else if (_data == "s")
            {
                typeName = "string";
            }
            else if (_data == "f")
            {
                typeName = "float";
            }
            else if (_data == "sp")
            {
                typeName = "Sprite";
            }
            else if (_data == "spine")
            {
                typeName = "SkeletonAnimation";
            }
            else if (_data == "skeDat")
            {
                typeName = "SkeletonDataAsset";
            }
            else if (_data == "pref")
            {
                typeName = "GameObject";
            }
            else if (_data.StartsWith("p"))
            {
                int first = _data.IndexOf('<');
                int last = _data.IndexOf('>');
                string insideStr = _data.Substring(first + 1, last - first - 1);
                string[] elementList = insideStr.Split(',');
                string keyType = GetPrimitiveTypeName(elementList[0]).Capitalize();
                string valueType = GetPrimitiveTypeName(elementList[1]).Capitalize();
                typeName = "Pair" + keyType + valueType;
            }

            return typeName;
        }

        public static void CreateAsset(string name, ValueRange cellList)
        {
            name = name.Replace(" ", "");
            string typeName = cellList.Values[0][0].ToString();

            string dataClassName = cellList.Values[0][0].ToString() + "Data";
            Type assetType = GetTypeByName(dataClassName);
            if (assetType != null)
            {
                var dataHolder =
                    AssetDatabase.LoadAssetAtPath(SASettings.AssetFolder + "/" + "Raw" + name + ".asset", assetType);
                if (dataHolder == null)
                {
                    dataHolder = ScriptableObject.CreateInstance(assetType);
                    AssetDatabase.CreateAsset(dataHolder, SASettings.AssetFolder + "/" + "Raw" + name + ".asset");
                }

                Type dataType = GetTypeByName(typeName);

                FieldInfo dataListField =
                    assetType.GetField(typeName[0].ToString().ToLower() + typeName.Substring(1) + "s");
                var dataList = dataListField.GetValue(dataHolder);
                dataList.GetType().GetMethod("Clear").Invoke(dataList, null);
                for (int row = 2; row < cellList.Values.Count; row++)
                {
                    //if (string.IsNullOrEmpty(cellList[row][1]))
                    //{
                    //    break;
                    //}
                    var data = Activator.CreateInstance(dataType);
                    for (int col = 0; col < cellList.Values[1].Count; col++)
                    {
                        string _field = cellList.Values[1][col].ToString();
                        if (!_field.Contains("_")) continue;
                        string fieldName = _field.Substring(_field.IndexOf('_') + 1);
                        string fieldType = _field.Substring(0, _field.IndexOf('_'));
                        if (fieldType == "non")
                        {
                            continue;
                        }

                        if (cellList.Values[row].Count <= col) continue;
                        string _value = cellList.Values[row][col].ToString();
                        if (string.IsNullOrEmpty(_value.ToString()))
                        {
                            continue;
                        }

                        FieldInfo info = dataType.GetField(fieldName);
                        object value = null;
                        if (info == null)
                            continue;
                        if (info.FieldType == typeof(int))
                        {
                            if (int.TryParse(_value, out int rs))
                            {
                                value = rs;
                            }
                        }

                        if (info.FieldType == typeof(long))
                        {
                            if (long.TryParse(_value, out long rs))
                            {
                                value = rs;
                            }
                        }
                        else if (info.FieldType == typeof(float))
                        {
                            // value = float.Parse(cellList[row][col]);
                            if (float.TryParse(_value, out float rs))
                            {
                                value = rs;
                            }
                        }
                        else if (info.FieldType == typeof(string))
                        {
                            value = _value;
                        }

                        info.SetValue(data, value);
                    }

                    dataList.GetType().GetMethod("Add").Invoke(dataList, new object[] {data});
                }

                EditorUtility.SetDirty(dataHolder);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                EditorUtility.DisplayDialog("Error",
                    "Cannot find the script " + dataClassName + ", please generate the script first.", "ok");
            }
        }


        public static Type GetTypeByName(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name == name)
                        return type;
                }
            }

            return null;
        }


        //		private static SkeletonDataAsset GetSkeletonDataAsset(string data)
        //		{
        //			List<SkeletonDataAsset> skeDataList = AssetUtils.GetAtPath<SkeletonDataAsset>(SKELETONDATA_ASSET_PATH);
        //			SkeletonDataAsset skeleton = skeDataList.Find(x => x.name == data);
        //			if (skeleton == null)
        //			{
        //				skeleton = null;
        //				Debug.LogError($"{data} SkeletonDataAsset = null");
        //			}
        //			return skeleton;
        //		}
        private static List<int> GetListInt(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            List<int> result = new List<int>();
            string[] splitedData = data.Split(',');
            for (int i = 0; i < splitedData.Length; i++)
            {
                result.Add(int.Parse(splitedData[i].Trim()));
            }

            return result;
        }

        private static List<long> GetListLong(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            List<long> result = new List<long>();
            string[] splitedData = data.Split(',');
            for (int i = 0; i < splitedData.Length; i++)
            {
                result.Add(long.Parse(splitedData[i].Trim()));
            }

            return result;
        }
    }
}