using System.Windows.Forms;
using Microsoft.Azure;
using Microsoft.ServiceBus;
using System;
using Microsoft.ServiceBus.Messaging;
using System.Diagnostics;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Table;
using SBQWorker;
using System.Collections.Generic;
using System.Drawing;

namespace AzureClient {
    public partial class Form1 : Form {
        static QueueClient queueClient;
        static SubscriptionClient subClient;
        static IEnumerable<UserEntity> query;
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
           // MessageData md = new MessageData("hello", MessagePurpose.Initialize);
           // BrokeredMessage bm = new BrokeredMessage(md);
           // queueClient.Send(bm);

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
                    MessageData md = message.GetBody<MessageData>();
                    query = md.Query;
                    // Remove message from subscription.
                    message.Complete();
                } catch (Exception e) {
                    // Indicates a problem, unlock message in subscription.
                    Debug.WriteLine(e.StackTrace);
                    message.Abandon();
                }
            }, options);
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            string user = textBox.Text;
            MessageData md = new MessageData(user, "connect message", MessagePurpose.Connect);
            BrokeredMessage bm = new BrokeredMessage(md);
            queueClient.Send(bm);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (query != null)
            {
                float yInc = 0.0F;
                var g = e.Graphics;
                foreach (UserEntity entity in query)
                {
                    // Create font and brush.
                    Font drawFont = new Font("Arial", 16);
                    SolidBrush drawBrush = new SolidBrush(Color.Black);

                    // Create point for upper-left corner of drawing.
                    PointF drawPoint = new PointF(225.0F, 20.0F + yInc);

                    g.DrawString($"{entity.PartitionKey}, {entity.RowKey}\t{entity.Message}", drawFont, drawBrush, drawPoint);
                    yInc += 20.0F;
                }
            }
        }
    }
}
