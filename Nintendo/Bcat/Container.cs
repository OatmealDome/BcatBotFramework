using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Syroot.BinaryData;
using System;
using System.IO;
using System.Text;

namespace Nintendo.Bcat
{
    public class Container
    {
        // Taken from 5.0.1 bcat sysmodule
        private static string[] SecretData =
        {
            "a3e20c5c1cd7b720",
            "7f4c637432c8d420",
            "188d087d92a0c087",
            "8e7d23fa7fafe60f",
            "5252ae57c026d3cb",
            "2650f5e53554f01d",
            "b213a1e986307c9f",
            "875d8b01e3df5d7c",
            "c1b9a5ce866e00b1",
            "6a48ae69161e0138",
            "3f7b0401928b1f46",
            "0e9db55903a10f0e",
            "a8914bcbe7b888f9",
            "b15ef3ed6ce0e4cc",
            "f3b9d9f43dedf569",
            "bda4f7a0508c7462",
            "f5dc3586b1b2a8af",
            "7f6828b6f33dd118",
            "860de88547dcbf70",
            "ccbacacb70d11fb5",
            "b1475e5ea18151b9",
            "5f857ca15cf3374c",
            "cfa747c1d09d4f05",
            "30e7d70cb6f98101",
            "c8b3c78772bdcf43",
            "533dfc0702ed9874",
            "a29301cac5219e5c",
            "5776f5bec1b0df06",
            "1d4ab85a07ac4251",
            "7c1bd512b1cf5092",
            "2691cb8b3f76b411",
            "4400abee651c9eb9"
        };

        public byte Unknown1
        {
            get;
            set;
        }

        public EncryptionType EncryptionType
        {
            get;
            set;
        }

        public byte RsaHashType
        {
            get;
            set;
        }

        public byte SecretDataIdx
        {
            get;
            set;
        }

        public ulong Unknown2
        {
            get;
            set;
        }

        public byte[] EncryptionIv
        {
            get;
            set;
        }

        public byte[] Signature
        {
            get;
            set;
        }

        public byte[] EncryptedData
        {
            get;
            set;
        }

        public Container(byte[] data)
        {
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                ReadContainer(memoryStream);
            }
        }

        public Container(Stream stream)
        {
            ReadContainer(stream);
        }

        private void ReadContainer(Stream stream)
        {
            using (BinaryDataReader reader = new BinaryDataReader(stream, Encoding.ASCII))
            {
                // Read as big endian
                reader.ByteOrder = ByteOrder.BigEndian;

                // Check magic numbers
                if (reader.ReadUInt32() != 0x62636174) // "bcat"
                {
                    Console.WriteLine("Not a BCAT container file");

                    return;
                }

                // Read fields
                this.Unknown1 = reader.ReadByte();

                byte cryptoType = reader.ReadByte();
                if (cryptoType > 3)
                {
                    throw new Exception("Crypto type not supported");
                }
                else
                {
                    this.EncryptionType = (EncryptionType)cryptoType;
                }

                this.RsaHashType = reader.ReadByte();

                this.SecretDataIdx = reader.ReadByte();
                if (SecretData.Length < this.SecretDataIdx)
                {
                    throw new Exception("Unsupported secret data");
                }

                this.Unknown2 = reader.ReadUInt64();
                this.EncryptionIv = reader.ReadBytes(0x10);
                this.Signature = reader.ReadBytes(0x100);
                this.EncryptedData = reader.ReadBytes((int)(reader.Length - 0x120));
            }
        }

        public byte[] GetDecryptedData(string titleId, string passphrase)
        {
            // Create the salt from the title ID and secret data
            string salt = titleId.ToLower() + SecretData[this.SecretDataIdx];

            // Get the key size
            int keySize;
            switch (this.EncryptionType)
            {
                case EncryptionType.Aes128:
                    keySize = 128;
                    break;
                case EncryptionType.Aes192:
                    keySize = 192;
                    break;
                case EncryptionType.Aes256:
                    keySize = 256;
                    break;
                default:
                    throw new Exception("Invalid EncryptionType");
            }

            // Create a new Pkcs5S2ParametersGenerator and initialize it
            Pkcs5S2ParametersGenerator generator = new Pkcs5S2ParametersGenerator(new Sha256Digest());
            generator.Init(Encoding.ASCII.GetBytes(passphrase.ToLower()), Encoding.ASCII.GetBytes(salt), 4096);

            // Generate the key parameter
            KeyParameter parameter = (KeyParameter)generator.GenerateDerivedParameters("aes" + keySize, keySize);

            // Initialize a cipher in AES-CTR mode
            IBufferedCipher cipher = CipherUtilities.GetCipher("AES/CTR/NoPadding");
            cipher.Init(false, new ParametersWithIV(parameter, this.EncryptionIv));

            // Get the decrypted bytes
            return cipher.DoFinal(this.EncryptedData);
        }

    }
}
