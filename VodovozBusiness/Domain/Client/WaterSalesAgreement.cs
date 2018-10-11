using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using NHibernate.Util;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QSHistoryLog;
using QSOrmProject;
using Vodovoz.Domain.Goods;
using Vodovoz.Repository;

namespace Vodovoz.Domain.Client
{

	[OrmSubject (Gender = GrammaticalGender.Neuter,
		NominativePlural = "доп. соглашения продажи воды",
		Nominative = "доп. соглашение продажи воды")]
	public class WaterSalesAgreement : AdditionalAgreement
	{
		[HistoryDeepCloneItems]
		IList<WaterSalesAgreementFixedPrice> fixedPrices = new List<WaterSalesAgreementFixedPrice> ();

		[Display (Name = "Фиксированные цены")]
		public virtual IList<WaterSalesAgreementFixedPrice> FixedPrices {
			get { return fixedPrices; }
			set { 
				if(SetField (ref fixedPrices, value, () => FixedPrices)){
					observableFixedPrices = null;
				} 
			}
		}

		GenericObservableList<WaterSalesAgreementFixedPrice> observableFixedPrices;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		[IgnoreHistoryTrace]
		public virtual GenericObservableList<WaterSalesAgreementFixedPrice> ObservableFixedPrices {
			get {
				if (observableFixedPrices == null)
					observableFixedPrices = new GenericObservableList<WaterSalesAgreementFixedPrice> (FixedPrices);
				return observableFixedPrices;
			}
		}

		public override IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			foreach (ValidationResult result in base.Validate (validationContext))
				yield return result;
			if (Contract.CheckWaterSalesAgreementExists (Id, DeliveryPoint)) {
				if (DeliveryPoint != null)
					yield return new ValidationResult ("Доп. соглашение для данной точки доставки уже существует. " +
					"Пожалуйста, закройте действующее соглашение для создания нового.", new[] { "DeliveryPoint" });
				else
					yield return new ValidationResult ("Общее доп. соглашение по продаже воды уже существует. " +
					"Пожалуйста, закройте действующее соглашение для создания нового.", new[] { "DeliveryPoint" });
			}
		}

		#region Расчетные

		public virtual bool HasFixedPrice => FixedPrices.Any();
	
		#endregion

		public virtual void  AddFixedPrice(Nomenclature nomenclature, decimal price)
		{
			var nomenculaturePrice = new WaterSalesAgreementFixedPrice{
				Nomenclature = nomenclature,
				AdditionalAgreement = this,
				Price = price
			};

			ObservableFixedPrices.Add(nomenculaturePrice);
		}

		/// <summary>
		/// Заполняет фиксированные цены на воду из цен в точке доставки, 
		/// которые были выгружены из 1с при переходе приема заказов из 1с.
		/// ID номенклатур воды берутся из параметров в базе.
		/// </summary>
		public virtual void FillFixedPricesFromDeliveryPoint(IUnitOfWork uow)
		{
			if(DeliveryPoint != null) {
				ObservableFixedPrices.Clear();

				//Семиозерье
				if(DeliveryPoint.FixPrice1 > 0) {
					Nomenclature nom1 = NomenclatureRepository.GetWaterSemiozerie(uow);
					AddFixedPrice(nom1, DeliveryPoint.FixPrice1);
				}
				//Кислородная
				if(DeliveryPoint.FixPrice2 > 0) {
					Nomenclature nom2 = NomenclatureRepository.GetWaterKislorodnaya(uow);
					AddFixedPrice(nom2, DeliveryPoint.FixPrice2);
				}
				//Снятогорская
				if(DeliveryPoint.FixPrice3 > 0) {
					Nomenclature nom3 = NomenclatureRepository.GetWaterSnyatogorskaya(uow);
					AddFixedPrice(nom3, DeliveryPoint.FixPrice3);
				}
				//Стройка
				if(DeliveryPoint.FixPrice4 > 0) {
					Nomenclature nom4 = NomenclatureRepository.GetWaterStroika(uow);
					AddFixedPrice(nom4, DeliveryPoint.FixPrice4);
				}
				//С ручками
				if(DeliveryPoint.FixPrice5 > 0) {
					Nomenclature nom5 = NomenclatureRepository.GetWaterRuchki(uow);
					AddFixedPrice(nom5, DeliveryPoint.FixPrice5);
				}
			}
		}

		public static IUnitOfWorkGeneric<WaterSalesAgreement> Create (CounterpartyContract contract)
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<WaterSalesAgreement> ();
			uow.Root.Contract = uow.GetById<CounterpartyContract>(contract.Id);
			uow.Root.AgreementNumber = AdditionalAgreement.GetNumberWithType (uow.Root.Contract, AgreementType.WaterSales);
			return uow;
		}
	}
	
}
