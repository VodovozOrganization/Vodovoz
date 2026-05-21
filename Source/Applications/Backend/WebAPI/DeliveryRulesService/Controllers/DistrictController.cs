using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeliveryRulesService.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using QS.DomainModel.UoW;
using Vodovoz.Application.Sale;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Delivery;
using VodovozBusiness.Services.Sale;

namespace DeliveryRulesService.Controllers
{
	public abstract class DistrictController : ControllerBase
	{
		protected DistrictController(
			ILogger<DistrictController> logger,
			IDeliveryRepository deliveryRepository,
			DistrictCacheService  districtCacheService,
			IDistrictRulesService districtRulesService)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			DeliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
			DistrictCacheService = districtCacheService ?? throw new ArgumentNullException(nameof(districtCacheService));
			DistrictRulesService = districtRulesService ?? throw new ArgumentNullException(nameof(districtRulesService));
		}

		protected ILogger<DistrictController> Logger { get; }
		protected DistrictCacheService DistrictCacheService { get; }
		protected IDistrictRulesService DistrictRulesService { get; }
		protected IDeliveryRepository DeliveryRepository { get; }
		
		protected async Task<District> GetDistrictAsync(IUnitOfWork uow, decimal latitude, decimal longitude, CancellationToken cancellationToken)
		{
			District district;
			try
			{
				district = await DeliveryRepository.GetDistrictAsync(uow, latitude, longitude, cancellationToken);
			}
			catch(Exception e)
			{
				Logger.LogError(e, "Ошибка при подборе района по координатам");
				Logger.LogInformation("Подбор района из кэша");
				
				district = DistrictCacheService.Districts.Values
					.FirstOrDefault(x => x.DistrictBorder.Contains(new Point((double)latitude, (double)longitude)));
			}

			return district;
		}
	}
}
