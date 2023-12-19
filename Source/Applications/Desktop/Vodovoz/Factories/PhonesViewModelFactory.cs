using Autofac;
using QS.Dialog.GtkUI.FileDialog;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using System;
using Vodovoz.EntityRepositories;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.ViewModels.Contacts;

namespace Vodovoz.Factories
{
	public class PhonesViewModelFactory : IPhonesViewModelFactory
	{
		public PhonesViewModel CreateNewPhonesViewModel(ILifetimeScope lifetimeScope, IUnitOfWork uow) =>
			lifetimeScope.Resolve<PhonesViewModel>(new TypedParameter(typeof(IUnitOfWork), uow));
	}
}
