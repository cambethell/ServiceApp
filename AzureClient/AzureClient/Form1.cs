using System.Windows.Forms;
using Microsoft.Azure;
using Microsoft.ServiceBus;
using System;
using Microsoft.ServiceBus.Messaging;
using System.Diagnostics;
using System.Configuration;

namespace AzureClient {
    public partial class Form1 : Form {
        static QueueClient queueClient;
        static SubscriptionClient subClient;
        const string queueName = "queuebus";
        const string topicName = "subtopic";
        const string subName = "UpdateMessages";
        static string NameSpace = ConfigurationManager.AppSettings["NameSpace"];
        static string SharedAccessKey = ConfigurationManager.AppSettings["SharedAccessKey"];
        static string SharedAccessKeyname = ConfigurationManager.AppSettings["SharedAccessKeyname"];
        static string ConnectionString = "Endpoint=sb://" + NameSpace + ";SharedAccessKeyName=" + SharedAccessKeyname + ";SharedAccessKey=" + SharedAccessKey;

        public Form1() {
            InitializeComponent();
        }

        private void LoadForm(object sender, EventArgs e) {
            Initialize();
        }

        // Create the namespace manager which gives you access to
        // management operations.
        public static NamespaceManager CreateNamespaceManager() {
            var uri = ServiceBusEnvironment.CreateServiceUri("sb", NameSpace, string.Empty);
            var tp = TokenProvider.CreateSharedAccessSignatureTokenProvider(SharedAccessKeyname, SharedAccessKey);
            return new NamespaceManager(uri, tp);
        }

        public static void Initialize() {
            var namespaceManager = CreateNamespaceManager();

            // Initialize the connection to Service Bus queue.
            queueClient = QueueClient.CreateFromConnectionString(ConnectionString, queueName);

            // Send Initial handshake
            MessageData md = new MessageData("", MessagePurpose.Initialize);
            BrokeredMessage bm = new BrokeredMessage(md);
            queueClient.Send(bm);

            if (!namespaceManager.SubscriptionExists(topicName, subName)) {
                namespaceManager.CreateSubscription(topicName, subName);
                Debug.WriteLine("creating new sub");
            }

            subClient = SubscriptionClient.CreateFromConnectionString(ConnectionString, topicName, subName);

            // Configure the callback options.
            OnMessageOptions options = new OnMessageOptions();
            options.AutoComplete = false;
            options.AutoRenewTimeout = TimeSpan.FromMinutes(1);

            subClient.OnMessage((message) => {
                try {
                    // Process message from subscription.
                    Debug.WriteLine("\n**Update Messages**");
                    Debug.WriteLine("Body: " + message.GetBody<MessageData>());
                    Debug.WriteLine("MessageID: " + message.MessageId);
                    Debug.WriteLine("Message Number: " +
                        message.Properties["MessageNumber"]);

                    // Remove message from subscription.
                    message.Complete();
                } catch (Exception) {
                    // Indicates a problem, unlock message in subscription.
                    message.Abandon();
                }
            }, options);
        }

    }
}
