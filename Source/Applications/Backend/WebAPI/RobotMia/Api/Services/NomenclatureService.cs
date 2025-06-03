using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Goods;
using Vodovoz.RobotMia.Contracts.Responses.V1;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.RobotMia.Api.Services
{
	/// <inheritdoc cref="INomenclatureService"/>
	public class NomenclatureService : INomenclatureService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IGenericRepository<Nomenclature> _nomenclatureRepository;
		private readonly IGenericRepository<RobotMiaParameters> _robotMiaParametersRepository;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="nomenclatureSettings"></param>
		/// <param name="nomenclatureRepository"></param>
		/// <param name="robotMiaParametersRepository"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public NomenclatureService(
			IUnitOfWork unitOfWork,
			INomenclatureSettings nomenclatureSettings,
			IGenericRepository<Nomenclature> nomenclatureRepository,
			IGenericRepository<RobotMiaParameters> robotMiaParametersRepository)
		{
			_unitOfWork = unitOfWork
				?? throw new ArgumentNullException(nameof(unitOfWork));
			_nomenclatureSettings = nomenclatureSettings
				?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_nomenclatureRepository = nomenclatureRepository
				?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_robotMiaParametersRepository = robotMiaParametersRepository
				?? throw new ArgumentNullException(nameof(robotMiaParametersRepository));
		}

		/// <inheritdoc/>
		public async Task<IEnumerable<NomenclatureDto>> GetNomenclatures()
		{
			var parameters = _robotMiaParametersRepository
				.Get(_unitOfWork, x => x.GoodsOnlineAvailability != null)
				.ToArray();

			var nomenclatureIds = parameters.Select(x => x.NomenclatureId).ToArray();

			var nomenclatures = _nomenclatureRepository.Get(
				_unitOfWork,
				nomenclature => !nomenclature.IsArchive
					&& nomenclatureIds.Contains(nomenclature.Id));

			var results = new List<NomenclatureDto>();

			foreach(var nomenclature in nomenclatures)
			{
				var parameter = parameters.FirstOrDefault(x => x.NomenclatureId == nomenclature.Id);
				if(parameter != null)
				{
					results.Add(new NomenclatureDto
					{
						Id = nomenclature.Id,
						ShortName = nomenclature.ShortName,
						Price = nomenclature.GetPrice(1),
						CanSale = parameters.First(x => x.NomenclatureId == nomenclature.Id).GoodsOnlineAvailability == Domain.Goods.NomenclaturesOnlineParameters.GoodsOnlineAvailability.ShowAndSale,
						SlangWords = parameters.First(x => x.NomenclatureId == nomenclature.Id).SlangWords.Select(s => s.Word)
					});
				}
			}

			return await ValueTask.FromResult(results);
		}

		/// <inheritdoc/>
		public async Task<NomenclatureDto> GetForfeitNomenclature()
		{
			var nomenclature = _nomenclatureRepository.Get(
				_unitOfWork,
				nomenclature => nomenclature.Id == _nomenclatureSettings.ForfeitId);

			return await ValueTask.FromResult(new NomenclatureDto
			{
				Id = nomenclature.First().Id,
				ShortName = nomenclature.First().ShortName,
				Price = nomenclature.First().GetPrice(1),
				CanSale = false,
				SlangWords = new List<string>()
			});
		}
	}
}
