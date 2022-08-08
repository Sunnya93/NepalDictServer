using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NepalDictLib.Models;
using NepalDictServer.DbContext;

namespace NepalDictServer.Controllers
{
    [Authorization.Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ILogger<UserController> _logger;
        private UserService _userService;

        public UserController(DataContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
            _userService = new UserService(context);
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate(string userPhone, string password)
        {
            //&& x.PassWord == password

            // wrapped in "await Task.Run" to mimic fetching user from a db
            var user = await _userService.Authenticate(userPhone, password);

            // on auth fail: null is returned because user is not found
            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            Response<UserModel> response = new Response<UserModel>(user!);

            // on auth success: user object is returned
            return Ok(response);
        }

        [HttpGet("{userid}/{username}/{useYN}")]
        public async Task<IActionResult> GetFromParams(string userid, string username, string useYN)
        {
            try
            {
                Response<List<UserModel>> users = new Response<List<UserModel>>(await _context.Users!.Where(i => i.UserPhone == userid)
                                                                                                     .Where(i => i.UserName == username)
                                                                                                     .Where(i => i.UseYN == useYN).ToListAsync());

                return Ok(users);
            }
            catch (Exception ex)
            {
                ErrorResponse errorResponse = new ErrorResponse(ex.Message);

                return StatusCode(500, errorResponse);
            }
        }


        [HttpPost]
        public IActionResult PostUser([FromBody] UserModel user)
        {
            try
            {
                _context.Database.BeginTransaction();

                bool IsExist = _context.Users!.Where(i => i.UserPhone == user.UserPhone).Any();

                if (IsExist)
                {
                    _context.Users!.Update(user);
                }
                else
                {
                    _context.Users!.Add(user);
                }

                _context.Database.CommitTransaction();

                Response<UserModel> response = new Response<UserModel>(user);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _context.Database.RollbackTransaction();

                ErrorResponse errorResponse = new ErrorResponse(ex.Message);

                return StatusCode(500, errorResponse);
            }
        }

        [AllowAnonymous]
        [HttpPost("{id}/{name}")]
        public IActionResult ResetPassWord(string id, string name)
        {
            try
            {
                _context.Database.BeginTransaction();

                UserModel? user = _context.Users!.Where(i => i.UserPhone == id)
                                                 .Where(i => i.UserName!.Contains(name)).FirstOrDefault();
                if (user is not null)
                {

                    byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);
                    string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                                                                                password: id!,
                                                                                salt: salt,
                                                                                prf: KeyDerivationPrf.HMACSHA256,
                                                                                iterationCount: 100000,
                                                                                numBytesRequested: 256 / 8));

                    user.PassWord = hashed;
                    user.PassWord_Salt = salt;
                    user.Update_Date = DateTime.Now;
                    user.Update_User = user.UserPhone;

                    _context.Users!.Update(user);

                    _context.Database.CommitTransaction();

                    Response<string> response = new Response<string>(Convert.ToBase64String(salt));
                    return Ok(response);

                }
                else
                {
                    throw new Exception("사용자 정보가 맞지 않습니다. 비밀번호를 변경할 수 없습니다.");
                }
            }
            catch (Exception ex)
            {
                _context.Database.RollbackTransaction();

                ErrorResponse errorResponse = new ErrorResponse(ex.Message);

                return StatusCode(500, errorResponse);
            }
        }

        [HttpDelete]
        public IActionResult DeleteUser([FromBody] UserModel user)
        {
            try
            {
                _context.Database.BeginTransaction();

                bool IsExist = _context.Users!.Where(i => i.UserPhone == user.UserPhone).Any();

                if (IsExist)
                {
                    _context.Users!.Remove(user);
                }

                _context.Database.CommitTransaction();

                Response<UserModel> response = new Response<UserModel>(user);

                return Ok($"{user.UserName} Deleted");
            }
            catch (Exception ex)
            {
                _context.Database.RollbackTransaction();

                ErrorResponse errorResponse = new ErrorResponse(ex.Message);

                return StatusCode(500, errorResponse);
            }
        }
    }
}
