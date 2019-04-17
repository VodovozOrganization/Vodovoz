using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Repository;
using Vodovoz.Repository.Client;

namespace Vodovoz.Repositories.Client
{
	public class ClientDocumentsRepository
	{
		/// <summary>
		/// Создает договор с заданными параметрами
		/// </summary>
		public static CounterpartyContract CreateDefaultContract(IUnitOfWork UoW, Counterparty client, PaymentType paymentType, DateTime? issueDate)
		{
			var contractType = DocTemplateRepository.GetContractTypeForPaymentType(client.PersonType, paymentType);
			CounterpartyContract result;
			using(var uow = CounterpartyContract.Create(client)) {
				var contract = uow.Root;
				var org = OrganizationRepository.GetOrganizationByPaymentType(uow, client.PersonType, paymentType);
				contract.Organization = org;
				contract.IsArchive = false;
				contract.ContractType = contractType;
				if(issueDate.HasValue) {
					contract.IssueDate = issueDate.Value;
				}
				contract.AdditionalAgreements = new List<AdditionalAgreement>();
				uow.Save();
				result = UoW.GetById<CounterpartyContract>(uow.Root.Id);
			}
			return result;
		}

		/// <summary>
		/// Создает дополнительное соглашение для доставки воды
		/// </summary>
		public static WaterSalesAgreement CreateDefaultWaterAgreement(IUnitOfWork UoW, DeliveryPoint deliveryPoint, DateTime? deliveryDate, CounterpartyContract contract)
		{
			WaterSalesAgreement result = null;
			using(var uow = WaterSalesAgreement.Create(contract)) {
				uow.Root.DeliveryPoint = deliveryPoint;
				uow.Root.FillFixedPricesFromDeliveryPoint(uow);
				if(deliveryDate.HasValue) {
					uow.Root.IssueDate = deliveryDate.Value;
					uow.Root.StartDate = deliveryDate.Value;
				}
				uow.Save();
				result = UoW.GetById<WaterSalesAgreement>(uow.Root.Id);
			}
			return result;
		}

		/// <summary>
		/// Создает дополнительное соглашение для посуточной аренды оборудования
		/// </summary>
		public static DailyRentAgreement CreateDefaultDailyRentAgreement(IUnitOfWork UoW, 
		                                                                     DeliveryPoint deliveryPoint, 
		                                                                     DateTime? deliveryDate, 
		                                                                     CounterpartyContract contract,
		                                                                     List<PaidRentEquipment> equipments,
		                                                                     int rentDays)
		{
			if(equipments.Count == 0) {
				throw new ArgumentException("При автоматическом создании дополнительного соглашения " +
				                            "аренды оборудования, список должен иметь оборудование для аренды");
			}
			DailyRentAgreement result = null;
			using(var uow = DailyRentAgreement.Create(contract)) {
				uow.Root.DeliveryPoint = deliveryPoint;
				if(deliveryDate.HasValue) {
					uow.Root.IssueDate = deliveryDate.Value;
					uow.Root.StartDate = deliveryDate.Value;
				}
				uow.Root.RentDays = rentDays;
				foreach(var item in equipments) {
					uow.Root.ObservableEquipment.Add( new PaidRentEquipment(){
						Count = item.Count,
						Deposit = item.Deposit,
						Price = item.Price,
						Equipment = item.Equipment,
						IsNew = item.IsNew,
						Nomenclature = item.Nomenclature,
						PaidRentPackage = item.PaidRentPackage
					});
				}
				uow.Save();
				result = UoW.GetById<DailyRentAgreement>(uow.Root.Id);
			}
			return result;
		}

