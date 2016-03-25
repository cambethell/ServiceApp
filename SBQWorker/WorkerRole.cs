using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using AzureClient;

namespace SBQWorker {
    public class WorkerRole : RoleEntryPoint {
        // The name of your queue
        const string queueName = "queuebus";
        const string topicName = "subtopic";

        // QueueClient is thread-safe. Recommended that you cache 
        // rather than recreating it on every request
        QueueClient queueClient;
        TopicClient topicClient;
        ManualResetEvent CompletedEvent = new ManualResetEvent(false);

        public override void Run() {
            Trace.WriteLine("Starting processing of messages");

            // Initiates the message pump and callback is invoked for each message that is received, calling close on the client will stop the pump.
            queueClient.OnMessage((receivedMessage) => {
                try {
                    // Process the message
                    Trace.WriteLine("Processing Service Bus message: " + receivedMessage.SequenceNumber);
                    // View the message as a MessageData.
                    MessageData md = receivedMessage.GetBody<MessageData>();
                    Trace.WriteLine(md.Message, "ProcessingMessage");

                    if (md.Purpose == MessagePurpose.Connect) {
                        var resp = new MessageData("", MessagePurpose.Update);
                        var bm = new BrokeredMessage(resp);
                        topicClient.Send(bm);
                    }

                    receivedMessage.Complete();
                } catch {
                    // Handle any message processing specific exceptions here
                    receivedMessage.Abandon();
                }
            });

            CompletedEvent.WaitOne();
        }

        public override bool OnStart() {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;
            string SBConnectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            string DBConnectionString = CloudConfigurationManager.GetSetting("Microsoft.Database.ConnectionString");

            // Create the queue if it does not exist already
            var namespaceManager = NamespaceManager.CreateFromConnectionString(SBConnectionString);
            if (!namespaceManager.QueueExists(queueName)) {
                namespaceManager.CreateQueue(queueName);
                Trace.WriteLine("queue did not exist");
            }

            // Configure Topic Settings
            TopicDescription td = new TopicDescription(topicName);
            td.MaxSizeInMegabytes = 5120;
            td.DefaultMessageTimeToLive = new TimeSpan(0, 1, 0);

            if (!namespaceManager.TopicExists(topicName)) {
                namespaceManager.CreateTopic(td);
                Trace.WriteLine("topic did not exist");
            }

            topicClient = TopicClient.CreateFromConnectionString(SBConnectionString, topicName);
            queueClient = QueueClient.CreateFromConnectionString(SBConnectionString, queueName);
            return base.OnStart();
        }

        public override void OnStop() {
            // Close the connection to Service Bus
            topicClient.Close();
            queueClient.Close();
            CompletedEvent.Set();
            base.OnStop();
        }
    }
}
