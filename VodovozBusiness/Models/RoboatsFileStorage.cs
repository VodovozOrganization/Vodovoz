using QS.Dialog;
using System;
using System.IO;
using Vodovoz.Tools;
using VodovozInfrastructure.FileSystem;

namespace Vodovoz.Models
{
	public class RoboatsFileStorage : FileStorage
	{
		private readonly IInteractiveService _interactiveService;
		private readonly IErrorReporter _errorReporter;

		public RoboatsFileStorage(
			string storagePath,
			IInteractiveService interactiveService,
			IErrorReporter errorReporter
		) : base(storagePath)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_errorReporter = errorReporter ?? throw new ArgumentNullException(nameof(errorReporter));
		}

		public override bool FileExist(string fileName)
		{
			try
			{
				return base.FileExist(fileName);
			}
			catch(DirectoryNotFoundException)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, $"Каталог файлового хранилища не найден. Обратитесь в тех. поддержку.", "Ошибка");
			}
			return false;
		}

		public bool TryPut(string inputFilePath, bool overwrite = false)
		{
			try
			{
				base.Put(inputFilePath, overwrite);
				return true;
			}
			catch(UnauthorizedAccessException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, $"Нет доступа к файлу или каталогу. Обратитесь в тех. поддержку. \n {ex.Message}", "Ошибка");
			}
			catch(PathTooLongException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, $"Путь содержит слишком много символов. Переместите файл в другой каталог и повторите попытку. \n {ex.Message}", "Ошибка");
			}
			catch(DirectoryNotFoundException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, $"Каталог не найден. \n {ex.Message}", "Ошибка");
			}
			catch(FileNotFoundException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, $"Файл не найден. \n {ex.Message}", "Ошибка");
			}
			catch(Exception ex)
			{
				if(ex is ArgumentException ||
					ex is ArgumentNullException ||
					ex is IOException ||
					ex is NotSupportedException)
				{
					_interactiveService.ShowMessage(ImportanceLevel.Error, $"Произошла ошибка при сохранении файла в хранилище: {ex.Message}. \n Обратитесь в тех. поддержку", "Ошибка");
					_errorReporter.SendErrorReport(new[] { ex });
					return false;
				}
				throw;
			}
			return false;
		}

		public bool TryPut(string inputFilePath, string newName, bool overwrite = false)
		{
			try
			{
				base.Put(inputFilePath, newName, overwrite);
				return true;
			}
			catch(UnauthorizedAccessException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, $"Нет доступа к файлу или каталогу. Обратитесь в тех. поддержку. \n {ex.Message}", "Ошибка");
			}
			catch(PathTooLongException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, $"Путь содержит слишком много символов. Переместите файл в другой каталог и повторите попытку. \n {ex.Message}", "Ошибка");
			}
			catch(DirectoryNotFoundException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, $"Каталог не найден. \n {ex.Message}", "Ошибка");
			}
			catch(FileNotFoundException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, $"Файл не найден. \n {ex.Message}", "Ошибка");
			}
			catch(Exception ex)
			{
				if(ex is ArgumentException ||
					ex is ArgumentNullException ||
					ex is IOException ||
					ex is NotSupportedException)
				{
					_interactiveService.ShowMessage(ImportanceLevel.Error, $"Произошла ошибка при сохранении файла в хранилище: {ex.Message}. \n Обратитесь в тех. поддержку", "Ошибка");
					_errorReporter.SendErrorReport(new[] { ex });
					return false;
				}
				throw;
			}
			return false;
		}

		public bool TryTakeTo(string fileName, string outputFilePath, bool overwrite = false)
		{
			try
			{
				base.TakeTo(fileName, outputFilePath, overwrite);
				return true;
			}
			catch(UnauthorizedAccessException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, $"Нет доступа к файлу или каталогу. Обратитесь в тех. поддержку. \n {ex.Message}", "Ошибка");
			}
			catch(PathTooLongException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, $"Путь содержит слишком много символов. Обратитесь в тех. поддержку. \n {ex.Message}", "Ошибка");
			}
			catch(DirectoryNotFoundException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, $"Каталог не найден. Обратитесь в тех. поддержку. \n {ex.Message}", "Ошибка");
			}
			catch(FileNotFoundException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, $"Файл не найден. Обратитесь в тех. поддержку. \n {ex.Message}", "Ошибка");
			}
			catch(Exception ex)
			{
				if(ex is ArgumentException ||
					ex is ArgumentNullException ||
					ex is IOException ||
					ex is NotSupportedException)
				{
					_interactiveService.ShowMessage(ImportanceLevel.Error, $"Произошла ошибка при получении файла из хранилища: {ex.Message}. \n Обратитесь в тех. поддержку", "Ошибка");
					_errorReporter.SendErrorReport(new[] { ex });
					return false;
				}
				throw;
			}
			return false;
		}

		public bool TryDelete(string fileName)
		{
			try
			{
				base.Delete(fileName);
				return true;
			}
			catch(UnauthorizedAccessException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, $"Нет доступа к файлу или каталогу. Обратитесь в тех. поддержку \n {ex.Message}", "Ошибка");
			}
			catch(PathTooLongException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, $"Путь содержит слишком много символов. Обратитесь в тех. поддержку. \n {ex.Message}", "Ошибка");
			}
			catch(DirectoryNotFoundException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, $"Каталог не найден. Обратитесь в тех. поддержку. \n {ex.Message}", "Ошибка");
			}
			catch(FileNotFoundException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, $"Файл не найден. Обратитесь в тех. поддержку. \n {ex.Message}", "Ошибка");
			}
			catch(Exception ex)
			{
				if(ex is ArgumentException ||
					ex is ArgumentNullException ||
					ex is IOException ||
					ex is NotSupportedException)
				{
					_interactiveService.ShowMessage(ImportanceLevel.Error, $"Произошла ошибка при удалении файла из хранилища: {ex.Message}. \n Обратитесь в тех. поддержку", "Ошибка");
					_errorReporter.SendErrorReport(new[] { ex });
					return false;
				}
				throw;
			}
			return false;
		}

		public bool TryRefresh()
		{
			try
			{
				base.Refresh();
				return true;
			}
			catch(DirectoryNotFoundException)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, $"Каталог файлового хранилища не найден. Обратитесь в тех. поддержку.", "Ошибка");
			}
			return false;
		}
	}
}
