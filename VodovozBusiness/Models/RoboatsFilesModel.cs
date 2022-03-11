using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Parameters;

namespace Vodovoz.Models
{
	public class RoboatsFilesModel
	{
		private readonly RoboatsSettings _roboatsSettings;

		/// <summary>
		/// Если вызывается async метод, то событие не потокобезопасно.
		/// </summary>
		public event EventHandler<SaveFilesArgs> OnFileCopy;

		public RoboatsFilesModel(RoboatsSettings roboatsSettings)
		{
			_roboatsSettings = roboatsSettings ?? throw new ArgumentNullException(nameof(roboatsSettings));
		}

		public void SaveDeliveryScheduleFiles(IEnumerable<string> fileNames, string outputFolder, bool overwrite = true)
		{
			SaveFiles(_roboatsSettings.DeliverySchedulesAudiofilesFolder, fileNames, outputFolder, overwrite);
		}

		public async Task SaveDeliveryScheduleFilesAsync(IEnumerable<string> fileNames, string outputFolder, bool overwrite = true)
		{
			await Task.Run(() => SaveFiles(_roboatsSettings.DeliverySchedulesAudiofilesFolder, fileNames, outputFolder, overwrite));
		}

		public void SaveAddressesFiles(IEnumerable<string> fileNames, string outputFolder, bool overwrite = true)
		{
			SaveFiles(_roboatsSettings.AddressesAudiofilesFolder, fileNames, outputFolder, overwrite);
		}

		public async void SaveAddressesFilesAsync(IEnumerable<string> fileNames, string outputFolder, bool overwrite = true)
		{
			await Task.Run(() => SaveFiles(_roboatsSettings.AddressesAudiofilesFolder, fileNames, outputFolder, overwrite));			
		}

		public void SaveWaterTypesFiles(IEnumerable<string> fileNames, string outputFolder, bool overwrite = true)
		{
			SaveFiles(_roboatsSettings.WaterTypesAudiofilesFolder, fileNames, outputFolder, overwrite);
		}

		public async void SaveWaterTypesFilesAsync(IEnumerable<string> fileNames, string outputFolder, bool overwrite = true)
		{
			await Task.Run(() => SaveFiles(_roboatsSettings.WaterTypesAudiofilesFolder, fileNames, outputFolder, overwrite));
		}

		private void SaveFiles(string sourceFolder, IEnumerable<string> fileNames, string outputFolder, bool overwrite)
		{
			var totalFiles = fileNames.Count();
			int copiedFiles = 0;

			foreach(var fileName in fileNames)
			{
				string sourcePath = Path.Combine(_roboatsSettings.DeliverySchedulesAudiofilesFolder, fileName);
				string destPath = Path.Combine(outputFolder, fileName);
				File.Copy(sourcePath, destPath, overwrite);
				var args = new SaveFilesArgs(++copiedFiles, totalFiles);
				OnFileCopy?.Invoke(this, args);
			}
		}

		public bool ValidateDeliveryScheduleFiles(out IEnumerable<string> validationResults, params string[] fileNames)
		{
			validationResults = Validate(_roboatsSettings.DeliverySchedulesAudiofilesFolder, fileNames);
			return !validationResults.Any();
		}

		public bool ValidateAddressesFiles(out IEnumerable<string> validationResults, params string[] fileNames)
		{
			validationResults = Validate(_roboatsSettings.AddressesAudiofilesFolder, fileNames);
			return !validationResults.Any();
		}

		public bool ValidateWaterTypesFiles(out IEnumerable<string> validationResults, params string[] fileNames)
		{
			validationResults = Validate(_roboatsSettings.WaterTypesAudiofilesFolder, fileNames);
			return !validationResults.Any();
		}

		private IEnumerable<string> Validate(string folderPath, IEnumerable<string> fileNames)
		{
			if(!Directory.Exists(folderPath))
			{
				yield return $"Каталог {folderPath} не существует или к нему нет доступа.";
				yield break;
			}

			foreach(var fileName in fileNames)
			{
				string fullPath = Path.Combine(folderPath, fileName);
				if(!File.Exists(fullPath))
				{
					yield return $"Файл {fullPath} не существует или к нему нет доступа.";
				}
			}
		}

		public class SaveFilesArgs : EventArgs
		{
			public int CopiedFiles { get; }
			public int TotalFiles { get; }

			public SaveFilesArgs(int copiedFiles, int totalFiles)
			{
				CopiedFiles = copiedFiles;
				TotalFiles = totalFiles;
			}
		}
	}
}
