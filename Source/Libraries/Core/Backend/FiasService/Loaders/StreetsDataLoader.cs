using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fias.Search.DTO;
using Microsoft.Extensions.Logging;

namespace Fias.Client.Loaders
{
	internal class StreetsDataLoader : FiasDataLoader, IStreetsDataLoader
	{
		private readonly ILogger<StreetsDataLoader> _logger;

		protected Task<IEnumerable<StreetDTO>> CurrentLoadTask;

		protected StreetDTO[] Streets;

		public StreetsDataLoader(
			ILogger<StreetsDataLoader> logger,
			IFiasApiClient fiasApiClient)
			: base(fiasApiClient)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public event Action StreetLoaded;

		public int Delay { get; set; } = 1000;

		public void LoadStreets(Guid? cityId, string searchString, int limit = 50)
		{
			CancelLoading();

			CurrentLoadTask = Task.Run(() => GetStreetDTOs(cityId, searchString, limit, cancelTokenSource.Token));
			CurrentLoadTask.ContinueWith(SaveStreets, cancelTokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
			CurrentLoadTask.ContinueWith(arg => _logger.LogError(arg.Exception, "Ошибка при загрузке улиц"), TaskContinuationOptions.OnlyOnFaulted);
		}

		protected IEnumerable<StreetDTO> GetStreetDTOs(Guid? cityId, string searchString, int limit, CancellationToken token)
		{
			if(cityId == null)
			{
				return new List<StreetDTO>();
			}
			try
			{
				Task.Delay(Delay, token).Wait();
				_logger.LogInformation("Запрос улиц... Строка поиска : {SearchString} , Кол-во записей {Limit}", searchString, limit);
				return Fias.GetStreetsByCriteria((Guid) cityId, searchString, limit);
			}
			catch(AggregateException ae)
			{
				ae.Handle(ex =>
				{
					if(ex is TaskCanceledException)
					{
						_logger.LogInformation("Запрос улиц отменен");
					}

					return ex is TaskCanceledException;
				});
				return new List<StreetDTO>();
			}
		}

		protected void SaveStreets(Task<IEnumerable<StreetDTO>> newStreets)
		{
			Streets = newStreets.Result.ToArray();
			_logger.LogInformation("Улиц загружено : {StreetsCount}", Streets.Length);
			StreetLoaded?.Invoke();
		}

		protected override void CancelLoading()
		{
			if(CurrentLoadTask == null || !CurrentLoadTask.IsCompleted)
			{
				_logger.LogDebug("Отмена предыдущей загрузки улиц");
			}

			cancelTokenSource.Cancel();
			cancelTokenSource = new CancellationTokenSource();
		}

		public StreetDTO[] GetStreets()
		{
			return Streets?.Clone() as StreetDTO[];
		}
	}
}
