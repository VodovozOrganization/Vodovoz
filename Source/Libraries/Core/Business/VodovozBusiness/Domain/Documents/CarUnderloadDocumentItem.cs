using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки документа  недопогрузки",
		Nominative = "строка документа недопогрузки")]
	[HistoryTrace]

	public class CarUnderloadDocumentItem : PropertyChangedBase, IDomainObject
	{
		private Nomenclature _nomenclature;
		private decimal _amount;
		private DeliveryFreeBalanceOperation _deliveryFreeBalanceOperation;
		private CarUnderloadDocument _carUnderloadDocument;

		public virtual int Id { get; set; }

		public virtual CarUnderloadDocument CarUnderloadDocument
		{
			get => _carUnderloadDocument;
			set => SetField(ref _carUnderloadDocument, value);
		}

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		[Display(Name = "Количество")]
		public virtual decimal Amount
		{
			get => _amount;
			set => SetField(ref _amount, value);
		}

		[Display(Name = "Операция со свободными остатками МЛ")]
		public virtual DeliveryFreeBalanceOperation DeliveryFreeBalanceOperation
		{
			get => _deliveryFreeBalanceOperation;
			set => SetField(ref _deliveryFreeBalanceOperation, value);
		}

		public virtual string Title => $"Строка документа недопогрузки МЛ № {CarUnderloadDocument.RouteList.Id} {Nomenclature.Name} кол-во {Amount}";


		public virtual void CreateOrUpdateOperation()
		{
			var operation = DeliveryFreeBalanceOperation ?? new DeliveryFreeBalanceOperation();
			operation.Amount = Amount;
			operation.Nomenclature = Nomenclature;
			operation.RouteList = CarUnderloadDocument.RouteList;

			DeliveryFreeBalanceOperation = operation;
		}
	}
}
