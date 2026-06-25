using System;
using CustomerAppsApi.Factories;
using CustomerAppsApi.Library.Converters;
using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Models;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.EntityRepositories.RentPackages;

namespace CustomerAppsApi.Library.Models
{
	public class RentPackageModel : IRentPackageModel
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IRentPackageRepository _rentPackageRepository;
		private readonly ISourceConverter _sourceConverter;
		private readonly IRentPackageFactory _rentPackageFactory;

		public RentPackageModel(
			IUnitOfWork unitOfWork,
			IRentPackageRepository rentPackageRepository,
			ISourceConverter sourceConverter,
			IRentPackageFactory rentPackageFactory)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_rentPackageRepository = rentPackageRepository ?? throw new ArgumentNullException(nameof(rentPackageRepository));
			_sourceConverter = sourceConverter ?? throw new ArgumentNullException(nameof(sourceConverter));
			_rentPackageFactory = rentPackageFactory ?? throw new ArgumentNullException(nameof(rentPackageFactory));
		}

		public FreeRentPackagesDto GetFreeRentPackages(Source source)
		{
			var parameterType = _sourceConverter.ConvertToNomenclatureOnlineParameterType(source);
			var packageNodes = _rentPackageRepository.GetFreeRentPackagesForSend(_unitOfWork, parameterType);

			return _rentPackageFactory.CreateFreeRentPackagesDto(packageNodes);
		}
	}
}
