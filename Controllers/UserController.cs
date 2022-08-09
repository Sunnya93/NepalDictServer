using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AutoMapper;
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
        private readonly IMapper _mapper;
        private UserService _userService;

        public UserController(DataContext context, ILogger<UserController> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _userService = new UserService(context);
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate(string userPhone, string password)
        {
            var user = await _userService.Authenticate(userPhone, password);

            if (user is null)
                return BadRequest(new { message = "Username or password is incorrect" });

            var resources = _mapper.Map<UserModel, UserResource>(user);
            Response<UserResource> response = new Response<UserResource>(resources);

            return Ok(response);
        }

        [HttpGet("{userid}/{username}/{useYN}")]
        public async Task<IActionResult> GetFromParams(string userid, string username, string useYN)
        {
            try
            {
                List<UserModel> users = new List<UserModel>(await _context.Users!.Where(i => i.UserPhone == userid)
                                                                                 .Where(i => i.UserName == username)
                                                                                 .Where(i => i.UseYN == useYN).ToListAsync());

                var resources = _mapper.Map<List<UserModel>, List<UserResource>>(users);
                Response<List<UserResource>> response = new Response<List<UserResource>>(resources);

                return Ok(response);
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
                user.PassWord = BCrypt.Net.BCrypt.HashPassword(user.PassWord);

                if (IsExist)
                {
                    _context.Users!.Update(user);
                }
                else
                {
                    _context.Users!.Add(user);
                }
                _context.Database.CommitTransaction();

                var resources = _mapper.Map<UserModel, UserResource>(user);
                Response<UserResource> response = new Response<UserResource>(resources);

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
        public IActionResult ResetPassWord(string id, string name, string hashedPassword)
        {
            try
            {
                _context.Database.BeginTransaction();

                UserModel? user = _context.Users!.Where(i => i.UserPhone == id)
                                                 .Where(i => i.UserName!.Contains(name)).FirstOrDefault();
                if (user is not null)
                {
                    user.PassWord = BCrypt.Net.BCrypt.HashPassword(hashedPassword);
                    user.Update_Date = DateTime.Now;
                    user.Update_User = user.UserPhone;

                    _context.Users!.Update(user);

                    _context.Database.CommitTransaction();

                    Response<string> response = new Response<string>("비밀번호 변경 완료");
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

                Response<string> response = new Response<string>($"{user.UserName} 삭제 완료");

                return Ok(response);
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
