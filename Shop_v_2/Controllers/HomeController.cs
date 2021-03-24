using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shop_v_2.Models;

namespace Shop_v_2.Controllers
{
    public class HomeController : Controller
    {

        private ApplicationContext db;


        public HomeController(ApplicationContext context)
        {
            this.db = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await db.Products.ToListAsync());
        }

        public IActionResult Reg()
        {
           
            return View();
        }
        public IActionResult Cart()
        {
            Cart cart = new Cart();

            if (HttpContext.Session.Keys.Contains("Cart"))//Проверяем есть ли сохранённая корзина у пользователя

                cart = JsonSerializer.Deserialize<Cart>(HttpContext.Session.GetString("Cart")); //десериализируем корзину из сессии

            return View(cart);

        }
        public IActionResult AddToCart()

        {

            int ID = Convert.ToInt32(Request.Query["ID"]); // Получаем ID из строки

            Cart cart = new Cart();

            if (HttpContext.Session.Keys.Contains("Cart"))//Проверяем есть ли сохранённая корзина у пользователя

                cart = JsonSerializer.Deserialize<Cart>(HttpContext.Session.GetString("Cart")); //десериализируем корзину из сессии

            cart.CartLines.Add(db.Products.Find(ID)); //Добавляем в корзину элемент Product по первичному ключу

            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize<Cart>(cart)); // Сохраняем новую корзину в сессию

            return Redirect("~/Home/Index"); //Возвращение пользователя на первоначальную страницу

        }

        public IActionResult RemoveFromCart()

        {

            int number = Convert.ToInt32(Request.Query["Number"]); // Получаем номер позиции в корзине

            Cart cart = new Cart();

            if (HttpContext.Session.Keys.Contains("Cart")) // Проверяем есть ли сохранённая корзина у пользователя

                cart = JsonSerializer.Deserialize<Cart>(HttpContext.Session.GetString("Cart")); // десериализируем корзину из сессии

            cart.CartLines.RemoveAt(number); // Удаляем товар из корзины но индексу

            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize<Cart>(cart)); // Сохраняем новую корзину в сессию

            return Redirect("~/Home/Cart"); //Возвращение пользователя на страницу с корзиной

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reg(Users person)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(person);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
             
            else
            {
                return View(person);
            }
                
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Jabka(LoginModel model)
        {

            if (ModelState.IsValid)
            {
                Users user = await db.Users.FirstOrDefaultAsync(u => u.login == model.Login && u.password == model.Password);
                if (user != null)
                {
                    await Authenticate(model.Login); // аутентификация

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Некорректные логин и(или) пароль");
            }
            return RedirectToAction("Reg", "Home");

        }
        private async Task Authenticate(string userName)
        {
            // создаем один claim
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Order()
        {
            if (User.Identity.IsAuthenticated)
            {
                Cart cart = new Cart();

                if (HttpContext.Session.Keys.Contains("Cart")) // Проверяем есть ли сохранённая корзина у пользователя

                    cart = JsonSerializer.Deserialize<Cart>(HttpContext.Session.GetString("Cart")); // десериализируем корзину из сессии

                string email = "";
                List<Users> users = await db.Users.ToListAsync();
                foreach (var item in users)
                {
                    if (User.Identity.Name == item.login)
                    {

                        email = item.email;
                        // отправитель - устанавливаем адрес и отображаемое в письме имя
                        MailAddress from = new MailAddress("seryakushka@bk.ru", "QASmoke");
                        // кому отправляем
                        MailAddress to = new MailAddress(email);
                        // создаем объект сообщения
                        MailMessage m = new MailMessage(from, to);
                        // тема письма
                        m.Subject = "Оформление заказа";
                        // текст письма
                        StringBuilder str = new StringBuilder("<ul>");
                        foreach (var item1 in cart.CartLines)
                        {
                            str.Append("<li>" + item1.titile + ' ' + item1.price + "</li> ");
                        }
                        str.Append("</ul>");
                        m.Body = Convert.ToString(str);
                        // письмо представляет код html
                        m.IsBodyHtml = true;
                        // адрес smtp-сервера и порт, с которого будем отправлять письмо
                        SmtpClient smtp = new SmtpClient("smtp.mail.ru", 587);
                        smtp.Credentials = new NetworkCredential("seryakushka@bk.ru", "EBR_MAMKI");
                        smtp.EnableSsl = true;
                        smtp.Send(m);
                    }

                }
            }
            return RedirectToAction("Index");
        }
    }
}