		/// <summary>
		/// Создает дополнительное соглашение для долгосрочной аренды оборудования
		/// </summary>
		public static NonfreeRentAgreement CreateDefaultNonfreeRentAgreement(IUnitOfWork UoW,
																			 DeliveryPoint deliveryPoint,
																			 DateTime? deliveryDate,
																			 CounterpartyContract contract,
																			 List<PaidRentEquipment> equipments,
																			 int rentMonths)
		{
			if(equipments.Count == 0) {
				throw new ArgumentException("При автоматическом создании дополнительного соглашения " +
											"аренды оборудования, список должен иметь оборудование для аренды");
			}
			NonfreeRentAgreement result = null;
			using(var uow = NonfreeRentAgreement.Create(contract)) {
				uow.Root.DeliveryPoint = deliveryPoint;
				if(deliveryDate.HasValue) {
					uow.Root.IssueDate = deliveryDate.Value;
					uow.Root.StartDate = deliveryDate.Value;
				}
				uow.Root.RentMonths = rentMonths;
				foreach(var item in equipments) {
					uow.Root.ObservableEquipment.Add(new PaidRentEquipment() {
						Count = item.Count,
						Deposit = item.Deposit,
						Price = item.Price,
						Equipment = item.Equipment,
						IsNew = item.IsNew,
						Nomenclature = item.Nomenclature,
						PaidRentPackage = item.PaidRentPackage
					});
				}
				uow.Save();
				result = UoW.GetById<NonfreeRentAgreement>(uow.Root.Id);
			}
			return result;
		}

		/// <summary>
		/// Создает дополнительное соглашение для бесплатной аренды оборудования
		/// </summary>
		public static FreeRentAgreement CreateDefaultFreeRentAgreement(IUnitOfWork UoW,
																			 DeliveryPoint deliveryPoint,
																			 DateTime? deliveryDate,
																			 CounterpartyContract contract,
																			 List<FreeRentEquipment> equipments)
		{
			if(equipments.Count == 0) {
				throw new ArgumentException("При автоматическом создании дополнительного соглашения " +
											"аренды оборудования, список должен иметь оборудование для аренды");
			}
			FreeRentAgreement result = null;
			using(var uow = FreeRentAgreement.Create(contract)) {
				uow.Root.DeliveryPoint = deliveryPoint;
				if(deliveryDate.HasValue) {
					uow.Root.IssueDate = deliveryDate.Value;
					uow.Root.StartDate = deliveryDate.Value;
				}
				foreach(var item in equipments) {
					uow.Root.ObservableEquipment.Add(new FreeRentEquipment() {
						Count = item.Count,
						Deposit = item.Deposit,
						Equipment = item.Equipment,
						IsNew = item.IsNew,
						Nomenclature = item.Nomenclature,
						FreeRentPackage = item.FreeRentPackage,
						WaterAmount = item.WaterAmount
					});
				}
				uow.Save();
				result = UoW.GetById<FreeRentAgreement>(uow.Root.Id);
			}
			return result;
		}

		/// <summary>
		/// Создает дополнительное соглашение для продажи оборудования
		/// </summary>
		public static SalesEquipmentAgreement CreateDefaultSalesEquipmentAgreement(IUnitOfWork UoW,
																			 DeliveryPoint deliveryPoint,
																			 DateTime? deliveryDate,
																			 CounterpartyContract contract,
																			 List<SalesEquipment> equipments)
		{
			if(equipments.Count == 0) {
				throw new ArgumentException("При автоматическом создании дополнительного соглашения " +
											"аренды оборудования, список должен иметь оборудование для аренды");
			}
			SalesEquipmentAgreement result = null;
			using(var uow = SalesEquipmentAgreement.Create(contract)) {
				uow.Root.DeliveryPoint = deliveryPoint;
				if(deliveryDate.HasValue) {
					uow.Root.IssueDate = deliveryDate.Value;
					uow.Root.StartDate = deliveryDate.Value;
				}
				foreach(var item in equipments) {
					uow.Root.ObservableSalesEqipments.Add(new SalesEquipment() {
						Count = item.Count,
						AdditionalAgreement = item.AdditionalAgreement,
						Nomenclature = item.Nomenclature,
						Price = item.Price
					});
				}
				uow.Save();
				result = UoW.GetById<SalesEquipmentAgreement>(uow.Root.Id);
			}
			return result;
		}

	}
}
