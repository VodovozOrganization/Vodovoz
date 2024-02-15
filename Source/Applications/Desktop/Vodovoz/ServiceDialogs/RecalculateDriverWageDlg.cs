using System;
using System.Collections.Generic;
using NHibernate.Util;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Services;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.WageCalculation;
using NHibernate.Criterion;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;

namespace Vodovoz.ServiceDialogs
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class RecalculateDriverWageDlg : QS.Dialog.Gtk.TdiTabBase, ISingleUoWDialog
    {
	    private readonly IWageCalculationRepository _wageCalculationRepository = new WageCalculationRepository();
	    private readonly WageParameterService _wageParameterService;
	    public IUnitOfWork UoW { get; }

	    public RecalculateDriverWageDlg()
        {
            this.Build();
            TabName = "Пересчет ЗП водителей";
            UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
            ConfigureDlg();
            _wageParameterService =
	            new WageParameterService(_wageCalculationRepository, new BaseParametersProvider(new ParametersProvider()));
        }

        private void ConfigureDlg()
        {
            var employeeFactory = new EmployeeJournalFactory(Startup.MainWin.NavigationManager);
            evmeDriver.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateWorkingEmployeeAutocompleteSelectorFactory());
            datePickerFrom.IsEditable = true;
            datePickerTo.IsEditable = true;

            buttonRecalculate.Clicked += ButtonRecalculate_Clicked;
            buttonRecalculateForwarder.Clicked += ButtonRecalculateForwarder_Clicked;;
        }

        void ButtonRecalculate_Clicked(object sender, EventArgs e)
        {
            var driver = evmeDriver.Subject as Employee;

            if(datePickerFrom.DateOrNull == null){
                throw new ArgumentNullException("Не выбрана дата с!");
            }

            if(datePickerTo.DateOrNull == null){
                throw new ArgumentNullException("Не выбрана дата по!");
            }

            using (var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot())
            {
                var dateTimeFrom = datePickerFrom.Date.Date;
                var dateTimeTo = datePickerTo.Date.Date.AddDays(1).AddMilliseconds(-1);

                RouteList routeListAlias = null;
                var rlsQuery = uow.Session.QueryOver<RouteList>(() => routeListAlias);
                if(driver != null){
                    rlsQuery.Where(() => routeListAlias.Driver.Id == driver.Id);
                }
                    
                var rls = rlsQuery
                    .Where(() => routeListAlias.ClosingDate >= dateTimeFrom)
                    .Where(() => routeListAlias.ClosingDate <= dateTimeTo)
                    .List();
                if (!rls.Any()) {
                    ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Info, "Не найдено МЛ");
                    return;
                }

                var message = $"Будут очищены расчеты ЗП в {rls.Count} найденных МЛ (с {dateTimeFrom} по {dateTimeTo}) и пересчитаны заново с текущими параметрами расчета. Продолжить?";
                if(ServicesConfig.InteractiveService.Question(message, "Внимание!"))
                {
                    RecalculateDriverWages(uow, rls);
                }
            }
        }

        void ButtonRecalculateForwarder_Clicked(object sender, EventArgs e)
        {
            var forwarder = evmeDriver.Subject as Employee;

            if(datePickerFrom.DateOrNull == null) {
                throw new ArgumentNullException("Не выбрана дата с!");
            }

            if(datePickerTo.DateOrNull == null){
                throw new ArgumentNullException("Не выбрана дата по!");
            }

            using (var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot())
            {
                var dateTimeFrom = datePickerFrom.Date.Date;
                var dateTimeTo = datePickerTo.Date.Date.AddDays(1).AddMilliseconds(-1);

                RouteList routeListAlias = null;
                var rlsQuery = uow.Session.QueryOver<RouteList>(() => routeListAlias);

                if(forwarder != null) {
                    rlsQuery.Where(() => routeListAlias.Forwarder.Id == forwarder.Id);
                }
                else{
                    rlsQuery.Where(Restrictions.IsNotNull(Projections.Property(() => routeListAlias.Forwarder)));
                }

                var rls = rlsQuery
                    .Where(() => routeListAlias.ClosingDate >= dateTimeFrom)
                    .Where(() => routeListAlias.ClosingDate <= dateTimeTo)
                    .List();
                
                if (!rls.Any())
                {
                    ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Info, "Не найдено МЛ");
                    return;
                }

                var message = $"Будут очищены расчеты ЗП в {rls.Count} найденных МЛ (с {dateTimeFrom} по {dateTimeTo}) и пересчитаны заново с текущими параметрами расчета. Продолжить?";
                if (ServicesConfig.InteractiveService.Question(message, "Внимание!"))
                {
                    RecalculateForwarderWages(uow, rls);
                }
            }
        }

        private void RecalculateDriverWages(IUnitOfWork uow, IList<RouteList> rls)
        {
            foreach (var rl in rls)
            {
                foreach (var address in rl.Addresses)
                {
                    address.DriverWageCalculationMethodic = null;
                }
                rl.RecalculateAllWages(_wageParameterService);
                rl.UpdateWageOperation();
                uow.Save(rl);
            }
            uow.Commit();
        }

        private void RecalculateForwarderWages(IUnitOfWork uow, IList<RouteList> rls)
        {
	        foreach (var rl in rls)
            {
                foreach (var address in rl.Addresses)
                {
                    address.ForwarderWageCalculationMethodic = null;
                }
                rl.RecalculateAllWages(_wageParameterService);
                rl.UpdateWageOperation();
                uow.Save(rl);
            }
            uow.Commit();
        }
    }
}
