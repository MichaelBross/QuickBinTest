using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace QuickBinTest.Data
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class QuickBinsController : ControllerBase
    {
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

        [HttpPost]
        public void Update(string value)
        {
            var newMessage = value;
        }

        // GET api/<QuickBinsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<QuickBinsController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
            var newValue = value;
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
