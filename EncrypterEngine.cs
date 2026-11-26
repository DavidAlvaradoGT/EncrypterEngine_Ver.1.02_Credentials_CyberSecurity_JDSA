using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace EncrypterEngine_JDSA
{
    public static class EncrypterEngine
    {
        private const int BlockLength = 10;
        private const int PayloadCharsPerBlock = 4;

        private const string CipherAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789/-*._?";
        private const int BaseCount = 42;
        private const int FixedShift = 19;
        private const int FixedMask = 0x5A3C;

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return string.Empty;
            }

            var blocks = new List<string>(plainText.Length);

            foreach (char ch in plainText)
            {
                int rawCode = ch;
                int encodedVal = ((rawCode + FixedShift) & 0xFFFF) ^ FixedMask;
                char[] core = new char[PayloadCharsPerBlock];
                int temp = encodedVal;
                for (int i = PayloadCharsPerBlock - 1; i >= 0; i--)
                {
                    core[i] = CipherAlphabet[temp % BaseCount];
                    temp /= BaseCount;
                }

                string padding = GenerateRandomPadding(BlockLength - PayloadCharsPerBlock);
                string tenCharBlock = new string(core) + padding;

                if (tenCharBlock.Length == BlockLength)
                {
                    blocks.Add(tenCharBlock);
                }
            }

            return string.Join("|", blocks);
        }

        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrWhiteSpace(encryptedText))
            {
                return string.Empty;
            }

            string[] rawBlocks = encryptedText.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder(rawBlocks.Length);

            foreach (string rawBlock in rawBlocks)
            {
                string block = rawBlock.Trim();
                if (block.Length != BlockLength)
                {
                    continue;
                }

                int decodedVal = 0;
                bool valid = true;

                for (int i = 0; i < PayloadCharsPerBlock; i++)
                {
                    int digit = CipherAlphabet.IndexOf(block[i]);
                    if (digit < 0)
                    {
                        valid = false;
                        break;
                    }
                    decodedVal = (decodedVal * BaseCount) + digit;
                }

                if (!valid)
                {
                    continue;
                }

                int unmasked = decodedVal ^ FixedMask;
                int originalCode = (unmasked - FixedShift) & 0xFFFF;
                sb.Append((char)originalCode);
            }

            return sb.ToString();
        }

        public static string ReverseText(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            char[] chars = value.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        private static string GenerateRandomPadding(int length)
        {
            var eng = new StringBuilder(length);
            byte[] bytes = new byte[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            for (int i = 0; i < length; i++)
            {
                eng.Append(CipherAlphabet[bytes[i] % CipherAlphabet.Length]);
            }

            return eng.ToString();
        }
    }
}