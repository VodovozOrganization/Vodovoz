using System;
using QS.Commands;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Payments;
using Vodovoz.ViewModelBased;
using Vodovoz.ViewModels.WageCalculation;
using TabViewModelBase = QS.ViewModels.TabViewModelBase;

namespace Vodovoz.ViewModels.ViewModels.Payments
{
    public class ImportRegisterOfPaymentsFromYookassaViewModel : TabViewModelBase
    {
        private PaymentsFromYookassaParser parser;
        
        public ImportRegisterOfPaymentsFromYookassaViewModel(
            IInteractiveService interactiveService,
            INavigationManager navigationManager) : base (interactiveService, navigationManager)
        {
            TabName = "Загрузка оплат из Юкасса";
        }

        private DelegateCommand importRegisterPayments = null;
        public DelegateCommand ImportRegisterPayments => importRegisterPayments ?? (
            importRegisterPayments = new DelegateCommand(
                () => Save(),
                () => true
            )    
        );
        
        private DelegateCommand<string> readFileCommand = null;
        public DelegateCommand<string> ReadFileCommand => readFileCommand ?? (
            readFileCommand = new DelegateCommand<string>(
                (docPath) =>
                {
                    parser = new PaymentsFromYookassaParser(docPath);
                    parser.Parse();
                },
                (docPath) => !string.IsNullOrEmpty(docPath)
            )    
        );

        private void Save()
        {
            
        }
    }
}
