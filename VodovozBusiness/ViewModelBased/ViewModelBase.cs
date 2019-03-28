using System;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;

namespace Vodovoz.ViewModelBased
{
	public abstract class ViewModelBase : PropertyChangedBase, IDisposable
	{
		public object RootEntity { get; set; }

		public IUnitOfWork UoW { get; protected set; }

		public bool HasChanges => UoW.HasChanges;

		public virtual bool Save()
		{
			UoW.Save();
			EntitySaved?.Invoke(this, EventArgs.Empty);
			return true;
		}

		public event EventHandler EntitySaved;

		public void Dispose()
		{
			if(UoW != null) {
				UoW.Dispose();
			}
		}
	}
}
