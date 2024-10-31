using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Cokee.ClassService.Shared;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // 指定自增
    public int UserID { get; set; }

    public int Phone { get; set; }

    public string? Email { get; set; }

    public string UserName { get; set; }

    [JsonIgnore]
    public string Password { get; set; }

    [JsonIgnore]
    public PasswordType PasswordType { get; set; }

    public string SchoolName { get; set; }

    // 导航属性，表示一个用户拥有的班级
    public ICollection<Class> OwnedClasses { get; set; }

    public DateTime? Birthday { get; set; }
    public DateTime RegiestedTime { get; set; }
    public DateTime? LastLoginTime { get; set; }
    public string Role { get; set; } = "User";
}

public enum PasswordType
{
    Plain, Hash, AES256
}

public enum UserStatus
{
    Normal, Frozing, Banned,
}

public class LoginRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public class UserInfo
{
    public string? UserName { get; set; }
    public int UserID { get; set; }
    public int Phone { get; set; }
    public string? Email { get; set; }
    public string SchoolName { get; set; }
    public DateTime? Birthday { get; set; }
    public DateTime RegiestedTime { get; set; }
    public DateTime? LastLoginTime { get; set; }
    public string Role { get; set; }

    public UserInfo(User user)
    {
        UserName = user.UserName;
        UserID = user.UserID;
        Phone = user.Phone;
        Email = user.Email;
        SchoolName = user.SchoolName;
        Birthday = user.Birthday;
        RegiestedTime = user.RegiestedTime;
        LastLoginTime = user.LastLoginTime;
        Role = user.Role;
    }
}

public class RegisterRequest
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string SchoolName { get; set; }
    public DateTime Birthday { get; set; }
}
public class LoginResult
{
    public string Token { get; set; }
    public UserInfo UserInfo { get; set; }
    public DateTime ExpirationTime { get; set; }
}