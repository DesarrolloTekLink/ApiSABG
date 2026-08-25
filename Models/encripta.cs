using Microsoft.VisualBasic;
using System.Security.Cryptography;
using System.Text;

namespace ApiPractice.Models {
    public partial class encripta {

        public string llave = "6pqad&kAnTNXI8zfCXb)%fv4FyaWyglW";

        public string sup1(string s) {
            string sup = "";
            int i;
            var loopTo = s.Length;
            for (i = 1; i <= loopTo; i++)
                sup = sup + System.Web.HttpUtility.UrlEncode(Strings.Mid(s, i, 1));
            return sup;
        }


        public string EncryptString128Bit(string text, string key) {
            byte[] myByte = System.Text.Encoding.UTF8.GetBytes(text);
            text = Convert.ToBase64String(myByte);
            byte[] plaintextbytes = System.Text.ASCIIEncoding.ASCII.GetBytes(text);
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            byte[] iv = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.Key = System.Text.ASCIIEncoding.ASCII.GetBytes(key);
            aes.IV = iv;
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.CBC;
            ICryptoTransform crypto = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] encrypted = crypto.TransformFinalBlock(plaintextbytes, 0, plaintextbytes.Length);
            crypto.Dispose();
            return Convert.ToBase64String(encrypted);
        }


        public string DecryptString128Bit(string encrypted, string key) {
            byte[] encryptedbytes = Convert.FromBase64String(encrypted);
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            byte[] iv = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.Key = System.Text.ASCIIEncoding.ASCII.GetBytes(key);
            aes.IV = iv;
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.CBC;
            ICryptoTransform crypto = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] secret = crypto.TransformFinalBlock(encryptedbytes, 0, encryptedbytes.Length);
            crypto.Dispose();

            byte[] data0 = Convert.FromBase64String(System.Text.ASCIIEncoding.ASCII.GetString(secret));
            return Encoding.UTF8.GetString(data0);

        }


        public string StripNullCharacters(string vstrStringWithNulls) {

            int intPosition;
            string strStringWithOutNulls;

            intPosition = 1;
            strStringWithOutNulls = vstrStringWithNulls;

            while (intPosition > 0) {
                intPosition = Strings.InStr(intPosition, vstrStringWithNulls, Constants.vbNullChar);

                if (intPosition > 0) {
                    strStringWithOutNulls = Strings.Left(strStringWithOutNulls, intPosition - 1) + Strings.Right(strStringWithOutNulls, Strings.Len(strStringWithOutNulls) - intPosition);
                }

                if (intPosition > strStringWithOutNulls.Length) {
                    break;
                }
            }

            return strStringWithOutNulls;

        }



    }
}
