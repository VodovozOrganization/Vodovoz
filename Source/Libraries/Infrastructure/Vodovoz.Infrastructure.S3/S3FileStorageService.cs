using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Application.FileStorage;
using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Infrastructure.S3
{
	internal sealed class S3FileStorageService : IS3FileStorageService
	{
		private readonly ILogger<S3FileStorageService> _logger;
		private readonly AmazonS3Client _amazonS3Client;

		public S3FileStorageService(
			ILogger<S3FileStorageService> logger,
			AmazonS3Client amazonS3Client)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_amazonS3Client = amazonS3Client
				?? throw new ArgumentNullException(nameof(amazonS3Client));
		}

		public async Task<Result> CreateFileAsync(
			string bucketName,
			string fileName,
			Stream inputStream,
			CancellationToken cancellationToken)
		{
			try
			{
				var fileExistsResult = await FileExistsAsync(bucketName, fileName, cancellationToken);

				if(fileExistsResult.IsFailure)
				{
					return Result.Failure(fileExistsResult.Errors);
				}

				if(fileExistsResult.Value)
				{
					_logger.LogWarning(
						"Попытка создания уже существующего файла {Filename} в бакете {BucketName}",
						fileName,
						bucketName);

					return await Task.FromResult(Result.Failure(Application.Errors.S3.FileAlreadyExists));
				}

				return await PutFileAsync(bucketName, fileName, inputStream, cancellationToken);
			}
			catch(AmazonServiceException e)
			{
				HandleCommonExceptions(e, "создании файла в S3 хранилище");

				return Result.Failure(Application.Errors.S3.ServiceUnavailable);
			}
			catch(OperationCanceledException e)
			{
				_logger.LogError(e, "Операция создания файла в S3 была отменена (таймаут или отмена токена)");
				return Result.Failure(Application.Errors.S3.OperationCanceled);
			}
		}

		//[Appellative(Prepositional = "получении файла из S3 хранилища")]
		public async Task<Result<Stream>> GetFileAsync(
			string bucketName,
			string fileName,
			CancellationToken cancellationToken)
		{
			try
			{
				var getObjectRequest = new GetObjectRequest
				{
					BucketName = bucketName,
					Key = fileName
				};

				GetObjectResponse response = await _amazonS3Client.GetObjectAsync(getObjectRequest, cancellationToken);

				return Result.Success(response.ResponseStream);
			}
			catch(AmazonServiceException e)
			{
				HandleCommonExceptions(e, "получении файла из S3 хранилища");

				return Result.Failure<Stream>(Application.Errors.S3.ServiceUnavailable);
			}
			catch(OperationCanceledException e)
			{
				_logger.LogError(e, "Операция получения файла из S3 была отменена (таймаут или отмена токена)");
				return Result.Failure<Stream>(Application.Errors.S3.OperationCanceled);
			}
		}

		public async Task<Result> UpdateFileAsync(
			string bucketName,
			string fileName,
			Stream inputStream,
			CancellationToken cancellationToken)
		{
			try
			{
				var fileExistsResult = await FileExistsAsync(bucketName, fileName, cancellationToken);

				if(fileExistsResult.IsFailure)
				{
					return Result.Failure(fileExistsResult.Errors);
				}

				if(!fileExistsResult.Value)
				{
					_logger.LogWarning(
						"Попытка обновления не существующего файла {Filename} в бакете {BucketName}",
						fileName,
						bucketName);

					return await Task.FromResult(Result.Failure(Application.Errors.S3.FileNotExists));
				}

				return await PutFileAsync(bucketName, fileName, inputStream, cancellationToken);
			}
			catch(AmazonServiceException e)
			{
				HandleCommonExceptions(e, "обновлении файла в S3 хранилище");

				return Result.Failure(Application.Errors.S3.ServiceUnavailable);
			}
			catch(OperationCanceledException e)
			{
				_logger.LogError(e, "Операция обновления файла из S3 была отменена (таймаут или отмена токена)");
				return Result.Failure(Application.Errors.S3.OperationCanceled);
			}
		}

		public async Task<Result> DeleteFileAsync(
			string bucketName,
			string fileName,
			CancellationToken cancellationToken)
		{
			try
			{
				var deleteObjectRequest = new DeleteObjectRequest
				{
					BucketName = bucketName,
					Key = fileName
				};

				await _amazonS3Client.DeleteObjectAsync(deleteObjectRequest, cancellationToken);

				return Result.Success();
			}
			catch(AmazonServiceException e)
			{
				HandleCommonExceptions(e, "удалении файла из S3 хранилища");

				return Result.Failure(Application.Errors.S3.ServiceUnavailable);
			}
			catch(OperationCanceledException e)
			{
				_logger.LogError(e, "Операция удаления файла из S3 была отменена (таймаут или отмена токена)");
				return Result.Failure(Application.Errors.S3.OperationCanceled);
			}
		}

		public async Task<Result<IEnumerable<string>>> GetAllObjectsFileNamesInBucketAsync(
			string bucketName,
			CancellationToken cancellationToken)
		{
			try
			{
				var result = await GetAllObjectsInBucketAsync(bucketName, cancellationToken);

				if(result.IsSuccess)
				{
					return await Task.FromResult(Result.Success(result.Value.Select(s3o => s3o.Key)));
				}
				else
				{
					return Result.Failure<IEnumerable<string>>(result.Errors);
				}
			}
			catch(AmazonServiceException e)
			{
				HandleCommonExceptions(e, "получении списка файлов из S3 хранилища");

				return Result.Failure<IEnumerable<string>>(Application.Errors.S3.ServiceUnavailable);
			}
			catch(OperationCanceledException e)
			{
				_logger.LogError(e, "Операция получения списка файлов из S3 была отменена (таймаут или отмена токена)");
				return Result.Failure<IEnumerable<string>>(Application.Errors.S3.OperationCanceled);
			}
		}

		public async Task<Result<bool>> FileExistsAsync(
			string bucketName,
			string fileName,
			CancellationToken cancellationToken)
		{
			try
			{
				var objects = await GetAllObjectsInBucketAsync(bucketName, cancellationToken);

				return await Task.FromResult(objects.Value.Any(o => o.Key == fileName));
			}
			catch(AmazonServiceException e)
			{
				HandleCommonExceptions(e, "проверке наличия файла в S3 хранилище");

				return Result.Failure<bool>(Application.Errors.S3.ServiceUnavailable);
			}
			catch(OperationCanceledException e)
			{
				_logger.LogError(e, "Операция проверки наличия файла из S3 была отменена (таймаут или отмена токена)");
				return Result.Failure<bool>(Application.Errors.S3.OperationCanceled);
			}
		}

		private async Task<Result<IEnumerable<S3Object>>> GetAllObjectsInBucketAsync(
			string bucketName,
			CancellationToken cancellationToken)
		{
			var listContentrequest = new ListObjectsRequest
			{
				BucketName = bucketName
			};

			ListObjectsResponse listContentresponse = await _amazonS3Client.ListObjectsAsync(listContentrequest, cancellationToken);

			return await Task.FromResult(Result.Success(listContentresponse.S3Objects.AsEnumerable()));
		}

		/// <summary>
		/// Общий метод для загрузки файла в S3 хранилище
		/// </summary>
		/// <param name="bucketName"></param>
		/// <param name="fileName"></param>
		/// <param name="inputStream"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		private async Task<Result> PutFileAsync(
			string bucketName,
			string fileName,
			Stream inputStream,
			CancellationToken cancellationToken)
		{
			var putObjectRequest = new PutObjectRequest
			{
				BucketName = bucketName,
				Key = fileName,
				ContentType = AmazonS3Util.MimeTypeFromExtension(fileName),
				InputStream = inputStream
			};

			await _amazonS3Client.PutObjectAsync(putObjectRequest, cancellationToken);

			return Result.Success();
		}

		/// <summary>
		/// Обработка низкоуровневых ошибок при взаимодействии с S3 хранилищем
		/// </summary>
		/// <param name="e"></param>
		/// <param name="actionNamePrepositional">название действия в предложном падеже (о ком? о чем?)</param>
		private void HandleCommonExceptions(
			AmazonServiceException e,
			string actionNamePrepositional)
		{
			var exceptionMessage = e.Message;

			if(e.InnerException is System.Net.WebException we
				&& we.InnerException is System.Net.Sockets.SocketException se)
			{
				exceptionMessage = se.Message;
			}

			_logger.LogError(e, "Ошибка при {ActionNamePropositional}: {ExceptionMessage}", actionNamePrepositional, exceptionMessage);
		}
	}
}
