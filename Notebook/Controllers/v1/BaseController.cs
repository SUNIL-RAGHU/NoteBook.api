using System;
using Microsoft.AspNetCore.Mvc;
using Notebook.DataService.IConfiguration;

namespace Notebook.Controllers.v1
{


    [Route("api/{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class BaseController : ControllerBase
    {
        public IUnitofWork _unitofWork;

        //private AppDbContext _Context;

        public BaseController(IUnitofWork unitofWork)
        {
            _unitofWork = unitofWork;
        }

    }
}
    

