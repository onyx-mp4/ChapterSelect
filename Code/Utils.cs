using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ChapterSelect.Code;

class OnyxUtils
    {
        public static Dictionary<string, object> ParseSaveDataString(string data)
        {
            Dictionary<string, object> gameVariables = new Dictionary<string, object>();

            string[] pairs = data.Split(',');
            foreach (string pair in pairs)
            {
                if (pair == "")
                    continue;
                if (!pair.Contains('='))
                {
                    Plugin.LOG.LogWarning("OnyxUtils.ParseSaveDataString: Invalid pair: " + pair);
                    continue;
                }
                string[] keyValue = pair.Split('=');
                string key = keyValue[0];
                string[] typeValue = keyValue[1].Split('|');
                int typeId = int.Parse(typeValue[0]);
                object value = typeValue[1];

                switch (typeId)
                {
                    case 0: // Boolean
                        gameVariables[key] = bool.Parse(value.ToString());
                        break;
                    case 1: // Float ("Single")
                        gameVariables[key] = float.Parse(value.ToString());
                        break;
                    case 2: // Integer ("Int32")
                        gameVariables[key] = int.Parse(value.ToString());
                        break;
                    case 3: // String
                        gameVariables[key] = value;
                        break;
                }
            }

            return gameVariables;
        }

        public static bool IsClassLoaded(string className)
        {
            // Get all assemblies in the current application domain
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                // Get all types within the assembly
                Type[] types = assembly.GetTypes();

                // Check if any of the types match the class name
                if (types.Any(type => type.Name == className))
                {
                    return true;
                }
            }

            return false;
        }
        
        public static AssetBundle LoadEmbeddedAssetBundle(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            // list all resources
            foreach (var res in assembly.GetManifestResourceNames())
            {
                Plugin.LOG.LogInfo("Resource: " + res);
            }

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    Debug.LogError("Could not find resource: " + resourceName);
                    return null;
                }

                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                Plugin.LOG.LogInfo("SZ: " + buffer.Length);
                return AssetBundle.LoadFromMemory(buffer);
            }
        }
    }