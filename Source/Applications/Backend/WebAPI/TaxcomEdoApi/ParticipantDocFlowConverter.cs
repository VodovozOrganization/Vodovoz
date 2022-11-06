using System;
using System.Linq;
using Taxcom.Client.Api.Document.DocumentByFormat1115131;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;

namespace TaxcomEdoApi
{
	public class ParticipantDocFlowConverter
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
							Item = GetCustomAddress(deliveryPoint != null ? deliveryPoint.CompiledAddress : client.JurAddress)
						}
					};
				case CargoReceiverSource.Special:
					if(!string.IsNullOrWhiteSpace(client.CargoReceiver) && client.UseSpecialDocFields)
					{
						return new UchastnikTip
						{
							IdSv = new UchastnikTipIdSv
							{
								Item = GetSpecialConsignee(client, deliveryPoint?.KPP)
							},
							/*Adres = new AdresTip
							{
								Item = new AdrInfTip
								{
									KodStr = _russiaCode,
									AdrTekst = deliveryPoint != null ? deliveryPoint.CompiledAddress : client.JurAddress
								}
							}*/
						};
					}
					return ConvertCounterpartyToUchastnikTip(client);
				default:
					return ConvertCounterpartyToUchastnikTip(client);
			}
		}
		
		public UchastnikTip ConvertOrganizationToUchastnikTip(Organization org, DateTime? deliveryDate)
		{
			var orgVersion = org.OrganizationVersions.SingleOrDefault(
				x => x.StartDate <= deliveryDate
					&& (x.EndDate == null || x.EndDate >= deliveryDate));
			
			return new UchastnikTip
			{
				IdSv = new UchastnikTipIdSv
				{
					Item = GetUchastnikUl(org.INN, org.KPP, org.Name)
				},
				Adres = new AdresTip
				{
					Item = GetCustomAddress(orgVersion != null ? orgVersion.JurAddress : "Не найден адрес")
				}
			};
		}

		private object GetConcreteBuyer(Counterparty client)
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

					return client.UseSpecialDocFields && !string.IsNullOrWhiteSpace(client.PayerSpecialKPP)
						? GetUchastnikUl(client.INN, client.PayerSpecialKPP, client.FullName)
						: GetUchastnikUl(client.INN, client.KPP, client.FullName);
				case PersonType.natural:
				default:
					throw new InvalidOperationException("Нельзя сделать УПД для физического лица");
			}
		}
		
		private object GetConcreteConsignee(Counterparty client, string deliveryPointKpp)
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

					return !string.IsNullOrWhiteSpace(deliveryPointKpp)
						? GetUchastnikUl(client.INN, deliveryPointKpp, client.FullName)
						: GetUchastnikUl(client.INN, client.KPP, client.FullName);
				case PersonType.natural:
				default:
					throw new InvalidOperationException("Нельзя сделать УПД для физического лица");
			}
		}
		
		private object GetSpecialConsignee(Counterparty client, string deliveryPointKpp)
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

					return !string.IsNullOrWhiteSpace(deliveryPointKpp)
						? GetUchastnikUl(client.INN, deliveryPointKpp, client.CargoReceiver)
						: GetUchastnikUl(client.INN, client.KPP, client.CargoReceiver);
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

		private object GetUchastnikUl(string inn, string kpp, string name)
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
