using System;
using QS.Tdi;
using QS.DomainModel.UoW;
using System.Reflection;
using QS.DomainModel.Entity;
using System.ComponentModel;
using Vodovoz.Infrastructure.Services;

namespace Vodovoz.Infrastructure.ViewModels
{
	public abstract class UoWTabViewModelBase : TabViewModelBase, ITdiDialog, IDisposable
	{
		protected UoWTabViewModelBase(IInteractiveService interactiveService) : base(interactiveService)
		{
		}

		#region ITdiDialog implementation

		public virtual IUnitOfWork UoW { get; set; }

		private bool manualChange = false;

		public virtual bool HasChanges {
			get { return manualChange || UoW.HasChanges; }
			set { manualChange = value; }
		}

		public event EventHandler<EntitySavedEventArgs> EntitySaved;

		public virtual void SaveAndClose()
		{
			if(!HasChanges || SaveUoW()) {
				Close(false);
			}
		}

		public virtual bool Save()
		{
			return SaveUoW();
		}

		private bool SaveUoW()
		{
			UoW.Save();
			if(UoW.RootObject != null) {
				EntitySaved?.Invoke(this, new EntitySavedEventArgs(UoW.RootObject));
			}
			return true;
		}

		public virtual void Dispose()
		{
			if(UoW != null) {
				UoW.Dispose();
			}
		}

		#endregion
	}
}
