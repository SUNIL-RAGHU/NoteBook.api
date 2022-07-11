using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notebook.DataService.Data;
using Notebook.DataService.IConfiguration;
using Notebook.Entities.DbSet;
using Notebook.Entities.Dtos.Incoming;

namespace Notebook.Controllers.v1
{
    [Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : BaseController
	{

        //private AppDbContext _Context;


        public UsersController(IUnitofWork unitofWork) : base(unitofWork)
        {

        }


        [HttpGet]
        [HttpHead]
        
        public async Task<IActionResult> GetUsers()
        {
            var users = await _unitofWork.Users.All();
            

            return Ok(users);
        }



        [HttpPost]

        public async Task<IActionResult> AddUser(UserDto user)
        {
            User _user = new User();

            _user.FirstName = user.FirstName;
            _user.LastName = user.LastName;
            _user.Phone = user.Phone;
            _user.Email = user.Email;
            _user.DateOfBirth = Convert.ToDateTime(user.DateOfBirth);
            _user.Country = user.Country;
            _user.Status = 1;

            await _unitofWork.Users.Add(_user);
            await _unitofWork.CompleteAsync();

            return CreatedAtRoute("GetUser", new { id = _user.Id },user); //RETURN A 201
        }


        [HttpGet]

        [Route("GetUser",Name="GetUser")]

        public async Task<IActionResult> GetUser(Guid id)
        {

            var user = await _unitofWork.Users.GetbyId(id);
            

            return Ok(user);
        }
	}
}

