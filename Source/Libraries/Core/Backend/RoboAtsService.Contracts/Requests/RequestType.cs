using System;
using System.Runtime.Serialization;

namespace RoboAtsService.Contracts.Requests
{
	[Serializable]
	public enum RequestType
	{
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
