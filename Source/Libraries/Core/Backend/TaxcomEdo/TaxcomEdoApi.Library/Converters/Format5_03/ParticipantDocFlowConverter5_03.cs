using System;
using Taxcom.Client.Api.Document.DocumentByFormat1115131_5_03;
using TaxcomEdo.Contracts.Counterparties;
using TaxcomEdo.Contracts.Organizations;

namespace TaxcomEdoApi.Library.Converters.Format5_03
{
	public class ParticipantDocFlowConverter5_03 : IParticipantDocFlowConverter5_03
	{
		private const string _russiaCode = "643";  //Россия
		private const string _russia = "Россия";
		
		public UchastnikTip ConvertCounterpartyToUchastnikTip(CounterpartyInfoForEdo client)
		{
			return new UchastnikTip
			{
				IdSv = new UchastnikTipIdSv
				{
					Item = GetConcreteBuyer(client)
				},
				Adres = new AdresTip
				{
					Item = GetCustomAddress(client.JurAddress)
				}
			};
		}

		public UchastnikTip ConvertCounterpartyToUchastnikTip(CounterpartyInfoForEdo client, DeliveryPointInfoForEdo deliveryPoint)
		{
			switch(client.CargoReceiverSource)
			{
				case CargoReceiverSourceType.FromDeliveryPoint:
					return new UchastnikTip
					{
						IdSv = new UchastnikTipIdSv
						{
							Item = GetConcreteConsignee(client, deliveryPoint?.KPP)
						},
						Adres = new AdresTip
						{
							Item = GetCustomAddress(deliveryPoint != null ? deliveryPoint.ShortAddress : client.JurAddress)
						}
					};
				case CargoReceiverSourceType.Special:
					if(!string.IsNullOrWhiteSpace(client.CargoReceiver) && client.UseSpecialDocFields)
					{
						return new UchastnikTip
						{
							IdSv = new UchastnikTipIdSv
							{
								Item = GetSpecialConsignee(client, client.PayerSpecialKpp)
							},
							Adres = new AdresTip
							{
								Item = GetCustomAddress(client.CargoReceiver)
							}
						};
					}
					return ConvertCounterpartyToUchastnikTip(client);
				default:
					return ConvertCounterpartyToUchastnikTip(client);
			}
		}

		public UchastnikTip ConvertOrganizationToUchastnikTip(OrganizationInfoForEdo org)
		{
			return new UchastnikTip
			{
				IdSv = new UchastnikTipIdSv
				{
					Item = GetLegalCounterpartyInfo(org.Inn, org.Kpp, org.Name)
				},
				Adres = new AdresTip
				{
					Item = GetCustomAddress(!string.IsNullOrWhiteSpace(org.JurAddress) ? org.JurAddress : "Не найден адрес")
				}
			};
		}

		private object GetConcreteBuyer(CounterpartyInfoForEdo client)
		{
			var clientName = client.FullName;
			var clientKpp = client.Kpp;

			if(client.UseSpecialDocFields)
			{
				if(!string.IsNullOrWhiteSpace(client.SpecialCustomer))
				{
					clientName = client.SpecialCustomer;
				}
				if(!string.IsNullOrWhiteSpace(client.PayerSpecialKpp))
				{
					clientKpp = client.PayerSpecialKpp;
				}
			}

			switch(client.PersonType)
			{
				case CounterpartyInfoType.Legal:
					if(client.Inn.Length == 12)
					{
						return new SvIPTip
						{
							FIO = FillFIOTip(clientName),
							INNFL = client.Inn
						};
					}

					return GetLegalCounterpartyInfo(client.Inn, clientKpp, clientName);
				case CounterpartyInfoType.Natural:
				default:
					throw new InvalidOperationException("Нельзя сделать УПД для физического лица");
			}
		}
		
		private object GetConcreteConsignee(CounterpartyInfoForEdo client, string specialKpp)
		{
			switch(client.PersonType)
			{
				case CounterpartyInfoType.Legal:
					if(client.Inn.Length == 12)
					{
						return new SvIPTip
						{
							FIO = FillFIOTip(client.FullName),
							INNFL = client.Inn
						};
					}

					return !string.IsNullOrWhiteSpace(specialKpp)
						? GetLegalCounterpartyInfo(client.Inn, specialKpp, client.FullName)
						: GetLegalCounterpartyInfo(client.Inn, client.Kpp, client.FullName);
				case CounterpartyInfoType.Natural:
				default:
					throw new InvalidOperationException("Нельзя сделать УПД для физического лица");
			}
		}
		
		private object GetSpecialConsignee(CounterpartyInfoForEdo client, string specialKpp)
		{
			switch(client.PersonType)
			{
				case CounterpartyInfoType.Legal:
					if(client.Inn.Length == 12)
					{
						return new SvIPTip
						{
							FIO = FillSpecialFIOTip(client.CargoReceiver),
							INNFL = client.Inn
						};
					}

					return !string.IsNullOrWhiteSpace(specialKpp)
						? GetLegalCounterpartyInfo(client.Inn, specialKpp, client.FullName)
						: GetLegalCounterpartyInfo(client.Inn, client.Kpp, client.FullName);
				case CounterpartyInfoType.Natural:
				default:
					throw new InvalidOperationException("Нельзя сделать УПД для физического лица");
			}
		}
		
		private object GetCustomAddress(string address)
		{
			return new AdrInfTip
			{
				KodStr = _russiaCode,
				NaimStran = _russia,
				AdrTekst = address
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
		
		private FIOTip FillSpecialFIOTip(string specialName)
		{
			var middlePoint = specialName.Length / 2;

			var specialFio = new[]
			{
				specialName[..middlePoint],
				specialName[middlePoint..]
			};
			
			var fioTip = new FIOTip
			{
				Familija = specialFio[0],
				Imja = specialFio[1]
			};

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
