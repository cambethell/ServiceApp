using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using AzureClient;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace SBQWorker {
    public class WorkerRole : RoleEntryPoint {
        // The name of your queue
        const string queueName = "queuebus";
        const string topicName = "subtopic";
        
        CloudStorageAccount storageAccount;
        CloudTableClient tableClient;
        CloudTable table;
        TableBatchOperation batchOperation;

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

                    if (md.Purpose == MessagePurpose.Connect)
                        AddPlayer(md.User);
                    else if (md.Purpose == MessagePurpose.Disconnect) {
                        // Create the batch operation.
                        batchOperation = new TableBatchOperation();

                        // Create a customer entity and add it to the table.
                        UserEntity user1 = new UserEntity("Player", md.User);

                        // Add both customer entities to the batch insert operation.
                        batchOperation.Delete(user1);

                        // Execute the batch operation.
                        try {
                            table.ExecuteBatch(batchOperation);
                        } catch (StorageException e) {
                            Debug.WriteLine(e.RequestInformation.HttpStatusCode);
                            Debug.WriteLine(e.RequestInformation.ExtendedErrorInformation.ErrorCode);
                            Debug.WriteLine(e.RequestInformation.ExtendedErrorInformation.ErrorMessage);
                        }
                    }

                    // Construct the query operation for all users entities where PartitionKey="Player".
                    var query = new TableQuery<UserEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Player"));
                    var result = table.ExecuteQuery(query);
                    
                    // Print the fields for each customer.
                    foreach (UserEntity entity in result)
                        Trace.WriteLine($"{entity.PartitionKey}, {entity.RowKey}");

                    var resp = new MessageData("default", "update message", MessagePurpose.Update, result);
                    var bm = new BrokeredMessage(resp);
                    topicClient.Send(bm);

                    receivedMessage.Complete();
                } catch (Exception e) {
                    Trace.WriteLine(e.StackTrace);

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

            // Retrieve the storage account from the connection string.
            storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the table client.
            tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            table = tableClient.GetTableReference("people");
            table.CreateIfNotExists();

            return base.OnStart();
        }

        public override void OnStop() {
            // Close the connection to Service Bus
            topicClient.Close();
            queueClient.Close();
            CompletedEvent.Set();
            base.OnStop();
        }

        //this function defaults the partition type be Player since its adding players
        public void AddPlayer(string username) {
            // Create the batch operation.
            batchOperation = new TableBatchOperation();

            // Create a customer entity and add it to the table.
            UserEntity user1 = new UserEntity("Player", username);

            // Add both customer entities to the batch insert operation.
            batchOperation.Insert(user1);

            // Execute the batch operation.
            try {
                table.ExecuteBatch(batchOperation);
            } catch (StorageException e) {
                Debug.WriteLine(e.RequestInformation.HttpStatusCode);
                Debug.WriteLine(e.RequestInformation.ExtendedErrorInformation.ErrorCode);
                Debug.WriteLine(e.RequestInformation.ExtendedErrorInformation.ErrorMessage);
            }
        }
    }
}
