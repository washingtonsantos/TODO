using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;

namespace ReceiveTodo
{
    public class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() 
            {
               HostName = "localhost",
               UserName = "admin",
               Password = "tests123456"
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "tarefas",
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var todo = JsonSerializer.Deserialize<Todo>(message);

                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                        Console.WriteLine($"Tarefa id: {todo.Id}, descrição: {todo.Description} criada!");
                    }
                    catch (Exception ex)
                    {
                        channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                        throw ex;
                    }
                };

                channel.BasicConsume(queue: "tarefas",
                    autoAck: false,
                    consumer: consumer);

                Console.WriteLine("");
                Console.ReadLine();
            }
        }
    }
}
