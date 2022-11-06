using System;
using System.Collections.Generic;
using System.Linq;
using Taxcom.Client.Api.Document.DocumentByFormat1115131;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace TaxcomEdoApi
{
	public class EdoUpdFactory
	{
		private readonly ParticipantDocFlowConverter _participantDocFlowConverter;
		private readonly UpdProductConverter _updProductConverter;

		public EdoUpdFactory(
			ParticipantDocFlowConverter participantDocFlowConverter,
			UpdProductConverter updProductConverter)
		{
			_participantDocFlowConverter =
				participantDocFlowConverter ?? throw new ArgumentNullException(nameof(participantDocFlowConverter));
			_updProductConverter = updProductConverter ?? throw new ArgumentNullException(nameof(updProductConverter));
		}
		
		public Fajl CreateNewUpdXml(Order order)
		{
			var org = order.Contract.Organization;

			//TODO уточнить нужно ли по дефолту брать логин Водовоза, как отправителя УПД
			var upd = new Fajl
			{
				VersForm = FajlVersForm.Item501,
				VersProg = "ВерсПрог",
				SvUchDokObor = new FajlSvUchDokObor
				{
					IdOtpr = org.TaxcomEdoAccountId, //ЭкоСовКод
					IdPol = /*order.Client.PersonalAccountIdInEdo*/"2AL-3978EE3E-C84E-49F7-A214-4D533028AAD9-00000" //ФинСофтХим
				}
			};

			var uniqueId = Guid.NewGuid().ToString("D").ToUpper();
			upd.IdFajl = $"ON_NSCHFDOPPRMARK_{upd.SvUchDokObor.IdPol}_{upd.SvUchDokObor.IdOtpr}_{order.DeliveryDate:yyyyMMdd}_{uniqueId}";
			
			upd.Dokument = new FajlDokument
			{
				Funkcija = FajlDokumentFunkcija.SChFDOP,
				PoFaktHZh = "Документ об отгрузке товаров (выполнении работ), передаче имущественных прав (документ об оказании услуг)",
				NaimDokOpr = "Счет-фактура и документ об отгрузке товаров (выполнении работ), передаче имущественных прав (документ об оказании услуг)",
				KND = FajlDokumentKND.Item1115131,
				DataInfPr = DateTime.Now.ToShortDateString(),
				VremInfPr = $"{DateTime.Now:HH.mm.ss}",
				NaimJekonSubSost = $"{org.Name}, ИНН/КПП {org.INN}/{org.KPP}",
				SvSchFakt = new FajlDokumentSvSchFakt
				{
					NomerSchF = order.Id.ToString(),
					DataSchF = order.DeliveryDate.Value.ToShortDateString(),
					KodOKV = "643",
					IsprSchF = new FajlDokumentSvSchFaktIsprSchF
					{
						DefNomIsprSchFSpecified = true,
						DefNomIsprSchF = FajlDokumentSvSchFaktIsprSchFDefNomIsprSchF.Item,
						DefDataIsprSchFSpecified = true,
						DefDataIsprSchF = FajlDokumentSvSchFaktIsprSchFDefDataIsprSchF.Item
					}
				}
			};

			//Сведения о продавце

			upd.Dokument.SvSchFakt.SvProd = new[]
			{
				_participantDocFlowConverter.ConvertOrganizationToUchastnikTip(org, order.DeliveryDate)
			};

			upd.Dokument.SvSchFakt.GruzOt = new[]
			{
				new FajlDokumentSvSchFaktGruzOt
				{
					Item = FajlDokumentSvSchFaktGruzOtOnZhe.onzhe
				}
			};
			
			//Сведения о покупателе
			upd.Dokument.SvSchFakt.SvPokup = new[]
			{
				_participantDocFlowConverter.ConvertCounterpartyToUchastnikTip(order.Client)
			};
			
			upd.Dokument.SvSchFakt.GruzPoluch = new[]
			{
				_participantDocFlowConverter.ConvertCounterpartyToUchastnikTip(order.Client, order.DeliveryPoint?.Id)
			};

			upd.Dokument.SvSchFakt.DokPodtvOtgr = new[]
			{
				new FajlDokumentSvSchFaktDokPodtvOtgr
				{
					NaimDokOtgr = "Документ об отгрузке товаров (выполнении работ), передаче имущественных прав (документ об оказании услуг)",
					NomDokOtgr = order.Id.ToString(),
					DataDokOtgr = order.DeliveryDate.Value.ToShortDateString()
				}
			};

			var taxesSum = order.OrderItems.Sum(x => x.IncludeNDS) ?? 0m;
			upd.Dokument.TablSchFakt = new FajlDokumentTablSchFakt
			{
				SvedTov = _updProductConverter.ConvertOrderItemsToUpdProducts(order.OrderItems),
				VsegoOpl = new FajlDokumentTablSchFaktVsegoOpl
				{
					StTovBezNDSVsego = order.OrderItems.Sum(x => x.SumWithoutVat),
					StTovBezNDSVsegoSpecified = true,
					StTovUchNalVsego = order.OrderSum,
					StTovUchNalVsegoSpecified = true,
					SumNalVsego = new SumNDSTip
					{
						Item = taxesSum == 0m ? SumNDSTipBezNDS.bezNDS : taxesSum
					}
				}
			};

			upd.Dokument.SvProdPer = new FajlDokumentSvProdPer
			{
				SvPer = new FajlDokumentSvProdPerSvPer
				{
					SodOper = GetOperationName(order.OrderItems),
					OsnPer = new[]
					{
						new OsnovanieTip
						{
							NaimOsn = "Без документа-основания"
						}
					}
				}
			};

			upd.Dokument.Podpisant = new[]
			{
				new FajlDokumentPodpisant
				{
					OblPoln = FajlDokumentPodpisantOblPoln.Item0,
					Status = FajlDokumentPodpisantStatus.Item1,
					OsnPoln = "Должностные обязанности",
					Item = new FajlDokumentPodpisantJuL
					{
						FIO = new FIOTip
						{
							Familija = "Шевалье",
							Imja = "Ефрем",
							Otchestvo = "Филимонович"
						},
						INNJuL = "8978118407",//org.INN,
						NaimOrg = "ООО \"ЭкоСовКод\"",//org.Name,
						Dolzhn = "Гл бух"
					}
				}
			};

			return upd;
		}

		private string GetOperationName(IList<OrderItem> orderItems)
		{
			var result = string.Empty;
			
			if(orderItems.Any(x =>
					x.Nomenclature.Category != NomenclatureCategory.service && x.Nomenclature.Category != NomenclatureCategory.master))
			{
				result = "Товары переданы";
			}

			if(orderItems.Any(x =>
					x.Nomenclature.Category == NomenclatureCategory.service || x.Nomenclature.Category == NomenclatureCategory.master))
			{
				result = !string.IsNullOrWhiteSpace(result)
					? string.Join(',', result, "услуги оказаны в полном объеме")
					: "Услуги оказаны в полном объеме";
			}
			
			return result;
		}
	}
}
