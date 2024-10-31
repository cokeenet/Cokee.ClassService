using System.Security.Claims;
using System.Text.Json;

using Cokee.ClassService.Shared;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Cokee.ClassService.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassController : ControllerBase
{
    private readonly GlobalDbContext _dbContext;
    private readonly ILogger<ClassController> _logger;

    public ClassController(GlobalDbContext dbContext, ILogger<ClassController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet("GetClassInfo")]
    [Authorize(Roles = "Teacher,Student,Admin")]
    public async Task<IActionResult> GetClassInfo(int classId)
    {
        // 根据 classId 查询班级信息
        var cls = await _dbContext.Classes.Include(s => s.Owner).FirstOrDefaultAsync(s => s.ID == classId);
        if (cls == null)
        {
            return NotFound("Class not found");
        }

        if (!CanModifyClass(GetUserID(), classId)) return Forbid();

        ClassInfoRequest classInfo = new ClassInfoRequest
        {
            Name = cls.Name,
            ID = cls.ID,
            OwnerID = cls.Owner.UserID,
            SchoolName = cls.SchoolName
        };

        return Ok(classInfo); // 返回班级信息的匿名对象
    }

    [HttpPost("AddClass")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> AddClass(ClassInfoRequest classInfoRequest)
    {
        var user = await _dbContext.Users.FindAsync(GetUserID());
        if (user == null) return Forbid();
        var userClassCount = await _dbContext.Classes.CountAsync(uc => uc.Owner.UserID == GetUserID());
        if (userClassCount >= 10)
        {
            return BadRequest("You can't add more than 10 classes");
        }
        var newClass = new Class
        {
            Name = classInfoRequest.Name,
            Owner = user,
            SchoolName = classInfoRequest.SchoolName,
            CreatedTime = DateTime.Now,
            LastAccessTime = DateTime.Now,
            Grade = classInfoRequest.Grade
        };

        _dbContext.Classes.Add(newClass);
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "Class added successfully", classId = newClass.ID });
    }

    // 删除班级
    [HttpDelete("DeleteClass")]
    [Authorize(Roles = "Teacher,Admin")] // 仅管理员有权限删除班级
    public async Task<IActionResult> DeleteClass(int classId)
    {
        var cls = await _dbContext.Classes.FindAsync(classId);
        if (cls == null)
        {
            return NotFound("Class not found");
        }
        if (!CanModifyClass(GetUserID(), classId)) return Forbid();
        _dbContext.Classes.Remove(cls);
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "Class deleted successfully" });
    }

    // 修改班级信息
    [HttpPut("UpdateClassInfo")]
    [Authorize(Roles = "Teacher,Admin")] // 仅管理员有权限修改班级信息
    public async Task<IActionResult> UpdateClassInfo(int classId, ClassInfoRequest classInfoRequest)
    {
        var cls = await _dbContext.Classes.FindAsync(classId);
        if (cls == null)
        {
            return NotFound("Class not found");
        }
        if (!CanModifyClass(GetUserID(), classId)) return Forbid();

        cls.Name = classInfoRequest.Name;
        cls.SchoolName = classInfoRequest.SchoolName;
        cls.Score = classInfoRequest.Score;

        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "Class updated successfully" });
    }

    [HttpGet("GetStudents")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<string>> GetStudents(int classId)
    {
        if (!CanModifyClass(GetUserID(), classId)) return Forbid();
        var cls = await _dbContext.Classes
            .Include(c => c.Students)
            .FirstOrDefaultAsync(c => c.ID == classId);
        if (cls == null) return NotFound("Class not found");
        // 检查是否有关联的学生
        if (cls.Students == null || !cls.Students.Any()) return NotFound("No students found for this class");
        return Ok(JsonSerializer.Serialize(cls.Students.ToList()));
    }

    private int? GetUserID()
    {
        string? _id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (_id.IsNullOrEmpty()) return null;
        return int.Parse(_id);
    }

    private bool CanModifyClass(int? userId, int classId)
    {
        // 如果是管理员，直接返回 true
        if (User.IsInRole("Admin"))
        {
            return true;
        }
        if (!userId.HasValue) return false;
        // 其他用户需要检查是否拥有该班级
        return _dbContext.Classes.Any(uc => uc.Owner.UserID == userId && uc.ID == classId);
    }
}