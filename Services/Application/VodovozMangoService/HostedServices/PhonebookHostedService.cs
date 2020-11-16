using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MangoService;
using Microsoft.Extensions.Hosting;

namespace VodovozMangoService.HostedServices
{
    public class PhonebookHostedService : PhonebookService.PhonebookServiceBase, IHostedService
    {
        private readonly MangoController mangoController;
        private readonly CallsHostedService callsService;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

        private List<PhoneEntry> phones = new List<PhoneEntry>();

        #region GRPC

        public override Task<PhoneBook> GetBook(Empty request, ServerCallContext context)
        {
            var book = new PhoneBook();
            foreach (var phone in phones)
            {
                if(phone.PhoneType == PhoneEntryType.Extension)
                    phone.PhoneState = callsService.Calls.Values.Any(c =>
                        (c.LastEvent.to.Extension == phone.Extension || c.LastEvent.from.Extension == phone.Extension) &&
                        c.IsActive)
                        ? PhoneState.Busy
                        : PhoneState.Ready;
                book.Entries.Add(phone);
            }
            return Task.FromResult(book);
        }

        #endregion
        
        #region Timer
        private Timer timer;

        public PhonebookHostedService(MangoController mangoController, CallsHostedService callsService)
        {
            this.mangoController = mangoController ?? throw new ArgumentNullException(nameof(mangoController));
            this.callsService = callsService ?? throw new ArgumentNullException(nameof(callsService));
        }

        private void RefreshPhones(object state)
        {
            var list = new List<PhoneEntry>();
            foreach(var user in mangoController.GetAllVPBXUsers())
            {
                if (uint.TryParse(user.telephony.extension, out uint extension))
                {
                    list.Add(new PhoneEntry
                    {
                        Extension = extension,
                        Name = user.general.name,
                        Department = user.general.department ?? String.Empty,
                        PhoneType = PhoneEntryType.Extension
                    });
                }
            }

            foreach(var group in mangoController.GetAllVpbxGroups())
            {
                if (uint.TryParse(group.extension, out uint extension))
                {
                    list.Add(new PhoneEntry
                    {
                        Extension = extension,
                        Name = group.name,
                        Department = group.descpription ?? String.Empty,
                        PhoneType = PhoneEntryType.Group,
                        PhoneState = PhoneState.Ready
                    });
                }
            }

            phones = list;
        }
        #endregion

        #region FindPhone

        public PhoneEntry FindPhone(string number)
        {
            if (uint.TryParse(number, out uint extension))
            {
                var phone = phones.Find(x => x.Extension == extension);
                if (phone == null)
                {
                    RefreshPhones(null);
                    phone = phones.Find(x => x.Extension == extension);
                }
                return phone;
            }
            return null;
        }

        #endregion
        
        #region IHostedService
        public Task StartAsync(CancellationToken cancellationToken)
        {
             logger.Info("Сервис телефонной книги запущен.");
             timer = new Timer(RefreshPhones, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));
             return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.Info("Сервис телефонной книги остановлен.");

            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
        #endregion
    }
}