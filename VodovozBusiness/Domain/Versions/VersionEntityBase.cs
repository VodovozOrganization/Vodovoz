using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using VodovozInfrastructure.Versions;

namespace Vodovoz.Domain.Versions
{
	public abstract class VersionEntityBase : PropertyChangedBase
	{
		private int _id;
		private DateTime _creationDate;
		private DateTime? _activationDate;
		private DateTime? _closingDate;
		private VersionStatus _status;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Время создания")]
		public virtual DateTime CreationDate
		{
			get => _creationDate;
			set => SetField(ref _creationDate, value);
		}

		[Display(Name = "Время активации")]
		public virtual DateTime? ActivationDate
		{
			get => _activationDate;
			set => SetField(ref _activationDate, value);
		}

		[Display(Name = "Время закрытия")]
		public virtual DateTime? ClosingDate
		{
			get => _closingDate;
			set => SetField(ref _closingDate, value);
		}

		[Display(Name = "Статус")]
		public virtual VersionStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}
	}
}
