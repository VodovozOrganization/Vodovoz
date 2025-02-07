using DynamicData;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Application.FileStorage;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Cash;
using Vodovoz.Presentation.ViewModels.Administration;
using VodovozBusiness.Domain.Cash.CashRequest;
using Vodovoz.Core.Domain.Extensions;
using System.IO;

namespace Vodovoz.ViewModels.AdministrationTools
{
	public class CashlessRequestFileTransferViewModel : AdministrativeOperationViewModelBase
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IS3FileStorageService _s3FileStorageService;
		private readonly IInteractiveService _interactiveService;
		private bool _isRunning;
		private string _cashlessRequestPrefix;
		private string _cashlessRequestCommentPrefix;

		public CashlessRequestFileTransferViewModel(
			ILogger<AdministrativeOperationViewModelBase> logger,
			INavigationManager navigation,
			IUnitOfWorkFactory unitOfWorkFactory,
			IS3FileStorageService s3FileStorageService,
			ICashlessRequestFileStorageService cashlessRequestFileStorageService,
			ICashlessRequestCommentFileStorageService cashlessRequestCommentFileStorageService,
			IInteractiveService interactiveService,
			IGenericRepository<CashlessRequestComment> cashlessRequestCommentsRepository) : base(logger, navigation)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_s3FileStorageService = s3FileStorageService ?? throw new ArgumentNullException(nameof(s3FileStorageService));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			if(cashlessRequestFileStorageService is null)
			{
				throw new ArgumentNullException(nameof(cashlessRequestFileStorageService));
			}

			if(cashlessRequestCommentFileStorageService is null)
			{
				throw new ArgumentNullException(nameof(cashlessRequestCommentFileStorageService));
			}

			_cashlessRequestPrefix = cashlessRequestFileStorageService.BucketNameWithPrefix;
			_cashlessRequestCommentPrefix = cashlessRequestCommentFileStorageService.BucketNameWithPrefix;

			Title = "Перенос файлов запросов безналичных оплат";

			RunCommand = new DelegateCommand(RunHandler, () => !IsRunning);

