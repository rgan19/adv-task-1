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

namespace Task.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        // GET: api/Task
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Task/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Task
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Consumes("application/json")]
        public async Task<ActionResult<TaskItem>> Post(TaskItem taskItem)
        {
            string _address = "https://reqres.in/api/register";
            string json = JsonConvert.SerializeObject(taskItem);
            Console.WriteLine("JSON");
            Console.WriteLine(json);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            var client = new HttpClient();
            var response = await client.PostAsync(_address, stringContent);

            var responseString = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(responseString);

            var jObject = JObject.Parse(responseString);

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
                string message = JsonConvert.SerializeObject(jObject); // this is to pass response obj
                
                var body = Encoding.UTF8.GetBytes(message);
                Console.WriteLine(body);

                channel.BasicPublish(exchange: "",
                                    routingKey: "tasks",
                                    basicProperties: null,
                                    body: body);
            }

            return NoContent();
            // return if anything
        }

        // PUT: api/Task/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/Task/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
