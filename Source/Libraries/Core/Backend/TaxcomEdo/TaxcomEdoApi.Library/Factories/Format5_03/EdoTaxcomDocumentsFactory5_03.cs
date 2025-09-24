using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DateTimeHelpers;
using Edo.Contracts.Messages.Dto;
using Taxcom.Client.Api.Document.DocumentByFormat1115131_5_03;
using TaxcomEdo.Contracts.Counterparties;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Contracts.Goods;
using TaxcomEdo.Contracts.Orders;
using TaxcomEdoApi.Library.Builders.Format5_03;
using TaxcomEdoApi.Library.Config;
using TaxcomEdoApi.Library.Converters.Format5_03;
using TISystems.TTC.Common;

namespace TaxcomEdoApi.Library.Factories.Format5_03
{
	public class EdoTaxcomDocumentsFactory5_03 : IEdoTaxcomDocumentsFactory5_03
	{
		private const string _productsShipped = "Товары переданы";
		private const string _servicesHaveBeenProvided = "Услуги оказаны в полном объеме";
		private const string _jobResponsibilities = "Должностные обязанности";
		private const string _upd = "Универсальный передаточный документ";
		
		private readonly IParticipantDocFlowConverter5_03 _participantDocFlowConverter;
		private readonly IErpDocumentInfoConverter5_03 _erpDocumentInfoConverter;
		private readonly IUpdProductConverter5_03 _updProductConverter;

		public EdoTaxcomDocumentsFactory5_03(
			IParticipantDocFlowConverter5_03 participantDocFlowConverter,
			IErpDocumentInfoConverter5_03 erpDocumentInfoConverter,
			IUpdProductConverter5_03 updProductConverter)
		{
			_participantDocFlowConverter =
				participantDocFlowConverter ?? throw new ArgumentNullException(nameof(participantDocFlowConverter));
			_erpDocumentInfoConverter = erpDocumentInfoConverter ?? throw new ArgumentNullException(nameof(erpDocumentInfoConverter));
			_updProductConverter = updProductConverter ?? throw new ArgumentNullException(nameof(updProductConverter));
		}

		#region УПД

		public Fajl CreateUpdXml5_03(
			InfoForCreatingEdoUpd infoForCreatingEdoUpd,
			WarrantOptions warrantOptions,
			string organizationAccountId,
			string certificateSubject)
		{
			var orderInfoForEdo = infoForCreatingEdoUpd.OrderInfoForEdo;
			var org = orderInfoForEdo.ContractInfoForEdo.OrganizationInfoForEdo;
			var updDate = orderInfoForEdo.DeliveryDate.ToEdoShortDateString();
			
			var upd = new Fajl
			{
				VersForm = FajlVersForm.Item503,
				VersProg = "ВерсПрог",
				SvUchDokObor = new FajlSvUchDokObor
				{
					IdOtpr = organizationAccountId,
					IdPol = orderInfoForEdo.CounterpartyInfoForEdo.PersonalAccountIdInEdo
				}
			};

			var hasMarkGoods = orderInfoForEdo.OrderItems.Any(x => !string.IsNullOrWhiteSpace(x.NomenclatureInfoForEdo.Gtin));

			var updNameBuilder =
				EdoSellerUpdNameBuilder5_03
					.Create()
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
					NomerDoc = orderInfoForEdo.Id.ToString(),
					DataDoc = updDate,
					
					DenIzm = new FajlDokumentSvSchFaktDenIzm
					{
						KodOKV = "643",
						NaimОКВ = "Российский рубль"
					},
					
					//Сведения о продавце
					SvProd = new[]
					{
						_participantDocFlowConverter.ConvertOrganizationToUchastnikTip(org)
					},
					
					//Грузоотправитель
					GruzOt = new[]
					{
						new FajlDokumentSvSchFaktGruzOt
						{
							Item = FajlDokumentSvSchFaktGruzOtOnZhe.onzhe
						}
					},
					
					//Сведения о покупателе
					SvPokup = new[]
					{
						_participantDocFlowConverter.ConvertCounterpartyToUchastnikTip(orderInfoForEdo.CounterpartyInfoForEdo)
					},
					
					//Грузополучатель
					GruzPoluch = new[]
					{
						_participantDocFlowConverter.ConvertCounterpartyToUchastnikTip(
							orderInfoForEdo.CounterpartyInfoForEdo, orderInfoForEdo.DeliveryPointInfoForEdo)
					}
				}
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
					NaimDokOtgr = _upd,
					NomDokOtgr = orderInfoForEdo.Id.ToString(),
					DataDokOtgr = updDate
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
					DataPer = updDate,
					
					Items = new[]
					{
						GetDocumentConfirmingShipment(orderInfoForEdo)
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
					SposPodtPolnom = FajlDokumentPodpisantSposPodtPolnom.Item1,
					DataPodDok = updDate,
					OsnPoln = _jobResponsibilities,
					Dolzn = warrantOptions.JobPosition,
					FIO = new FIOTip
					{
						Familija = certDetails.SurName,
						Imja = firstNameAndPatronymic[0],
						Otchestvo = patronymic
					},
				}
			};

