using System.Collections.Generic;

namespace EncrypterEngine_JDSA.DataBase
{
    // Encabezados:
    // Id | UserName | Password | EncryptedPassword | DecryptedUserName | DecryptedPassword | UserDateCreation
    public static class Data
    {
        public static CredentialRecord[] RecordsArray = new CredentialRecord[0];

        public static List<CredentialRecord> GetRecordsReversed()
        {
            var list = new List<CredentialRecord>();
            if (RecordsArray != null)
            {
                for (int i = RecordsArray.Length - 1; i >= 0; i--)
                {
                    if (RecordsArray[i] != null)
                    {
                        list.Add(RecordsArray[i]);
                    }
                }
            }
            return list;
        }
    }
}