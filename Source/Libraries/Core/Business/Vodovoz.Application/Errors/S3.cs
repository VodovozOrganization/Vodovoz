using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Application.Errors
{
	public static class S3
	{
		public static Error FileAlreadyExists =>
			new Error(
				typeof(S3),
				nameof(FileAlreadyExists),
				"Файл уже существует в хранилище");

		public static Error FileNotExists =>
			new Error(
				typeof(S3),
				nameof(FileNotExists),
				"Файла не существует в хранилище");

		public static Error ServiceUnavailable =>
			new Error(
				typeof(S3),
				nameof(ServiceUnavailable),
				"Сервис S3 недосутпен");

		public static Error OperationCanceled =>
			new Error(
				typeof(S3),
				nameof(OperationCanceled),
				"Операция была отменена (таймаут или отмена токена)");
	}
}
