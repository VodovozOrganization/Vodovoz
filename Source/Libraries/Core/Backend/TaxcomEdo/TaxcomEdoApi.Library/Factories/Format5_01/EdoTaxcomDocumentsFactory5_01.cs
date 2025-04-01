using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Edo.Contracts.Messages.Dto;
using Taxcom.Client.Api.Document.DocumentByFormat1115131;
using TaxcomEdo.Contracts.Counterparties;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Contracts.Goods;
using TaxcomEdo.Contracts.Orders;
using TaxcomEdoApi.Library.Builders;
using TaxcomEdoApi.Library.Builders.Format5_01;
using TaxcomEdoApi.Library.Config;
using TaxcomEdoApi.Library.Converters;
using TaxcomEdoApi.Library.Converters.Format5_01;
using TISystems.TTC.Common;
using Fajl = Taxcom.Client.Api.Document.DocumentByFormat1115131.Fajl;
using FajlSvUchDokObor = Taxcom.Client.Api.Document.DocumentByFormat1115131.FajlSvUchDokObor;
using FajlVersForm = Taxcom.Client.Api.Document.DocumentByFormat1115131.FajlVersForm;
using FIOTip = Taxcom.Client.Api.Document.DocumentByFormat1115131.FIOTip;

namespace TaxcomEdoApi.Library.Factories.Format5_01
{
	public class EdoTaxcomDocumentsFactory5_01 : IEdoTaxcomDocumentsFactory5_01
	{
		private const string _productsShipped = "Товары переданы";
		private const string _servicesHaveBeenProvided = "Услуги оказаны в полном объеме";
		private const string _jobResponsibilities = "Должностные обязанности";
		
		private readonly IParticipantDocFlowConverter5_01 _participantDocFlowConverter;
		private readonly IErpDocumentInfoConverter5_01 _erpDocumentInfoConverter;
		private readonly IUpdProductConverter5_01 _updProductConverter;

		public EdoTaxcomDocumentsFactory5_01(
			IParticipantDocFlowConverter5_01 participantDocFlowConverter,
			IErpDocumentInfoConverter5_01 erpDocumentInfoConverter,
			IUpdProductConverter5_01 updProductConverter)
		{
			_participantDocFlowConverter =
				participantDocFlowConverter ?? throw new ArgumentNullException(nameof(participantDocFlowConverter));
			_erpDocumentInfoConverter = erpDocumentInfoConverter ?? throw new ArgumentNullException(nameof(erpDocumentInfoConverter));
			_updProductConverter = updProductConverter ?? throw new ArgumentNullException(nameof(updProductConverter));
		}

		#region УПД

		public Fajl CreateNewUpdXml5_01(
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

			var hasMarkGoods = orderInfoForEdo.OrderItems.Any(x => !string.IsNullOrWhiteSpace(x.NomenclatureInfoForEdo.Gtin));

			var updNameBuilder =
				EdoSellerUpdNameBuilder5_01.Create()
					.ReceiverId(upd.SvUchDokObor.IdPol)
					.SenderId(upd.SvUchDokObor.IdOtpr)
					.Date(orderInfoForEdo.DeliveryDate);

			if(hasMarkGoods)
			{
				updNameBuilder.ControlMarkGoods();
			}

			upd.IdFajl = updNameBuilder.ToString();

			upd.Dokument = new FajlDokument
			{
				Funkcija = FajlDokumentFunkcija.SChFDOP,
				PoFaktHZh = "Документ об отгрузке товаров (выполнении работ), передаче имущественных прав (документ об оказании услуг)",
				NaimDokOpr = "Счет-фактура и документ об отгрузке товаров (выполнении работ), передаче имущественных прав (документ об оказании услуг)",
				KND = FajlDokumentKND.Item1115131,
				DataInfPr = DateTime.Now.ToShortDateString(),
				VremInfPr = $"{DateTime.Now:HH.mm.ss}",
				NaimJekonSubSost = $"{org.Name}, ИНН/КПП {org.Inn}/{org.Kpp}",
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
				_participantDocFlowConverter.ConvertOrganizationToUchastnikTip(org)
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
					NaimDokOtgr = "Универсальный передаточный документ,",
					NomDokOtgr = orderInfoForEdo.Id.ToString(),
					DataDokOtgr = orderInfoForEdo.DeliveryDate.ToShortDateString()
				}
			};

			var tekstInfTipList = new List<TekstInfTip>();

