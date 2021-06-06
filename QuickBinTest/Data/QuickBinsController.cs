using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EventAggregator;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace QuickBinTest.Data
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuickBinsController : ControllerBase
    {
        private readonly IHubContext<QuickBinHub> _hubContext;

        public QuickBinsController(IHubContext<QuickBinHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // GET: api/<QuickBinsController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        public string GetHello()
        { 
            return "Hello World";
        }
               

        // GET api/<QuickBinsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<QuickBinsController>
        [HttpPost]
        public void Post([FromBody] QuickBin quickBin)
        {
            var quickBinJson = JsonSerializer.Serialize(quickBin);
            _hubContext.Clients.All.SendAsync("Broadcast", "QuickBins", quickBinJson);
        }

        // PUT api/<QuickBinsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<QuickBinsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
