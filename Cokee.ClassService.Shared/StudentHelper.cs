using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Cokee.ClassService.Shared
{
    public class Student
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? ID { get; set; }

        public Sex Sex { get; set; } // 0 girl 1 boy
        public string Name { get; set; }
        public int Score { get; set; } = 0;
        public DateTime? BirthDay { get; set; }
        public string? RoleStr { get; set; }
        public int Role { get; set; } = 0; // 0-3
        public string? Desc { get; set; }
        public long? QQ { get; set; }
        public bool IsMinorLang { get; set; }
        public string? HeadPicUrl { get; set; } = "/Resources/head.jpg";
        public string? Password { get; set; }
        public PasswordType? PasswordType { get; set; }
        public int EnrollClassID { get; set; }

        // 外键
        [JsonIgnore]
        public virtual Class? Class { get; set; } // 导航属性，表示一个学生所在的班级
    }

    public enum SexCombo
    {
        None,
        Boy,
        Girl
    }

    public enum Sex
    {
        Girl,
        Boy
    }
}