using Autofac;
using NLog;
using QS.Dialog;
using Vodovoz.Domain.Contacts;
using QS.Project.Services;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.ViewModels.ViewModels.Contacts;
using Vodovoz.Settings.Common;

namespace Vodovoz
{
	public partial class ContactDlg : QS.Dialog.Gtk.EntityDialogBase<Contact>
	{
		protected static Logger logger = LogManager.GetCurrentClassLogger();
		private readonly IInteractiveService _interactiveService = ServicesConfig.InteractiveService;
		private readonly IExternalCounterpartyRepository _externalCounterpartyRepository = new ExternalCounterpartyRepository();
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private PhonesViewModel _phonesViewModel;

		public ContactDlg (Counterparty counterparty)
		{
			_externalCounterpartyRepository = _lifetimeScope.Resolve<IExternalCounterpartyRepository>();
			this.Build ();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<Contact>();
			UoWGeneric.Root.Counterparty = counterparty;
			ConfigureDlg ();
		}

		public ContactDlg (Contact sub) : this (sub.Id)
		{
		}

		public ContactDlg (int id)
		{
			_externalCounterpartyRepository = _lifetimeScope.Resolve<IExternalCounterpartyRepository>();
			this.Build ();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<Contact> (id);
			ConfigureDlg ();
		}


		void ConfigureDlg ()
		{
			referencePost.SubjectType = typeof(Post);
			referencePost.Binding.AddBinding(Entity, e => e.Post, w => w.Subject).InitializeFromSource();

			entryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			entrySurname.Binding.AddBinding(Entity, e => e.Surname, w => w.Text).InitializeFromSource();
			entryPatronymic.Binding.AddBinding(Entity, e => e.Patronymic, w => w.Text).InitializeFromSource();

			checkbuttonFired.Binding.AddBinding(Entity, e => e.IsFired, w => w.Active).InitializeFromSource();

			dataComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			var emailsViewModel =
				new EmailsViewModel(
					UoWGeneric,
					Entity.Emails,
					_lifetimeScope.Resolve<IEmailSettings>(),
					_externalCounterpartyRepository,
					_interactiveService,
					Entity.Counterparty.PersonType);
			emailsView.ViewModel = emailsViewModel;

			_phonesViewModel = _lifetimeScope.Resolve<PhonesViewModel>(new TypedParameter(typeof(IUnitOfWork), UoW));
			_phonesViewModel.PhonesList = UoWGeneric.Root.ObservablePhones;
			phonesView.ViewModel = _phonesViewModel;
		}

		public override bool Save ()
		{
			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}

			logger.Info ("Сохраняем  контактное лицо...");
			_phonesViewModel.RemoveEmpty();
			emailsView.ViewModel.RemoveEmpty();
			UoWGeneric.Save ();
			logger.Info ("Ok");
			return true;
		}

		public override void Destroy()
		{
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
			base.Destroy();
		}
	}
}

