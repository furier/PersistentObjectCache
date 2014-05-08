#region File Header

// // ***********************************************************************
// // Author           : Sander Struijk
// // ***********************************************************************

#endregion


#region Using statements

using System.Text;
using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

#endregion

// ReSharper disable once CheckNamespace
namespace PersistentObjectCachenetcore451
{
    /// <summary>   Values that represent StorageType. </summary>
    /// <remarks>   Sander.struijk, 25.04.2014. </remarks>
    public enum StorageType
    {
        Local,
        Temporary,
        Roaming
    }

    /// <summary>   An ISO storage. </summary>
    /// <remarks>   Sander.struijk, 25.04.2014. </remarks>
    /// <typeparam name="T">    Generic type parameter. </typeparam>
    internal class IsoStorage<T>
    {
        /// <summary>   Pathname of the storage folder. </summary>
        private string _storagePath;

        /// <summary>   Type of the storage. </summary>
        private StorageType _storageType;

        /// <summary>   Default constructor. </summary>
        /// <remarks>   Sander.struijk, 25.04.2014. </remarks>
        public IsoStorage() : this(StorageType.Local) { }

        /// <summary>   Constructor. </summary>
        /// <remarks>   Sander.struijk, 25.04.2014. </remarks>
        /// <param name="type"> The type. </param>
        public IsoStorage(StorageType type)
        {
            StorageType = type;
        }

        /// <summary>   Gets or sets the type of the storage. </summary>
        /// <exception cref="Exception">    Thrown when an exception error condition occurs. </exception>
        /// <value> The type of the storage. </value>
        public StorageType StorageType
        {
            get { return _storageType; }
            set
            {
                _storageType = value;
                // set the storage folder
                switch (_storageType)
                {
                    case StorageType.Local:
                        _storagePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        break;
                    case StorageType.Temporary:
                        _storagePath = string.Format("{0}\\Temp", Environment.GetFolderPath(Environment.SpecialFolder.System).Substring(0, 2));
                        break;
                    case StorageType.Roaming:
                        _storagePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                        break;
                    default:
                        throw new Exception(String.Format("Unknown StorageType: {0}", _storageType));
                }
            }
        }

        /// <summary>   Saves a serialized object to storage asynchronously. </summary>
        /// <remarks>   Sander.struijk, 25.04.2014. </remarks>
        /// <exception cref="Exception">    Thrown when an exception error condition occurs. </exception>
        /// <param name="fileName">     . </param>
        /// <param name="data">         The data. </param>
        public async void SaveAsync(string fileName, T data)
        {
            if (data == null)
                return;
            fileName = PrependPath(AppendExt(fileName));
            if (File.Exists(fileName)) File.Delete(fileName);
            await WriteTextAsync(fileName, JsonConvert.SerializeObject(data));
        }

        /// <summary>   Load a given filename asynchronously. </summary>
        /// <remarks>   Sander.struijk, 25.04.2014. </remarks>
        /// <exception cref="Exception">    Thrown when an exception error condition occurs. </exception>
        /// <param name="fileName"> . </param>
        /// <returns>   The asynchronous. </returns>
        public async Task<T> LoadAsync(string fileName)
        {
            try
            {
                fileName = PrependPath(AppendExt(fileName));
                var json = await ReadTextAsync(fileName);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (FileNotFoundException)
            {
                //file not existing is perfectly valid so simply return the default 
                return default(T);
                //throw;
            }
        }

        /// <summary>   Prepends the storage path. </summary>
        /// <remarks>   Sander.struijk, 08.05.2014. </remarks>
        /// <param name="fileName"> . </param>
        /// <returns>   A string. </returns>
        private string PrependPath(string fileName)
        {
            return Path.Combine(_storagePath, fileName);
        }

        /// <summary>   Appends the file extension to the given filename. </summary>
        /// <remarks>   Sander.struijk, 25.04.2014. </remarks>
        /// <param name="fileName"> . </param>
        /// <returns>   A string. </returns>
        private string AppendExt(string fileName)
        {
            return fileName.Contains(".json") ? fileName : string.Format("{0}.json", fileName);
        }

        /// <summary>   Writes a text asynchronous. </summary>
        /// <remarks>   Sander.struijk, 08.05.2014. </remarks>
        /// <param name="filePath"> Full pathname of the file. </param>
        /// <param name="text">     The text. </param>
        /// <returns>   A Task. </returns>
        private async Task WriteTextAsync(string filePath, string text)
        {
            var encodedText = Encoding.Unicode.GetBytes(text);

            using (var sourceStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None, 4096, true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
            }
        }

        /// <summary>   Reads text asynchronous. </summary>
        /// <remarks>   Sander.struijk, 08.05.2014. </remarks>
        /// <param name="filePath"> Full pathname of the file. </param>
        /// <returns>   The text asynchronous. </returns>
        private async Task<string> ReadTextAsync(string filePath)
        {
            if(!File.Exists(filePath)) return string.Empty;
            using (var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                var sb = new StringBuilder();

                var buffer = new byte[0x1000];
                int numRead;
                while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    var text = Encoding.Unicode.GetString(buffer, 0, numRead);
                    sb.Append(text);
                }

                return sb.ToString();
            }
        }
    }
}
