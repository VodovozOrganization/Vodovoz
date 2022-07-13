using System;
using System.Runtime.Serialization;

namespace RoboAtsService.Requests
{
	[Serializable]
	public enum RequestType
	{
		/// <summary>
		/// Запрос количества бутылей воды в последнем заказе
		/// </summary>
		//[DataMember(Name = "lastorderdata_bottles")]
		//LastOrderDataBottles,

		//[DataMember(Name = "lastorderdata_return")]
		//LastOrderDataReturn,

		//[DataMember(Name = "lastorderdata_addresshome")]
		//LastOrderDataAddressHome,

		//[DataMember(Name = "lastorderdata_addressflat")]
		//LastOrderDataAddressFlat,

		//[DataMember(Name = "lastorderdata_addresscorp")]
		//LastOrderDataAddressCorp,

		//[DataMember(Name = "lastorderdata_addressofis")]
		//LastOrderDataAddressOffice,

		[DataMember(Name = "lastorderdata_firstname_code")]
		LastOrderDataFirstNameCode,

		[DataMember(Name = "lastorderdata_patronymic_code")]
		LastOrderDataPatronymicCode,

		[DataMember(Name = "lastorderdata_street_code")]
		LastOrderDataStreetCode,

		[DataMember(Name = "quantityaddress")]
		QuantityAddress,

		[DataMember(Name = "check")]
		Check,

		[DataMember(Name = "watertype")]
		WaterType,

		[DataMember(Name = "lastorderdata_addresregionspb")]
		LastOrderDataAddressRegionSpb,

		[DataMember(Name = "lastordercheck_driverphone")]
		LastOrderCheckDriverPhone,

		[DataMember(Name = "lastorderdata_firstname")]
		LastOrderDataFirstName,

		[DataMember(Name = "lastorderdata_lastname")]
		LastOrderDataLastName,

		[DataMember(Name = "lastorderdata_patronymic")]
		LastOrderDataPatronymic,

		[DataMember(Name = "lastorderdata_addressfull")]
		LastOrderDataAddressFull,

		[DataMember(Name = "last_order")]
		LastOrder
	}
}
