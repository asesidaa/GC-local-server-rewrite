using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace MainServer.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly MusicDbContext context;

        public TestController(MusicDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public MusicUnlock GetOne()
        {
            return context.MusicUnlocks.First();
        }

        [HttpPost]
        public ActionResult<string> TestXmlInputOutput([FromForm(Name = "my_model")]TestModel model, 
            [FromForm(Name = "my_type")]int type)
        {
            return Ok($"{model.Name}\n{model.Age}\n{type}");
        }
    }

    public class TestModel
    {
        public string Name { get; set; } = string.Empty;

        public int Age { get; set; }
    }
    
}
