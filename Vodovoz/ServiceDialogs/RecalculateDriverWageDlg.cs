using System;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.ViewModel;
using QS.Project.Services;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using System.Collections;
using System.Collections.Generic;
using NHibernate.Util;
using QS.Dialog;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Core.DataService;

namespace Vodovoz.ServiceDialogs
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class RecalculateDriverWageDlg : QS.Dialog.Gtk.TdiTabBase
    {
        public RecalculateDriverWageDlg()
        {
            this.Build();
            TabName = "Пересчет ЗП водителей";
            ConfigureDlg();
        }

        private void ConfigureDlg()
        {
            var filterDriver = new EmployeeFilterViewModel();
            filterDriver.SetAndRefilterAtOnce(
                x => x.Status = EmployeeStatus.IsWorking
            );
            entryDriver.RepresentationModel = new EmployeesVM(filterDriver);
            datePicker.IsEditable = true;

            buttonRecalculate.Clicked += ButtonRecalculate_Clicked;
            buttonRecalculateForwarder.Clicked += ButtonRecalculateForwarder_Clicked;;
        }

        void ButtonRecalculate_Clicked(object sender, EventArgs e)
        {
            var driver = entryDriver.Subject as Employee;
            if (driver == null) {
                throw new ArgumentNullException("Не выбран водитель!");
            }
            if(datePicker.DateOrNull == null) {
                throw new ArgumentNullException("Не выбрана дата!");
            }

            using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
            {
                RouteList routeListAlias = null;
                var rls = uow.Session.QueryOver<RouteList>(() => routeListAlias)
                    .Where(() => routeListAlias.Driver.Id == driver.Id)
                    .Where(() => routeListAlias.ClosingDate >= datePicker.Date)
                    .List();
                if (!rls.Any()) {
                    ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Info, "Не найдено МЛ");
                    return;
                }

                var message = $"Будут очищены расчеты ЗП в {rls.Count} найденных МЛ и пересчитаны заново с текущими параметрами расчета. Продолжить?";
                if(ServicesConfig.InteractiveService.Question(message, "Внимание!"))
                {
                    RecalculateDriverWages(uow, rls);
                }
            }
        }

        void ButtonRecalculateForwarder_Clicked(object sender, EventArgs e)
        {
            var forwarder = entryDriver.Subject as Employee;
            if (forwarder == null)
            {
                throw new ArgumentNullException("Не выбран экспедитор!");
            }
            if (datePicker.DateOrNull == null)
            {
                throw new ArgumentNullException("Не выбрана дата!");
            }

            using (var uow = UnitOfWorkFactory.CreateWithoutRoot())
            {
                RouteList routeListAlias = null;
                var rls = uow.Session.QueryOver<RouteList>(() => routeListAlias)
                    .Where(() => routeListAlias.Forwarder.Id == forwarder.Id)
                    .Where(() => routeListAlias.ClosingDate >= datePicker.Date)
                    .List();
                
                if (!rls.Any())
                {
                    ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Info, "Не найдено МЛ");
                    return;
                }

                var message = $"Будут очищены расчеты ЗП в {rls.Count} найденных МЛ и пересчитаны заново с текущими параметрами расчета. Продолжить?";
                if (ServicesConfig.InteractiveService.Question(message, "Внимание!"))
                {
                    RecalculateForwarderWages(uow, rls);
                }
            }
        }

        private void RecalculateDriverWages(IUnitOfWork uow, IList<RouteList> rls)
        {
            WageParameterService wageParameterService = new WageParameterService(
                WageSingletonRepository.GetInstance(), 
                new BaseParametersProvider()
            );
            foreach (var rl in rls)
            {
                foreach (var address in rl.Addresses)
                {
                    address.DriverWageCalculationMethodic = null;
                }
                rl.RecalculateAllWages(wageParameterService);
                rl.UpdateWageOperation();
                uow.Save(rl);
            }
            uow.Commit();
        }

        private void RecalculateForwarderWages(IUnitOfWork uow, IList<RouteList> rls)
        {
            WageParameterService wageParameterService = new WageParameterService(
                WageSingletonRepository.GetInstance(),
                new BaseParametersProvider()
            );
            foreach (var rl in rls)
            {
                foreach (var address in rl.Addresses)
                {
                    address.ForwarderWageCalculationMethodic = null;
                }
                rl.RecalculateAllWages(wageParameterService);
                rl.UpdateWageOperation();
                uow.Save(rl);
            }
            uow.Commit();
        }
    }
}
