using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public static class SaveUtility
    {
        /*  --- Usage ----
         
        -- Lets say we have a serialized class called Save --
        
        [System.Serializable]
        public class Save
        {
            public int testInt;
            public List<string> TestStrings = new List<string>();
        }
        
        -- For saving this class we would do the following -->> --
        
        Save save = new Save();
        save.testInt = 5;
        save.TestStrings.Add("Hello");
        
        SaveUtility.Save(save, "testPath");
        
        -- For loading this class we would do the following --> --
        
        Save save = SaveUtility.Load<Save, string>("testPath");
        
        Important : Since save operation uses PlayerPrefs and does not contain any encrypting. Dont save&load any confidential data using this class.
        
        --- End of Usage ---- */
        
        
        /// <summary>
        /// Saves any serialized class on the specified savepath.
        /// </summary>
        public static void Save<T>(this T save, string savePath)
            where T : class
        {
            var data = JsonUtility.ToJson(save);
            PlayerPrefs.SetString(savePath, data);
        }

        /// <summary>
        /// Loads and returns the saved class on the specified savepath.
        /// If no save is found, it will return a new instance of the class.
        /// </summary>
        /// <returns>Loaded serialized save class</returns>
        public static T Load<T,TSt>(TSt path)
            where T : class, new()
            where TSt : IComparable, ICloneable, IConvertible, IComparable<string>, IEnumerable<char>, IEnumerable, IEquatable<string>
        {
            var data = new T();

            if (PlayerPrefs.HasKey(path as string) == false)
            {
                data.Save(path as string);
            }

            {
                data = JsonUtility.FromJson<T>(PlayerPrefs.GetString(path as string));
            }
            return data;
        }
    }
}
