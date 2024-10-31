using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Cokee.ClassService.Shared;

public class Class
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // 指定自增
    public int ID { get; set; }

    public string Name { get; set; }
    public int Score { get; set; }
    public GradeType Grade { get; set; }
    [JsonIgnore]
    public virtual User Owner { get; set; }

    public string? SchoolName { get; set; }
    public DateTime CreatedTime { get; set; }

    [JsonIgnore]
    public DateTime? LastAccessTime { get; set; }

    // 导航属性，表示一个班级包含的学生
    public ICollection<Student> Students { get; set; }
}

public enum GradeType
{
    Primary1, Primary2, Primary3, Primary4, Primary5, Primary6,
    Mid1, Mid2, Mid3,
    High1, High2, High3
}

public class ClassInfoRequest
{
    public string Name { get; set; }
    public int ID { get; set; }
    public int OwnerID { get; set; }
    public GradeType Grade { get; set; }
    public string? SchoolName { get; set; }
    public int Score { get; set; }
}