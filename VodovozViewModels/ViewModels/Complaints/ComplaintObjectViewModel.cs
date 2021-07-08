using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.ViewModels.ViewModels.Complaints
{
	public class ComplaintObjectViewModel : EntityTabViewModelBase<ComplaintObject>
	{
		private readonly bool _isArchive;
		public ComplaintObjectViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory uowFactory, ICommonServices commonServices)
			: base(uowBuilder, uowFactory, commonServices)
		{
			_isArchive = Entity.IsArchive;
			if(Entity.Id == 0)
			{
				Entity.CreateDate = DateTime.Now;
			}
		}

		protected override void BeforeSave()
		{
			if(!_isArchive && Entity.IsArchive)
			{
				Entity.ArchiveDate = DateTime.Now;
			}

			if(_isArchive && !Entity.IsArchive)
			{
				Entity.ArchiveDate = null;
			}

			base.BeforeSave();
		}

		public bool ArchiveDateVisible => Entity.ArchiveDate != null;
	}
}
