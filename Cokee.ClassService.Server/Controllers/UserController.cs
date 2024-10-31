using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Cokee.ClassService.Shared;
using BCrypty = BCrypt.Net.BCrypt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Cokee.ClassService.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly GlobalDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public UserController(GlobalDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            var user = await TryFindUser(model);
            if (user == null)
            {
                return Unauthorized("Invalid username or password"); // 用户不存在或密码错误，返回未授权状态
            }
            var (token, expirationTime) = GetToken(user); // 获取用户的 JWT 令牌和过期时间
            return Ok(new LoginResult { Token = token, ExpirationTime = expirationTime, UserInfo = new UserInfo(user) }); // 返回包含令牌、过期时间和用户信息的成功响应
        }

        /*[AllowAnonymous]
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            if (User == null)
            {
                return Unauthorized(); // 用户不存在或密码错误，返回未授权状态
            }
            var exp = User.FindFirst(ClaimTypes.Expiration).Value;
            var (token, expirationTime) = GetToken(); // 获取用户的 JWT 令牌和过期时间
            return Ok(new LoginResult { Token = token, ExpirationTime = expirationTime, UserInfo = new UserInfo(user) }); // 返回包含令牌、过期时间和用户信息的成功响应
        }*/

        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {
            // 检查用户名是否已存在
            if (await _dbContext.Users.AnyAsync(u => u.UserName == registerRequest.UserName))
            {
                return Conflict("Username already exists"); // 用户名已存在，返回冲突状态码
            }

            var newUser = new User
            {
                UserName = registerRequest.UserName,
                //Password = AES256Encryption.Encrypt(registerRequest.Password),
                //PasswordType = PasswordType.AES256,
                Password = registerRequest.Password,
                PasswordType = PasswordType.Plain,
                SchoolName = registerRequest.SchoolName,
                Birthday = registerRequest.Birthday,
                RegiestedTime = DateTime.Now
            };

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();
            var (token, expirationTime) = GetToken(newUser); // 获取用户的 JWT 令牌和过期时间

            return Ok(new LoginResult { Token = token, ExpirationTime = expirationTime, UserInfo = new UserInfo(newUser) }); // 返回包含令牌、过期时间和用户信息的成功响应
        }

        // 删除用户
        [HttpDelete("DeleteUser/{userId}")]
        [Authorize(Roles = "Admin")] // 仅管理员有权限删除用户
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully" });
        }

        [NonAction]
        private async Task<User?> TryFindUser(LoginRequest req)
        {
            // 根据用户名从数据库中查询用户信息
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == req.Username);
            if (user != null)
            {
                switch (user.PasswordType)
                {
                    case PasswordType.Plain:
                        if (req.Password == user.Password) return user;
                        break;

                    case PasswordType.Hash:
                        if (BCrypty.Verify(req.Password, user.Password)) return user;
                        break;
                }
                return null;
            }
            return null; // 用户不存在或密码错误，返回 null
        }

        [NonAction]
        public (string, DateTime) GetToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWTKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "Cokee", // 发行者
                audience: "CokeeClassService", // 订阅者
                claims: new[] {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.MobilePhone,user.Phone.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                },
                expires: DateTime.Now.AddDays(365), // 令牌过期时间
                signingCredentials: credentials
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return (tokenString, token.ValidTo); // 返回令牌字符串和过期时间
        }
    }
}