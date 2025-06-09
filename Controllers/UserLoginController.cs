using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using User_Login.Model;

namespace User_Login.Controllers
{
    public class AccountController : Controller
    {
        string conStr = "server=localhost;database=login_db;uid=root;pwd=Admin;";

        // Login GET
        public ActionResult Login() => View();

        // Login POST
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            using (MySqlConnection con = new MySqlConnection(conStr))
            {
                string query = "SELECT * FROM Users WHERE Username=@u AND Password=@p";
                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);

                con.Open();
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    HttpContext.Session.SetString("UserId", reader["UserId"].ToString());
                    HttpContext.Session.SetString("Username", reader["Username"].ToString());

                    return RedirectToAction("Index");
                }
                ViewBag.Error = "Invalid credentials.";
            }
            return View();
        }

        // Logout
        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // List All Users
        public ActionResult Index()
        {
            if (HttpContext.Session.GetString("UserId") == null)
                return RedirectToAction("Login");

            int loggedUserId = int.Parse(HttpContext.Session.GetString("UserId"));

            List<UserModel> list = new List<UserModel>();
            using (MySqlConnection con = new MySqlConnection(conStr))
            {
                con.Open();
                var cmd = new MySqlCommand("SELECT * FROM Users", con);
                var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new UserModel
                    {
                        UserId = Convert.ToInt32(dr["UserId"]),
                        Username = dr["Username"].ToString(),
                        Email = dr["Email"].ToString(),
                        Password = dr["Password"].ToString()
                    });
                }
            }

            var orderedList = list
                .OrderByDescending(u => u.UserId == loggedUserId)  
                .ThenBy(u => u.Username)
                .ToList();

            ViewBag.Username = HttpContext.Session.GetString("Username");

            return View(orderedList);
        }


        // Register GET
        public ActionResult Register()
        {
            if (HttpContext.Session.GetString("UserId") == null)
                return RedirectToAction("Login");

            return View();
        }

        // Register POST
        [HttpPost]
        public ActionResult Register(UserModel user)
        {
            using (MySqlConnection con = new MySqlConnection(conStr))
            {
                string query = "INSERT INTO Users (Username, Email, Password) VALUES (@u, @e, @p)";
                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@u", user.Username);
                cmd.Parameters.AddWithValue("@e", user.Email);
                cmd.Parameters.AddWithValue("@p", user.Password);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            // Send confirmation email after user creation
            try
            {
                SendConfirmationEmail(user.Email, user.Username);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return RedirectToAction("Index");
        }

        private void SendConfirmationEmail(string email, string username)
        {
            var fromAddress = new MailAddress("jineshbakhai59@gmail.com", "User Login");
            var toAddress = new MailAddress(email, username);
            const string fromPassword = "xksj nwoz kawt anmp";
            const string subject = "Registration Successful";
            string body = $"Hello {username},\n\nYour account has been created successfully.\n\nThank you for registering!";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                Timeout = 20000,
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = false,
            })
            {
                smtp.Send(message);
            }
        }

        // Edit GET
        public ActionResult Edit(int id)
        {
            if (HttpContext.Session.GetString("UserId") == null)
                return RedirectToAction("Login");

            UserModel user = new UserModel();
            using (MySqlConnection con = new MySqlConnection(conStr))
            {
                var cmd = new MySqlCommand("SELECT * FROM Users WHERE UserId=@id", con);
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                var dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    user.UserId = Convert.ToInt32(dr["UserId"]);
                    user.Username = dr["Username"].ToString();
                    user.Email = dr["Email"].ToString();
                    user.Password = dr["Password"].ToString();
                }
            }
            return View(user);
        }

        // Edit POST
        [HttpPost]
        public ActionResult Edit(UserModel user)
        {
            using (MySqlConnection con = new MySqlConnection(conStr))
            {
                var cmd = new MySqlCommand("UPDATE Users SET Username=@u, Email=@e, Password=@p WHERE UserId=@id", con);
                cmd.Parameters.AddWithValue("@id", user.UserId);
                cmd.Parameters.AddWithValue("@u", user.Username);
                cmd.Parameters.AddWithValue("@e", user.Email);
                cmd.Parameters.AddWithValue("@p", user.Password);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

       
        public ActionResult Delete(int id)
        {
            // Corrected the session check
            if (HttpContext.Session.GetString("UserId") == null)
                return RedirectToAction("Login");

            using (MySqlConnection con = new MySqlConnection(conStr))
            {
                var cmd = new MySqlCommand("DELETE FROM Users WHERE UserId=@id", con);
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }
    }
}
