using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Employees;

namespace Vodovoz.Controllers
{
    public class AddressTransferController
    {
        public AddressTransferController(IEmployeeRepository employeeRepository)
        {
            this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        }
        
        private readonly IEmployeeRepository employeeRepository;

        public void UpdateDocuments(RouteList from, RouteList to, IUnitOfWork uow)
        {
            var transferredAddresses = from.Addresses
                .Where(x => to.Addresses.Contains(x.TransferedTo) && x.Status == RouteListItemStatus.Transfered)
                .ToList();

            if(!transferredAddresses.Any())
                DeleteAddressTransferDocuments(from, to, uow);
            else
                CreateOrUpdateAddressTransferDocuments(from, to, uow);
        }
        
        private void DeleteAddressTransferDocuments(RouteList from, RouteList to, IUnitOfWork uow)
        {
            var transferDocument = uow.Session.QueryOver<AddressTransferDocument>()
                .Where(x => x.RouteListFrom.Id == from.Id)
                .And(x => x.RouteListTo.Id == to.Id)
                .SingleOrDefault();
            
            if(transferDocument != null) {
                foreach(var documentItem in transferDocument.AddressTransferDocumentItems) {
                    uow.Delete(documentItem);
                }
                transferDocument.AddressTransferDocumentItems.Clear();
                
                uow.Delete(transferDocument);
            }
        }
        
        private void CreateOrUpdateAddressTransferDocuments(RouteList from, RouteList to, IUnitOfWork uow)
        {
            var transferDocument = uow.Session.QueryOver<AddressTransferDocument>()
                .Where(x => x.RouteListFrom.Id == from.Id)
                .And(x => x.RouteListTo.Id == to.Id)
                .SingleOrDefault() ?? new AddressTransferDocument();
            
            foreach(var documentItem in transferDocument.AddressTransferDocumentItems) {
                uow.Delete(documentItem);
            }
            transferDocument.AddressTransferDocumentItems.Clear();

            var employeeForCurrentUser = employeeRepository.GetEmployeeForCurrentUser(uow);

            if(transferDocument.Author == null)
                transferDocument.Author = employeeForCurrentUser;
            
            if(transferDocument.TimeStamp == default(DateTime))
                transferDocument.TimeStamp = DateTime.Now;
                
            transferDocument.LastEditor = employeeForCurrentUser;
            transferDocument.LastEditedTime = DateTime.Now;
            transferDocument.RouteListFrom = from;
            transferDocument.RouteListTo = to;
            
            transferDocument.UpdateItems();
            uow.Save(transferDocument);
        }
    }
}