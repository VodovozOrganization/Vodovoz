using DriverAPI.Library.Models;
using Microsoft.Extensions.Logging;
using System;
using Vodovoz.Domain.Client;

namespace DriverAPI.Library.Converters
{
    public class DeliveryPointConverter
    {
        private readonly ILogger<DeliveryPointConverter> logger;

        public DeliveryPointConverter(ILogger<DeliveryPointConverter> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public APIAddress extractAPIAddressFromDeliveryPoint(DeliveryPoint deliveryPoint)
        {
            return new APIAddress()
            {
                City = deliveryPoint.City,
                Street = deliveryPoint.Street,
                Building = deliveryPoint.Building + deliveryPoint.Letter,
                Entrance = deliveryPoint.Entrance,
                Floor = deliveryPoint.Floor,
                Apartment = deliveryPoint.Room,
                DeliveryPointCategory = deliveryPoint.Category?.Name,
                EntranceType = deliveryPoint.EntranceType.ToString(),
                RoomType = deliveryPoint.RoomType.ToString()
            };
        }
    }
}