			if(hasMarkGoods
				&& orderInfoForEdo.CounterpartyInfoForEdo.ReasonForLeaving == ReasonForLeavingType.ForOwnNeeds)
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
					OsnPer = new []{ GetBasis(orderInfoForEdo) },
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
					OsnPoln = _jobResponsibilities,
					Item = new FajlDokumentPodpisantJuL
					{
						FIO = new FIOTip
						{
							Familija = certDetails.SurName,
							Imja = firstNameAndPatronymic[0],
							Otchestvo = patronymic
						},
						INNJuL = org.Inn,
						NaimOrg = org.Name,
						Dolzhn = warrantOptions.JobPosition
					}
				}
			};

			return upd;
		}

		public Fajl CreateNewUpdXml5_01(
			UniversalTransferDocumentInfo updInfo,
			WarrantOptions warrantOptions,
			string organizationAccountId,
			string certificateSubject)
		{
			var org = updInfo.Seller.Organization;
			
			var upd = new Fajl
			{
				VersForm = FajlVersForm.Item501,
				VersProg = "ВерсПрог",
				SvUchDokObor = new FajlSvUchDokObor
				{
					IdOtpr = organizationAccountId,
					IdPol = updInfo.Customer.Organization.EdoAccountId
				}
			};

			var hasMarkGoods = updInfo.Products.Any(x => x.TrueMarkCodes.Any());

			var updNameBuilder =
				EdoSellerUpdNameBuilder5_01.Create()
					.ReceiverId(upd.SvUchDokObor.IdPol)
					.SenderId(upd.SvUchDokObor.IdOtpr)
					.Date(updInfo.Date);

			if(hasMarkGoods)
			{
				updNameBuilder.ControlMarkGoods();
			}
				
			upd.IdFajl = updNameBuilder.ToString();
			
			upd.Dokument = new FajlDokument
			{
				Funkcija = FajlDokumentFunkcija.SChFDOP,
				PoFaktHZh = "Документ об отгрузке товаров (выполнении работ), передаче имущественных прав (документ об оказании услуг)",
				NaimDokOpr = "Счет-фактура и документ об отгрузке товаров (выполнении работ), передаче имущественных прав (документ об оказании услуг)",
				KND = FajlDokumentKND.Item1115131,
				DataInfPr = DateTime.Now.ToShortDateString(),
				VremInfPr = $"{DateTime.Now:HH.mm.ss}",
				NaimJekonSubSost = $"{org.Name}, ИНН/КПП {org.Inn}/{org.Kpp}",
				SvSchFakt = new FajlDokumentSvSchFakt
				{
					NomerSchF = updInfo.Number.ToString(),
					DataSchF = updInfo.Date.ToShortDateString(),
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
				_erpDocumentInfoConverter.ConvertOrganizationToSellerInfo(org)
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
				_erpDocumentInfoConverter.ConvertCounterpartyToCustomerInfo(updInfo.Customer)
			};
			
			//Грузополучатель
			upd.Dokument.SvSchFakt.GruzPoluch = new[]
			{
				_erpDocumentInfoConverter.ConvertCounterpartyToConsigneeInfo(updInfo.Consignee)
			};

			//К платежно-расчетному документу
			if(updInfo.Payments.Any())
			{
				upd.Dokument.SvSchFakt.SvPRD = updInfo.Payments
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
					NaimDokOtgr = "Универсальный передаточный документ,",
					NomDokOtgr = updInfo.Number.ToString(),
					DataDokOtgr = updInfo.Date.ToShortDateString()
				}
			};

			if(updInfo.AdditionalInformation.Any())
			{
				upd.Dokument.SvSchFakt.InfPolFHZh1 = new FajlDokumentSvSchFaktInfPolFHZh1
				{
					TekstInf = updInfo.AdditionalInformation.Select(
						x => new TekstInfTip
							{
								Identif = x.Id,
								Znachen = x.Value
							})
						.ToArray()
				};
			}

			var products = updInfo.Products.Where(x => x.Count > 0).ToList();

			var taxesSum = products.Sum(x => x.IncludeVat);
			upd.Dokument.TablSchFakt = new FajlDokumentTablSchFakt
			{
				SvedTov = _updProductConverter.ConvertProductsToUpdProducts(products),
				VsegoOpl = new FajlDokumentTablSchFaktVsegoOpl
				{
					StTovBezNDSVsego = products.Sum(x => x.SumWithoutVat),
					StTovBezNDSVsegoSpecified = true,
					StTovUchNalVsego = updInfo.Sum,
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
					SodOper = GetOperationName(products),
					DataPer = updInfo.Date.ToShortDateString(),
					OsnPer = new OsnovanieTip[]
					{
						new()
						{
							NaimOsn = updInfo.DocumentConfirmingShipment.Document,
							NomOsn = updInfo.DocumentConfirmingShipment.Number,
							DataOsn = updInfo.DocumentConfirmingShipment.Date
						}
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
					OsnPoln = _jobResponsibilities,
					Item = new FajlDokumentPodpisantJuL
					{
						FIO = new FIOTip
						{
							Familija = certDetails.SurName,
							Imja = firstNameAndPatronymic[0],
							Otchestvo = patronymic
						},
						INNJuL = org.Inn,
						NaimOrg = org.Name,
						Dolzhn = warrantOptions.JobPosition
					}
				}
			};

			return upd;
		}

		#endregion

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
				x.NomenclatureInfoForEdo.Category != NomenclatureInfoCategory.Service
				&& x.NomenclatureInfoForEdo.Category != NomenclatureInfoCategory.Master))
			{
				result = _productsShipped;
			}

			if(orderItems.Any(x =>
				x.NomenclatureInfoForEdo.Category == NomenclatureInfoCategory.Service
				|| x.NomenclatureInfoForEdo.Category == NomenclatureInfoCategory.Master))
			{
				result = !string.IsNullOrWhiteSpace(result)
					? string.Join(',', result, _servicesHaveBeenProvided.ToLower())
					: _servicesHaveBeenProvided;
			}
			
			return result;
		}
		
		private string GetOperationName(IEnumerable<ProductInfo> products)
		{
			var sb = new StringBuilder();
			
			if(products.Any(x => !x.IsService))
			{
				sb.Append(_productsShipped);
			}

			if(products.Any(x => x.IsService))
			{
				if(sb.Length > 0)
				{
					sb.Append(", ");
					sb.Append(_servicesHaveBeenProvided.ToLower());
				}
				else
				{
					sb.Append(_servicesHaveBeenProvided);
				}
			}
			
			return sb.ToString();
		}
	}
}
