using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using Gamma.Widgets;
using Gtk;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Widgets.GtkUI;
using QSWidgetLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;

namespace Vodovoz.Views.Logistic
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class DeliveryPointResponsiblePersonsView : Gtk.Bin
    {
        private GenericObservableList<DeliveryPointResponsiblePerson> responsiblePersonsList;
        private IList<DeliveryPointResponsiblePersonType> responsiblePersonTypes;
        private IUnitOfWork uow;

        private IList<DeliveryPointResponsiblePerson> responsiblePersons;

        public IList<DeliveryPointResponsiblePerson> ResponsiblePersons
        {
            get => responsiblePersons;
            set {
                if (responsiblePersons == value)
                    return;
                responsiblePersons = value;
                ResponsiblePersonsList = responsiblePersons != null ? new GenericObservableList<DeliveryPointResponsiblePerson>(responsiblePersons) : null;
            }
        }

        public GenericObservableList<DeliveryPointResponsiblePerson> ResponsiblePersonsList
        {
            get => responsiblePersonsList;
            set
            {
                if (ResponsiblePersonsList != null)
                    ResponsiblePersonsList.Clear();

                responsiblePersonsList = value;

                buttonAdd.Sensitive = responsiblePersonsList != null;
                if (value != null) {
                    ResponsiblePersonsList.ElementAdded += OnResponsiblePersonsElementAdded;
                    ResponsiblePersonsList.ElementRemoved += OnResponsiblePersonsElementRemoved;

                    if (ResponsiblePersonsList.Count == 0) {
                        ResponsiblePersonsList.Add(new DeliveryPointResponsiblePerson());
                    }
                    else {
                        foreach (DeliveryPointResponsiblePerson responsiblePerson in ResponsiblePersonsList) {
                            AddResponsiblePersonRow(responsiblePerson);
                        }
                    }
                }
                SetEditable();
            }
        }

        private void SetEditable()
        {
            // Not Implemented Yet
        }

        /// <summary>
        /// Генерация строки таблицы
        /// </summary>
        /// <param name="responsiblePerson">Responsible person.</param>
        private void AddResponsiblePersonRow(DeliveryPointResponsiblePerson responsiblePerson)
        {
            var rowsCount = ++datatableResponsiblePersons.NRows;

            Label textResponsiblePersonTypeLabel = new Label("Тип:");
            datatableResponsiblePersons.Attach(
                textResponsiblePersonTypeLabel,
                (uint)0, (uint)1, rowsCount, rowsCount + 1,
                (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

            var responsiblePersonTypeDataCombo = new yListComboBox();
            responsiblePersonTypeDataCombo.WidthRequest = 100;
            responsiblePersonTypeDataCombo.SetRenderTextFunc((DeliveryPointResponsiblePersonType x) => x.Title);
            responsiblePersonTypeDataCombo.ItemsList = responsiblePersonTypes;

            responsiblePersonTypeDataCombo.Binding.AddBinding(responsiblePerson,
                e => e.DeliveryPointResponsiblePersonType,
                w => w.SelectedItem).InitializeFromSource();

            datatableResponsiblePersons.Attach(
                responsiblePersonTypeDataCombo,
                (uint)1, (uint)2, rowsCount, rowsCount + 1,
                AttachOptions.Fill | AttachOptions.Expand,
                (AttachOptions)0, (uint)0, (uint)0);

            Label textPhoneLabel = new Label("Телефон: +7 ");
            datatableResponsiblePersons.Attach(
                textPhoneLabel,
                (uint)2, (uint)3, rowsCount, rowsCount + 1,
                (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

            var phoneDataEntry = new yValidatedEntry();
            phoneDataEntry.ValidationMode = ValidationType.phone;
            phoneDataEntry.Tag = responsiblePerson;
            phoneDataEntry.WidthChars = 19;
            phoneDataEntry.Binding.AddBinding(responsiblePerson, e => e.Phone, w => w.Text).InitializeFromSource();
            datatableResponsiblePersons.Attach(
                phoneDataEntry,
                (uint)3, (uint)4, rowsCount, rowsCount + 1,
                AttachOptions.Expand | AttachOptions.Fill,
                (AttachOptions)0, (uint)0, (uint)0);

            Label textEmployeeLabel = new Label("Сотрудник:");
            datatableResponsiblePersons.Attach(
                textEmployeeLabel,
                (uint)4, (uint)5, rowsCount, rowsCount + 1,
                (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

            var employeeEntry = new EntityViewModelEntry();
            employeeEntry.WidthRequest = 50;
            employeeEntry.SetEntityAutocompleteSelectorFactory(
                new DefaultEntityAutocompleteSelectorFactory<Employee, EmployeesJournalViewModel, EmployeeFilterViewModel>(ServicesConfig.CommonServices));

            employeeEntry.Binding.AddBinding(responsiblePerson, e => e.Employee, w => w.Subject).InitializeFromSource();
            datatableResponsiblePersons.Attach(
                employeeEntry,
                (uint)5, (uint)6, rowsCount, rowsCount + 1,
                AttachOptions.Expand | AttachOptions.Fill,
                (AttachOptions)0, (uint)0, (uint)0);

            Button deleteButton = new Button();
            Image image = new Image();
            image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-delete", IconSize.Menu);
            deleteButton.Image = image;
            deleteButton.Clicked += OnButtonDeleteClicked;
            datatableResponsiblePersons.Attach(
                deleteButton,
                (uint)6, (uint)7, rowsCount, rowsCount + 1,
                (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

            datatableResponsiblePersons.ShowAll();
        }

        private void OnButtonDeleteClicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnResponsiblePersonsElementRemoved(object aList, int[] aIdx, object aObject)
        {
            Widget foundWidget = null;

            throw new NotImplementedException();

            foreach (Widget widget in datatableResponsiblePersons.AllChildren) {
                
            }
        }

        private void OnResponsiblePersonsElementAdded(object aList, int[] aIdx)
        {
            foreach (int i in aIdx)
            {
                AddResponsiblePersonRow(ResponsiblePersonsList[i]);
            }
        }

        public DeliveryPointResponsiblePersonsView()
        {
            this.Build();

            buttonAdd.Clicked += OnButtonAddClicked;
        }

        protected void OnButtonAddClicked(object sender, EventArgs e)
        {
            var empty = new DeliveryPointResponsiblePerson();
            ResponsiblePersonsList.Add(empty);
        }
    }
}
