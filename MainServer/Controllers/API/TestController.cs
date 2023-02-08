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
    }
}
