using Edo.Contracts.Messages.Dto;
using Edo.Contracts.Xml.FormalizedDocuments.UPD;
using Edo.Contracts.Xml.Other;

namespace TaxcomEdoApi.Library.Converters.Format5_03
{
	public class ErpDocumentInfoConverter5_03 : IErpDocumentInfoConverter5_03
	{
		public УчастникТип ConvertCounterpartyToCustomerInfo(CustomerInfo customer)
		{
			return new УчастникТип
			{
				ИдСв = new УчастникТипИдСв
				{
					Item = GetConcreteBuyer(customer)
				},
				Адрес = new АдресТип
				{
					Item = GetCustomAddress(customer.Organization.Address)
				}
			};
		}

		public УчастникТип ConvertCounterpartyToConsigneeInfo(ConsigneeInfo consignee)
		{
			return new УчастникТип
			{
				ИдСв = new УчастникТипИдСв
				{
					Item = GetConsigneeInfo(consignee)
				},
				Адрес = new АдресТип
				{
					Item = GetCustomAddress(consignee.Organization.Address)
				}
			};
		}

		public УчастникТип ConvertOrganizationToSellerInfo(OrganizationInfo org)
		{
			return new УчастникТип
			{
				ИдСв = new УчастникТипИдСв
				{
					Item = GetLegalCounterpartyInfo(org.Inn, org.Kpp, org.Name)
				},
				Адрес = new АдресТип
				{
					Item = GetCustomAddress(org.Address)
				}
			};
		}
		
		private object GetConcreteBuyer(CustomerInfo customer)
		{
			var clientInn = customer.Organization.Inn;
			var clientName = customer.Organization.Name;
			var clientKpp = customer.Organization.Kpp;
			
			if(clientInn.Length == 12)
			{
				return new УчастникТипИдСвСвИП
				{
					ФИО = FillFullName(clientName),
					ИННФЛ = clientInn
				};
			}

			return GetLegalCounterpartyInfo(clientInn, clientKpp, clientName);
		}
		
		private object GetConsigneeInfo(ConsigneeInfo consignee)
		{
			var clientInn = consignee.Organization.Inn;
			var clientName = consignee.Organization.Name;
			var clientKpp = consignee.Organization.Kpp;
			
			if(clientInn.Length == 12)
			{
				return new УчастникТипИдСвСвИП
				{
					ФИО = FillFullName(consignee.CargoReceiver ?? clientName),
					ИННФЛ = clientInn
				};
			}

			return GetLegalCounterpartyInfo(clientInn, clientKpp, clientName);
		}
		
		private object GetCustomAddress(AddressInfo address)
		{
			return new АдрИнфТип
			{
				КодСтр = address.CountryCode,
				НаимСтран = address.CountryName,
				АдрТекст = address.Address
			};
		}
		
		private object GetLegalCounterpartyInfo(string inn, string kpp, string name)
		{
			return new УчастникТипИдСвСвЮЛУч
			{
				ИННЮЛ = inn,
				КПП = kpp,
				НаимОрг = name
			};
		}
		
		private FullName FillFullName(string fullName)
		{
			var fio = GetFullNameFromPrivateBusinessman(fullName);
			var fioTip = new FullName();

			if(fio.Length >= 1)
			{
				fioTip.LastName = fio[0];
				fioTip.Name = "не указано";
			}

			if(fio.Length >= 2)
			{
				fioTip.Name = fio[1];
			}
			
			if(fio.Length >= 3)
			{
				fioTip.Patronymic = fio[2];
			}

			return fioTip;
		}
		
		private string[] GetFullNameFromPrivateBusinessman(string fullName)
		{
			var fio = fullName.Trim('"');

			if(fio.ToLower().StartsWith("ип"))
			{
				fio = fio.Remove(0, 2).Trim(' ');
			}

			var str = fio.Split(' ');
			return str;
		}
	}
}
