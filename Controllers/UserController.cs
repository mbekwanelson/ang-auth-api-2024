using ang_auth_api_2024.Context;
using ang_auth_api_2024.Helpers;
using ang_auth_api_2024.Helpers.HelperModels;
using ang_auth_api_2024.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace ang_auth_api_2024.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        public UserController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;

        }

        [HttpPost("authenticate")]
        public async Task<ResponseMessage<UserDto>> Authenticate([FromBody] UserDto userObj)
        {

            if (userObj == null) return new ResponseMessage<UserDto>()
            {
                data = userObj,
                Message = "Object not parsed : null object",
                success = false,
                StatusCode = 500
            };

            var user = await _appDbContext.Users.FirstOrDefaultAsync(x => x.UserName == userObj.UserName && x.Password == userObj.Password);
            if (user == null)
            {
                return new ResponseMessage<UserDto>() 
                {
                    data = userObj,
                    Message = $"Incorrect Username: {userObj.UserName} /Password : {userObj.Password}",
                    success =  false,
                    StatusCode = 500
                };
            }

            userObj.Id = user.Id;
            return new ResponseMessage<UserDto>() 
            { 
                   data = userObj,
                   Message = "Login Success!",
                   success = true ,
                   StatusCode = 200
            };
        }

        [HttpPost("Register")]
        public async Task<ResponseMessage<UserDto>> RegisterUser([FromBody] UserDto userObj)
        {

            
            try
            {
                if (userObj == null)
                {
                    return new ResponseMessage<UserDto>()
                    {
                        data = new UserDto() { Id = userObj.Id, UserName = userObj.UserName },
                        Message = "Error Saving User, user object is null",
                        StatusCode = StatusCodes.Status400BadRequest,
                        success = false
                    };
                }

                if (string.IsNullOrEmpty(userObj.UserName) || string.IsNullOrEmpty(userObj.Password))
                {
                    return new ResponseMessage<UserDto>()
                    {
                        data = new UserDto() { Id = userObj.Id, UserName = userObj.UserName },
                        Message = "User Name and/ password is empty",
                        StatusCode = StatusCodes.Status400BadRequest,
                        success = false
                    };
                }

                //check user Name
                
                if (checkIfUserNameExists(userObj.UserName).Result)
                {
                    return new ResponseMessage<UserDto>()
                    {
                        data = new UserDto() { Id = userObj.Id, UserName = userObj.UserName },
                        Message = "Username is already taken",
                        StatusCode = StatusCodes.Status401Unauthorized,
                        success = false
                    };
                }

                // check Email
                if (checkIfEmailExists(userObj.Email).Result)
                {
                    return new ResponseMessage<UserDto>()
                    {
                        data = new UserDto() { Id = userObj.Id, UserName = userObj.UserName, Email = userObj.Email },
                        Message = "Email is already taken",
                        StatusCode = StatusCodes.Status401Unauthorized,
                        success = false
                    };
                }

                //check for Password Strength
                var passStrengthChecks = checkPasswordStrength(userObj.Password);
                if(!string.IsNullOrEmpty(passStrengthChecks.Result))
                    return new ResponseMessage<UserDto>()
                    {
                        data = new UserDto() { Id = userObj.Id, UserName = userObj.UserName, Email = userObj.Email },
                        Message = passStrengthChecks.Result,
                        StatusCode = StatusCodes.Status401Unauthorized,
                        success = false
                    };




                userObj.Password = PasswordHasher.HashPassword(userObj.Password);
                userObj.Role = "User";
                userObj.Token = "";
                await _appDbContext.Users.AddAsync(new Models.User
                {
                    FirstName = userObj.FirstName,
                    LastName = userObj.LastName,
                    UserName = userObj.UserName,
                    Password = userObj.Password,
                    Role = userObj.Role,
                    Token = userObj.Token,
                    Email = userObj.Email
                });

                if (_appDbContext.SaveChanges() > 0)
                {
                    var user = await _appDbContext.Users.FirstOrDefaultAsync(x => x.UserName == userObj.UserName && x.Password == userObj.Password);

                    return new ResponseMessage<UserDto>()
                    {
                        data = new UserDto() { Id = user.Id, UserName = userObj.UserName },
                        Message = "success",
                        StatusCode = 200,
                        success = true
                    };
                }

                return new ResponseMessage<UserDto>()
                {
                    data = new UserDto() { Id = 0, UserName = userObj.UserName },
                    Message = "Could not save User",
                    StatusCode = StatusCodes.Status400BadRequest,
                    success = false
                };


            }
            catch(Exception e) 
            {
                return new ResponseMessage<UserDto>()
                {
                    data = userObj,
                    Message = $"Error : {e.Message}",
                    success = false,
                    StatusCode = StatusCodes.Status422UnprocessableEntity
                };
            
            
            }
            
        }

        private  Task<bool> checkIfUserNameExists(string username) 
            =>  _appDbContext.Users.AnyAsync(x => x.UserName == username);

        private Task<bool> checkIfEmailExists(string email)
           => _appDbContext.Users.AnyAsync(x => x.Email == email);

        private Task<string> checkPasswordStrength(string password) 
        {

            StringBuilder sb = new StringBuilder();
            if(password.Length < 8) 
                sb.Append("Minimum Password length should be 8"+ Environment.NewLine);
            

            if( !(Regex.IsMatch(password,"[a-z]")  && 
                Regex.IsMatch(password, "[A-Z]")  &&
                Regex.IsMatch(password,"[0-9]"))
             ) 
                sb.Append("Password should be Alphanumeric" + Environment.NewLine);
            

            if (!Regex.IsMatch(password, "[<,>,@,!,#,$,%,^,&,*,(,),_,+,\\[,\\],{,},?,:,;,|,',\\,.,/,~,`,-,=]")) 
                sb.Append("Password Should contain at least one special character" + Environment.NewLine);

            return Task.FromResult(sb.ToString());
 
        }
    }
}
