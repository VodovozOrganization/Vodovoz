using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "погрузочно-разгрузочные операции",
		Nominative = "погрузочно-разгрузочная операция")]
	public class LoadingUnloadingOperation : BusinessObjectBase<LoadingUnloadingOperation>, IDomainObject
	{
		public virtual int Id { get; set; }

		RouteList routeList;
		[Display(Name = "Маршрутный лист")]
		public virtual RouteList RouteList {
			get => routeList;
			set => SetField(ref routeList, value, () => RouteList);
		}

		Warehouse warehouse;
		[Display(Name = "Склад погрузки-разгрузки")]
		public virtual Warehouse Warehouse {
			get => warehouse;
			set => SetField(ref warehouse, value, () => Warehouse);
		}

		OperationType operType;
		[Display(Name = "Тип операции")]
		public virtual OperationType OperType {
			get => operType;
			set => SetField(ref operType, value, () => OperType);
		}

		bool isComplete;
		[Display(Name = "Погрузка\\разгрузка завершена?")]
		public virtual bool IsComplete {
			get => isComplete;
			set => SetField(ref isComplete, value, () => IsComplete);
		}

		bool isActive;
		[Display(Name = "Погрузка\\разгрузка на очереди?")]
		public virtual bool IsActive {
			get => isActive;
			set => SetField(ref isActive, value, () => IsActive);
		}
	}

	public enum OperationType
	{
		[Display(Name = "Погрузка")]
		[Appellative(Gender = GrammaticalGender.Feminine, NominativePlural = "погрузки", Nominative = "погрузка", Accusative = "погрузку")]
		Loading,
		[Display(Name = "Разгрузка")]
		[Appellative(Gender = GrammaticalGender.Feminine, NominativePlural = "разгрузки", Nominative = "разгрузка", Accusative = "разгрузку")]
		Unloading
	}

	public class OperationTypeStringType : NHibernate.Type.EnumStringType
	{
		public OperationTypeStringType() : base(typeof(OperationType)) { }
	}
}
