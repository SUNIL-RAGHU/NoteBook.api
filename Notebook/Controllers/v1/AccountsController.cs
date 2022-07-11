using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Notebook.Authentication.Configuration;
using Notebook.Authentication.Models.DTOS.Generic;
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

        private readonly TokenValidationParameters _tokenValidationParameters;


        public AccountsController(IUnitofWork unitofWork, UserManager<IdentityUser> userManager, TokenValidationParameters tokenValidationParameters, IOptionsMonitor<JwtConfig> optionsMonitor) : base(unitofWork)
        {
            _userManager = userManager;
            _jwtConfig = optionsMonitor.CurrentValue;
            _tokenValidationParameters = tokenValidationParameters;
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

                var token = await GenerateJwtToken(newUser);

                return Ok(new UserRegistrationResponse()
                {
                    Success = true,
                    Token = token.JwtToken,
                    RefreshToken=token.RefreshToken
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


        [HttpPost]
        [Route("Login")]

        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto loginRequestDto)
        {
            if (ModelState.IsValid)
            {
                // check email exist
                var userexist = await _userManager.FindByEmailAsync(loginRequestDto.Email);

                if (userexist == null)
                {
                    return BadRequest(new UserLoginResponse()
                    {
                        Success = false,
                        Errors = new List<string>()
                    {
                        "Invalid Authentication Request"
                    }
                    });
                }

                // check valid password

                var iscorrect = await _userManager.CheckPasswordAsync(userexist, loginRequestDto.Password);

                if (iscorrect)
                {
                    var jwtToken = await GenerateJwtToken(userexist);

                    return Ok(new UserLoginResponse()
                    {
                        Success = true,
                        Token = jwtToken.JwtToken,
                        RefreshToken=jwtToken.RefreshToken,

                    });
                }

                else
                {
                    return BadRequest(new UserLoginResponse()
                    {
                        Success = false,
                        Errors = new List<string>()
                    {
                        "Password does not exist"
                    }
                    });
                }

            }

            else
            {
                return BadRequest(new UserLoginResponse()
                {
                    Success = false,
                    Errors = new List<string>()
                    {
                        "Invalid Payload"
                    }
                });
            }

        }

        [HttpPost]
        [Route("RefreshToken")]

        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto tokenRequestDto)
        {
            if (ModelState.IsValid)
            {
                var result = await VerifyToken(tokenRequestDto);


                if (result == null)
                {
                    return BadRequest(new UserLoginResponse()
                    {
                        Success = false,
                        Errors = new List<string>()
                    {
                        "Token Validation Failed"
                    }
                    });
                }

                return Ok(result);

            }

            else
            {
                return BadRequest(new UserLoginResponse()
                {
                    Success = false,
                    Errors = new List<string>()
                    {
                        "Invalid Payload"
                    }
                });
            }
        }



        private async Task<AuthResult> VerifyToken(TokenRequestDto tokenRequest)
        {
            var TokenHandler = new JwtSecurityTokenHandler();

            try
            {

                //Validate output the token
                var principal = TokenHandler.ValidateToken(tokenRequest.Token, _tokenValidationParameters, out var validateToken);


                if (validateToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                    if (!result)
                    {
                        return null;
                    }

                }
                var UtcExpiryDate = long.Parse(principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expDate = UnixTimeStampToDateTime(UtcExpiryDate);


                if (expDate > DateTime.UtcNow)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Jwt token Has not expired"
                        }
                    };


                }

                var refreshTokenExist = await _unitofWork.RefreshToken.GetByRefreshToken(tokenRequest.RefreshToken);

                if (refreshTokenExist == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Invalid Refresh Token"
                        }
                    };
                }

                if (refreshTokenExist.ExpiryDate < DateTime.UtcNow)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            " Refresh Token Has Expired Please Login Again"
                        }
                    };
                }

                if (refreshTokenExist.IsUsed)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            " Refresh Token Has been Used It Cannot be Reused"
                        }
                    };
                }


                if (refreshTokenExist.IsRevoked)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            " Refresh Token Has been Revoked It cannot be Used"
                        }
                    };
                }

                var jti = principal.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

                if (refreshTokenExist.JwtId != jti)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            " Refresh Token Referance Does not match Jwt Token"
                        }
                    };
                }

                refreshTokenExist.IsUsed = true;

                var updateresult=await _unitofWork.RefreshToken.MarkRefreshTokenAsUsed(refreshTokenExist);

                if (updateresult)
                {

                    await _unitofWork.CompleteAsync();

                    var dbuser = await _userManager.FindByIdAsync(refreshTokenExist.UserId);

                    if (dbuser == null)
                    {
                        return new AuthResult
                        {
                            Success = false,
                            Errors = new List<string>()
                        {
                            "Error Processing Request"
                        }
                        };
                    }


                    var tokens = await GenerateJwtToken(dbuser);
                    return new AuthResult{
                     Token=tokens.JwtToken,
                     Success=true,
                     RefreshToken=tokens.RefreshToken

                    };
                 }

                return new AuthResult
                {
                    Success = false,
                    Errors = new List<string>()
                        {
                            "Error Processing Request"
                        }
                };

            }
            catch (Exception ex)
            {
                return null;
            }
        
        }


        private DateTime UnixTimeStampToDateTime(long uxinDate)
        {
            //set the time to 1,jan 1970
            var DateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            //add the number of seconds from 1 jan 1970 
            DateTime = DateTime.AddSeconds(uxinDate).ToUniversalTime();
            return DateTime;

        }

        private async Task<TokenData> GenerateJwtToken(IdentityUser user)
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
                Expires = DateTime.UtcNow.Add(_jwtConfig.ExpiryTimeFrame),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };



            var token = jwtHandler.CreateToken(tokenDescriptor);



            //Generate  a refresh Token

            var refreshtoken = new RefreshToken()
            {
                AddedData = DateTime.Now,
                Token =$"{RandomstringGenerator(25)}_{Guid.NewGuid()}",
                UserId = user.Id,
                IsRevoked = false,
                IsUsed = false,
                Status = 1,
                JwtId=token.Id,
                ExpiryDate=DateTime.UtcNow.AddMonths(6),


            };

            var JwtToken = jwtHandler.WriteToken(token);
       
            await _unitofWork.RefreshToken.Add(refreshtoken);
            await _unitofWork.CompleteAsync();

            var tokenData = new TokenData()
            {
                JwtToken = JwtToken,
                RefreshToken = refreshtoken.Token
            };

            return tokenData;
        }


        private string RandomstringGenerator(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


       



    }
}
    

