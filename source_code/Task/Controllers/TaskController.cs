using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Task.Models;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace Task.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {

        private readonly TaskDBContext _context;
        public TaskController(TaskDBContext context)
        {
            _context = context;
        }

        // GET: api/Task
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
        {
            return await _context.Tasks.ToListAsync();
        }

        // POST: api/Task
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Consumes("application/json")]
        public async Task<ActionResult<TaskItem>> Post(TaskItem taskItem)
        {
            _context.Tasks.Add(taskItem);

            string json = JsonConvert.SerializeObject(taskItem);
            Console.WriteLine("JSON");
            Console.WriteLine(json);
            //var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            var factory = new ConnectionFactory()
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT"))
            };

            Console.WriteLine(factory.HostName + ":" + factory.Port);
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "tasks",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                // string message = user.task;
                string message = JsonConvert.SerializeObject(taskItem); // this is to pass response obj
                
                var body = Encoding.UTF8.GetBytes(message);
                Console.WriteLine(body);

                channel.BasicPublish(exchange: "",
                                    routingKey: "tasks",
                                    basicProperties: null,
                                    body: body);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Post), new {id = taskItem.taskID}, taskItem);
        }

        
    }
}