			return upd;
		}

		public Fajl CreateUpdXml5_03(
			UniversalTransferDocumentInfo updInfo,
			WarrantOptions warrantOptions,
			string organizationAccountId,
			string certificateSubject)
		{
			var org = updInfo.Seller.Organization;
			var updDate = updInfo.Date.ToShortDateString();
			
			var upd = new Fajl
			{
				VersForm = FajlVersForm.Item503,
				VersProg = "ВерсПрог",
				SvUchDokObor = new FajlSvUchDokObor
				{
					IdOtpr = organizationAccountId,
					IdPol = updInfo.Customer.Organization.EdoAccountId
				}
			};

			var hasMarkGoods = updInfo.Products.Any(x => x.TrueMarkCodes.Any());

			var updNameBuilder =
				EdoSellerUpdNameBuilder5_03
					.Create()
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
				NaimDokOpr = _upd,
				KND = FajlDokumentKND.Item1115131,
				DataInfPr = DateTime.Now.ToShortDateString(),
				VremInfPr = $"{DateTime.Now:HH.mm.ss}",
				NaimJekonSubSost = $"{org.Name}, ИНН/КПП {org.Inn}/{org.Kpp}",
				SvSchFakt = new FajlDokumentSvSchFakt
				{
					NomerDoc = updInfo.Number.ToString(),
					DataDoc = updDate,

					DenIzm = new FajlDokumentSvSchFaktDenIzm
					{
						KodOKV = "643",
						NaimОКВ = "Российский рубль"
					},

					//Сведения о продавце
					SvProd = new[]
					{
						_erpDocumentInfoConverter.ConvertOrganizationToSellerInfo(org)
					},

					//Грузоотправитель
					GruzOt = new[]
					{
						new FajlDokumentSvSchFaktGruzOt
						{
							Item = FajlDokumentSvSchFaktGruzOtOnZhe.onzhe
						}
					},

					//Сведения о покупателе
					SvPokup = new[]
					{
						_erpDocumentInfoConverter.ConvertCounterpartyToCustomerInfo(updInfo.Customer)
					},

					//Грузополучатель
					GruzPoluch = new[]
					{
						_erpDocumentInfoConverter.ConvertCounterpartyToConsigneeInfo(updInfo.Consignee)
					},

					// Идентификатор государственного контракта, договора
					DopSvFHZh1 = new FajlDokumentSvSchFaktDopSvFHZh1
					{
						IdGosKon = updInfo.GovContract
					}

				}
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

			upd.Dokument.SvSchFakt.DocPodtvOtgrNom = new[]
			{
				new RekvDocTip
				{
					RekvNaimDoc = _upd,
					RekvNomDoc = updInfo.Number.ToString(),
					RekvDataDoc = updDate
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
					DataPer = updDate,

					Items = new[]
					{
						GetDocumentConfirmingShipment(updInfo.DocumentConfirmingShipment)
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
			
			//Уточнить насчет даты подписания документа в случае переотправки в другой день
			upd.Dokument.Podpisant = new[]
			{
				new FajlDokumentPodpisant
				{
					SposPodtPolnom = FajlDokumentPodpisantSposPodtPolnom.Item1,
					DataPodDok = updDate,
					OsnPoln = _jobResponsibilities,
					Dolzn = warrantOptions.JobPosition,
					FIO = new FIOTip
					{
						Familija = certDetails.SurName,
						Imja = firstNameAndPatronymic[0],
						Otchestvo = patronymic
					},
				}
			};

			return upd;
		}

		private object GetDocumentConfirmingShipment(OrderInfoForEdo orderInfoForEdo)
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
			
			if(orderInfoForEdo.CounterpartyInfoForEdo.UseSpecialDocFields
				&& !string.IsNullOrWhiteSpace(orderInfoForEdo.CounterpartyInfoForEdo.SpecialContractName))
			{
				return FajlDokumentSvProdPerBezDocOsnPer.Item1;
			}

			if(orderInfoForEdo.ContractInfoForEdo != null)
			{
				basis.NaimOsn = "Договор";
				basis.NomOsn = orderInfoForEdo.ContractInfoForEdo.Number;
				basis.DataOsn = $"{orderInfoForEdo.ContractInfoForEdo.IssueDate:dd.MM.yyyy}";
				
				return basis;
			}

			return FajlDokumentSvProdPerBezDocOsnPer.Item1;
		}
		
		private object GetDocumentConfirmingShipment(DocumentConfirmingShipmentInfo documentConfirmingShipmentInfo)
		{
			if(string.IsNullOrWhiteSpace(documentConfirmingShipmentInfo.Number))
			{
				return FajlDokumentSvProdPerBezDocOsnPer.Item1;
			}

			return new RekvDocTip
			{
				RekvNaimDoc = documentConfirmingShipmentInfo.Document,
				RekvNomDoc = documentConfirmingShipmentInfo.Number,
				RekvDataDoc = documentConfirmingShipmentInfo.Date
			};
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
		
		#endregion УПД
	}
}
