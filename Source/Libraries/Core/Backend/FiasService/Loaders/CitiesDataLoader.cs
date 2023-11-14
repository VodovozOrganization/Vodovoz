using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fias.Search.DTO;
using Microsoft.Extensions.Logging;

namespace Fias.Client.Loaders
{
	internal class CitiesDataLoader : FiasDataLoader, ICitiesDataLoader
	{
		private readonly ILogger<CitiesDataLoader> _logger;
		
		private CityDTO[] _cities;

		protected Task<IEnumerable<CityDTO>> CurrentLoadTask;

		public CitiesDataLoader(
			ILogger<CitiesDataLoader> logger,
			IFiasApiClient fiasApiClient)
			: base(fiasApiClient)
		{
			_logger = logger;
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
			CurrentLoadTask.ContinueWith(arg => _logger.LogError(arg.Exception, "Ошибка при загрузке городов"), TaskContinuationOptions.OnlyOnFaulted);
		}

		public CityDTO[] GetCities()
		{
			return _cities?.Clone() as CityDTO[];
		}

		private IEnumerable<CityDTO> GetFiasCities(string searchString, int limit, CancellationToken token)
		{
			try
			{
				Task.Delay(Delay, token).Wait();
				_logger.LogInformation("Запрос городов... Строка поиска : {SearchString} , Кол-во записей {Limit}", searchString, limit);

				return Fias.GetCitiesByCriteria(searchString, limit);
			}
			catch(AggregateException ae)
			{
				ae.Handle(ex =>
				{
					if(ex is TaskCanceledException)
					{
						_logger.LogInformation("Запрос городов отменен");
					}

					return ex is TaskCanceledException;
				});

				return new List<CityDTO>();
			}
		}

		private void SaveCities(Task<IEnumerable<CityDTO>> newCities)
		{
			_cities = newCities.Result.ToArray();
			_logger.LogInformation($"Городов загружено : { _cities.Length }");
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
				_logger.LogDebug("Отмена предыдущей загрузки городов");
			}

			cancelTokenSource.Cancel();
			cancelTokenSource = new CancellationTokenSource();
		}
	}
}
