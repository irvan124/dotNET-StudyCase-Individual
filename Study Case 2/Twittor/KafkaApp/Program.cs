using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using KafkaApp.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace KafkaApp
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                    .AddJsonFile($"appsettings.json", true, true);

            var config = builder.Build();


            var Serverconfig = new ConsumerConfig
            {
                BootstrapServers = config["Settings:KafkaServer"],
                GroupId = "tester",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true; // prevent the process from terminating.
                cts.Cancel();
            };
            Console.WriteLine("--------------.NET Application--------------");
            using (var consumer = new ConsumerBuilder<string, string>(Serverconfig).Build())
            {
                Console.WriteLine("Connected");
                var topics = new string[]
                {
                     // User Profiling   
                    "add-user",
                    "edit-user",
                    "change-password",
                    "update-userRole",
                    // Twits
                    "post-twit",
                    "comment-twit",
                    "delete-twit"
                };
                consumer.Subscribe(topics);

                Console.WriteLine("Waiting messages....");
                try
                {
                    while (true)
                    {
                        var cr = consumer.Consume(cts.Token);
                        Console.WriteLine($"Consumed record with Topic: {cr.Topic} key: {cr.Message.Key} and value: {cr.Message.Value}");

                        using (var dbcontext = new TwittorDBContext())
                        {
                            if (cr.Topic == "add-user")
                            {
                                User user = JsonConvert.DeserializeObject<User>(cr.Message.Value);
                                dbcontext.Users.Add(user);
                            }
                            if (cr.Topic == "edit-user")
                            {
                                User userUpdate = JsonConvert.DeserializeObject<User>(cr.Message.Value);
                                dbcontext.Users.Update(userUpdate);
                            }
                            if (cr.Topic == "change-password")
                            {
                                User changePassword = JsonConvert.DeserializeObject<User>(cr.Message.Value);
                                dbcontext.Users.Update(changePassword);
                            }
                            if (cr.Topic == "post-twit")
                            {
                                Tweet twit = JsonConvert.DeserializeObject<Tweet>(cr.Message.Value);
                                dbcontext.Tweets.Add(twit);
                            }
                            if (cr.Topic == "comment-twit")
                            {
                                Comment comment = JsonConvert.DeserializeObject<Comment>(cr.Message.Value);
                                dbcontext.Comments.Add(comment);
                            }
                            if (cr.Topic == "update-userRole")
                            {
                                UserRole userRole = JsonConvert.DeserializeObject<UserRole>(cr.Message.Value);
                                dbcontext.UserRoles.Add(userRole);
                            }
                            if (cr.Topic == "delete-twit")
                            {
                                Tweet tweet = JsonConvert.DeserializeObject<Tweet>(cr.Message.Value);
                                dbcontext.Tweets.Remove(tweet);
                            }
                            await dbcontext.SaveChangesAsync();
                            Console.WriteLine("Data was saved into database");
                        }


                    }
                }
                catch (OperationCanceledException)
                {
                    // Ctrl-C was pressed.
                }
                finally
                {
                    consumer.Close();
                }

            }

            return 1;
        }
    }
}
