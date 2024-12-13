using System;
using System.ComponentModel.DataAnnotations;
using FluentNHibernate.Data;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Service;

namespace Vodovoz.Domain.Documents
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки талона разгрузки",
		Nominative = "строка талоны разгрузки")]
	[HistoryTrace]
	public class CarUnloadDocumentItem: PropertyChangedBase, IDomainObject
	{
		private DeliveryFreeBalanceOperation _deliveryFreeBalanceOperation;

		public virtual int Id { get; set; }

		private CarUnloadDocument document;
		public virtual CarUnloadDocument Document {
			get => document;
			set { SetField (ref document, value, () => Document); }
		}

		private ReciveTypes reciveType;
		public virtual ReciveTypes ReciveType { 
			get => reciveType;
			set { SetField (ref reciveType, value, () => ReciveType); }
		}

		private WarehouseBulkGoodsAccountingOperation _goodsAccountingOperation;
		public virtual WarehouseBulkGoodsAccountingOperation GoodsAccountingOperation
		{ 
			get => _goodsAccountingOperation;
			set => SetField (ref _goodsAccountingOperation, value);
		}

		private EmployeeNomenclatureMovementOperation employeeNomenclatureMovementOperation;
		public virtual EmployeeNomenclatureMovementOperation EmployeeNomenclatureMovementOperation { 
			get => employeeNomenclatureMovementOperation;
			set => SetField (ref employeeNomenclatureMovementOperation, value);
		}

		public virtual DeliveryFreeBalanceOperation DeliveryFreeBalanceOperation
		{
			get => _deliveryFreeBalanceOperation;
			set => SetField(ref _deliveryFreeBalanceOperation, value);
		}

		ServiceClaim serviceClaim;
		[Display (Name = "Заявка на сервис")]
		public virtual ServiceClaim ServiceClaim {
			get => serviceClaim;
			set { SetField (ref serviceClaim, value, () => ServiceClaim); }
		}

		string redhead;
		[Display(Name = "№ кулера")]
		public virtual string Redhead {
			get => redhead;
			set { SetField(ref redhead, value, () => Redhead); }
		}

		CullingCategory typeOfDefect;
		[Display(Name = "Тип брака")]
		public virtual CullingCategory TypeOfDefect {
			get => typeOfDefect;
			set { SetField(ref typeOfDefect, value, () => TypeOfDefect); }
		}

		DefectSource defectSource;
		[Display(Name = "Источник брака")]
		public virtual DefectSource DefectSource {
			get => defectSource;
			set { SetField(ref defectSource, value, () => DefectSource); }
		}

		public virtual string Title =>
			String.Format("[{2}] {0} - {1}",
				GoodsAccountingOperation.Nomenclature.Name,
				GoodsAccountingOperation.Nomenclature.Unit.MakeAmountShortStr(GoodsAccountingOperation.Amount),
				document.Title);

		public virtual void CreateOrUpdateDeliveryFreeBalanceOperation(int terminalId)
		{
			if(reciveType == ReciveTypes.Defective || GoodsAccountingOperation.Nomenclature.Id == terminalId)
			{
				return;
			}

			var deliveryFreeBalanceOperation = DeliveryFreeBalanceOperation
				?? new DeliveryFreeBalanceOperation
				{
					Nomenclature = new Nomenclature { Id = GoodsAccountingOperation.Nomenclature.Id },
					RouteList = Document.RouteList
				};

			deliveryFreeBalanceOperation.Amount = -GoodsAccountingOperation.Amount;
			DeliveryFreeBalanceOperation = deliveryFreeBalanceOperation;
		}
	}

	public enum ReciveTypes
	{
		[Display(Name = "Возврат тары")]
		Bottle,
		[Display(Name = "Оборудование по заявкам")]
		Equipment,
		[Display(Name = "Возврат недовоза")]
		Returnes,
		[Display(Name = "Брак")]
		Defective,
		[Display(Name = "Возврат кассового оборудования")]
		ReturnCashEquipment
	}

}
