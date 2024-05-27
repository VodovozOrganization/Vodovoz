using System;
using System.Linq;
using Taxcom.Client.Api.Document.DocumentByFormat1115131;
using Vodovoz.Core.Data.Clients;
using Vodovoz.Core.Data.Organizations;
using Vodovoz.Core.Domain.Clients;

namespace TaxcomEdoApi.Converters
{
	public class ParticipantDocFlowConverter : IParticipantDocFlowConverter
	{
		private const string _russiaCode = "643";  //Россия
		
		public UchastnikTip ConvertCounterpartyToUchastnikTip(Counterparty client)
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

		public UchastnikTip ConvertCounterpartyToUchastnikTip(Counterparty client, int? deliveryPointId)
		{
			var deliveryPoint = client.DeliveryPoints.SingleOrDefault(x => x.Id == deliveryPointId);
			
			switch(client.CargoReceiverSource)
			{
				case CargoReceiverSource.FromDeliveryPoint:
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
				case CargoReceiverSource.Special:
					if(!string.IsNullOrWhiteSpace(client.CargoReceiver) && client.UseSpecialDocFields)
					{
						return new UchastnikTip
						{
							IdSv = new UchastnikTipIdSv
							{
								Item = GetSpecialConsignee(client, client.PayerSpecialKPP)
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
		
		public UchastnikTip ConvertOrganizationToUchastnikTip(Organization org, DateTime? deliveryDate)
		{
			return new UchastnikTip
			{
				IdSv = new UchastnikTipIdSv
				{
					Item = GetLegalCounterpartyInfo(org.INN, org.KPP, org.Name)
				},
				Adres = new AdresTip
				{
					Item = GetCustomAddress(!string.IsNullOrWhiteSpace(org.JurAddress) ? org.JurAddress : "Не найден адрес")
				}
			};
		}

		private object GetConcreteBuyer(Counterparty client)
		{
			var clientName = client.FullName;
			var clientKpp = client.KPP;

			if(client.UseSpecialDocFields)
			{
				if(!string.IsNullOrWhiteSpace(client.SpecialCustomer))
				{
					clientName = client.SpecialCustomer;
				}
				if(!string.IsNullOrWhiteSpace(client.PayerSpecialKPP))
				{
					clientKpp = client.PayerSpecialKPP;
				}
			}

			switch(client.PersonType)
			{
				case PersonType.legal:
					if(client.INN.Length == 12)
					{
						return new SvIPTip
						{
							FIO = FillFIOTip(clientName),
							INNFL = client.INN
						};
					}

					return GetLegalCounterpartyInfo(client.INN, clientKpp, clientName);
				case PersonType.natural:
				default:
					throw new InvalidOperationException("Нельзя сделать УПД для физического лица");
			}
		}
		
		private object GetConcreteConsignee(Counterparty client, string specialKpp)
		{
			switch(client.PersonType)
			{
				case PersonType.legal:
					if(client.INN.Length == 12)
					{
						return new SvIPTip
						{
							FIO = FillFIOTip(client.FullName),
							INNFL = client.INN
						};
					}

					return !string.IsNullOrWhiteSpace(specialKpp)
						? GetLegalCounterpartyInfo(client.INN, specialKpp, client.FullName)
						: GetLegalCounterpartyInfo(client.INN, client.KPP, client.FullName);
				case PersonType.natural:
				default:
					throw new InvalidOperationException("Нельзя сделать УПД для физического лица");
			}
		}
		
		private object GetSpecialConsignee(Counterparty client, string specialKpp)
		{
			switch(client.PersonType)
			{
				case PersonType.legal:
					if(client.INN.Length == 12)
					{
						return new SvIPTip
						{
							FIO = FillSpecialFIOTip(client.CargoReceiver),
							INNFL = client.INN
						};
					}

					return !string.IsNullOrWhiteSpace(specialKpp)
						? GetLegalCounterpartyInfo(client.INN, specialKpp, client.FullName)
						: GetLegalCounterpartyInfo(client.INN, client.KPP, client.FullName);
				case PersonType.natural:
				default:
					throw new InvalidOperationException("Нельзя сделать УПД для физического лица");
			}
		}
		
		private object GetCustomAddress(string address)
		{
			return new AdrInfTip
			{
				KodStr = _russiaCode,
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
