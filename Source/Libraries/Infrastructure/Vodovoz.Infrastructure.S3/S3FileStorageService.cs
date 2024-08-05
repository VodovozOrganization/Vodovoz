using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using MassTransit.Initializers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Application.FileStorage;
using Vodovoz.Errors;

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

		public async Task<Result> CreateFileAsync(string bucketName, string fileName, Stream inputStream, CancellationToken cancellationToken)
		{
			if(await FileExistsAsync(bucketName, fileName, cancellationToken))
			{
				_logger.LogWarning("Попытка создания уже существующего файла {Filename} в бакете {BucketName}", fileName, bucketName);

				return await Task.FromResult(Result.Failure(Application.Errors.S3.FileAlreadyExists));
			}

			return await PutFileAsync(bucketName, fileName, inputStream, cancellationToken);
		}

		public async Task<Result<Stream>> GetFileAsync(string bucketName, string fileName, CancellationToken cancellationToken)
		{
			var getObjectRequest = new GetObjectRequest
			{
				BucketName = bucketName,
				Key = fileName
			};

			GetObjectResponse response = await _amazonS3Client.GetObjectAsync(getObjectRequest, cancellationToken);

			return Result.Success(response.ResponseStream);
		}

		public async Task<Result> UpdateFileAsync(string bucketName, string fileName, Stream inputStream, CancellationToken cancellationToken)
		{
			if(!await FileExistsAsync(bucketName, fileName, cancellationToken))
			{
				_logger.LogWarning("Попытка обновления не существующего файла {Filename} в бакете {BucketName}", fileName, bucketName);

				return await Task.FromResult(Result.Failure(Application.Errors.S3.FileNotExists));
			}

			return await PutFileAsync(bucketName, fileName, inputStream, cancellationToken);
		}

		public async Task<Result> DeleteFileAsync(string bucketName, string fileName, CancellationToken cancellationToken)
		{
			var deleteObjectRequest = new DeleteObjectRequest
			{
				BucketName = bucketName,
				Key = fileName
			};

			await _amazonS3Client.DeleteObjectAsync(deleteObjectRequest, cancellationToken);

			return Result.Success();
		}

		public async Task<Result<IEnumerable<string>>> GetAllObjectsFileNamesInBucketAsync(string bucketName, CancellationToken cancellationToken)
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

		public async Task<bool> FileExistsAsync(string bucketName, string fileName, CancellationToken cancellationToken)
		{
			var objects = await GetAllObjectsInBucketAsync(bucketName, cancellationToken);

			return await Task.FromResult(objects.Value.Any(o => o.Key == fileName));
		}

		private async Task<Result<IEnumerable<S3Object>>> GetAllObjectsInBucketAsync(string bucketName, CancellationToken cancellationToken)
		{
			var listContentrequest = new ListObjectsRequest
			{
				BucketName = bucketName
			};

			ListObjectsResponse listContentresponse = await _amazonS3Client.ListObjectsAsync(listContentrequest);

			return await Task.FromResult(Result.Success(listContentresponse.S3Objects.AsEnumerable()));
		}

		private async Task<Result> PutFileAsync(string bucketName, string fileName, Stream inputStream, CancellationToken cancellationToken)
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
	}
}
