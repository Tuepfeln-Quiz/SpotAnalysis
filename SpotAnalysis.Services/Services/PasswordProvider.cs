using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.Encoders;

namespace SpotAnalysis.Services.Services;

public static class PasswordProvider
{
    private static readonly int Argon2Type = Argon2Parameters.Argon2id;
    private static readonly int Argon2Version = Argon2Parameters.Version13;
    private const int ArgonMemory = 4096;
    private const int ArgonParallelism = 4;
    private const int ArgonIterations = 4;
    private const int ArgonOutputLength = 32;
    
    private static string ParameterString(byte[] hash, byte[] salt)
    {
        return $"$argon2id$v={Argon2Version}$m={ArgonMemory},t={ArgonIterations},p={ArgonParallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public sealed class Password
    {
        private readonly byte[] _hash;
        private readonly byte[] _salt;
        
        public Password(string password, Guid salt)
        {
            if (string.IsNullOrEmpty(password)) throw new ArgumentException("Password cannot be null or empty");
            if (password.Length < 8) throw new ArgumentException("Password must be at least 8 characters long");
            
            _salt = salt.ToByteArray();
            _hash = DigestPw(password, _salt);
        }

        private Password(byte[] hash, byte[] salt)
        {
            _hash = hash;
            _salt = salt;
        }
        
        public string Hash() => string.Join("", Hex.Encode(_hash));
        public string ParamString() => ParameterString(_hash, _salt);
        public static Password FromParamString(string paramString)
        {
            if (string.IsNullOrEmpty(paramString)) throw new ArgumentException("Parameter string cannot be null or empty");
            
            var parts = paramString.Split('$');
            if (parts.Length != 6) throw new ArgumentException("Invalid parameter string");
            var hash = Convert.FromBase64String(parts[5]);
            var salt = Convert.FromBase64String(parts[4]);
            return new Password(hash, salt);
        }

        public bool Compare(Password output)
        {
            return CryptographicOperations.FixedTimeEquals(_hash, new ReadOnlySpan<byte>(output._hash));
        }
    }
    
    private static Argon2BytesGenerator NewGenerator(byte[] salt)
    {
        var gen = new Argon2BytesGenerator();

        var argon2Parameters = new Argon2Parameters.Builder(Argon2Type)
            .WithVersion(Argon2Version)
            .WithSalt(salt)
            .WithIterations(ArgonIterations)
            .WithMemoryAsKB(ArgonMemory)
            .WithParallelism(ArgonParallelism)
            .Build();

        gen.Init(argon2Parameters);

        return gen;
    }
    
    private static byte[] DigestPw(string password, byte[] salt)
    {
        var gen = NewGenerator(salt);
        var result = new byte[ArgonOutputLength];
        gen.GenerateBytes(password.ToArray(), result);

        return result;
    }
}