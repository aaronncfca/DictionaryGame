using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DictionaryGame.Controllers
{

    [Route("InGameApi")]
    [ApiController]
    public class InGameController : Controller
    {
        public IActionResult PostAnswer()
        {
            //TODO
            return Ok();
        }
    }
}
