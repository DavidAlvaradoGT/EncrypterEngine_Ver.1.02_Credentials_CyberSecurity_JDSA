using System.Collections.Generic;
using System.Web.Mvc;
using EncrypterEngine_JDSA.DataBase;
using EncrypterEngine_JDSA.Models;

namespace EncrypterEngine_JDSA.Controllers
{
    public class IndexController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            var model = BuildPageModel(string.Empty, string.Empty);
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(string UserNameInput, string PasswordInput)
        {
            string userName = UserNameInput ?? string.Empty;
            string password = PasswordInput ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(password))
            {
                string encryptedUser = EncrypterEngine.Encrypt(userName);
                string encryptedPass = EncrypterEngine.Encrypt(password);

                // Desencripta desde el texto encriptado y luego invierte el string (al revés)
                string decryptedUserPlain = EncrypterEngine.Decrypt(encryptedUser);
                string decryptedPassPlain = EncrypterEngine.Decrypt(encryptedPass);

                string decryptedUserReversed = EncrypterEngine.ReverseText(decryptedUserPlain);
                string decryptedPassReversed = EncrypterEngine.ReverseText(decryptedPassPlain);

                string dateNow = DateTimeController.GetFormattedNetworkDate();

                DataConfig.AddRecord(
                    userName,
                    password,
                    encryptedUser,
                    encryptedPass,
                    decryptedUserReversed,
                    decryptedPassReversed,
                    dateNow
                );
            }

            var model = BuildPageModel(string.Empty, string.Empty);
            return View(model);
        }

        private UserCredentialPageModel BuildPageModel(string userNameInput, string passwordInput)
        {
            CredentialRecord[] reversedRecords = DataConfig.LoadReversedArray();
            var rows = new List<UserCredentialRow>(reversedRecords.Length);

            foreach (var item in reversedRecords)
            {
                rows.Add(new UserCredentialRow
                {
                    Id = item.Id,
                    UserName = item.UserName,
                    Password = item.Password,
                    EncryptedUserName = item.EncryptedUserName,
                    EncryptedPassword = item.EncryptedPassword,
                    DecryptedUserName = item.DecryptedUserName,
                    DecryptedPassword = item.DecryptedPassword,
                    UserDateCreation = item.UserDateCreation
                });
            }

            return new UserCredentialPageModel
            {
                UserNameInput = userNameInput,
                PasswordInput = passwordInput,
                NetworkDateUsed = DateTimeController.GetFormattedNetworkDate(),
                Users = rows
            };
        }
    }
}