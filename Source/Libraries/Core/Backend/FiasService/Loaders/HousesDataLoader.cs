using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fias.Search.DTO;
using NLog;
using QS.Utilities.Text;

namespace Fias.Client.Loaders
{
	public interface IHousesDataLoader
	{
		event Action HousesLoaded;

		/// <summary>
		///     Initialize loading geodata from service
		/// </summary>
		void LoadHouses(string searchString = null, Guid? streetGuid = null, Guid? cityGuid = null);

		HouseDTO[] GetHouses();

		Task<PointDTO> GetCoordinatesByGeocoderAsync(string address, CancellationToken cancellationToken);
	}

	public class HousesDataLoader : FiasDataLoader, IHousesDataLoader
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		protected Task<IEnumerable<HouseDTO>> CurrentLoadTask;

		protected HouseDTO[] Houses;

		public HousesDataLoader(IFiasApiClient fiasApiClient) : base(fiasApiClient)
		{
		}

		public event Action HousesLoaded;

		public void LoadHouses(string searchString = null, Guid? streetGuid = null, Guid? cityGuid = null)
		{
			CancelLoading();

			_logger.Info("Запрос домов...");

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

			CurrentLoadTask.ContinueWith(arg => _logger.Error("Ошибка при загрузке домов", arg.Exception), TaskContinuationOptions.OnlyOnFaulted);
		}

		public HouseDTO[] GetHouses()
		{
			return Houses?.Clone() as HouseDTO[];
		}

		private void SaveHouses(Task<IEnumerable<HouseDTO>> newHouses)
		{
			var houses = newHouses.Result;

			Houses = houses.OrderBy(x => x.ComplexNumber, new NaturalStringComparer()).ToArray();
			_logger.Info($"Домов загружено : { Houses.Length }");
			HousesLoaded?.Invoke();
		}

		protected override void CancelLoading()
		{
			if(CurrentLoadTask == null || !CurrentLoadTask.IsCompleted)
			{
				_logger.Debug("Отмена предыдущей загрузки домов");
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
