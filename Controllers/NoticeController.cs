using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class NoticeController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ILogger<NoticeController> _logger;

        public NoticeController(DataContext context, ILogger<NoticeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        // GET: Word
        public IActionResult Get()
        {
            try
            {
                Response<List<NoticeModel>> notices = new Response<List<NoticeModel>>(_context.Notices!.ToList()); 

                return Ok(notices);
            }
            catch(Exception ex)
            {
                ErrorResponse errorResponse = new ErrorResponse(ex.Message);

                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("{FromDate}")]
        public async Task<IActionResult> GetFromDate(string FromDate)
        {
            try
            {
                DateTime dtFrom = Convert.ToDateTime(Func.ConvertDateString(FromDate));
                Response<List<NoticeModel>> notices = new Response<List<NoticeModel>>(await _context.Notices!.Where(p => p.Insert_Date >= dtFrom).ToListAsync());

                return Ok(notices);
            }
            catch (Exception ex)
            {
                ErrorResponse errorResponse = new ErrorResponse(ex.Message);

                return StatusCode(500, errorResponse);
            }
        }


        [HttpPost]
        public IActionResult PostNotice([FromBody] NoticeModel notice)
        {
            try
            {
                _context.Database.BeginTransaction();

                bool IsExist = _context.Notices!.Where(i => i.Id == notice.Id).Any();

                if (IsExist)
                {
                    _context.Notices!.Update(notice);
                }
                else
                {
                    _context.Notices!.Add(notice);
                }

                _context.Database.CommitTransaction();

                Response<NoticeModel> response = new Response<NoticeModel>(notice);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _context.Database.RollbackTransaction();

                ErrorResponse errorResponse = new ErrorResponse(ex.Message);

                return StatusCode(500, errorResponse);
            }
        }

        [HttpDelete]
        public IActionResult DeleteNotice([FromBody] NoticeModel notices)
        {
            try
            {
                _context.Database.BeginTransaction();

                bool IsExist = _context.Notices!.Where(i => i.Id == notices.Id).Any();

                if (IsExist)
                {
                    _context.Notices!.Remove(notices);
                }

                _context.Database.CommitTransaction();

                Response<NoticeModel> response = new Response<NoticeModel>(notices);

                return Ok($"{notices.Id} Deleted");
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
