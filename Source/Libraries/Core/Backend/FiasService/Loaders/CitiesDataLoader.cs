using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fias.Search.DTO;
using NLog;

namespace Fias.Client.Loaders
{
	public interface ICitiesDataLoader
	{
		/// <summary>
		///     Delay before query was executed (in millisecond)
		/// </summary>
		int Delay { get; set; }

		event Action CitiesLoaded;

		CityDTO GetCity(string cityName);

		/// <summary>
		///     Initialize loading geodata from service
		/// </summary>
		void LoadCities(string searchString, int limit = 50);

		CityDTO[] GetCities();
	}

	public class CitiesDataLoader : FiasDataLoader, ICitiesDataLoader
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		private CityDTO[] Cities;

		protected Task<IEnumerable<CityDTO>> CurrentLoadTask;

		public CitiesDataLoader(IFiasApiClient fiasApiClient) : base(fiasApiClient)
		{
		}

		public event Action CitiesLoaded;

		public int Delay { get; set; } = 600;

		public CityDTO GetCity(string cityName)
		{
			var cities = Fias.GetCitiesByCriteria(cityName, 1);
			return cities.SingleOrDefault();
		}

		public void LoadCities(string searchString, int limit = 50)
		{
			CancelLoading();
			CurrentLoadTask = Task.Run(() => GetFiasCities(searchString, limit, cancelTokenSource.Token));
			CurrentLoadTask.ContinueWith(SaveCities, cancelTokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
			CurrentLoadTask.ContinueWith(arg => logger.Error(arg.Exception, "Ошибка при загрузке городов"), TaskContinuationOptions.OnlyOnFaulted);
		}

		public CityDTO[] GetCities()
		{
			return Cities?.Clone() as CityDTO[];
		}

		private IEnumerable<CityDTO> GetFiasCities(string searchString, int limit, CancellationToken token)
		{
			try
			{
				Task.Delay(Delay, token).Wait();
				logger.Info($"Запрос городов... Строка поиска : { searchString } , Кол-во записей { limit }");
				return Fias.GetCitiesByCriteria(searchString, limit);
			}
			catch(AggregateException ae)
			{
				ae.Handle(ex =>
				{
					if(ex is TaskCanceledException)
					{
						logger.Info("Запрос городов отменен");
					}

					return ex is TaskCanceledException;
				});

				return new List<CityDTO>();
			}
		}

		private void SaveCities(Task<IEnumerable<CityDTO>> newCities)
		{
			Cities = newCities.Result.ToArray();
			logger.Info($"Городов загружено : { Cities.Length }");
			CitiesLoaded?.Invoke();
		}

		protected override void CancelLoading()
		{
			if(CurrentLoadTask == null)
			{
				return;
			}

			if(!CurrentLoadTask.IsCompleted)
			{
				logger.Debug("Отмена предыдущей загрузки городов");
			}

			cancelTokenSource.Cancel();
			cancelTokenSource = new CancellationTokenSource();
		}
	}
}
