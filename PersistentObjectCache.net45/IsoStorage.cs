#region File Header

// ***********************************************************************
// Author	: Sander Struijk
// File		: IsoStorage.cs
// Created	: 2014 05 08 09:43
// Updated	: 2014 05 23 12:32
// ***********************************************************************

#endregion

#region Using statements

using System.Linq;

#region Using statements

using System.Text;
using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

#endregion

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
        /// <summary>   The file ending. </summary>
        private const string FileEnding = ".cache.json";

        /// <summary>   Pathname of the base storage folder. </summary>
        private string _baseStoragePath;

        /// <summary>   Type of the storage. </summary>
        private StorageType _storageType;

        /// <summary>   Default constructor. </summary>
        /// <remarks>   Sander.struijk, 25.04.2014. </remarks>
        public IsoStorage() : this(StorageType.Local) {}

        /// <summary>   Constructor. </summary>
        /// <remarks>   Sander.struijk, 25.04.2014. </remarks>
        /// <param name="type"> The type. </param>
        public IsoStorage(StorageType type)
        {
            StorageType = type;
        }

        /// <summary>   Gets the full pathname of the storage file. </summary>
        /// <value> The full pathname of the storage file. </value>
        private string StoragePath
        {
            get { return Path.Combine(_baseStoragePath, "Cache"); }
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
                switch(_storageType)
                {
                    case StorageType.Local:
                        _baseStoragePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        break;
                    case StorageType.Temporary:
                        _baseStoragePath = string.Format("{0}\\Temp", Environment.GetFolderPath(Environment.SpecialFolder.System).Substring(0, 2));
                        break;
                    case StorageType.Roaming:
                        _baseStoragePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
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
            if(data == null)
                return;
            fileName = PrependPath(AppendExt(fileName));
            if(File.Exists(fileName)) File.Delete(fileName);
            else if(!Directory.Exists(StoragePath)) Directory.CreateDirectory(StoragePath);
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
            catch(FileNotFoundException)
            {
                //file not existing is perfectly valid so simply return the default 
                return default(T);
                //throw;
            }
        }

        /// <summary>   Deletes the given fileName. </summary>
        /// <remarks>   Sander.struijk, 12.05.2014. </remarks>
        /// <param name="fileName"> . </param>
        public void Delete(string fileName)
        {
            fileName = PrependPath(AppendExt(fileName));
            if(File.Exists(fileName))
                File.Delete(fileName);
        }

        /// <summary>   Deletes all files. </summary>
        /// <remarks>   Sander.struijk, 12.05.2014. </remarks>
        public void DeleteAll()
        {
            var files = Directory.EnumerateFiles(StoragePath).Where(file => file.EndsWith(FileEnding));
            foreach(var file in files)
            {
                Delete(file);
            }
        }

        /// <summary>   Prepends the storage path. </summary>
        /// <remarks>   Sander.struijk, 08.05.2014. </remarks>
        /// <param name="fileName"> . </param>
        /// <returns>   A string. </returns>
        private string PrependPath(string fileName)
        {
            return Path.Combine(StoragePath, fileName);
        }

        /// <summary>   Appends the file extension to the given filename. </summary>
        /// <remarks>   Sander.struijk, 25.04.2014. </remarks>
        /// <param name="fileName"> . </param>
        /// <returns>   A string. </returns>
        private string AppendExt(string fileName)
        {
            return fileName.EndsWith(".json") ? fileName.EndsWith(FileEnding) ? fileName : string.Format("{0}{1}", fileName, FileEnding) : string.Format("{0}{1}", fileName, FileEnding);
        }

        /// <summary>   Writes a text asynchronous. </summary>
        /// <remarks>   Sander.struijk, 08.05.2014. </remarks>
        /// <param name="filePath"> Full pathname of the file. </param>
        /// <param name="text">     The text. </param>
        /// <returns>   A Task. </returns>
        private async Task WriteTextAsync(string filePath, string text)
        {
            var encodedText = Encoding.Unicode.GetBytes(text);

            using(var sourceStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None, 4096, true))
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
            using(var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                var sb = new StringBuilder();

                var buffer = new byte[0x1000];
                int numRead;
                while((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    var text = Encoding.Unicode.GetString(buffer, 0, numRead);
                    sb.Append(text);
                }

                return sb.ToString();
            }
        }
    }
}