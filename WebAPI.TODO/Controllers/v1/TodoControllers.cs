using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace WebAPI.TODO.Controllers.v1
{
    [ApiController]    
    [Route("api/v1/[controller]")]
    public class TodoControllers : ControllerBase
    {
        [HttpPost("InsertTodo")]
        public IActionResult InsertTodo([FromBody] Models.Todo todo)
        {
            try
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

                    var message = JsonSerializer.Serialize(todo);
                    var body = Encoding.UTF8.GetBytes(message);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    channel.BasicPublish(exchange: "",
                        routingKey: "tarefas",
                        basicProperties: properties,
                        body: body);
                }

                return Ok("Tarefa enviada para fila!");
            }
            catch (System.Exception ex)
            {
                throw;
            }

        }        
    }
}
