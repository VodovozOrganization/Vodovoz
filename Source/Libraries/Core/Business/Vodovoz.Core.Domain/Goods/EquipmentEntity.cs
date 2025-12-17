using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Core.Domain.Goods
{
	/// <summary>
	/// Оборудование только для посерийного учета
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "оборудование",
		Nominative = "оборудование",
		Prepositional = "оборудовании"
	)]
	[EntityPermission]
	public class EquipmentEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private bool _onDuty;
		private string _serial;
		private string _comment;
		private NomenclatureEntity _nomenclature;
		private DateTime _lastServiceDate;
		private DateTime? _warrantyEndDate;
		private CounterpartyEntity _assignedToClient;

		public EquipmentEntity()
		{
			Comment = string.Empty;
		}

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
		/// Дежурный куллер
		/// </summary>
		[Display(Name = "Дежурный куллер")]
		public virtual bool OnDuty
		{
			get => _onDuty;
			set => SetField(ref _onDuty, value);
		}

		/// <summary>
		/// Серийный номер
		/// </summary>
		[Display(Name = "Серийный номер")]
		public virtual string Serial
		{
			get => Id > 0 ? Id.ToString() : "не определён"; 
			set => SetField(ref _serial, value); 
		}

		/// <summary>
		/// Комментарий
		/// </summary>
		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		/// <summary>
		/// Номенклатура
		/// </summary>
		[Display(Name = "Номенклатура")]
		public virtual NomenclatureEntity Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		/// <summary>
		/// Последняя дата сан. обработки
		/// </summary>
		[Display(Name = "Последняя сан. обработка")]
		public virtual DateTime LastServiceDate
		{
			get => _lastServiceDate;
			set => SetField(ref _lastServiceDate, value);
		}

		/// <summary>
		/// Окончание гарантии
		/// </summary>
		[Display(Name = "Окончание гарантии")]
		public virtual DateTime? WarrantyEndDate
		{
			get => _warrantyEndDate;
			set => SetField(ref _warrantyEndDate, value);
		}

		[Display(Name = "Привязан к клиенту")]
		public virtual CounterpartyEntity AssignedToClient
		{
			get => _assignedToClient;
			set => SetField(ref _assignedToClient, value);
		}

		/// <summary>
		/// Следующая дата сан. обработки
		/// </summary>
		[Display(Name = "Следующая сан. обработка")]
		public virtual DateTime? NextServiceDate => LastServiceDate == DateTime.MinValue ? null : (DateTime?)LastServiceDate.AddMonths(6);

		/// <summary>
		/// Заголовок
		/// </summary>
		[Display(Name = "Заголовок")]
		public virtual string Title
		{
			get
			{
				return Nomenclature == null ? string.Empty :
					string.Format("{0} (с/н: {1})",
						string.IsNullOrWhiteSpace(Nomenclature.Model) ? Nomenclature.Name : Nomenclature.Model,
						Serial);
			}
		}

		/// <summary>
		/// Наименование
		/// </summary>
		[Display(Name = "Наименование")]
		public virtual string NomenclatureName => Nomenclature == null ? string.Empty : Nomenclature.Name;
	}
}
