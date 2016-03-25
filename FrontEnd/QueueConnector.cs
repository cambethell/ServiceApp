using Microsoft.Azure;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FrontEnd {
    public class QueueConnector {
        // Thread-safe. Recommended that you cache rather than recreating it
        // on every request.
        public static QueueClient Client;

        // Obtain these values from the portal.
        public const string Namespace = "queuebus.servicebus.windows.net/";

        // The name of your queue.
        public const string QueueName = "queuebus";

        public static NamespaceManager CreateNamespaceManager() {
            // Create the namespace manager which gives you access to
            // management operations.
            var uri = ServiceBusEnvironment.CreateServiceUri(
                "sb", Namespace, String.Empty);
            var tP = TokenProvider.CreateSharedAccessSignatureTokenProvider(
                "RootManageSharedAccessKey", "5ZVTXEEBHiLZ0+PXkS0Vaq5or3XgG9HLRElx2JRGcaQ=");
            return new NamespaceManager(uri, tP);
        }

        public static void Initialize() {
            string connectionString =
             CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            var namespaceManager =
             NamespaceManager.CreateFromConnectionString(connectionString);

            if (!namespaceManager.QueueExists(QueueName)) {
                namespaceManager.CreateQueue(QueueName);
            }

            // Initialize the connection to Service Bus queue.
            Client = QueueClient.CreateFromConnectionString(connectionString, QueueName);
        }
    }
}