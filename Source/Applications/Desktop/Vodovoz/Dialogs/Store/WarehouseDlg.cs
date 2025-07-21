using Autofac;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.Project.Services;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz
{
	[Obsolete("Проверить соответствие с WarehouseView, если все ровно - удалить этот класс и все перевести на View" +
		"если нет - доработать и опять же все перевести на View")]
	public partial class WarehouseDlg : QS.Dialog.Gtk.EntityDialogBase<Warehouse>, INotifyPropertyChanged
	{
		private readonly ISubdivisionRepository _subdivisionRepository = ScopeProvider.Scope.Resolve<ISubdivisionRepository>();
		private readonly ILifetimeScope _lifetimeScope;
		private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
		private Subdivision _owningSubdivision;
		private Subdivision _movementDocumentsNotificationsSubdivisionRecipient;

		public event PropertyChangedEventHandler PropertyChanged;

		public WarehouseDlg(
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope)
		{
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));

			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<Warehouse>();
			ConfigureDialog();
		}

		public WarehouseDlg(
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			int id)
		{
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));

			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<Warehouse>(id);
			ConfigureDialog();
		}

		public WarehouseDlg(
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			Warehouse sub)
			: this(navigationManager, lifetimeScope, sub.Id) { }

		private bool CanEdit => permissionResult.CanUpdate || Entity.Id == 0 && permissionResult.CanCreate;

		private void ConfigureDialog()
		{
			buttonSave.Sensitive = CanEdit;
			btnCancel.Clicked += (sender, args) => OnCloseTab(false, CloseSource.Cancel);
			
			yentryName.Binding
				.AddBinding(UoWGeneric.Root, (warehouse) => warehouse.Name, (widget) => widget.Text)
				.InitializeFromSource();
			yentryName.IsEditable = CanEdit;
			ycheckOnlineStore.Binding
				.AddBinding(Entity, e => e.PublishOnlineStore, w => w.Active)
				.InitializeFromSource();
			ycheckOnlineStore.Sensitive = CanEdit;
			ycheckbuttonCanReceiveBottles.Binding
				.AddBinding(UoWGeneric.Root, (warehouse) => warehouse.CanReceiveBottles, (widget) => widget.Active)
				.InitializeFromSource();
			ycheckbuttonCanReceiveBottles.Sensitive = CanEdit;
			ycheckbuttonCanReceiveEquipment.Binding
				.AddBinding(UoWGeneric.Root, (warehouse) => warehouse.CanReceiveEquipment, (widget) => widget.Active)
				.InitializeFromSource();
			ycheckbuttonCanReceiveEquipment.Sensitive = CanEdit;
			ycheckbuttonArchive.Binding
				.AddBinding(UoWGeneric.Root, (warehouse) => warehouse.IsArchive, (widget) => widget.Active)
				.InitializeFromSource();
			ycheckbuttonArchive.Sensitive =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_archive_warehouse") && CanEdit;

			comboTypeOfUse.ItemsEnum = typeof(WarehouseUsing);
			comboTypeOfUse.Binding
				.AddBinding(Entity, e => e.TypeOfUse, w => w.SelectedItem)
				.InitializeFromSource();
			comboTypeOfUse.Sensitive = CanEdit;
			
			ySpecCmbOwner.SetRenderTextFunc<Subdivision>(s => s.Name);
			ySpecCmbOwner.ItemsList = _subdivisionRepository.GetAllDepartmentsOrderedByName(UoW);
			ySpecCmbOwner.Binding
				.AddBinding(this, d => d.OwningSubdivision, w => w.SelectedItem)
				.InitializeFromSource();
			ySpecCmbOwner.Sensitive = CanEdit;
			
			entryAddress.IsEditable = CanEdit;
			entryAddress.Binding
				.AddBinding(Entity, e => e.Address, w => w.Text)
				.InitializeFromSource();

			entryMovementNotificationsSubdivisionRecipient.Sensitive = CanEdit;
			entryMovementNotificationsSubdivisionRecipient.ViewModel = new LegacyEEVMBuilderFactory<WarehouseDlg>(this, this, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(e => e.MovementDocumentsNotificationsSubdivisionRecipient)
				.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel>()
				.UseViewModelDialog<SubdivisionViewModel>()
				.Finish();
		}

		public IEntityEntryViewModel SubdivisionViewModel { get; private set; }
		public INavigationManager NavigationManager { get; }

		public Subdivision MovementDocumentsNotificationsSubdivisionRecipient
		{
			get => GetIdRefField<Subdivision, Subdivision>(ref _movementDocumentsNotificationsSubdivisionRecipient, Entity.MovementDocumentsNotificationsSubdivisionRecipientId);
			set => SetIdRefField<Subdivision, Subdivision>(SetField, ref _movementDocumentsNotificationsSubdivisionRecipient, () => Entity.MovementDocumentsNotificationsSubdivisionRecipientId, value);
		}

		public Subdivision OwningSubdivision
		{
			get => GetIdRefField<Subdivision, Subdivision>(ref _owningSubdivision, Entity.OwningSubdivisionId);
			set => SetIdRefField<Subdivision, Subdivision>(SetField, ref _owningSubdivision, () => Entity.OwningSubdivisionId, value);
		}

		protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
		{
			if(NHibernate.NHibernateUtil.IsInitialized(value))
			{
				if(EqualityComparer<T>.Default.Equals(field, value))
					return false;
			}
			field = value;
			OnPropertyChanged(propertyName);
			return true;
		}

		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			if(PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
				var propertyInfos = this.GetType().GetProperties().Where(x => x.Name == propertyName);
				foreach(var propertyInfo in propertyInfos)
				{
					var attributes = propertyInfo.GetCustomAttributes(typeof(PropertyChangedAlsoAttribute), true);
					foreach(PropertyChangedAlsoAttribute attribute in attributes)
					{
						foreach(string propName in attribute.PropertiesNames)
						{
							PropertyChanged(this, new PropertyChangedEventArgs(propName));
						}
					}
				}
			}
		}

		public delegate bool SetFieldDelegate<T>(ref T field, T value, [CallerMemberName] string propertyName = "");

		public U GetIdRefField<T, U>(ref U field, int? entityFieldId)
			where T : class, IDomainObject, INotifyPropertyChanged, new()
			where U : IDomainObject
		{
			if(field?.Id != entityFieldId)
			{
				if(entityFieldId == null)
				{
					field = default;
				}
				else
				{
					field = UoW.GetById<U>(entityFieldId.Value);
				}
			}

			return field;
		}

		public bool SetIdRefField<T, U>(SetFieldDelegate<U> setField, ref U targetField, Expression<Func<int?>> targetPropertyExpr, U value, [CallerMemberName] string callerPropertyName = null)
			where T : class, IDomainObject, INotifyPropertyChanged, new()
			where U : IDomainObject
		{
			if(value?.Id == targetField?.Id)
			{
				return false;
			}

			if(targetPropertyExpr.Body is MemberExpression memberSelectorExpression
				&& memberSelectorExpression.Member is PropertyInfo property)
			{
				property.SetValue(Entity, value?.Id, null);
			}

			return setField(ref targetField, value, callerPropertyName);
		}

		#region implemented abstract members of OrmGtkDialogBase

		public override bool Save()
		{
			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}

			_logger.Info("Сохраняем склад...");
			UoWGeneric.Save();
			return true;
		}

		#endregion
	}
}
