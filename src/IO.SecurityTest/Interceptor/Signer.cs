using RestSharp;
using System;
using System.IO;
using System.Text;
using System.Xml;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.X509;
using IO.SecurityTest.Client;

namespace IO.SecurityTest.Interceptor
{
    public class Signer
    {
        public static string GetPayloadSignature(string payload)
        {            
            string keypairPath = Signer.getConfigByName("keypairPath");
            string keypairPassword = Signer.getConfigByName("keypairPassword");
            FileStream fileStream = null;
            Pkcs12Store keypairStore = null;

            try
            {
                fileStream = new FileStream(keypairPath, FileMode.Open, FileAccess.Read);
                keypairStore = new Pkcs12Store(fileStream, keypairPassword.ToCharArray());
            }
            catch (FileNotFoundException e)
            {
                throw new ApiException(400, "\n\nContenedor no encontrado, verifique la ruta.\n\n" + e.Message);

            }
            catch (Exception e)
            {
                throw new ApiException(400, "\n\nPassword del contenedor erróneo.\n\n" + e.Message);
            }
            finally
            {
                if (fileStream != null) fileStream.Close();
            }
            string alias = null;
            foreach (string al in keypairStore.Aliases)
                if (keypairStore.IsKeyEntry(al) && keypairStore.GetKey(al).Key.IsPrivate)
                    alias = al;
            AsymmetricKeyEntry pKey = keypairStore.GetKey(alias);
            ECPrivateKeyParameters privateKey = (ECPrivateKeyParameters)pKey.Key;
            byte[] tmpPayload = Encoding.UTF8.GetBytes(payload);
            ISigner sign = SignerUtilities.GetSigner("SHA-256withECDSA");
            sign.Init(true, privateKey);
            sign.BlockUpdate(tmpPayload, 0, tmpPayload.Length);
            byte[] signature = sign.GenerateSignature();
            byte[] asciiBytes = Hex.Encode(signature);
            char[] asciiChars = new char[Encoding.UTF8.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
            Encoding.UTF8.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);
            string xSignature = new string(asciiChars);

            return xSignature;
        }

        public static bool GetResponseVerification(IRestResponse response)
        {
            string certificatePath = Signer.getConfigByName("certificatePath");
            var responseSignature = response.Headers;
            var responseContent = response.Content;
            string xSignature = null;
            foreach (Parameter value in responseSignature)
                if (value.Name.Equals("x-signature"))
                    xSignature = value.Value.ToString();
            X509CertificateParser certParser = new X509CertificateParser();
            FileStream fileStream = null;
            X509Certificate cert = null;

            try
            {
                fileStream = new FileStream(certificatePath, FileMode.Open, FileAccess.Read);
                cert = certParser.ReadCertificate(fileStream);
            }
            catch (FileNotFoundException e)
            {
                throw new ApiException(400, "\n\nCertificado no encontrado, verifique la ruta.\n\n" + e.Message);
            }
            finally
            {
                if (fileStream != null)  fileStream.Close();
            }

            ECPublicKeyParameters key = (ECPublicKeyParameters)cert.GetPublicKey();
            ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");
            signer.Init(false, key);
            byte[] responseContentBytes = Encoding.UTF8.GetBytes(responseContent);
            signer.BlockUpdate(responseContentBytes, 0, responseContentBytes.Length);
            char[] asciiChars = xSignature.ToCharArray();
            byte[] ascciiBytes = Encoding.UTF8.GetBytes(asciiChars);
            byte[] signature = Hex.Decode(ascciiBytes);
            bool isVerified = signer.VerifySignature(signature);

            return isVerified;
        }

        private static string getConfigByName(string Name){
            string find = null;
            XmlDocument doc = new XmlDocument();
            doc.Load("../../../IO.SecurityTest/Interceptor/Config.xml");
            XmlNodeList nodeList = doc.DocumentElement.SelectNodes("/configuration");
            foreach (XmlNode node in nodeList){
                find = node.SelectSingleNode(Name).InnerText;
            }
            return find;
        }
    }
}