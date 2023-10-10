using System;
using System.Collections.Generic;
using System.Linq;
using Taxcom.Client.Api.Document.DocumentByFormat1115131;
using TaxcomEdoApi.Converters;
using TISystems.TTC.Common;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace TaxcomEdoApi.Factories
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
		
		public Fajl CreateNewUpdXml(Order order, string organizationAccountId, string certificateSubject)
		{
			var org = order.Contract.Organization;
			
			var upd = new Fajl
			{
				VersForm = FajlVersForm.Item501,
				VersProg = "ВерсПрог",
				SvUchDokObor = new FajlSvUchDokObor
				{
					IdOtpr = organizationAccountId,
					IdPol = order.Client.PersonalAccountIdInEdo
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

			//Грузоотправитель
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
			
			//Грузополучатель
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

			var tekstInfTipList = new List<TekstInfTip>();

			if(order.Client.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds)
			{
				tekstInfTipList.Add(
					new TekstInfTip
					{
						Identif = "СвВыбытияМАРК",
						Znachen = "1"
					}
				);
			}

			if(order.CounterpartyExternalOrderId != null && order.Client.UseSpecialDocFields)
			{
				tekstInfTipList.Add(
					new TekstInfTip
					{
						Identif = "номер_заказа",
						Znachen = $"N{order.CounterpartyExternalOrderId}"
					}
				);
			}

			if(tekstInfTipList.Any())
			{
				upd.Dokument.SvSchFakt.InfPolFHZh1 = new FajlDokumentSvSchFaktInfPolFHZh1
				{
					TekstInf = tekstInfTipList.ToArray()
				};
			}

			var orderItems = order.OrderItems.Where(x => x.CurrentCount > 0).ToList();

			var taxesSum = orderItems.Sum(x => x.IncludeNDS) ?? 0m;
			upd.Dokument.TablSchFakt = new FajlDokumentTablSchFakt
			{
				SvedTov = _updProductConverter.ConvertOrderItemsToUpdProducts(orderItems),
				VsegoOpl = new FajlDokumentTablSchFaktVsegoOpl
				{
					StTovBezNDSVsego = orderItems.Sum(x => x.SumWithoutVat),
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
					SodOper = GetOperationName(orderItems),
					OsnPer = new[]
					{
						GetBasis(order)
					}
				}
			};
			
			var certDetails = new CertificateParser().ParseCertificate(certificateSubject, Guid.NewGuid());
			var firstNameAndPatronymic = certDetails.GivenName.Split(' ');
			
			upd.Dokument.Podpisant = new[]
			{
				new FajlDokumentPodpisant
				{
					OblPoln = FajlDokumentPodpisantOblPoln.Item5,
					Status = FajlDokumentPodpisantStatus.Item1,
					OsnPoln = "Должностные обязанности",
					Item = new FajlDokumentPodpisantJuL
					{
						FIO = new FIOTip
						{
							Familija = certDetails.SurName,
							Imja = firstNameAndPatronymic[0],
							Otchestvo = firstNameAndPatronymic[1]
						},
						INNJuL = org.INN,
						NaimOrg = org.Name,
						Dolzhn = "Главный бухгалтер"
					}
				}
			};

			return upd;
		}

		private OsnovanieTip GetBasis(Order order)
		{
			var basis = new OsnovanieTip();

			if(order.Client.UseSpecialDocFields
				&& !string.IsNullOrWhiteSpace(order.Client.SpecialContractName)
				&& !string.IsNullOrWhiteSpace(order.Client.SpecialContractNumber)
				&& order.Client.SpecialContractDate.HasValue)
			{
				basis.NaimOsn = order.Client.SpecialContractName;
				basis.NomOsn = order.Client.SpecialContractNumber;
				basis.DataOsn = $"{order.Client.SpecialContractDate.Value:dd.MM.yyyy}";
				return basis;
			}
			if(order.Client.UseSpecialDocFields && !string.IsNullOrWhiteSpace(order.Client.SpecialContractName))
			{
				basis.NaimOsn = "Без документа-основания";
				return basis;
			}

			if(order.Contract != null)
			{
				basis.NaimOsn = "Договор";
				basis.NomOsn = order.Contract.Number;
				basis.DataOsn = $"{order.Contract.IssueDate:dd.MM.yyyy}";
			}
			else
			{
				basis.NaimOsn = "Без документа-основания";
			}

			return basis;
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
