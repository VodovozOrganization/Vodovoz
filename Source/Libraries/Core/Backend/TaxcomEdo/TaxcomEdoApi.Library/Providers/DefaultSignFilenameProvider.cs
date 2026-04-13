using System;
using TaxcomEdoApi.Library.Models.Containers.Interfaces;

namespace TaxcomEdoApi.Library.Providers
{
	/// <summary>
	/// Поставщик имени для файла с подписью по умолчанию
	/// </summary>
	public class DefaultSignFilenameProvider : ISignFilenameProvider
	{
		private string _fileName;
		private string _filePath;

		public string GetFilePath(IContainerDocument document)
		{
			_filePath ??= document.GetFilePath(GetSignFilename());
			return _filePath;
		}

		/// <inheritdoc/>
		public string GetSignFilename()
		{
			_fileName ??= Guid.NewGuid() + ".p7s";
			return _fileName;
		}
	}

	/// <summary>
	/// Поставщик имени для файла с подписью
	/// </summary>
	public interface ISignFilenameProvider
	{
		/// <summary>
		/// Получение пути до файла
		/// </summary>
		/// <returns>Путь до файла</returns>
		string GetFilePath(IContainerDocument document);
		/// <summary>
		/// Получение имени для файла с подписью
		/// </summary>
		/// <returns>Имя файла</returns>
		string GetSignFilename();
	}
}
