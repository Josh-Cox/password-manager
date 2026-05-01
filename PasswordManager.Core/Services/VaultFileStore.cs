//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace PasswordManager.Core.Services
//{
//    public class VaultFileStore : IVaultStore
//    {
//        private const string Filename = "vault.dat";

//        public bool Exists()
//        {
//            return File.Exists(Filename);
//        }

//        public byte[] ReadAll()
//        {
//            return File.ReadAllBytes(Filename);
//        }

//        public void WriteAll(byte[] data)
//        {
//            File.WriteAllBytes(Filename, data);
//        }
//    }
//}
