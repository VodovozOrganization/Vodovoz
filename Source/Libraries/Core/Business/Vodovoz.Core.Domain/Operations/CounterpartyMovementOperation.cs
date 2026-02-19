using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Domain.Operations
{
	/// <summary>
	/// Операция передвижения товаров
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "передвижения товаров",
		Nominative = "передвижение товаров")]
	public class CounterpartyMovementOperation : OperationBase
	{
		private NomenclatureEntity _nomenclature;
		private EquipmentEntity _equipment;
		private decimal _amount;
		private CounterpartyEntity _incomingCounterparty;
		private DeliveryPointEntity _incomingDeliveryPoint;
		private CounterpartyEntity _writeoffCounterparty;
		private DeliveryPointEntity _writeoffDeliveryPoint;
		private bool _forRent;

		/// <summary>
		/// Номенклатура
		/// </summary>
		[Required(ErrorMessage = "Номенклатура должна быть заполнена.")]
		[Display(Name = "Номенклатура")]
		public virtual NomenclatureEntity Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		/// <summary>
		/// Оборудование
		/// </summary>
		[Display(Name = "Оборудование")]
		public virtual EquipmentEntity Equipment
		{
			get => _equipment;
			set => SetField(ref _equipment, value);
		}

		/// <summary>
		/// Количество
		/// </summary>
		[Display(Name = "Количество")]
		public virtual decimal Amount
		{
			get => _amount;
			set => SetField(ref _amount, value);
		}

		/// <summary>
		/// Контрагент (получатель)
		/// </summary>
		[Display(Name = "Контрагент (получатель)")]
		public virtual CounterpartyEntity IncomingCounterparty
		{
			get => _incomingCounterparty;
			set => SetField(ref _incomingCounterparty, value);
		}

		/// <summary>
		/// Точка доставки (получатель)
		/// </summary>
		[Display(Name = "Точка доставки (получатель)")]
		public virtual DeliveryPointEntity IncomingDeliveryPoint
		{
			get => _incomingDeliveryPoint;
			set => SetField(ref _incomingDeliveryPoint, value);
		}

		/// <summary>
		/// Контрагент (отправитель)
		/// </summary>
		[Display(Name = "Контрагент (отправитель)")]
		public virtual CounterpartyEntity WriteoffCounterparty
		{
			get => _writeoffCounterparty;
			set => SetField(ref _writeoffCounterparty, value);
		}

		/// <summary>
		/// Точка доставки (отправитель)
		/// </summary>
		[Display(Name = "Точка доставки (отправитель)")]
		public virtual DeliveryPointEntity WriteoffDeliveryPoint
		{
			get => _writeoffDeliveryPoint;
			set => SetField(ref _writeoffDeliveryPoint, value);
		}

		/// <summary>
		/// Используется для того чтобы пометить перемещение оборудования с учетом того где оно находится.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> - оборудование перемещалось либо в аренду, либо в ремонт, либо по другой причине, 
		/// и должно вернуться. Должен учитываться как баланс.<br/>
		/// <see langword="false"/> - оборудование продали, переместили и забыли.
		/// </returns>
		[Display(Name = "В аренду")]
		public virtual bool ForRent
		{
			get => _forRent;
			set => SetField(ref _forRent, value);
		}

		#region Вычисляемые

		/// <summary>
		/// Заголовок
		/// </summary>
		public virtual string Title
		{
			get
			{
				if(IncomingCounterparty != null && WriteoffCounterparty != null)
				{
					return string.Format("Перемещение из {0}({4}) в {1}({5}), {2} - {3}", WriteoffCounterparty.Name, IncomingCounterparty.Name, Nomenclature.Name, Nomenclature.Unit.MakeAmountShortStr(Amount), IncomingDeliveryPoint.CompiledAddress, WriteoffDeliveryPoint.CompiledAddress);
				}
				else if(IncomingCounterparty != null)
				{
					return string.Format("Поступление клиенту {0}, {1} - {2}", IncomingCounterparty.Name, Nomenclature.Name, Nomenclature.Unit.MakeAmountShortStr(Amount));
				}
				else if(WriteoffCounterparty != null)
				{
					return string.Format("Забор от клиента {0}, {1} - {2}", WriteoffCounterparty.Name, Nomenclature.Name, Nomenclature.Unit.MakeAmountShortStr(Amount));
				}
				else
				{
					return null;
				}
			}
		}

		#endregion

	}
}

