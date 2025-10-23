using Edo.Contracts.Messages.Dto;
using Taxcom.Client.Api.Document.DocumentByFormat1115131_5_03;

namespace TaxcomEdoApi.Library.Converters.Format5_03
{
	public class ErpDocumentInfoConverter5_03 : IErpDocumentInfoConverter5_03
	{
		public UchastnikTip ConvertCounterpartyToCustomerInfo(CustomerInfo customer)
		{
			return new UchastnikTip
			{
				IdSv = new UchastnikTipIdSv
				{
					Item = GetConcreteBuyer(customer)
				},
				Adres = new AdresTip
				{
					Item = GetCustomAddress(customer.Organization.Address)
				}
			};
		}

		public UchastnikTip ConvertCounterpartyToConsigneeInfo(ConsigneeInfo consignee)
		{
			return new UchastnikTip
			{
				IdSv = new UchastnikTipIdSv
				{
					Item = GetConsigneeInfo(consignee)
				},
				Adres = new AdresTip
				{
					Item = GetCustomAddress(consignee.Organization.Address)
				}
			};
		}

		public UchastnikTip ConvertOrganizationToSellerInfo(OrganizationInfo org)
		{
			return new UchastnikTip
			{
				IdSv = new UchastnikTipIdSv
				{
					Item = GetSellerInfo(org)
				},
				Adres = new AdresTip
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
				return new SvIPTip
				{
					FIO = FillFIOTip(clientName),
					INNFL = clientInn
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
				return new SvIPTip
				{
					FIO = FillFIOTip(consignee.CargoReceiver ?? clientName),
					INNFL = clientInn
				};
			}

			return GetLegalCounterpartyInfo(clientInn, clientKpp, clientName);
		}
		
		private object GetSellerInfo(OrganizationInfo org)
		{
			var clientInn = org.Inn;
			var clientName = org.Name;
			var clientKpp = org.Kpp;
			
			if(clientInn.Length == 12)
			{
				return new SvIPTip
				{
					FIO = FillFIOTip(clientName),
					INNFL = clientInn
				};
			}

			return GetLegalCounterpartyInfo(clientInn, clientKpp, clientName);
		}
		
		private object GetCustomAddress(AddressInfo address)
		{
			return new AdrInfTip
			{
				KodStr = address.CountryCode,
				NaimStran = address.CountryName,
				AdrTekst = address.Address
			};
		}
		
		private object GetLegalCounterpartyInfo(string inn, string kpp, string name)
		{
			return new UchastnikTipIdSvSvJuLUch
			{
				INNJuL = inn,
				KPP = kpp,
				NaimOrg = name
			};
		}
		
		private FIOTip FillFIOTip(string fullName)
		{
			var fio = GetFIOFromPrivateBusinessman(fullName);
			var fioTip = new FIOTip();

			if(fio.Length >= 1)
			{
				fioTip.Familija = fio[0];
				fioTip.Imja = "не указано";
			}

			if(fio.Length >= 2)
			{
				fioTip.Imja = fio[1];
			}
			
			if(fio.Length >= 3)
			{
				fioTip.Otchestvo = fio[2];
			}

			return fioTip;
		}
		
		private string[] GetFIOFromPrivateBusinessman(string fullName)
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
