using System;
using System.Windows;

namespace FSX.Windows.Core
{
    /// <summary>
    /// Helper class for clipboard operations
    /// </summary>
    /// <typeparam name="T"></typeparam>    
    /// <see cref="http://www.codeproject.com/Articles/878773/Implement-Copy-Paste-Csharp-VB-NET-Application"/>
    public class ClipBoardManager<T>
        where T : class
    {
        public static T GetFromClipboard()
        {
            T retrievedObj = null;
            IDataObject dataObj = Clipboard.GetDataObject();
            string format = typeof(T).FullName;
            if (dataObj.GetDataPresent(format))
            {
                retrievedObj = dataObj.GetData(format) as T;
            }
            return retrievedObj;
        }

        public static void CopyToClipboard(T objectToCopy)
        {
            // register my custom data format with Windows or get it if it's already registered
            //DataFormats format = DataFormats.GetDataFormat(typeof(T).FullName);

            // now copy to clipboard
            IDataObject dataObj = new DataObject();
            dataObj.SetData(typeof(T).FullName, objectToCopy, false);
            Clipboard.SetDataObject(dataObj, false);
        }

        public static void Clear()
        {
            Clipboard.Clear();
        }
    }
}