			RunCommand.CanExecuteChangedWith(this, x => x.IsRunning);
		}

		public bool IsRunning
		{
			get => _isRunning;
			private set => SetField(ref _isRunning, value);
		}

		private void RunHandler()
		{
			IsRunning = true;

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Перенос файлов запросов безналичных оплат"))
			{
				LogInformation("Начало переноса файлов запросов безналичных оплат");

				var listOfFileInformations = unitOfWork.Session.Query<CashlessRequestFileInformation>()
					.OrderBy(x => x.CashlessReqwuestId)
					.ToList();

				LogInformation("Найдено {CashlessRequestFileInformationCount} файлов запросов безналичных оплат", listOfFileInformations.Count);

				var listToRemove = new List<CashlessRequestFileInformation>();
				var matchesAllreadyUploaded = new Dictionary<CashlessRequestFileInformation, CashlessRequestCommentFileInformation>();

				var lastCashlessRequestId = 0;

				var currentCashlessRequestComments = new List<CashlessRequestComment>();

				foreach(var fileInformation in listOfFileInformations)
				{
					var isExistsInS3 = _s3FileStorageService.FileExistsAsync(_cashlessRequestPrefix, fileInformation.CashlessReqwuestId + "/" + fileInformation.FileName, default)
						.GetAwaiter()
						.GetResult();

					if(isExistsInS3.IsSuccess && isExistsInS3.Value)
					{
						var isProcessedBefore = false;

						if(lastCashlessRequestId != fileInformation.CashlessReqwuestId)
						{
							lastCashlessRequestId = fileInformation.CashlessReqwuestId;

							currentCashlessRequestComments.Clear();

							currentCashlessRequestComments.AddRange(
								unitOfWork.Session
									.Query<CashlessRequestComment>()
									.Where(crc => crc.CashlessRequestId == lastCashlessRequestId));
						}

						if(currentCashlessRequestComments.Any())
						{
							isProcessedBefore = currentCashlessRequestComments.Any(x => x.AttachedFileInformations.Any(fi => fi.FileName == fileInformation.FileName));
						}

						if(isProcessedBefore)
						{
							matchesAllreadyUploaded.Add(
								fileInformation,
								currentCashlessRequestComments
									.FirstOrDefault(x => x.AttachedFileInformations.Any(fi => fi.FileName == fileInformation.FileName))
									.AttachedFileInformations.FirstOrDefault(fi => fi.FileName == fileInformation.FileName));

							listToRemove.Add(fileInformation);
							
							LogInformation("Файл {Filename} запроса на выдачу денежных средств {CashlessRequestId} успешно найден в S3 и уже был обработан ранее", fileInformation.FileName, fileInformation.CashlessReqwuestId);
							continue;
						}

						LogInformation("Файл {Filename} запроса на выдачу денежных средств {CashlessRequestId} успешно найден в S3", fileInformation.FileName, fileInformation.CashlessReqwuestId);
					}
					else if(isExistsInS3.IsSuccess && !isExistsInS3.Value)
					{
						LogWarning("Файл {Filename} запроса на выдачу денежных средств {CashlessRequestId} не найден в S3", fileInformation.FileName, fileInformation.CashlessReqwuestId);
						listToRemove.Add(fileInformation);
					}
					else
					{
						LogError("При запросе на наличие файла в S3 произошли ошибки: {ErrorsMessages}", string.Join(", ", isExistsInS3.Errors.Select(error => error.Message)));

						IsRunning = false;

						LogError("Подготовка прервана");
						return;
					}
				}

				lastCashlessRequestId = 0;

				listOfFileInformations.RemoveMany(listToRemove);
				listToRemove.Clear();

				LogInformation("Для переноса готово {CashlessRequestFileInformationCount} файлов запросов безналичных оплат", listOfFileInformations.Count);

				if(!_interactiveService.Question("Продолжаем?"))
				{
					IsRunning = false;

					LogError("Подготовка прервана");
					return;
				}

				// FileInformations Creation

				CashlessRequest currentCashlessRequest;

				var matchesToUploaded = new Dictionary<CashlessRequestFileInformation, CashlessRequestCommentFileInformation>();

				foreach(var fileInformation in listOfFileInformations)
				{
					currentCashlessRequest = unitOfWork.Session.Query<CashlessRequest>().FirstOrDefault(cd => cd.Id == fileInformation.CashlessReqwuestId);

					CashlessRequestComment comment = new CashlessRequestComment
					{
						CreatedAt = currentCashlessRequest.Date,
						AuthorId = currentCashlessRequest.Author.Id,
						Text = "Файл"
					};

					var newFileInformation = new CashlessRequestCommentFileInformation
					{
						FileName = fileInformation.FileName,
					};

					matchesToUploaded.Add(fileInformation, newFileInformation);

					comment.AttachedFileInformations.Add(newFileInformation);

					currentCashlessRequest.AddComment(comment);

					unitOfWork.Save(currentCashlessRequest);
				}

				// Uploading checked

				var filesFailedToUpload = new List<CashlessRequestCommentFileInformation>();

				foreach(var matchToUpload in matchesToUploaded)
				{
					var fileToReUpload = _s3FileStorageService
						.GetFileAsync(
							_cashlessRequestPrefix,
							matchToUpload.Key.CashlessReqwuestId + "/" + matchToUpload.Key.FileName,
							default)
						.GetAwaiter()
						.GetResult();

					if(fileToReUpload.IsFailure)
					{
						LogError("Не удалось загрузить файл {Error}", string.Join(", ", fileToReUpload.Errors.Select(e => e.Message)));
						filesFailedToUpload.Add(matchToUpload.Value);
						continue;
					}

					using(var reuploadStream = new MemoryStream())
					{
						fileToReUpload.Value.CopyTo(reuploadStream);

						var result = _s3FileStorageService
							.CreateFileAsync(
								_cashlessRequestCommentPrefix,
								matchToUpload.Value.CashlessRequestCommentId + "/" + matchToUpload.Value.FileName,
								reuploadStream,
								default)
							.GetAwaiter()
							.GetResult();

						if(result.IsFailure)
						{
							LogError("Не удалось загрузить файл {Filename} по причине: {Message}", matchToUpload.Value.FileName, string.Join(", ", result.Errors.Select(x => x.Message)));
						}

						fileToReUpload.Value.Dispose();
					}
				}

				if(filesFailedToUpload.Count > 0)
				{
					LogWarning("Не удалось загрузить {FailedToUploadCount} файлов из {TotalToUploadCount}", filesFailedToUpload.Count, matchesToUploaded.Count);

					foreach(var itemToRemove in filesFailedToUpload)
					{
						matchesToUploaded.Remove(matchesToUploaded.First(x => x.Value == itemToRemove).Key);
					}
				}
				else
				{
					LogInformation("Все файлы были загружены успешно");
				}

				// Check checksum for uploaded

				foreach(var pairToCheck in matchesToUploaded)
				{
					var sourceFileToCheck = _s3FileStorageService
						.GetFileAsync(
							_cashlessRequestPrefix,
							pairToCheck.Key.CashlessReqwuestId + "/" + pairToCheck.Key.FileName,
							default)
						.GetAwaiter()
						.GetResult();

					var targetFileToCheck = _s3FileStorageService
						.GetFileAsync(
							_cashlessRequestCommentPrefix,
							pairToCheck.Value.CashlessRequestCommentId + "/" + pairToCheck.Value.FileName,
							default)
						.GetAwaiter()
						.GetResult();

					if(sourceFileToCheck.IsFailure || targetFileToCheck.IsFailure)
					{
						LogError("Проверка файла {FileName} не удалась", pairToCheck.Key.FileName);

						continue;
					}

					var md5Matches = sourceFileToCheck.Value.GetMd5Hash() == targetFileToCheck.Value.GetMd5Hash();
					var sha256Matches = sourceFileToCheck.Value.GetSha256Hash() == targetFileToCheck.Value.GetSha256Hash();

					if(md5Matches && sha256Matches)
					{
						LogInformation("Файл {Filename} успешно проверен", pairToCheck.Key.FileName);
					}
				}

				// Check checksum for uploaded earlier

				var needToReupload = new Dictionary<CashlessRequestFileInformation, CashlessRequestCommentFileInformation>();

				foreach(var pairToCheck in matchesAllreadyUploaded)
				{
					var sourceFileToCheck = _s3FileStorageService
						.GetFileAsync(
							_cashlessRequestPrefix,
							pairToCheck.Key.CashlessReqwuestId + "/" + pairToCheck.Key.FileName,
							default)
						.GetAwaiter()
						.GetResult();

					var targetFileToCheck = _s3FileStorageService
						.GetFileAsync(
							_cashlessRequestCommentPrefix,
							pairToCheck.Value.CashlessRequestCommentId + "/" + pairToCheck.Value.FileName,
							default)
						.GetAwaiter()
						.GetResult();

					if(sourceFileToCheck.IsFailure || targetFileToCheck.IsFailure)
					{
						LogError("Проверка файла {FileName} не удалась", pairToCheck.Key.FileName);

						if(targetFileToCheck.IsFailure)
						{
							needToReupload.Add(pairToCheck.Key, pairToCheck.Value);
						}

						continue;
					}

					var md5Matches = sourceFileToCheck.Value.GetMd5Hash() == targetFileToCheck.Value.GetMd5Hash();
					var sha256Matches = sourceFileToCheck.Value.GetSha256Hash() == targetFileToCheck.Value.GetSha256Hash();

					if(md5Matches && sha256Matches)
					{
						LogInformation("Файл {Filename} успешно проверен", pairToCheck.Key.FileName);
					}
				}

				// TODO: Upload missing

				var filesFailedToReUpload = new List<CashlessRequestCommentFileInformation>();

				foreach(var matchToUpload in needToReupload)
				{
					var fileToReUpload = _s3FileStorageService
						.GetFileAsync(
							_cashlessRequestPrefix,
							matchToUpload.Key.CashlessReqwuestId + "/" + matchToUpload.Key.FileName,
							default)
						.GetAwaiter()
						.GetResult();

					using(var reuploadStream = new MemoryStream())
					{
						fileToReUpload.Value.CopyTo(reuploadStream);

						if(fileToReUpload.IsFailure)
						{
							LogError("Не удалось загрузить файл {Error}", string.Join(", ", fileToReUpload.Errors.Select(e => e.Message)));
							filesFailedToUpload.Add(matchToUpload.Value);
							continue;
						}

						var result = _s3FileStorageService
							.CreateFileAsync(
								_cashlessRequestCommentPrefix,
								matchToUpload.Value.CashlessRequestCommentId + "/" + matchToUpload.Value.FileName,
								reuploadStream,
								default)
							.GetAwaiter()
							.GetResult();

						if(result.IsFailure)
						{
							LogError("Не удалось загрузить файл {Filename} по причине: {Message}", matchToUpload.Value.FileName, string.Join(", ", result.Errors.Select(x => x.Message)));
						}

						fileToReUpload.Value.Dispose();
					}
				}

				if(filesFailedToUpload.Count > 0)
				{
					LogWarning("Не удалось загрузить {FailedToUploadCount} файлов из {TotalToUploadCount}", filesFailedToUpload.Count, matchesToUploaded.Count);

					foreach(var itemToRemove in filesFailedToUpload)
					{
						matchesToUploaded.Remove(matchesToUploaded.First(x => x.Value == itemToRemove).Key);
					}
				}
				else
				{
					LogInformation("Все файлы были загружены успешно");
				}

				// Check checksumm for processed before if requested

				foreach(var pairToCheck in needToReupload)
				{
					var sourceFileToCheck = _s3FileStorageService
						.GetFileAsync(
							_cashlessRequestPrefix,
							pairToCheck.Key.CashlessReqwuestId + "/" + pairToCheck.Key.FileName,
							default)
						.GetAwaiter()
						.GetResult();

					var targetFileToCheck = _s3FileStorageService
						.GetFileAsync(
							_cashlessRequestCommentPrefix,
							pairToCheck.Value.CashlessRequestCommentId + "/" + pairToCheck.Value.FileName,
							default)
						.GetAwaiter()
						.GetResult();

					if(sourceFileToCheck.IsFailure || targetFileToCheck.IsFailure)
					{
						LogError("Проверка файла {FileName} не удалась", pairToCheck.Key.FileName);

						continue;
					}

					var md5Matches = sourceFileToCheck.Value.GetMd5Hash() == targetFileToCheck.Value.GetMd5Hash();
					var sha256Matches = sourceFileToCheck.Value.GetSha256Hash() == targetFileToCheck.Value.GetSha256Hash();

					if(md5Matches && sha256Matches)
					{
						LogInformation("Файл {Filename} успешно проверен", pairToCheck.Key.FileName);
					}
				}

				unitOfWork.Commit();
			}

			IsRunning = false;
		}
	}
}
