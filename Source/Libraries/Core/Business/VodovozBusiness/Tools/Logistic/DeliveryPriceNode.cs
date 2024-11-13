using System.Collections.Generic;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Tools.Logistic
{
	public class DeliveryPriceNode
	{
		public string Distance { get; set; }
		public string Price { get; set; }
		public string MinBottles { get; set; }
		public List<DeliveryRuleRow> DeliveryRules { get; set; }
		public List<DeliveryPriceRow> Prices { get; set; }
		public bool ByDistance { get; set; }
		public bool WithPrice { get; set; }
		public string DistrictName { get; set; }
		public string GeographicGroups { get; set; }
		public string WageDistrict { get; set; }
		public int DistrictId { get; set; }
		public int ServiceDistrictId { get; set; }

		private string _errorMessage;
		public string ErrorMessage {
			get => _errorMessage;
			set {
				ClearValues();
				_errorMessage = value;
			}
		}

		public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

		public DeliveryPriceNode()
		{
			ClearValues();
			ErrorMessage = string.Empty;
		}

		public void ClearValues()
		{
			Distance = string.Empty;
			Price = string.Empty;
			MinBottles = string.Empty;
			DistrictName = string.Empty;
			GeographicGroups = string.Empty;
			Prices = new List<DeliveryPriceRow>();
		}
	}
}
