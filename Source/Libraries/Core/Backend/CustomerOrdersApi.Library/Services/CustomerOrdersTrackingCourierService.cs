using System;
using CustomerOrdersApi.Library.Config;
using CustomerOrdersApi.Library.Dto.Orders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Logistic;
using VodovozInfrastructure.Cryptography;

namespace CustomerOrdersApi.Library.Services
{
	public class CustomerOrdersTrackingCourierService : SignatureService, ICustomerOrdersTrackingCourierService
	{
		private readonly ILogger<ICustomerOrdersTrackingCourierService> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly SignatureOptions _signatureOptions;
		private readonly IDeliveryPointRepository _deliveryPointRepository;
		private readonly ITrackRepository _trackRepository;
		private readonly ISignatureManager _signatureManager;

		public CustomerOrdersTrackingCourierService(
			ILogger<ICustomerOrdersTrackingCourierService> logger,
			IUnitOfWork unitOfWork,
			IOptions<SignatureOptions> signatureOptions,
			IDeliveryPointRepository deliveryPointRepository,
			ITrackRepository trackRepository,
			ISignatureManager signatureManager)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_signatureOptions = (signatureOptions ?? throw new ArgumentNullException(nameof(signatureOptions))).Value;
			_deliveryPointRepository = deliveryPointRepository ?? throw new ArgumentNullException(nameof(deliveryPointRepository));
			_trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			_signatureManager = signatureManager ?? throw new ArgumentNullException(nameof(signatureManager));
		}
		
		public CourierCoordinates GetCurrentCourierCoordinates(CourierCoordinatesRequest coordinatesRequest)
		{
			var courierCoordinate =
				_trackRepository.GetDriverLastCoordinateFromOnlineOrder(_unitOfWork, coordinatesRequest.ExternalOrderId);

			if(!courierCoordinate.Latitude.HasValue || !courierCoordinate.Longitude.HasValue)
			{
				
			}

			var deliveryPointCoordinate =
				_deliveryPointRepository.DeliveryPointCoordinatesFromOnlineOrder(_unitOfWork, coordinatesRequest.ExternalOrderId);

			if(!deliveryPointCoordinate.Latitude.HasValue || !deliveryPointCoordinate.Longitude.HasValue)
			{
				
			}

			return new CourierCoordinates
			{
				CourierCoordinate = courierCoordinate,
				DeliveryPointCoordinate = deliveryPointCoordinate,
				ExternalOrderId = coordinatesRequest.ExternalOrderId
			};
		}

		public bool ValidateCourierCoordinatesSignature(CourierCoordinatesRequest coordinatesRequest, out string generatedSignature)
		{
			var sourceSign = GetSourceSign(coordinatesRequest.Source, _signatureOptions);
			
			return _signatureManager.Validate(
				coordinatesRequest.Signature,
				new CourierCoordinatesSignatureParams
				{
					ShopId = (int)coordinatesRequest.Source,
					Sign = sourceSign,
					OrderId = coordinatesRequest.ExternalOrderId.ToString()
				},
				out generatedSignature);
		}
	}
}
