using System.Windows.Forms;
using Microsoft.ServiceBus;
using System;
using Microsoft.ServiceBus.Messaging;
using System.Diagnostics;
using System.Configuration;
using SBQWorker;
using System.Collections.Generic;
using System.Drawing;

namespace AzureClient {
    public partial class Form1 : Form {
        QueueClient queueClient;
        SubscriptionClient subClient;
        IEnumerable<UserEntity> query;
        Button disconnectButton;

        const string queueName = "queuebus";
        const string topicName = "subtopic";
        const string subName = "UpdateMessages";
        string userName = "default";

        static string NameSpace = ConfigurationManager.AppSettings["NameSpace"];
        static string SharedAccessKey = ConfigurationManager.AppSettings["SharedAccessKey"];
        static string SharedAccessKeyname = ConfigurationManager.AppSettings["SharedAccessKeyname"];
        string ConnectionString = "Endpoint=sb://" + NameSpace +
                ";SharedAccessKeyName=" + SharedAccessKeyname +
                ";SharedAccessKey=" + SharedAccessKey;

        public Form1() {
            InitializeComponent();
        }

        private void LoadForm(object sender, EventArgs e) {
            Initialize();
        }

        // Create the namespace manager which gives you access to
        // management operations.
        public NamespaceManager CreateNamespaceManager() {
            var uri = ServiceBusEnvironment.CreateServiceUri("sb", NameSpace, string.Empty);
            var tp = TokenProvider.CreateSharedAccessSignatureTokenProvider(SharedAccessKeyname, SharedAccessKey);
            return new NamespaceManager(uri, tp);
        }

        public void Initialize() {
            var namespaceManager = CreateNamespaceManager();

            // Initialize the connection to Service Bus queue.
            queueClient = QueueClient.CreateFromConnectionString(ConnectionString, queueName);

            if (!namespaceManager.SubscriptionExists(topicName, subName)) {
                namespaceManager.CreateSubscription(topicName, subName);
                Debug.WriteLine("creating new sub");
            }

            subClient = SubscriptionClient.CreateFromConnectionString(ConnectionString, topicName, subName);

            // Configure the callback options.
            OnMessageOptions options = new OnMessageOptions();
            options.AutoComplete = false;
            options.AutoRenewTimeout = TimeSpan.FromSeconds(1);

            subClient.OnMessage((message) => {
                try {
                    // Process message from subscription.
                    MessageData md = message.GetBody<MessageData>();
                    query = md.Query;
                    Invalidate();

                    // Remove message from subscription.
                    message.Complete();
                } catch (Exception e) {
                    // Indicates a problem, unlock message in subscription.
                    Debug.WriteLine(e.StackTrace);
                    message.Abandon();
                }
            }, options);

            MessageData init = new MessageData(userName, "init message", MessagePurpose.Initialize);
            BrokeredMessage bm = new BrokeredMessage(init);
            queueClient.Send(bm);
        }

        private void Connect(object sender, EventArgs e) {
            userName = textBox.Text;
            MessageData connect = new MessageData(userName, "connect message", MessagePurpose.Connect);
            BrokeredMessage bm = new BrokeredMessage(connect);
            queueClient.Send(bm);

            disconnectButton = new Button();
            disconnectButton.Location = new Point(12, 38);
            disconnectButton.Text = "Disconnect";
            disconnectButton.Click += DisconnectUser;
            Controls.Add(disconnectButton);

            textBox.Visible = false;
            connectButton.Visible = false;

            Invalidate();
        }

        private void OnPaint(object sender, PaintEventArgs e) {
            float y = 0.0F;
            var g = e.Graphics;

            // Create font and brush.
            Font f = new Font("Arial", 16);
            SolidBrush b = new SolidBrush(Color.Black);

            if (query != null) {
                foreach (UserEntity entity in query) {
                    g.DrawString(entity.RowKey, f, b, 225.0F, 20.0F + y);
                    y += 20.0F;
                }
            }

            if (userName != "default")
                g.DrawString(userName, f, b, 13.0F, 12.0F);

            g.DrawLine(Pens.Black, 200.0F, 10.0F, 200.0F, 510.0F);
        }

        private void OnClosing(object sender, FormClosingEventArgs e) {
            DisconnectUser(sender, e);
        }

        private void DisconnectUser(object sender, EventArgs e) {
            MessageData md = new MessageData(userName, "disconnect message", MessagePurpose.Disconnect);
            BrokeredMessage bm = new BrokeredMessage(md);
            queueClient.Send(bm);

            textBox.Visible = true;
            connectButton.Visible = true;
            if (disconnectButton != null)
                disconnectButton.Visible = false;
            userName = "default";

            Invalidate();
        }
    }
}
