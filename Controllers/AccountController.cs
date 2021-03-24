﻿using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Servize.Authentication;
using Servize.Domain.Repositories;
using Servize.DTO;
using Servize.DTO.ADMIN;
using Servize.Utility;
using Servize.Utility.Logging;
using Servize.Utility.QueryFilter;
using Servize.Utility.Sms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Servize.Controllers
{
    [EnableCors("MyPolicy")]
    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ServizeDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly ContextTransaction _transaction;
        private readonly TokenValidationParameters _tokenValidationParameter;
        private readonly AccountRepository _repository;

        public AccountController(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
             SignInManager<ApplicationUser> signInManager,
             ServizeDBContext context,
             IAuthService service, ContextTransaction transaction,
             TokenValidationParameters tokenValidationParameter,
             AccountRepository repository
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _signInManager = signInManager;
            _context = context;
            _transaction = transaction;
            _tokenValidationParameter = tokenValidationParameter;
            _repository = repository;
        }


        [HttpPost("RegisterClient")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<ActionResult<Response<AuthSuccessResponse>>> RegisterUser([FromBody] RegistrationInputModel model)
        {

            Response<AuthSuccessResponse> response = await _repository.AddUserToIdentityWithSpecificRoles(model, "CLIENT");
            if (response.IsSuccessStatusCode())
            {              
                return Ok(response.Resource);
            }
            return Problem(statusCode: response.StatusCode, detail: response.Message);
        }



        [HttpPost("RegisterAdmin")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<ActionResult<Response<AuthSuccessResponse>>> RegisterAdmin([FromBody] RegistrationInputModel model)
        {
            Response<AuthSuccessResponse> response = await _repository.AddUserToIdentityWithSpecificRoles(model, "ADMIN");
            if (response.IsSuccessStatusCode())
            {
                return Ok(response.Resource);
            }
            return Problem(statusCode: response.StatusCode, detail: response.Message);
        }



        [HttpPost]
        [Route("RegisterVendor")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<ActionResult<Response<AuthSuccessResponse>>> RegisterVendor([FromBody] RegistrationInputModel model)
        {
            Response<AuthSuccessResponse> response = await _repository.AddUserToIdentityWithSpecificRoles(model, "VENDOR");
            if (response.IsSuccessStatusCode())
            {
                return Ok(response.Resource);
            }
            return Problem(statusCode: response.StatusCode, detail: response.Message);
        }

        /* /// <summary>
         /// Get token for User
         /// </summary>
         /// <param name="model"></param>
         /// <returns></returns>
         //Authentication/UserToken
         [HttpGet("UserToken")]
         [Produces("application/json")]
         [Consumes("application/json")]
         public async Task<ActionResult> GetUserToken([FromBody] InputLoginModel model)
         {
             var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
             if (result.Succeeded)
             {
                 var user = await _userManager.FindByEmailAsync(model.Email);
                 if (user != null)
                 {
                     var refreshToken = await _userManager.GetAuthenticationTokenAsync(user, "ServizeApp", "RefreshToken");
                     if (refreshToken != null)
                     {
                         return Ok(refreshToken);
                     }
                 }
             }
             return Unauthorized();
         }*/
        [HttpPost("RefreshToken")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<ActionResult<Response<AuthSuccessResponse>>> RefreshToken([FromBody] RefreshTokenRequest refreshRequest)
        {

            Response<AuthSuccessResponse> response = await _repository.RefreshTokenAsync(refreshRequest.Token, refreshRequest.RefreshToken);
            if (response.IsSuccessStatusCode())
            {               
                return Ok(response.Resource);
            }


            return Problem(detail: response.Message, statusCode: response.StatusCode);
        }





        /// <summary>
        /// Getotp token for number
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        [HttpPost("GetOtp/{number}")]
        public async Task<ActionResult<Response<AuthSuccessResponse>>> SMSToken(string phoneNumber)
        {

           Response<int> response =await _repository.SendSMSTokenAsync(phoneNumber);
            if (response.IsSuccessStatusCode())
            {
                HttpContext.Session.SetInt32("otp", response.Resource);
                return Ok("Otp send Sucessfully");
            }
            return Problem(statusCode: response.StatusCode, detail: response.Message);
        }


        /// <summary>
        /// Verify SmS token
        /// </summary>
        /// <param name="model"> RegistrationModel With all Detail</param>
        /// <returns></returns>

        [HttpPost("VerifyOtp")]
        public async Task<ActionResult<Response<AuthSuccessResponse>>> VerifySMSToken(RegistrationInputModel model)
        {
            try
            {
                if (HttpContext.Session.GetInt32("otp") == model.Otp)
                {
                    HttpContext.Session.SetInt32("otp", 0);
                    return Ok("Code verfied Sucessfully");
                    //return await Register(model);
                }
                return Problem(detail: "TryAgain", statusCode: StatusCodes.Status503ServiceUnavailable);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Problem(detail: "Error While Verifying Otp", statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<Response<AuthSuccessResponse>> Register(RegistrationInputModel model)
        {
            return await _repository.AddUserToIdentityWithSpecificRoles(model, model.Role.ToUpper());

        }

        //Authentication/Login
        [HttpPost("Login")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<ActionResult> Login([FromBody] InputLoginModel model)
        {
            Response<AuthSuccessResponse> response = await _repository.HandleLoginRequest(model);
            if (response.IsSuccessStatusCode())
                return Ok(response.Resource);
            return Problem(detail: response.Message, statusCode: response.StatusCode);
        }


        //Authentication/Logout
        [HttpPost("LogOut")]
        public async Task<ActionResult> LogOut()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return Ok("logout Sucessfully");
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response("Error On LogOut ", StatusCodes.Status500InternalServerError));
            }

        }

        /// <summary>
        /// Get User by id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        /* [HttpGet]
         [Route("LoginData/{id}")]
         public async Task<ActionResult<InputLoginModel>> GetUserDataById(string Id)
         {
             try
             {
                 var userExist = await _userManager.FindByIdAsync(Id);
                 if (userExist == null)
                     return Problem(detail: "No able to find User of specific Id", statusCode: StatusCodes.Status404NotFound);

                 InputLoginModel model = new InputLoginModel
                 {

                     Email = userExist.UserName,

                 };
                 return Ok(model);
             }
             catch
             {
                 return Problem(detail: "Error while getting UserDetaild From Identity", statusCode: StatusCodes.Status500InternalServerError);
             }
         }*/

        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="changepassword"></param>
        /// <returns></returns>
        [HttpPost("ChangePassword")]
        public async Task<ActionResult<Response<InputLoginModel>>> ChangePassword(ChangePasswordRequest changepassword)
        {
            Response<InputLoginModel> response = await _repository.HandleChangedPassword(changepassword);
            if (response.IsSuccessStatusCode())
                return Ok(response.Resource);
            return Problem(detail: response.Message, statusCode: response.StatusCode);
        }




        /// <summary>
        /// Google login 
        /// </summary>
        /// <param name="externalInputModel"></param>
        /// <returns></returns>
        [HttpPost("Googlelogin")]
        public async Task<ActionResult<Response<AuthSuccessResponse>>> Google([FromBody] GoogleLoginRequest externalInputModel)
        {

            Response<AuthSuccessResponse> response = await _repository.HandleGoogleLogin(externalInputModel);
            if (response.IsSuccessStatusCode())
                return Ok(response.Resource);
            return Problem(detail: response.Message, statusCode: response.StatusCode);

        }



        [HttpGet("AccountHolder/{roleName}")]
        public async Task<ActionResult<IList<IdentityUser>>> GetListofAllAccountHolder(string roleName)
        {

            try
            {
                //IQueryable<IdentityRole> identityRole = _roleManager.Roles.ApplyQuery(query);
               // IList<IdentityRole> identityRoleList = await identityRole.ToListAsync();
                IList<ApplicationUser> users = await _userManager.Users.ToListAsync();
                List<ApplicationUser> userWithRoles = new List<ApplicationUser>();
                foreach (var user in users)
                {
                    if (user != null && await _userManager.IsInRoleAsync(user, roleName))
                        userWithRoles.Add(user);
                }
                return Ok(userWithRoles);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                return Problem(detail:e.Message);

            }
           
        }


    }

}

