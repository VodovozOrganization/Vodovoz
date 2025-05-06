using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Application.Errors
{
	public static class FileStorage
	{
		public static Error AttachedFileMatchesPhotoFileName
			=> new Error(
				typeof(FileStorage),
				nameof(AttachedFileMatchesPhotoFileName),
				"Прикрепленный файл имеет тоже имя, что и имя файла фотографии");

		public static Error PhotoMatchesAttachedFileFileName
			=> new Error(
				typeof(FileStorage),
				nameof(AttachedFileMatchesPhotoFileName),
				"Файл фотографии имеет тоже имя, что и имя одного из прикрепленных файлов");
	}
}
