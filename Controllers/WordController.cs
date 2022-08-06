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
    public class WordController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ILogger<WordController> _logger;

        public WordController(DataContext context, ILogger<WordController> logger)
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
                Response<List<WordModel>> words = new Response<List<WordModel>>(_context.Words!.ToList()); 

                return Ok(words);
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
                Response<List<WordModel>> words = new Response<List<WordModel>>(await _context.Words!.Where(p => p.Update_Date >= dtFrom).ToListAsync());

                return Ok(words);
            }
            catch (Exception ex)
            {
                ErrorResponse errorResponse = new ErrorResponse(ex.Message);

                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("{list}")]
        public IActionResult PostWords([FromBody]List<WordModel> words)
        {
            try
            {
                _context.Database.BeginTransaction();

                foreach(WordModel word in words)
                {
                    bool IsExist = _context.Words!.Where(i => i.Id == word.Id).Any();

                    if (IsExist)
                    {
                        _context.Words!.Update(word);
                    }
                    else
                    {
                        _context.Words!.Add(word);
                    }
                }

                _context.Database.CommitTransaction();

                Response<List<WordModel>> response = new Response<List<WordModel>>(words);

                return Ok(response);
            }
            catch(Exception ex)
            {
                _context.Database.RollbackTransaction();

                ErrorResponse errorResponse = new ErrorResponse(ex.Message);

                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost]
        public IActionResult PostWord([FromBody] WordModel word)
        {
            try
            {
                _context.Database.BeginTransaction();

                bool IsExist = _context.Words!.Where(i => i.Id == word.Id).Any();

                if (IsExist)
                {
                    _context.Words!.Update(word);
                }
                else
                {
                    _context.Words!.Add(word);
                }

                _context.Database.CommitTransaction();

                Response<WordModel> response = new Response<WordModel>(word);

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
        public IActionResult DeleteWord([FromBody] WordModel word)
        {
            try
            {
                _context.Database.BeginTransaction();

                bool IsExist = _context.Words!.Where(i => i.Id == word.Id).Any();

                if (IsExist)
                {
                    _context.Words!.Remove(word);
                }

                _context.Database.CommitTransaction();

                Response<WordModel> response = new Response<WordModel>(word);

                return Ok($"{word.Id} Deleted");
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
