#region File Header

// ***********************************************************************
// Author	: Sander Struijk
// File		: IsoStorage.cs
// Created	: 2014 04 25 14:41
// Updated	: 2014 05 23 12:32
// ***********************************************************************

#endregion

#region Using statements

using System.Collections.Generic;
using System.Linq;

#region Using statements

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
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

        /// <summary>   Information describing the application. </summary>
        private readonly ApplicationData _appData = ApplicationData.Current;

        /// <summary>   Pathname of the storage folder. </summary>
        private StorageFolder _storageFolder;

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
                        _storageFolder = _appData.LocalFolder;
                        break;
                    case StorageType.Temporary:
                        _storageFolder = _appData.TemporaryFolder;
                        break;
                    case StorageType.Roaming:
                        _storageFolder = _appData.RoamingFolder;
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
            fileName = AppendExt(fileName);
            var file = await _storageFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(data));
        }

        /// <summary>   Delete a file asynchronously. </summary>
        /// <remarks>   Sander.struijk, 25.04.2014. </remarks>
        /// <exception cref="Exception">    Thrown when an exception error condition occurs. </exception>
        /// <param name="fileName"> . </param>
        public async void DeleteAsync(string fileName)
        {
            fileName = AppendExt(fileName);
            var file = await GetFileIfExistsAsync(fileName);
            if(file != null)
                await file.DeleteAsync();
        }

        /// <summary>   Deletes all asynchronous. </summary>
        /// <remarks>   Sander.struijk, 12.05.2014. </remarks>
        public async void DeleteAllAsync()
        {
            var files = await GetAllFilesAsync();
            if(files != null)
                foreach(var file in files.Where(file => file.Name.EndsWith(FileEnding)).Select(file => file.Name))
                    DeleteAsync(file);
        }

        /// <summary>   Gets all files asynchronous. </summary>
        /// <remarks>   Sander.struijk, 12.05.2014. </remarks>
        /// <returns>   all files asynchronous. </returns>
        private async Task<IReadOnlyList<StorageFile>> GetAllFilesAsync()
        {
            try
            {
                return await _storageFolder.GetFilesAsync();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>   At the moment the only way to check if a file exists to catch an exception... :/. </summary>
        /// <remarks>   Sander.struijk, 25.04.2014. </remarks>
        /// <param name="fileName"> . </param>
        /// <returns>   The file if exists asynchronous. </returns>
        private async Task<StorageFile> GetFileIfExistsAsync(string fileName)
        {
            try
            {
                return await _storageFolder.GetFileAsync(fileName);
            }
            catch
            {
                return null;
            }
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
                fileName = AppendExt(fileName);
                var file = await _storageFolder.GetFileAsync(fileName);
                var json = await FileIO.ReadTextAsync(file);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch(FileNotFoundException)
            {
                //file not existing is perfectly valid so simply return the default 
                return default(T);
                //throw;
            }
        }

        /// <summary>   Appends the file extension to the given filename. </summary>
        /// <remarks>   Sander.struijk, 25.04.2014. </remarks>
        /// <param name="fileName"> . </param>
        /// <returns>   A string. </returns>
        private string AppendExt(string fileName)
        {
            return fileName.EndsWith(".json") ? fileName.EndsWith(FileEnding) ? fileName : string.Format("{0}{1}", fileName, FileEnding) : string.Format("{0}{1}", fileName, FileEnding);
        }
    }
}