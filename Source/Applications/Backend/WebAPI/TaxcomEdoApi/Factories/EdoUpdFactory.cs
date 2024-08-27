﻿using System;
using System.Collections.Generic;
using System.Linq;
using Taxcom.Client.Api.Document.DocumentByFormat1115131;
using TaxcomEdoApi.Config;
using TaxcomEdoApi.Converters;
using TISystems.TTC.Common;
using Vodovoz.Core.Data.Documents;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Goods;

namespace TaxcomEdoApi.Factories
{
	public class EdoUpdFactory : IEdoUpdFactory
	{
		private readonly IParticipantDocFlowConverter _participantDocFlowConverter;
		private readonly IUpdProductConverter _updProductConverter;

		public EdoUpdFactory(
			IParticipantDocFlowConverter participantDocFlowConverter,
			IUpdProductConverter updProductConverter)
		{
			_participantDocFlowConverter =
				participantDocFlowConverter ?? throw new ArgumentNullException(nameof(participantDocFlowConverter));
			_updProductConverter = updProductConverter ?? throw new ArgumentNullException(nameof(updProductConverter));
		}
		
		public Fajl CreateNewUpdXml(
			InfoForCreatingEdoUpd infoForCreatingEdoUpd,
			WarrantOptions warrantOptions,
			string organizationAccountId,
			string certificateSubject)
		{
			var orderInfoForEdo = infoForCreatingEdoUpd.OrderInfoForEdo;
			var org = orderInfoForEdo.ContractInfoForEdo.OrganizationInfoForEdo;
			
			var upd = new Fajl
			{
				VersForm = FajlVersForm.Item501,
				VersProg = "ВерсПрог",
				SvUchDokObor = new FajlSvUchDokObor
				{
					IdOtpr = organizationAccountId,
					IdPol = orderInfoForEdo.CounterpartyInfoForEdo.PersonalAccountIdInEdo
				}
			};

			var uniqueId = Guid.NewGuid().ToString("D").ToUpper();
			upd.IdFajl = $"ON_NSCHFDOPPRMARK_{upd.SvUchDokObor.IdPol}_{upd.SvUchDokObor.IdOtpr}_{orderInfoForEdo.DeliveryDate:yyyyMMdd}_{uniqueId}";
			
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
					NomerSchF = orderInfoForEdo.Id.ToString(),
					DataSchF = orderInfoForEdo.DeliveryDate.ToShortDateString(),
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
				_participantDocFlowConverter.ConvertOrganizationToUchastnikTip(org, orderInfoForEdo.DeliveryDate)
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
				_participantDocFlowConverter.ConvertCounterpartyToUchastnikTip(orderInfoForEdo.CounterpartyInfoForEdo)
			};
			
			//Грузополучатель
			upd.Dokument.SvSchFakt.GruzPoluch = new[]
			{
				_participantDocFlowConverter.ConvertCounterpartyToUchastnikTip(
					orderInfoForEdo.CounterpartyInfoForEdo, orderInfoForEdo.DeliveryPointInfoForEdo)
			};

			//К платежно-расчетному документу
			if(infoForCreatingEdoUpd.PaymentsInfoForEdo.Any())
			{
				upd.Dokument.SvSchFakt.SvPRD = infoForCreatingEdoUpd.PaymentsInfoForEdo
					.Select(p => new FajlDokumentSvSchFaktSvPRD
					{
						NomerPRD = p.PaymentNum,
						DataPRD = p.PaymentDate
					})
					.ToArray();
			}

			upd.Dokument.SvSchFakt.DokPodtvOtgr = new[]
			{
				new FajlDokumentSvSchFaktDokPodtvOtgr
				{
					NaimDokOtgr = "Документ об отгрузке товаров (выполнении работ), передаче имущественных прав (документ об оказании услуг)",
					NomDokOtgr = orderInfoForEdo.Id.ToString(),
					DataDokOtgr = orderInfoForEdo.DeliveryDate.ToShortDateString()
				}
			};

			var tekstInfTipList = new List<TekstInfTip>();

			if(orderInfoForEdo.CounterpartyInfoForEdo.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds)
			{
				tekstInfTipList.Add(
					new TekstInfTip
					{
						Identif = "СвВыбытияМАРК",
						Znachen = "1"
					}
				);
			}

