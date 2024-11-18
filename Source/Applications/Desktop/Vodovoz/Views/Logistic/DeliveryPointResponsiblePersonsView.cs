using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Autofac;
using Gamma.Widgets;
using Gtk;
using NLog;
using QS.DomainModel.UoW;
using QS.Widgets.GtkUI;
using QSWidgetLib;
using Vodovoz.Domain.Client;
using Vodovoz.TempAdapters;

namespace Vodovoz.Views.Logistic
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class DeliveryPointResponsiblePersonsView : Gtk.Bin
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IEmployeeJournalFactory _employeeJournalFactory;
        private GenericObservableList<DeliveryPointResponsiblePerson> responsiblePersonsList;
        private IList<DeliveryPointResponsiblePersonType> responsiblePersonTypes;
        private IUnitOfWork uow;
        private ILifetimeScope _scope = Startup.AppDIContainer.BeginLifetimeScope();

        public IUnitOfWork UoW {
            get => uow;
            set
            {
                uow = value;
                responsiblePersonTypes = uow.Session.QueryOver<DeliveryPointResponsiblePersonType>().List();
            }
        }

        public DeliveryPoint DeliveryPoint { get; set; }

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
				if(value != null)
				{
					ResponsiblePersonsList.ElementAdded += OnResponsiblePersonsElementAdded;
					ResponsiblePersonsList.ElementRemoved += OnResponsiblePersonsElementRemoved;

					foreach(DeliveryPointResponsiblePerson responsiblePerson in ResponsiblePersonsList)
					{
						AddResponsiblePersonRow(responsiblePerson);
					}
				}
			}
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

            Label textPhoneLabel = new Label("+7");
            datatableResponsiblePersons.Attach(
                textPhoneLabel,
                (uint)2, (uint)3, rowsCount, rowsCount + 1,
                (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

            var phoneDataEntry = new yValidatedEntry();
            phoneDataEntry.ValidationMode = ValidationType.phone;
            phoneDataEntry.Tag = responsiblePerson;
            phoneDataEntry.WidthChars = 19;
            phoneDataEntry.WidthRequest = 100;
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
            employeeEntry.CanDisposeEntitySelectorFactory = false;
            employeeEntry.WidthRequest = 200;
            employeeEntry.SetEntityAutocompleteSelectorFactory(_employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory());

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

            Button copyToAllDeliveryPointsButton = new Button();
            copyToAllDeliveryPointsButton.Label = "Добавить ко всем точкам доставки контрагента";
            copyToAllDeliveryPointsButton.Clicked += OnButtonCopyToAllDeliveryPointsClicked;
            datatableResponsiblePersons.Attach(
                copyToAllDeliveryPointsButton,
                (uint)7, (uint)8, rowsCount, rowsCount + 1,
                (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

            datatableResponsiblePersons.ShowAll();
        }

        private void OnButtonCopyToAllDeliveryPointsClicked(object sender, EventArgs e)
        {
            Table.TableChild delButtonInfo = ((Table.TableChild)(this.datatableResponsiblePersons[(Widget)sender]));
            Widget foundWidget = null;
            foreach (Widget wid in datatableResponsiblePersons.AllChildren)
            {
                if (wid is yValidatedEntry && delButtonInfo.TopAttach == (datatableResponsiblePersons[wid] as Table.TableChild).TopAttach)
                {
                    foundWidget = wid;
                    break;
                }
            }
            if (foundWidget == null)
            {
                logger.Warn("Не найден виджет ассоциированный с телефоном.");
                return;
            }

            var responsiblePersonToCopy = (DeliveryPointResponsiblePerson)(foundWidget as yValidatedEntry).Tag;

            var currentDeliveryPoint = responsiblePersonToCopy.DeliveryPoint;

            foreach (var deliveryPoint in currentDeliveryPoint.Counterparty.DeliveryPoints)
            {
                if (currentDeliveryPoint == deliveryPoint)
                {
                    continue;
                }
                if (deliveryPoint.ResponsiblePersons.Any(x 
                    => x.DeliveryPointResponsiblePersonType == responsiblePersonToCopy.DeliveryPointResponsiblePersonType
                    && x.Employee == responsiblePersonToCopy.Employee
                    && x.Phone == responsiblePersonToCopy.Phone
                )){
                    continue;
                }

                deliveryPoint.ResponsiblePersons.Add(
                    new DeliveryPointResponsiblePerson()
                    {
                        DeliveryPointResponsiblePersonType = responsiblePersonToCopy.DeliveryPointResponsiblePersonType,
                        DeliveryPoint = deliveryPoint,
                        Phone = responsiblePersonToCopy.Phone,
                        Employee = responsiblePersonToCopy.Employee
                    }
                );
            }
        }

        private void OnButtonDeleteClicked(object sender, EventArgs e)
        {
            Table.TableChild delButtonInfo = ((Table.TableChild)(this.datatableResponsiblePersons[(Widget)sender]));
            Widget foundWidget = null;
            foreach (Widget wid in datatableResponsiblePersons.AllChildren)
            {
                if (wid is yValidatedEntry && delButtonInfo.TopAttach == (datatableResponsiblePersons[wid] as Table.TableChild).TopAttach)
                {
                    foundWidget = wid;
                    break;
                }
            }
            if (foundWidget == null)
            {
                logger.Warn("Не найден виджет ассоциированный с удаленным телефоном.");
                return;
            }

            ResponsiblePersonsList.Remove((DeliveryPointResponsiblePerson)(foundWidget as yValidatedEntry).Tag);
        }

        protected void MoveRowUp(uint Row)
        {
            foreach (Widget w in datatableResponsiblePersons.Children)
                if (((Table.TableChild)(this.datatableResponsiblePersons[w])).TopAttach == Row)
                {
                    uint Left = ((Table.TableChild)(this.datatableResponsiblePersons[w])).LeftAttach;
                    uint Right = ((Table.TableChild)(this.datatableResponsiblePersons[w])).RightAttach;
                    datatableResponsiblePersons.Remove(w);
                    if (w.GetType() == typeof(yListComboBox))
                        datatableResponsiblePersons.Attach(w, Left, Right, Row - 1, Row, AttachOptions.Fill | AttachOptions.Expand, (AttachOptions)0, (uint)0, (uint)0);
                    else
                        datatableResponsiblePersons.Attach(w, Left, Right, Row - 1, Row, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);
                }
        }

        private void OnResponsiblePersonsElementRemoved(object aList, int[] aIdx, object aObject)
        {
            Widget foundWidget = null;

            foreach (Widget widget in datatableResponsiblePersons.AllChildren) {
                if (widget is yValidatedEntry && (widget as yValidatedEntry).Tag == aObject)
                {
                    foundWidget = widget;
                    break;
                }
            }
            if (foundWidget == null)
            {
                logger.Warn("Не найден виджет ассоциированный с удаленным ответственным лицом.");
                return;
            }

            Table.TableChild child = ((Table.TableChild)(this.datatableResponsiblePersons[foundWidget]));
            RemoveRow(child.TopAttach);
        }

        private void RemoveRow(uint Row)
        {
            foreach (Widget w in datatableResponsiblePersons.Children)
                if (((Table.TableChild)(this.datatableResponsiblePersons[w])).TopAttach == Row)
                {
                    datatableResponsiblePersons.Remove(w);
                    w.Destroy();
                }
            for (uint i = Row + 1; i < datatableResponsiblePersons.NRows; i++)
                MoveRowUp(i);
            datatableResponsiblePersons.NRows = --Row;
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
	        _employeeJournalFactory = new EmployeeJournalFactory(_scope);
            Build();
        }

        protected void OnButtonAddClicked(object sender, EventArgs e)
        {
            var empty = new DeliveryPointResponsiblePerson() { DeliveryPoint = DeliveryPoint }; 
            ResponsiblePersonsList.Add(empty);
        }

        internal void RemoveEmpty()
        {
            ResponsiblePersonsList?.Where(
                p => p.Phone?.Length == 0 
                  || p.DeliveryPoint == null 
                  || p.DeliveryPointResponsiblePersonType == null 
                  || p.Employee == null
                ).ToList().ForEach(p => ResponsiblePersonsList.Remove(p));
        }

        protected override void OnDestroyed()
        {
	        if(_scope != null)
	        {
		        _scope.Dispose();
		        _scope = null;
	        }
	        base.OnDestroyed();
        }
    }
}
