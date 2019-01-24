using System.IO;
using System.Security.Cryptography;
namespace RoslynCompiler
{
    public class AssemblyHash : HashAlgorithm
    {
        private MemoryStream _memoryStream = new MemoryStream();
        static AssemblyHash()
        {
            CryptoConfig.AddAlgorithm(typeof(AssemblyHash), typeof(AssemblyHash).FullName);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _memoryStream.Dispose();
            }
            base.Dispose(disposing);
        }
        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            _memoryStream.Write(array, ibStart, cbSize);
        }
        protected override byte[] HashFinal()
        {
            return _memoryStream.ToArray();
        }
        public override void Initialize()
        {
            _memoryStream.Dispose();
            _memoryStream = new MemoryStream();
        }
    }
}