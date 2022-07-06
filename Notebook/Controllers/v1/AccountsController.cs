using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Notebook.Authentication.Configuration;
using Notebook.Authentication.Models.DTOS.Incoming;
using Notebook.Authentication.Models.DTOS.Outgoing;
using Notebook.DataService.IConfiguration;
using Notebook.Entities.DbSet;

namespace Notebook.Controllers.v1
{
    public class AccountsController : BaseController
    {
        private readonly UserManager<IdentityUser> _userManager;

        private readonly JwtConfig _jwtConfig;
        public AccountsController(IUnitofWork unitofWork, UserManager<IdentityUser> userManager, IOptionsMonitor<JwtConfig> optionsMonitor) : base(unitofWork)
        {
            _userManager = userManager;
            _jwtConfig = optionsMonitor.CurrentValue;
        }


        [HttpPost]
        [Route("Register")]

        public async Task<IActionResult> Register([FromBody] UserRegisterationRequestDto registerationRequestDto)
        {
            if (ModelState.IsValid)
            {
                var userExist = await _userManager.FindByEmailAsync(registerationRequestDto.Email);

                if (userExist != null)
                {
                    return BadRequest(new UserRegistrationResponse()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Email Already In Use"
                        }
                    });
                }


                var newUser = new IdentityUser()
                {
                    Email = registerationRequestDto.Email,
                    UserName = registerationRequestDto.Email,
                    EmailConfirmed = true

                };


                var iscreated = await _userManager.CreateAsync(newUser, registerationRequestDto.PassWord);
                if (!iscreated.Succeeded)
                {
                    return BadRequest(new UserRegistrationResponse()
                    {
                        Success = iscreated.Succeeded,
                        Errors = iscreated.Errors.Select(x => x.Description).ToList()
                    });
                }

                User _user = new User();

                _user.Id = new Guid(newUser.Id);
                _user.FirstName = registerationRequestDto.FirstName;
                _user.LastName = registerationRequestDto.LastName;
                _user.Phone = "";
                _user.Email = registerationRequestDto.Email;
                _user.DateOfBirth = DateTime.UtcNow;
                _user.Country = "";
                _user.Status = 1;

                await _unitofWork.Users.Add(_user);
                await _unitofWork.CompleteAsync();

                var token = GenerateJwtToken(newUser);

                return Ok(new UserRegistrationResponse()
                {
                    Success = true,
                    Token = token
                });
            }
        

            else
            {
                return BadRequest(new UserRegistrationResponse()
                {
                    Success = false,
                    Errors = new List<string>()
                    {
                        "Invalid Payload"
                    }
                });

            }
        }

       
        private string GenerateJwtToken(IdentityUser user)
            {
                var jwtHandler = new JwtSecurityTokenHandler();

                var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);


                var tokenDescriptor = new SecurityTokenDescriptor()
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                }),
                    Expires = DateTime.UtcNow.AddHours(3),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };



                var token = jwtHandler.CreateToken(tokenDescriptor);



                return jwtHandler.WriteToken(token);
        }



        }
    }

