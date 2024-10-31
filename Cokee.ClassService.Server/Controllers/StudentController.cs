using System.Data;

using Cokee.ClassService.Shared;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cokee.ClassService.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly GlobalDbContext _dbContext;

        public StudentsController(GlobalDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("GetAllStudents")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Student>>> GetAllStudents()
        {
            return await _dbContext.Students.ToListAsync();
        }

        // 根据ID获取学生
        [HttpGet("GetStudentInfo")]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<ActionResult<Student>> GetStudentInfo(int id)
        {
            var student = await _dbContext.Students.FindAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            return student;
        }

        // 创建学生
        [HttpPost("CreateStudent")]
        [AllowAnonymous]
        public async Task<ActionResult<Student>> CreateStudent(Student student)
        {
            _dbContext.Students.Add(student);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStudentInfo), new { id = student.ID }, student);
        }

        // 更新学生
        [HttpPut("UpdateStudent")]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> UpdateStudent(int id, Student student)
        {
            if (id != student.ID)
            {
                return BadRequest();
            }

            _dbContext.Entry(student).State = EntityState.Modified;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
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

        // 删除学生
        [HttpDelete("DeleteStudent")]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _dbContext.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            _dbContext.Students.Remove(student);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [NonAction]
        private bool StudentExists(int id)
        {
            return _dbContext.Students.Any(e => e.ID == id);
        }
    }
}