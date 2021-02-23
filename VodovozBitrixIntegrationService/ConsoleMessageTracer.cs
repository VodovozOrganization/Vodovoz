using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using NLog;

namespace VodovozBitrixIntegrationService
{
    public class ConsoleMessageTracer : IDispatchMessageInspector
    {
        static Logger logger = LogManager.GetCurrentClassLogger();

        enum Action
        {
            Send,
            Receive
        };

        private Message TraceMessage(MessageBuffer buffer, Action action)
        {
            Message msg = buffer.CreateMessage();
            try {
                if(action == Action.Receive) {
                    logger.Info("Received: {0}", msg.Headers.To.AbsoluteUri);
                    if(!msg.IsEmpty)
                        logger.Debug("Received Body: {0}", msg);
                } else
                    logger.Debug("Sended: {0}", msg);
            }
            catch(Exception ex) {
                logger.Error(ex, "Ошибка логгирования сообщения.");
            }
            return buffer.CreateMessage();
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            logger.Info($"Headers.Action {request.Headers.Action}");
            logger.Info($"Headers.From {request.Headers.From}");
            logger.Info($"Headers.To {request.Headers.To}");
            logger.Info($"Headers.Count {request.Headers.Count}");
            logger.Info($"Properties.Keys {request.Properties.Keys.ToString()}");
            logger.Info($"Properties.Keys.Count {request.Properties.Keys.Count}");
            logger.Info($"Properties.Properties.Count {request.Properties.Count}");
            logger.Info($"Properties.Properties.Values {request.Properties.Values}");
            foreach (var keyValuePair in request.Properties){
                logger.Info($"{keyValuePair.Key} => {keyValuePair.Value}");
            }
            
            logger.Info($"Properties.Properties.Via.Segments {request.Properties.Via.Segments}");
            logger.Info($"Properties.request.IsEmpty {request.IsEmpty}");
            
            request = TraceMessage(request.CreateBufferedCopy(int.MaxValue), Action.Receive);
            
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            reply = TraceMessage(reply.CreateBufferedCopy(int.MaxValue), Action.Send);
        }
    }
}