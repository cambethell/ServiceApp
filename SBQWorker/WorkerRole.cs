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
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;


using System.Data.SqlClient;
using System.Data;

namespace SBQWorker
{
    public class WorkerRole : RoleEntryPoint
    {
        // The name of your queue
        const string queueName = "queuebus";
        const string topicName = "subtopic";

        // QueueClient is thread-safe. Recommended that you cache 
        // rather than recreating it on every request
        QueueClient queueClient;
        TopicClient topicClient;
        ManualResetEvent CompletedEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.WriteLine("Starting processing of messages");

            // Initiates the message pump and callback is invoked for each message that is received, calling close on the client will stop the pump.
            queueClient.OnMessage((receivedMessage) => {
                try
                {
                    // Process the message
                    Trace.WriteLine("Processing Service Bus message: " + receivedMessage.SequenceNumber);
                    // View the message as a MessageData.
                    MessageData md = receivedMessage.GetBody<MessageData>();
                    Trace.WriteLine(md.Message, "ProcessingMessage");

                    if (md.Purpose == MessagePurpose.Connect)
                    {
                        var resp = new MessageData("", MessagePurpose.Update);
                        var bm = new BrokeredMessage(resp);
                        topicClient.Send(bm);
                    }

                    receivedMessage.Complete();
                }
                catch
                {
                    // Handle any message processing specific exceptions here
                    receivedMessage.Abandon();
                }
            });

            CompletedEvent.WaitOne();
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;
            string SBConnectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            // Create the queue if it does not exist already
            var namespaceManager = NamespaceManager.CreateFromConnectionString(SBConnectionString);
            if (!namespaceManager.QueueExists(queueName))
            {
                namespaceManager.CreateQueue(queueName);
                Trace.WriteLine("queue did not exist");
            }

            //*****************************

            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("people");
            table.CreateIfNotExists();

            // Create the batch operation.
            TableBatchOperation batchOperation = new TableBatchOperation();

            // Create a customer entity and add it to the table.
            CustomerEntity customer1 = new CustomerEntity("Smith", "Jeff");
            customer1.Email = "Jeff@contoso.com";
            customer1.PhoneNumber = "425-555-0104";

            // Create another customer entity and add it to the table.
            CustomerEntity customer2 = new CustomerEntity("Smith", "Ben");
            customer2.Email = "Ben@contoso.com";
            customer2.PhoneNumber = "425-555-0102";

            // Add both customer entities to the batch insert operation.
            batchOperation.Insert(customer1);
            batchOperation.Insert(customer2);

            // Execute the batch operation.
            try
            {
                table.ExecuteBatch(batchOperation);
            }
            catch (StorageException e)
            {
                Debug.WriteLine(e.RequestInformation.HttpStatusCode);

                Debug.WriteLine(e.RequestInformation.ExtendedErrorInformation.ErrorCode);

                Debug.WriteLine(e.RequestInformation.ExtendedErrorInformation.ErrorMessage);             
            }

            // Construct the query operation for all customer entities where PartitionKey="Smith".
            TableQuery<CustomerEntity> query = new TableQuery<CustomerEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Smith"));

            // Print the fields for each customer.
            foreach (CustomerEntity entity in table.ExecuteQuery(query))
            {
                Trace.WriteLine($"{entity.PartitionKey}, {entity.RowKey}\t{entity.Email}\t{entity.PhoneNumber}");
            } 





             
            
            //*****************************

            // Configure Topic Settings
            TopicDescription td = new TopicDescription(topicName);
            td.MaxSizeInMegabytes = 5120;
            td.DefaultMessageTimeToLive = new TimeSpan(0, 1, 0);

            if (!namespaceManager.TopicExists(topicName))
            {
                namespaceManager.CreateTopic(td);
                Trace.WriteLine("topic did not exist");
            }

            topicClient = TopicClient.CreateFromConnectionString(SBConnectionString, topicName);
            queueClient = QueueClient.CreateFromConnectionString(SBConnectionString, queueName);
            return base.OnStart();
        }

        public override void OnStop()
        {
            // Close the connection to Service Bus
            topicClient.Close();
            queueClient.Close();
            CompletedEvent.Set();
            base.OnStop();
        }
    }
}
