using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml;

namespace FrameworkSupport
{
    public class SoapXmlLogger : IEndpointBehavior
    {
        private readonly ThreadedLogger _tLogger;

        public SoapXmlLogger(ThreadedLogger tLogger)
        {
            _tLogger = tLogger;
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            // add the inspector to the client runtime
            clientRuntime.ClientMessageInspectors.Add(new DebugMessageInspector(_tLogger));
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }
    }

    public class DebugMessageInspector : IClientMessageInspector
    {
        private readonly ThreadedLogger _tLogger;

        public DebugMessageInspector(ThreadedLogger tLogger)
        {
            _tLogger = tLogger;
        }

        public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel)
        {
            return LogAndReturnRequest(ref request);
        }

        /// <summary>
        /// Logs the going out request message details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private System.ServiceModel.Channels.Message LogAndReturnRequest(ref System.ServiceModel.Channels.Message request)
        {
            return LogMessage("Request", ref request);
        }

        /// <summary>
        /// Logs the response message details
        /// </summary>
        /// <param name="reply"></param>
        /// <param name="correlationState"></param>
        public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            LogMessage("Response", ref reply);
        }

        /// <summary>
        /// Log the message to our threaded logger set on the class
        /// </summary>
        /// <param name="sendType"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private System.ServiceModel.Channels.Message LogMessage(string sendType, ref System.ServiceModel.Channels.Message message)
        {
            // Messages can only be read once, at which point they are practically useless and need to be disposed

            using (MessageBuffer bufferedCopy = message.CreateBufferedCopy(Int32.MaxValue))
            {
                // Creating a buffered copy reads the original message, so we need to create a new one to restore the original message
                message = bufferedCopy.CreateMessage();

                // Create a separate copy to use for logging since we may need to redact sensitive info and we don't want to modify the original message
                using (System.ServiceModel.Channels.Message messageCopy = bufferedCopy.CreateMessage())
                {
                    XmlDocument xmlDoc = CreateXmlFromMessage(messageCopy);

                    // Todo: refactor this in a way that makes this method general so that the redacting functionality can be used for other services besides Telecharge
                    if (xmlDoc.GetElementsByTagName("SignOn").Count > 0)
                    {
                        XmlDocumentExtensions.Redact(xmlDoc, "Password");
                    }

                    string xmlAsString = xmlDoc.OuterXml; // Todo: prettify this
                    _tLogger.Info($"{sendType} - {xmlAsString}");
                }
            }

            return message;
        }

        private XmlDocument CreateXmlFromMessage(System.ServiceModel.Channels.Message message)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(ms))
                {
                    message.WriteMessage(writer);
                    writer.Flush();
                    ms.Position = 0;
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.PreserveWhitespace = true;
                    xmlDoc.Load(ms);

                    return xmlDoc;
                }
            }
        }
    }
}