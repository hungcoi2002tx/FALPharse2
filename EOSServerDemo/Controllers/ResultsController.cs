using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EOSServerDemo.Models;
using EOSServerDemo.Dtos;

namespace EOSServerDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly CompareFaceContext _context;

        public ResultsController(CompareFaceContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ResultCompareFaceDto>>> GetResults(
         [FromQuery] string? status = null,
         [FromQuery] string? studentCode = null,
         [FromQuery] string? examCode = null,
         [FromQuery] string? sortField = null,
         [FromQuery] bool sortDesc = false,
         [FromQuery] DateTime? startDate = null,
         [FromQuery] DateTime? endDate = null,
         [FromQuery] double? minConfidence = null,
         [FromQuery] double? maxConfidence = null)
        {
            var query = _context.Results.AsQueryable();

            // 1. Lọc theo status
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status);
            }

            // 2. Tìm kiếm theo mã sinh viên
            if (!string.IsNullOrEmpty(studentCode))
            {
                query = query.Where(r => r.Source.StudentCode.Contains(studentCode));
            }            
            
            if (!string.IsNullOrEmpty(examCode))
            {
                query = query.Where(r => r.ExamCode != null && r.ExamCode.Contains(examCode));
            }

            // 3. Lọc theo ngày (startDate và endDate)
            if (startDate.HasValue)
            {
                query = query.Where(r => r.Time >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(r => r.Time <= endDate.Value);
            }

            // 4. Lọc theo confidence
            if (minConfidence.HasValue)
            {
                query = query.Where(r => r.Confidence >= minConfidence.Value);
            }
            if (maxConfidence.HasValue)
            {
                query = query.Where(r => r.Confidence <= maxConfidence.Value);
            }

            // 5. Sắp xếp theo trường được chỉ định
            if (!string.IsNullOrEmpty(sortField))
            {
                query = sortField.ToLower() switch
                {
                    "studentcode" => sortDesc ? query.OrderByDescending(r => r.Source.StudentCode) : query.OrderBy(r => r.Source.StudentCode),
                    "time" => sortDesc ? query.OrderByDescending(r => r.Time) : query.OrderBy(r => r.Time),
                    "status" => sortDesc ? query.OrderByDescending(r => r.Status) : query.OrderBy(r => r.Status),
                    "confidence" => sortDesc ? query.OrderByDescending(r => r.Confidence) : query.OrderBy(r => r.Confidence),
                    _ => query.OrderBy(r => r.ResultId) // Mặc định sắp xếp theo ResultId nếu không có field hợp lệ
                };
            }
            else
            {
                query = query.OrderBy(r => r.ResultId); // Mặc định sắp xếp theo ResultId
            }

            // Thực hiện truy vấn và chuyển đổi kết quả
            var results = await query
                .Select(r => new ResultCompareFaceDto
                {
                    StudentCode = r.Source.StudentCode,
                    Time = r.Time,
                    Status = r.Status,
                    Confidence = r.Confidence,
                    Message = r.Message
                })
                .ToListAsync();

            return results;
        }


        // GET: api/Results/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ResultCompareFaceDto>> GetResult(int id)
        {
            var result = await _context.Results
                .Include(r => r.Source)
                .Where(r => r.ResultId == id)
                .Select(r => new ResultCompareFaceDto
                {
                    StudentCode = r.Source.StudentCode,
                    Time = r.Time,
                    Status = r.Status,
                    Confidence = r.Confidence,
                    ExamCode = r.ExamCode,
                    Note = r.Note,
                    Message = r.Message
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound();
            }

            return result;
        }

        // PUT: api/Results/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutResult(int id, Result result)
        {
            if (id != result.ResultId)
            {
                return BadRequest();
            }

            _context.Entry(result).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResultExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Results
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Result>> PostResult(Result result)
        {
            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetResult", new { id = result.ResultId }, result);
        }

        // DELETE: api/Results/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            var result = await _context.Results.FindAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ResultExists(int id)
        {
            return _context.Results.Any(e => e.ResultId == id);
        }
    }
}
