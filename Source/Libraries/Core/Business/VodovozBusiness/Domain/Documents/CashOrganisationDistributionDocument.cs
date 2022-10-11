using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Domain.Documents
{
    [Appellative(Gender = GrammaticalGender.Masculine,
        NominativePlural = "Документы распределения налички по юр лицу",
        Nominative = "Документ распределения налички по юр лицу")]
    public class CashOrganisationDistributionDocument : PropertyChangedBase, IDomainObject
    {
        public virtual int Id { get; set; }

        private Employee author;
        [Display (Name = "Автор")]
        public virtual Employee Author
        {
            get => author;
            set => SetField(ref author, value);
        }
        
        private Employee employee;
        [Display (Name = "Сотрудник")]
        public virtual Employee Employee
        {
            get => employee;
            set => SetField(ref employee, value);
        }
        
        private Employee lastEditor;
        [Display (Name = "Последний редактор документа")]
        public virtual Employee LastEditor
        {
            get => lastEditor;
            set => SetField(ref lastEditor, value);
        }
        
        private DateTime creationDate;
        [Display (Name = "Дата создания")]
        public virtual DateTime CreationDate
        {
            get => creationDate;
            set => SetField(ref creationDate, value);
        }
        
        private DateTime lastEditedTime;
        [Display (Name = "Время последнего изменения")]
        public virtual DateTime LastEditedTime
        {
            get => lastEditedTime;
            set => SetField(ref lastEditedTime, value);
        }
        
        private Organization organisation;
        [Display (Name = "Организация")]
        public virtual Organization Organisation
        {
            get => organisation;
            set => SetField(ref organisation, value);
        }
        
        private OrganisationCashMovementOperation organisationCashMovementOperation;
        [Display (Name = "Операция распределения")]
        public virtual OrganisationCashMovementOperation OrganisationCashMovementOperation
        {
            get => organisationCashMovementOperation;
            set => SetField(ref organisationCashMovementOperation, value);
        }
        
        private Income income;
        [Display (Name = "Приход")]
        public virtual Income Income
        {
            get => income;
            set => SetField(ref income, value);
        }
        
        private Expense expense;
        [Display (Name = "Расход")]
        public virtual Expense Expense
        {
            get => expense;
            set => SetField(ref expense, value);
        }
        
        private decimal amount;
        [Display (Name = "Сумма")]
        public virtual decimal Amount
        {
            get => amount;
            set => SetField(ref amount, value);
        }
        
        public virtual CashOrganisationDistributionDocType Type { get; }
    }

    public enum CashOrganisationDistributionDocType
    {
        SelfDeliveryCashDistributionDoc,
        IncomeCashDistributionDoc,
        ExpenseCashDistributionDoc,
        AdvanceIncomeCashDistributionDoc,
        AdvanceExpenseCashDistributionDoc,
        RouteListItemCashDistributionDoc,
        FuelExpenseCashOrgDistributionDoc
    }
}