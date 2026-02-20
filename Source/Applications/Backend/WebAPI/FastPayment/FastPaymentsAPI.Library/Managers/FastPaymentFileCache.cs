using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using FastPaymentsApi.Contracts;

namespace FastPaymentsAPI.Library.Managers
{
	public class FastPaymentFileCache
	{
		private readonly string _filePath;
		private readonly object _locker = new();

		public FastPaymentFileCache(string filePath)
		{
			_filePath = string.IsNullOrWhiteSpace(filePath) ? throw new ArgumentNullException(nameof(filePath)) : filePath;

			var diretoryPath = Path.GetDirectoryName(_filePath);
			if(!Directory.Exists(diretoryPath))
			{
				Directory.CreateDirectory(diretoryPath);
			}

			if(!File.Exists(filePath))
			{
				var file = File.Create(filePath);
				file.Close();
			}

			var fileContent = File.ReadAllText(filePath);
			if(string.IsNullOrEmpty(fileContent))
			{
				File.WriteAllText(filePath, JsonConvert.SerializeObject(new List<FastPaymentDTO>()));
			}
		}

		public void WritePaymentCache(FastPaymentDTO fastPaymentDto)
		{
			lock(_locker)
			{
				var cache = JsonConvert.DeserializeObject<List<FastPaymentDTO>>(File.ReadAllText(_filePath));
				cache.Add(fastPaymentDto);
				File.WriteAllText(_filePath, JsonConvert.SerializeObject(cache));
			}
		}

		public IList<FastPaymentDTO> GetAllPaymentCaches()
		{
			lock(_locker)
			{
				return JsonConvert.DeserializeObject<List<FastPaymentDTO>>(File.ReadAllText(_filePath)).ToList();
			}
		}

		public void RemovePaymentCaches(IList<FastPaymentDTO> cachesToRemove)
		{
			lock(_locker)
			{
				var cache = JsonConvert.DeserializeObject<List<FastPaymentDTO>>(File.ReadAllText(_filePath));
				var newContent = JsonConvert.SerializeObject(cache.Except(cachesToRemove));
				File.WriteAllText(_filePath, newContent);
			}
		}
	}
}
