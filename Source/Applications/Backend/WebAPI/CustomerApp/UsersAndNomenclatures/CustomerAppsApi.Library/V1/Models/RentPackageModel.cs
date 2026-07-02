using System;
using CustomerAppsApi.Library.V1.Converters;
using CustomerAppsApi.Library.V1.Dto;
using CustomerAppsApi.Library.V1.Repositories;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.V1.Models
{
	public class RentPackageModel : IRentPackageModel
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICustomerAppRentPackageRepository _rentPackageRepository;
		private readonly ISourceConverter _sourceConverter;

		public RentPackageModel(
			IUnitOfWork unitOfWork,
			ICustomerAppRentPackageRepository rentPackageRepository,
			ISourceConverter sourceConverter
			)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_rentPackageRepository = rentPackageRepository ?? throw new ArgumentNullException(nameof(rentPackageRepository));
			_sourceConverter = sourceConverter ?? throw new ArgumentNullException(nameof(sourceConverter));
		}

		public FreeRentPackagesDto GetFreeRentPackages(Source source)
		{
			var parameterType = _sourceConverter.ConvertToNomenclatureOnlineParameterType(source);
			return new FreeRentPackagesDto
			{
				RentPackages = _rentPackageRepository.GetFreeRentPackagesForSend(_unitOfWork, parameterType)
			};
		}
	}
}
