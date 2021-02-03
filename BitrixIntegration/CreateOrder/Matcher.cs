using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BitrixApi.DTO;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Repositories;
using VodovozInfrastructure.Utils;
using Contact = BitrixApi.DTO.Contact;

namespace BitrixIntegration {
    public class Matcher {
        public static Order MatchOrderByBitrixId(/*IUnitOfWork uow,*/ Deal deal)
        {
            using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
            {

                return OrderSingletonRepository.GetInstance().GetOrderByBitrixId(uow, deal.ID);
            }

        }
            
        

        //TODO gavr как то обрабатывать ситуацию с компанией/чатсным лицом
        public static bool MatchContact(IUnitOfWork uow, Contact contact)
        {
            throw new NotImplementedException();
        }

        public static bool MatchCompany(Company company)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Находит 
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="phone"></param>
        /// <param name="contact"></param>
        /// <param name="counterparty"></param>
        /// <exception cref="NullReferenceException">This exception is thrown if the archive already exists</exception>
        /// <returns></returns>
        public static bool MatchCounterpartyByPhoneAndSecondName(/*IUnitOfWork uow,*/ Contact contact, out Counterparty counterparty)
        {

            var uow = UnitOfWorkFactory.CreateWithoutRoot();
            //Формат записанный в Value +7 (981) 944-86-31
            var phone = contact.PHONE.First().VALUE;
            var digitsNum = PhoneUtils.NumberTrim(phone, out var _);

            digitsNum = "9215667037";
            var counterpartiesByPhone = CounterpartyRepository.GetCounterpartesByPhone(uow, digitsNum);
            IList<Counterparty> counterpartiesByName = null;
            
            counterpartiesByName = CounterpartyRepository.GetCounterpartesByPartOfName(
                uow,
                contact.SECOND_NAME?? contact.LAST_NAME ?? contact.NAME ?? 
                    throw new NullReferenceException("Контакт не содержит имени, фамилии и отчества, необходимых для сопоставления")
            );
            
            var b = new HashSet<Counterparty>();
            b.UnionWith(counterpartiesByPhone);
            b.UnionWith(counterpartiesByName);
            if (b.Count == 1){
                counterparty = b.First();
                return true;
            }
            else{
                counterparty = null;
                return false;
            }
        }



        public static bool MatchOrderItem(Product product)
        {
            throw new NotImplementedException();

        }

        public static bool MatchDeliveryPoint(){
            throw new NotImplementedException();
        }
        
    }
}