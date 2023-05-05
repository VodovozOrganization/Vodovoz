using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.EntityRepositories.Profitability;

namespace Vodovoz.Controllers
{
	public class ProfitabilityConstantsViewModelHandler : IProfitabilityConstantsViewModelHandler
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IProfitabilityConstantsRepository _profitabilityConstantsRepository;

		public ProfitabilityConstantsViewModelHandler(
			IUnitOfWorkFactory unitOfWorkFactory,
			IProfitabilityConstantsRepository profitabilityConstantsRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_profitabilityConstantsRepository =
				profitabilityConstantsRepository ?? throw new ArgumentNullException(nameof(profitabilityConstantsRepository));
		}

		public IUnitOfWorkGeneric<ProfitabilityConstants> GetLastCalculatedProfitabilityConstants()
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var lastProfitabilityContants = _profitabilityConstantsRepository.GetLastProfitabilityConstants(uow);

				return lastProfitabilityContants != null
					? CreateNewUoW(lastProfitabilityContants.Id)
					: CreateNewUoW(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1));
			}
		}

		public IUnitOfWorkGeneric<ProfitabilityConstants> GetProfitabilityConstantsByCalculatedMonth(
			IUnitOfWork uow, DateTime calculatedMonth)
		{
			var calculatedConstants = _profitabilityConstantsRepository.GetProfitabilityConstantsByCalculatedMonth(uow, calculatedMonth);
			
			return calculatedConstants != null
				? CreateNewUoW(calculatedConstants.Id)
				: CreateNewUoW(calculatedMonth);
		}

		private IUnitOfWorkGeneric<ProfitabilityConstants> CreateNewUoW(DateTime dateTime)
		{
			var uowGeneric = _unitOfWorkFactory.CreateWithNewRoot<ProfitabilityConstants>();
			uowGeneric.Root.CalculatedMonth = dateTime;
			return uowGeneric;
		}
		
		private IUnitOfWorkGeneric<ProfitabilityConstants> CreateNewUoW(int profitabilityConstantsId) =>
			_unitOfWorkFactory.CreateForRoot<ProfitabilityConstants>(profitabilityConstantsId);
	}
}
