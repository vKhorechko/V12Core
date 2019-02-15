using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using V12Core.Application.Interfaces;
//using V12Core.Infrastructure.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace V12Core.WebUI.Controllers
{
    [Route("api/[controller]")]
    public class TwillioController : Controller
    {
        private readonly ISmsSender _twillioServices;

        public TwillioController(ISmsSender twillioServices)
        {
            _twillioServices = twillioServices;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            try
            {
                _twillioServices.SendSmsAsync("+380633685256", "hello world");
            }
            catch (Exception)
            {

                throw;
            }
            
            return "value";
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }


    }
}
