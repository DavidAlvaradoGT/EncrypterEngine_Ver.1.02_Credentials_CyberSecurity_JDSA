using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Hosting;

namespace EncrypterEngine_JDSA.DataBase
{
    public class CredentialRecord
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string EncryptedUserName { get; set; }
        public string EncryptedPassword { get; set; }
        public string DecryptedUserName { get; set; }
        public string DecryptedPassword { get; set; }
        public string UserDateCreation { get; set; }
    }

    public static class DataConfig
    {
        private static readonly object SyncLock = new object();

        public static string GetStorageDirectory()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string candidateProjectDir = Path.GetFullPath(Path.Combine(baseDir, @"..\.."));
            string projectDataBaseDir = Path.Combine(candidateProjectDir, "DataBase");

            if (Directory.Exists(projectDataBaseDir) || File.Exists(Path.Combine(candidateProjectDir, "Encrypter_JDSA.csproj")))
            {
                Directory.CreateDirectory(projectDataBaseDir);
                return projectDataBaseDir;
            }

            if (HostingEnvironment.IsHosted)
            {
                string mapped = HostingEnvironment.MapPath("~/DataBase");
                if (!string.IsNullOrWhiteSpace(mapped))
                {
                    Directory.CreateDirectory(mapped);
                    return mapped;
                }
            }

            string fallbackDir = Path.Combine(baseDir, "DataBase");
            Directory.CreateDirectory(fallbackDir);
            return fallbackDir;
        }

        public static string GetRecordsFilePath()
        {
            return Path.Combine(GetStorageDirectory(), "CredentialsData.dat");
        }

        public static CredentialRecord[] LoadReversedArray()
        {
            lock (SyncLock)
            {
                List<CredentialRecord> chronological = ReadChronologicalRecords();

                CredentialRecord[] reversed = new CredentialRecord[chronological.Count];
                for (int i = 0; i < chronological.Count; i++)
                {
                    reversed[i] = chronological[chronological.Count - 1 - i];
                }

                Data.RecordsArray = reversed;
                return reversed;
            }
        }

        public static CredentialRecord AddRecord(
            string userName,
            string password,
            string encryptedUserName,
            string encryptedPassword,
            string decryptedUserNameReversed,
            string decryptedPasswordReversed,
            string userDateCreation)
        {
            lock (SyncLock)
            {
                List<CredentialRecord> chronological = ReadChronologicalRecords();

                int nextId = 1;
                foreach (var rec in chronological)
                {
                    if (rec.Id >= nextId)
                    {
                        nextId = rec.Id + 1;
                    }
                }

                var newRecord = new CredentialRecord
                {
                    Id = nextId,
                    UserName = userName ?? string.Empty,
                    Password = password ?? string.Empty,
                    EncryptedUserName = encryptedUserName ?? string.Empty,
                    EncryptedPassword = encryptedPassword ?? string.Empty,
                    DecryptedUserName = decryptedUserNameReversed ?? string.Empty,
                    DecryptedPassword = decryptedPasswordReversed ?? string.Empty,
                    UserDateCreation = userDateCreation ?? string.Empty
                };

                chronological.Add(newRecord);
                WriteChronologicalRecords(chronological);
                LoadReversedArray();

                return newRecord;
            }
        }

        private static List<CredentialRecord> ReadChronologicalRecords()
        {
            var list = new List<CredentialRecord>();
            string path = GetRecordsFilePath();

            if (!File.Exists(path))
            {
                return list;
            }

            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] parts = line.Split('|');
                if (parts.Length < 8)
                {
                    continue;
                }

                int id;
                int.TryParse(DecodeBase64(parts[0]), out id);

                list.Add(new CredentialRecord
                {
                    Id = id,
                    UserName = DecodeBase64(parts[1]),
                    Password = DecodeBase64(parts[2]),
                    EncryptedUserName = DecodeBase64(parts[3]),
                    EncryptedPassword = DecodeBase64(parts[4]),
                    DecryptedUserName = DecodeBase64(parts[5]),
                    DecryptedPassword = DecodeBase64(parts[6]),
                    UserDateCreation = DecodeBase64(parts[7])
                });
            }

            return list;
        }

        private static void WriteChronologicalRecords(List<CredentialRecord> records)
        {
            string path = GetRecordsFilePath();
            var lines = new List<string>(records.Count);

            foreach (var r in records)
            {
                string row = string.Join("|",
                    EncodeBase64(r.Id.ToString()),
                    EncodeBase64(r.UserName),
                    EncodeBase64(r.Password),
                    EncodeBase64(r.EncryptedUserName),
                    EncodeBase64(r.EncryptedPassword),
                    EncodeBase64(r.DecryptedUserName),
                    EncodeBase64(r.DecryptedPassword),
                    EncodeBase64(r.UserDateCreation)
                );
                lines.Add(row);
            }

            File.WriteAllLines(path, lines, Encoding.UTF8);
        }

        private static string EncodeBase64(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text ?? string.Empty);
            return Convert.ToBase64String(bytes);
        }

        private static string DecodeBase64(string base64)
        {
            if (string.IsNullOrEmpty(base64))
            {
                return string.Empty;
            }

            byte[] bytes = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}