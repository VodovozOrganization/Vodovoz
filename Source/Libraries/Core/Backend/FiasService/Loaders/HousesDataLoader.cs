using Fias.Search.DTO;
using Microsoft.Extensions.Logging;
using QS.Utilities.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fias.Client.Loaders
{
	internal class HousesDataLoader : FiasDataLoader, IHousesDataLoader
	{
		private readonly ILogger<HousesDataLoader> _logger;
		
		protected Task<IEnumerable<HouseDTO>> CurrentLoadTask;

		protected HouseDTO[] Houses;

		public HousesDataLoader(ILogger<HousesDataLoader> logger, IFiasApiClient fiasApiClient) : base(fiasApiClient)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public event Action HousesLoaded;

		public void LoadHouses(string searchString = null, Guid? streetGuid = null, Guid? cityGuid = null)
		{
			CancelLoading();

			_logger.LogInformation("Запрос домов...");

			if(streetGuid != null)
			{
				CurrentLoadTask = Task.Run(() => Fias.GetHousesFromStreetByCriteria((Guid)streetGuid, searchString), cancelTokenSource.Token);
			}
			else if(cityGuid != null)
			{
				CurrentLoadTask = Task.Run(() => Fias.GetHousesFromCityByCriteria((Guid)cityGuid, searchString), cancelTokenSource.Token);
			}
			else
			{
				return;
			}

			CurrentLoadTask.ContinueWith(SaveHouses, cancelTokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);

			CurrentLoadTask.ContinueWith(arg => _logger.LogError(arg.Exception, "Ошибка при загрузке домов"), TaskContinuationOptions.OnlyOnFaulted);
		}

		public HouseDTO[] GetHouses()
		{
			return Houses?.Clone() as HouseDTO[];
		}

		private void SaveHouses(Task<IEnumerable<HouseDTO>> newHouses)
		{
			var houses = newHouses.Result;

			Houses = houses.OrderBy(x => x.ComplexNumber, new NaturalStringComparer()).ToArray();
			_logger.LogInformation("Домов загружено : {HousesCount}", Houses.Length);
			HousesLoaded?.Invoke();
		}

		protected override void CancelLoading()
		{
			if(CurrentLoadTask == null || !CurrentLoadTask.IsCompleted)
			{
				_logger.LogDebug("Отмена предыдущей загрузки домов");
			}

			cancelTokenSource.Cancel();
			cancelTokenSource = new CancellationTokenSource();
		}

		public async Task<PointDTO> GetCoordinatesByGeocoderAsync(string address, CancellationToken cancellationToken)
		{
			return await Fias.GetCoordinatesByGeoCoder(address, cancellationToken);
		}
	}
}
