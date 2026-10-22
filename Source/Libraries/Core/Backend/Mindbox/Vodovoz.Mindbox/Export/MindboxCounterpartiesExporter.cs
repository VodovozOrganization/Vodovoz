using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Mindbox.Services;
using Vodovoz.Settings.Mindbox;

namespace Vodovoz.Mindbox.Export
{
	/// <summary>
	/// Выгружает контрагентов в Mindbox через операцию массового импорта
	/// </summary>
	public interface IMindboxCounterpartiesExporter
	{
		/// <summary>
		/// Выполняет выгрузку
		/// </summary>
		Task<MindboxExportResult> ExportAsync(MindboxExportOptions options, CancellationToken cancellationToken);
	}

	/// <inheritdoc />
	public sealed class MindboxCounterpartiesExporter : IMindboxCounterpartiesExporter
	{
		private static readonly UTF8Encoding _utf8WithoutBom = new UTF8Encoding(false);

		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IMindboxClient _mindboxClient;
		private readonly IMindboxCustomerCsvMapper _csvMapper;
		private readonly IMindboxSettings _settings;
		private readonly ILogger<MindboxCounterpartiesExporter> _logger;

		public MindboxCounterpartiesExporter(
			IUnitOfWorkFactory unitOfWorkFactory,
			IMindboxClient mindboxClient,
			IMindboxCustomerCsvMapper csvMapper,
			IMindboxSettings settings,
			ILogger<MindboxCounterpartiesExporter> logger)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_mindboxClient = mindboxClient ?? throw new ArgumentNullException(nameof(mindboxClient));
			_csvMapper = csvMapper ?? throw new ArgumentNullException(nameof(csvMapper));
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public async Task<MindboxExportResult> ExportAsync(
			MindboxExportOptions options,
			CancellationToken cancellationToken)
		{
			if(options == null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			var result = new MindboxExportResult();
			var batchSize = Math.Max(1, options.BatchSize ?? _settings.ImportBatchSize);

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Выгрузка контрагентов в Mindbox"))
			{
				var total = unitOfWork.Session.Query<Counterparty>()
					.Count(counterparty => counterparty.CreateDate >= options.FromDate);

				result.Total = total;

				_logger.LogInformation(
					"Найдено контрагентов с датой создания от {FromDate:yyyy-MM-dd}: {Total}",
					options.FromDate,
					total);

				if(total == 0)
				{
					return result;
				}

				if(!string.IsNullOrWhiteSpace(options.OutputFile))
				{
					WriteToFile(unitOfWork, options, result);
					return result;
				}

				var batchesCount = (int)Math.Ceiling((double)total / batchSize);

				for(var batchNumber = 0; batchNumber < batchesCount; batchNumber++)
				{
					if(cancellationToken.IsCancellationRequested)
					{
						result.Errors.Add("Выгрузка прервана по запросу отмены");
						break;
					}

					var counterparties = unitOfWork.Session.Query<Counterparty>()
						.Where(counterparty => counterparty.CreateDate >= options.FromDate)
						.OrderBy(counterparty => counterparty.Id)
						.Skip(batchNumber * batchSize)
						.Take(batchSize)
						.ToList();

					if(counterparties.Count == 0)
					{
						continue;
					}

					result.Batches++;

					var csv = BuildCsv(counterparties);
					var maxCounterpartyId = counterparties.Max(counterparty => counterparty.Id);
					var transactionId = CreateTransactionId(options.FromDate, batchNumber, maxCounterpartyId);

					if(options.DryRun)
					{
						var path = WriteBatchToFile(csv, options.FromDate, batchNumber);
						result.Sent += counterparties.Count;

						_logger.LogInformation(
							"Пакет {Batch} сохранен в файл {Path} (dry-run), контрагентов: {Count}",
							batchNumber + 1,
							path,
							counterparties.Count);

						continue;
					}

					var operationResult = await _mindboxClient.ImportCustomersAsync(
						csv,
						transactionId,
						cancellationToken);

					if(operationResult.IsSuccess)
					{
						result.Sent += counterparties.Count;

						_logger.LogInformation(
							"Пакет {Batch} отправлен в Mindbox, контрагентов: {Count}, transactionId: {TransactionId}",
							batchNumber + 1,
							counterparties.Count,
							transactionId);
					}
					else
					{
						var error = $"Пакет {batchNumber + 1}: {operationResult.ErrorMessage}";
						result.Errors.Add(error);

						_logger.LogError(
							"Ошибка отправки пакета {Batch} в Mindbox: {Error}",
							batchNumber + 1,
							operationResult.ErrorMessage);
					}
				}
			}

			return result;
		}

		private void WriteToFile(
			IUnitOfWork unitOfWork,
			MindboxExportOptions options,
			MindboxExportResult result)
		{
			var directory = Path.GetDirectoryName(options.OutputFile);

			if(!string.IsNullOrWhiteSpace(directory))
			{
				Directory.CreateDirectory(directory);
			}

			var batchSize = Math.Max(1, options.BatchSize ?? _settings.ImportBatchSize);
			var written = 0;

			using(var writer = new StreamWriter(options.OutputFile, false, _utf8WithoutBom))
			{
				writer.Write(_csvMapper.Header);
				writer.Write('\n');

				while(written < result.Total)
				{
					var counterparties = unitOfWork.Session.Query<Counterparty>()
						.Where(counterparty => counterparty.CreateDate >= options.FromDate)
						.OrderBy(counterparty => counterparty.Id)
						.Skip(written)
						.Take(batchSize)
						.ToList();

					if(counterparties.Count == 0)
					{
						break;
					}

					foreach(var counterparty in counterparties)
					{
						writer.Write(_csvMapper.Map(counterparty));
						writer.Write('\n');
					}

					written += counterparties.Count;
				}
			}

			result.Sent = written;
			result.Batches = (int)Math.Ceiling((double)written / batchSize);

			_logger.LogInformation(
				"CSV сохранен в файл {Path}, контрагентов: {Count}",
				options.OutputFile,
				written);
		}

		private string WriteBatchToFile(byte[] csv, DateTime fromDate, int batchNumber)
		{
			var directory = Path.Combine(
				Directory.GetCurrentDirectory(),
				"mindbox-export",
				fromDate.ToString("yyyyMMdd"));

			Directory.CreateDirectory(directory);

			var path = Path.Combine(directory, $"batch-{batchNumber + 1:D4}.csv");

			File.WriteAllBytes(path, csv);

			return path;
		}

		private byte[] BuildCsv(IList<Counterparty> counterparties)
		{
			var builder = new StringBuilder();

			builder.Append(_csvMapper.Header);
			builder.Append('\n');

			foreach(var counterparty in counterparties)
			{
				builder.Append(_csvMapper.Map(counterparty));
				builder.Append('\n');
			}

			return _utf8WithoutBom.GetBytes(builder.ToString());
		}

		private static Guid CreateTransactionId(DateTime fromDate, int batchNumber, int maxCounterpartyId)
		{
			var source = $"vodovoz-mindbox-export:{fromDate:yyyyMMdd}:{batchNumber}:{maxCounterpartyId}";

			using(var md5 = MD5.Create())
			{
				return new Guid(md5.ComputeHash(Encoding.UTF8.GetBytes(source)));
			}
		}
	}
}
