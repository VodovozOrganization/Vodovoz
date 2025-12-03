using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.BasicHandbooks;

namespace Vodovoz.Core.Domain.Goods.Rent
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "пакеты бесплатной аренды",
		Nominative = "пакет бесплатной аренды")]
	[EntityPermission]
	public class FreeRentPackageEntity : BusinessObjectBase<FreeRentPackageEntity>, IDomainObject
	{
		private int _id;
		private int _minWaterAmount;
		private string _name;
		private string _onlineName;
		private decimal _deposit;
		private bool _isArchive;
		private EquipmentKind _equipmentKind;
		private NomenclatureEntity _depositService;
		private IList<FreeRentPackageOnlineParametersEntity> _onlineParameters = new List<FreeRentPackageOnlineParametersEntity>();

		public FreeRentPackageEntity()
		{
			Name = string.Empty;
		}

		#region Свойства

		/// <summary>
		/// Идентификатор
		/// </summary>
		[Display(Name = "Идентификатор")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Минимальное количество
		/// </summary>
		[Display(Name = "Минимальное количество")]
		public virtual int MinWaterAmount
		{
			get => _minWaterAmount;
			set => SetField(ref _minWaterAmount, value);
		}

		/// <summary>
		/// Название
		/// </summary>
		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		/// <summary>
		/// Название в ИПЗ
		/// </summary>
		[Display(Name = "Название в ИПЗ")]
		public virtual string OnlineName
		{
			get => _onlineName;
			set => SetField(ref _onlineName, value);
		}

		/// <summary>
		/// Залог
		/// </summary>
		[Display(Name = "Залог")]
		public virtual decimal Deposit
		{
			get => _deposit;
			set => SetField(ref _deposit, value);
		}

		/// <summary>
		/// Вид оборудования
		/// </summary>
		[Display(Name = "Вид оборудования")]
		public virtual EquipmentKind EquipmentKind
		{
			get => _equipmentKind;
			set => SetField(ref _equipmentKind, value);
		}

		/// <summary>
		/// Услуга залога
		/// </summary>
		[Display(Name = "Услуга залога")]
		public virtual NomenclatureEntity DepositService
		{
			get => _depositService;
			set => SetField(ref _depositService, value);
		}

		/// <summary>
		/// Онлайн параметры
		/// </summary>
		[Display(Name = "Онлайн параметры")]
		public virtual IList<FreeRentPackageOnlineParametersEntity> OnlineParameters
		{
			get => _onlineParameters;
			set => SetField(ref _onlineParameters, value);
		}

		/// <summary>
		/// Архив
		/// </summary>
		[Display(Name = "Архив")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		#endregion
	}
}
