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

		public async Task<Result> CreateFileAsync(string bucketName, string name, string content, CancellationToken cancellationToken)
		{
			if(await FileExistsAsync(bucketName, name, cancellationToken))
			{
				return await Task.FromResult(Result.Failure(Application.Errors.S3.FileAlreadyExists));
			}

			return await PutFileAsync(bucketName, name, content, cancellationToken);
		}

		public async Task<Result> GetFileAsync(string bucketName, string name, CancellationToken cancellationToken)
		{
			var getObjectRequest = new GetObjectRequest();
			getObjectRequest.BucketName = bucketName;
			getObjectRequest.Key = name;
			GetObjectResponse response = await _amazonS3Client.GetObjectAsync(getObjectRequest, cancellationToken);

			await response.WriteResponseStreamToFileAsync(
				Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
					name),
				false,
				cancellationToken);

			return Result.Success();
		}

		public Task<Result> UpdateFileAsync(string bucketName, string name, string content, CancellationToken cancellationToken)
		{
			return PutFileAsync(bucketName, name, content, cancellationToken);
		}

		public async Task<Result> DeleteFileAsync(string bucketName, string name, CancellationToken cancellationToken)
		{
			var deleteObjectRequest = new DeleteObjectRequest();
			deleteObjectRequest.BucketName = bucketName;
			deleteObjectRequest.Key = name;
			await _amazonS3Client.DeleteObjectAsync(deleteObjectRequest, cancellationToken);

			return Result.Success();
		}

		private async Task<Result<IEnumerable<S3Object>>> GetAllObjectsInBucketAsync(string bucketName, CancellationToken cancellationToken)
		{
			var listContentrequest = new ListObjectsRequest();
			listContentrequest.BucketName = bucketName;
			ListObjectsResponse listContentresponse = await _amazonS3Client.ListObjectsAsync(listContentrequest);

			return await Task.FromResult(Result.Success(listContentresponse.S3Objects.AsEnumerable()));
		}

		private async Task<bool> FileExistsAsync(string bucketName, string name, CancellationToken cancellationToken)
		{
			var objects = await GetAllObjectsInBucketAsync(bucketName, cancellationToken);
			return await Task.FromResult(objects.Value.Any(o => o.Key == name));
		}

		private async Task<Result> PutFileAsync(string bucketName, string name, string content, CancellationToken cancellationToken)
		{
			var putObjectRequest = new PutObjectRequest();
			putObjectRequest.BucketName = bucketName;
			putObjectRequest.Key = name;
			putObjectRequest.ContentType = AmazonS3Util.MimeTypeFromExtension(name);
			putObjectRequest.ContentBody = content;
			await _amazonS3Client.PutObjectAsync(putObjectRequest, cancellationToken);

			return Result.Success();
		}
	}
}
