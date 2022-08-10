using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using VodovozInfrastructure.Versions;

namespace Vodovoz.Domain.Versions
{
	public abstract class VersionEntityBase : PropertyChangedBase, IVersionEntity
	{
		private int _id;
		private DateTime _dateCreated;
		private DateTime? _dateActivated;
		private DateTime? _dateClosed;
		private VersionStatus _status;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Время создания")]
		public virtual DateTime DateCreated
		{
			get => _dateCreated;
			set => SetField(ref _dateCreated, value);
		}

		[Display(Name = "Время активации")]
		public virtual DateTime? DateActivated
		{
			get => _dateActivated;
			set => SetField(ref _dateActivated, value);
		}

		[Display(Name = "Время закрытия")]
		public virtual DateTime? DateClosed
		{
			get => _dateClosed;
			set => SetField(ref _dateClosed, value);
		}

		[Display(Name = "Статус")]
		public virtual VersionStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}
	}
}
