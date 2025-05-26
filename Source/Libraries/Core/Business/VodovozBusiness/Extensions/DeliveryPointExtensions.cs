using Gamma.Utilities;
using System.Text;
using Vodovoz.Domain.Client;

namespace VodovozBusiness.Extensions
{
	/// <summary>
	/// Расширение функционала <see cref="DeliveryPoint"/>
	/// </summary>
	public static class DeliveryPointExtensions
	{
		public static string GetAddressString(this DeliveryPoint deliveryPoint)
		{
			var addressStringBuilder = new StringBuilder();

			if(!string.IsNullOrWhiteSpace(deliveryPoint.LocalityType))
			{
				addressStringBuilder.Append($"{deliveryPoint.LocalityType} ");
			}

			if(!string.IsNullOrWhiteSpace(deliveryPoint.City))
			{
				addressStringBuilder.Append($"{deliveryPoint.City}, ");
			}

			if(!string.IsNullOrWhiteSpace(deliveryPoint.StreetType))
			{
				addressStringBuilder.Append($"{deliveryPoint.StreetType.ToLower()} ");
			}

			if(!string.IsNullOrWhiteSpace(deliveryPoint.Street))
			{
				addressStringBuilder.Append($"{deliveryPoint.Street}, ");
			}

			if(!string.IsNullOrWhiteSpace(deliveryPoint.Building))
			{
				addressStringBuilder.Append($"{deliveryPoint.Building}, ");
			}

			if(!string.IsNullOrWhiteSpace(deliveryPoint.Letter))
			{
				addressStringBuilder.Append($"литера {deliveryPoint.Letter}, ");
			}

			if(!string.IsNullOrWhiteSpace(deliveryPoint.Entrance))
			{
				addressStringBuilder.Append($"{deliveryPoint.EntranceType.GetEnumTitle()} {deliveryPoint.Entrance}, ");
			}

			if(!string.IsNullOrWhiteSpace(deliveryPoint.Floor))
			{
				addressStringBuilder.Append($"этаж {deliveryPoint.Floor}, ");
			}

			if(!string.IsNullOrWhiteSpace(deliveryPoint.Room))
			{
				addressStringBuilder.Append($"{deliveryPoint.RoomType.GetEnumTitle()} {deliveryPoint.Room}, ");
			}

			return addressStringBuilder
				.ToString()
				.TrimEnd(',', ' ');
		}
	}
}
