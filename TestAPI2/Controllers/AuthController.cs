using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Diagnostics;
using System.Security.Claims;
using Library_oAuth.Models;
using Library_oAuth.Repositories.Interfaces;
using Library_oAuth.Services;
using Library_oAuth.Utils;

namespace Library_oAuth.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IDataRepository _dataRepository;

        public AuthController(IUserService userService, IDataRepository dataRepository)
        {
            _userService = userService;
            _dataRepository = dataRepository;
        }

        [HttpGet, Authorize]
        [ActionName("GetMyEmail")]
        public ActionResult<string> GetMe()
        {
            return Ok(User.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.Email).Value);
        }

        [HttpPost]
        [ActionName("register")]
        public async Task<ActionResult<User>> Register(UserLoginModel userDto)
        {
            try
            {
                if (AuthUtils.ValidateEmail(userDto.Email) && AuthUtils.ValidatePassword(userDto.Password))
                {
                    AuthUtils.CreatePasswordHash(userDto.Password, out byte[] passwordHash, out byte[] passwordSalt);
                    User user = new User { Email = userDto.Email, PasswordHash = passwordHash, PasswordSalt = passwordSalt, TokenCreated = new DateTime(2000, 1, 1), TokenExpires = new DateTime(2000, 1, 1), Role = Convert.ToInt32(Roles.Customer) };
                    DataService dataService = new DataService(_dataRepository);

                    await dataService.InsertUser(user);

                    return Ok(user);
                }
                return BadRequest("Invalid email/password or user already exists.\nPassword must have at least 4 characters");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(Register)}");
                Debug.WriteLine(ex.Message);
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        [ActionName("login")]
        public async Task<ActionResult<string>> Login(UserLoginModel userDto)
        {
            try
            {
                DataService dataService = new DataService(_dataRepository);
                var user = await dataService.GetUserByEmail(userDto.Email);

                if (user == null)
                {
                    return BadRequest("User not found");
                }
                if (!AuthUtils.VerifyPasswordHash(userDto.Password, user.PasswordHash, user.PasswordSalt))
                {
                    return BadRequest("Wrong password.");
                }
                string token = AuthUtils.CreateToken(user);

                var refreshToken = AuthUtils.GenerateRefreshToken();
                AuthUtils.SetRefreshToken(refreshToken, Response, user);

                await dataService.UpdateUser(user);

                return Ok(token);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(Login)}");
                Debug.WriteLine(ex.Message);
                return StatusCode(500);
            }
        }

        [HttpPost]
        [ActionName("refresh-token")]
        public async Task<ActionResult<string>> RefreshToken()
        {
            try
            {
                var refreshToken = Request.Cookies["refreshToken"];

                DataService dataService = new DataService(_dataRepository);
                User user = await dataService.GetUserByEmail(User.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.Email).Value);

                if (!user.RefreshToken.Equals(refreshToken))
                {
                    return Unauthorized("Invalid Refresh Token.");
                }
                else if (user.TokenExpires < DateTime.Now)
                {
                    return Unauthorized("Token expired.");
                }

                string token = AuthUtils.CreateToken(user);
                var newRefreshToken = AuthUtils.GenerateRefreshToken();
                AuthUtils.SetRefreshToken(newRefreshToken, Response, user);

                return Ok(token);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(RefreshToken)}");
                Debug.WriteLine(ex.Message);
                return StatusCode(500);
            }
        }
    }
}
