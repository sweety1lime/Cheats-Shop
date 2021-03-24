using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Shop_v_2.Models
{
    public class Users:LoginModel
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "Введите логин")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Длина строки должна быть от 3 до 50 символов")]
        public string login { get; set; }
        [Required(ErrorMessage = "Укажите почту для связи")]
        public string email { get; set; }
        [Required(ErrorMessage = "Введите пароль")]
        public string password { get; set; }
        [Compare("password", ErrorMessage = "Пароли не совпадают")]
        public string password_confirm { get; set; }


    }
}
