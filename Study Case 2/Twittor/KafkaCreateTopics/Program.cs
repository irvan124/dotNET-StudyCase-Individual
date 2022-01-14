using Confluent.Kafka;
using Confluent.Kafka.Admin;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace KafkaCreateTopics
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = "127.0.0.1:9092",
                ClientId = Dns.GetHostName(),

            };
            var topics = new List<String>();
            // Events
            topics.Add("logging");
            // User Profiling          
            topics.Add("add-user");
            topics.Add("edit-user");
            topics.Add("change-password");
            // Roles
            topics.Add("add-rolename");
            topics.Add("update-userRole");

            topics.Add("post-twit");
            topics.Add("delete-twit");
            topics.Add("post-comment");
            foreach (var topic in topics)
            {
                using (var adminClient = new AdminClientBuilder(config).Build())
                {
                    Console.WriteLine("Creating a topic....");
                    try
                    {
                        await adminClient.CreateTopicsAsync(new List<TopicSpecification> {
                        new TopicSpecification { Name = topic, NumPartitions = 1, ReplicationFactor = 1 } });
                    }
                    catch (CreateTopicsException e)
                    {
                        if (e.Results[0].Error.Code != ErrorCode.TopicAlreadyExists)
                        {
                            Console.WriteLine($"An error occured creating topic {topic}: {e.Results[0].Error.Reason}");
                        }
                        else
                        {
                            Console.WriteLine("Topic already exists");
                        }
                    }
                }
            }

            return 0;
        }
    }
}
