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
        public static Order MatchOrderByBitrixId(IUnitOfWork uow, Deal deal) =>
            OrderSingletonRepository.GetInstance().GetOrderByBitrixId(uow, deal.ID);
            
        

        //TODO gavr как то обрабатывать ситуацию с 
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
        /// <returns></returns>
        public static bool MatchCounterpartyByPhoneAndSecondName(IUnitOfWork uow, Phone phone, Contact contact, out Counterparty counterparty) {
            //Формат записанный в Value +7 (981) 944-86-31
            var a = phone.VALUE;
            PhoneUtils.NumberTrim(phone.VALUE, out var _);
            var counterpartiesByPhone = CounterpartyRepository.GetCounterpartesByPhone(uow, phone.VALUE);
            var counterpartiesByName = CounterpartyRepository.GetCounterpartesBySecondName(uow, contact.SECOND_NAME);
            var b = new HashSet<Counterparty>();
            b.UnionWith(counterpartiesByPhone.Keys);
            b.UnionWith(counterpartiesByName.Keys);
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