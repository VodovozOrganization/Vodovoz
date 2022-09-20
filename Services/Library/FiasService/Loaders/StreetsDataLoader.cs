using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fias.Search.DTO;
using NLog;

namespace Fias.Service.Loaders
{
	public interface IStreetsDataLoader
	{
		/// <summary>
		///     Delay before query was executed (in millisecond)
		/// </summary>
		int Delay { get; set; }

		event Action StreetLoaded;

		/// <summary>
		///     Initialize loading geodata from service
		/// </summary>
		void LoadStreets(Guid? cityId, string searchString, int limit = 50);

		StreetDTO[] GetStreets();
	}

	public class StreetsDataLoader : FiasDataLoader, IStreetsDataLoader
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		protected Task<IEnumerable<StreetDTO>> CurrentLoadTask;

		protected StreetDTO[] Streets;

		public StreetsDataLoader(IFiasApiClient fiasApiClient) : base(fiasApiClient)
		{
		}

		public event Action StreetLoaded;

		public int Delay { get; set; } = 1000;

		public void LoadStreets(Guid? cityId, string searchString, int limit = 50)
		{
			CancelLoading();

			CurrentLoadTask = Task.Run(() => GetStreetDTOs(cityId, searchString, limit, cancelTokenSource.Token));
			CurrentLoadTask.ContinueWith(SaveStreets, cancelTokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
			CurrentLoadTask.ContinueWith(arg => logger.Error(arg.Exception, "Ошибка при загрузке улиц"), TaskContinuationOptions.OnlyOnFaulted);
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
				logger.Info($"Запрос улиц... Строка поиска : {searchString} , Кол-во записей {limit}");
				return Fias.GetStreetsByCriteria((Guid) cityId, searchString, limit);
			}
			catch(AggregateException ae)
			{
				ae.Handle(ex =>
				{
					if(ex is TaskCanceledException)
					{
						logger.Info("Запрос улиц отменен");
					}

					return ex is TaskCanceledException;
				});
				return new List<StreetDTO>();
			}
		}

		protected void SaveStreets(Task<IEnumerable<StreetDTO>> newStreets)
		{
			Streets = newStreets.Result.ToArray();
			logger.Info($"Улиц загружено : { Streets.Length }");
			StreetLoaded?.Invoke();
		}

		protected override void CancelLoading()
		{
			if(CurrentLoadTask == null || !CurrentLoadTask.IsCompleted)
			{
				logger.Debug("Отмена предыдущей загрузки улиц");
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
