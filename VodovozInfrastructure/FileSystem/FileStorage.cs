using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VodovozInfrastructure.FileSystem
{
	public class FileStorage
	{
		private readonly string _storagePath;
		private List<string> _files = new List<string>();

		public virtual event EventHandler FilesRefreshed;
		public virtual IEnumerable<string> Files => _files;

		public FileStorage(string storagePath)
		{
			if(string.IsNullOrWhiteSpace(storagePath))
			{
				throw new ArgumentException($"'{nameof(storagePath)}' cannot be null or whitespace.", nameof(storagePath));
			}

			_storagePath = storagePath;
		}

		public virtual bool FileExist(string fileName)
		{
			if(string.IsNullOrWhiteSpace(fileName))
			{
				return false;
			}
			ValidateStoragePath();
			var storagedFile = GetStoragedFilePath(fileName);
			return File.Exists(storagedFile);
		}

		public virtual void Put(string inputFilePath, bool overwrite = false)
		{
			ValidateStoragePath();
			var fileName = Path.GetFileName(inputFilePath);
			Put(inputFilePath, fileName, overwrite);
		}

		public virtual void Put(string inputFilePath, string newName, bool overwrite = false)
		{
			ValidateStoragePath();
			var storagedFile = GetStoragedFilePath(newName);
			File.Copy(inputFilePath, storagedFile, overwrite);
			AddToList(storagedFile);
		}

		public virtual void TakeTo(string fileName, string outputFilePath, bool overwrite = false)
		{
			ValidateStoragePath();
			var storagedFile = GetStoragedFilePath(fileName);
			File.Copy(storagedFile, outputFilePath, overwrite);
		}

		public virtual string Take(string fileName)
		{
			ValidateStoragePath();
			return GetStoragedFilePath(fileName);
		}

		public virtual void Delete(string fileName)
		{
			ValidateStoragePath();
			var storagedFile = GetStoragedFilePath(fileName);
			File.Delete(storagedFile);
			RemoveFromList(storagedFile);
		}

		public virtual void Refresh()
		{
			ValidateStoragePath();
			var files = Directory.GetFiles(_storagePath).ToList();
			_files.Clear();
			_files.AddRange(files);
			FilesRefreshed?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void AddToList(string fileNameOrPath)
		{
			var fileName = Path.GetFileName(fileNameOrPath);
			if(_files.Contains(fileName))
			{
				return;
			}

			_files.Add(fileName);
			FilesRefreshed?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void RemoveFromList(string fileNameOrPath)
		{
			var fileName = Path.GetFileName(fileNameOrPath);
			if(!_files.Contains(fileName))
			{
				return;
			}

			_files.Remove(fileName);
			FilesRefreshed?.Invoke(this, EventArgs.Empty);
		}

		protected virtual string GetStoragedFilePath(string filePathOrName)
		{
			var fileName = Path.GetFileName(filePathOrName);
			return Path.Combine(_storagePath, fileName);
		}

		protected virtual void ValidateStoragePath()
		{
			if(!Directory.Exists(_storagePath))
			{
				throw new DirectoryNotFoundException(_storagePath);
			}
		}
	}
}