			if(orderInfoForEdo.CounterpartyExternalOrderId != null && orderInfoForEdo.CounterpartyInfoForEdo.UseSpecialDocFields)
			{
				tekstInfTipList.Add(
					new TekstInfTip
					{
						Identif = "номер_заказа",
						Znachen = $"N{orderInfoForEdo.CounterpartyExternalOrderId}"
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

			var orderItems = orderInfoForEdo.OrderItems.Where(x => x.CurrentCount > 0).ToList();

			var taxesSum = orderItems.Sum(x => x.IncludeNDS) ?? 0m;
			upd.Dokument.TablSchFakt = new FajlDokumentTablSchFakt
			{
				SvedTov = _updProductConverter.ConvertOrderItemsToUpdProducts(orderItems, orderInfoForEdo.CounterpartyInfoForEdo.SpecialNomenclatures),
				VsegoOpl = new FajlDokumentTablSchFaktVsegoOpl
				{
					StTovBezNDSVsego = orderItems.Sum(x => x.SumWithoutVat),
					StTovBezNDSVsegoSpecified = true,
					StTovUchNalVsego = orderInfoForEdo.OrderSum,
					StTovUchNalVsegoSpecified = true,
					SumNalVsego = new SumNDSTip
					{
						Item = taxesSum == 0m ? SumNDSTipBezNDS.bezNDS : taxesSum
					}
				}
			};

			var certDetails = new CertificateParser().ParseCertificate(certificateSubject, Guid.NewGuid());
			var firstNameAndPatronymic = certDetails.GivenName.Split(' ');
			var patronymic = firstNameAndPatronymic.Length == 2 ? firstNameAndPatronymic[1] : null;

			upd.Dokument.SvProdPer = new FajlDokumentSvProdPer
			{
				SvPer = new FajlDokumentSvProdPerSvPer
				{
					SodOper = GetOperationName(orderItems),
					OsnPer = new[]
					{
						GetBasis(orderInfoForEdo)
					},
					SvLicPer = new FajlDokumentSvProdPerSvPerSvLicPer
					{
						Item = new FajlDokumentSvProdPerSvPerSvLicPerRabOrgProd
						{
							FIO = new FIOTip
							{
								Familija = certDetails.SurName,
								Imja = firstNameAndPatronymic[0],
								Otchestvo = patronymic
							},
							Dolzhnost = warrantOptions.JobPosition
						}
					}
				}
			};
			
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
							Otchestvo = patronymic
						},
						INNJuL = org.INN,
						NaimOrg = org.Name,
						Dolzhn = warrantOptions.JobPosition
					}
				}
			};

			return upd;
		}

		private OsnovanieTip GetBasis(OrderInfoForEdo orderInfoForEdo)
		{
			var basis = new OsnovanieTip();

			if(orderInfoForEdo.CounterpartyInfoForEdo.UseSpecialDocFields
				&& !string.IsNullOrWhiteSpace(orderInfoForEdo.CounterpartyInfoForEdo.SpecialContractName)
				&& !string.IsNullOrWhiteSpace(orderInfoForEdo.CounterpartyInfoForEdo.SpecialContractNumber)
				&& orderInfoForEdo.CounterpartyInfoForEdo.SpecialContractDate.HasValue)
			{
				basis.NaimOsn = orderInfoForEdo.CounterpartyInfoForEdo.SpecialContractName;
				basis.NomOsn = orderInfoForEdo.CounterpartyInfoForEdo.SpecialContractNumber;
				basis.DataOsn = $"{orderInfoForEdo.CounterpartyInfoForEdo.SpecialContractDate.Value:dd.MM.yyyy}";
				return basis;
			}
			if(orderInfoForEdo.CounterpartyInfoForEdo.UseSpecialDocFields && !string.IsNullOrWhiteSpace(orderInfoForEdo.CounterpartyInfoForEdo.SpecialContractName))
			{
				basis.NaimOsn = "Без документа-основания";
				return basis;
			}

			if(orderInfoForEdo.ContractInfoForEdo != null)
			{
				basis.NaimOsn = "Договор";
				basis.NomOsn = orderInfoForEdo.ContractInfoForEdo.Number;
				basis.DataOsn = $"{orderInfoForEdo.ContractInfoForEdo.IssueDate:dd.MM.yyyy}";
			}
			else
			{
				basis.NaimOsn = "Без документа-основания";
			}

			return basis;
		}

		private string GetOperationName(IList<OrderItemInfoForEdo> orderItems)
		{
			var result = string.Empty;
			
			if(orderItems.Any(x =>
					x.NomenclatureInfoForEdo.Category != NomenclatureCategory.service && x.NomenclatureInfoForEdo.Category != NomenclatureCategory.master))
			{
				result = "Товары переданы";
			}

			if(orderItems.Any(x =>
					x.NomenclatureInfoForEdo.Category == NomenclatureCategory.service || x.NomenclatureInfoForEdo.Category == NomenclatureCategory.master))
			{
				result = !string.IsNullOrWhiteSpace(result)
					? string.Join(',', result, "услуги оказаны в полном объеме")
					: "Услуги оказаны в полном объеме";
			}
			
			return result;
		}
	}
}
