using LibraryAPI.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers.v1
{
    [ApiController]
    [Route("api/v1/security")]
    public class SecurityController: ControllerBase
    {
        private readonly IDataProtector protector;
        private readonly ITimeLimitedDataProtector protectorToTimeLimit;
        private readonly IDataProtectionProvider protectionProvider;
        private readonly IHashService hashService;

        public SecurityController(IDataProtectionProvider protectionProvider, IHashService hashService)
        {
            protector = protectionProvider.CreateProtector("SecurityController");
            protectorToTimeLimit = protector.ToTimeLimitedDataProtector();
            this.protectionProvider = protectionProvider;
            this.hashService = hashService;
        }

        [HttpGet("hash")]
        public ActionResult Hash(string plainText)
        {
            var hash1 = hashService.Hash(plainText);
            var hash2 = hashService.Hash(plainText);
            var hash3 = hashService.Hash(plainText, hash2.Sal);
            var result = new { plainText, hash1, hash2, hash3 };
            return Ok(result);
        }

        [HttpGet("encrypt-to-time-limited")]
        public ActionResult EncryptToTimeLimited(string plainText)
        {
            string ciphertext = protectorToTimeLimit.Protect(plainText, 
                lifetime: TimeSpan.FromSeconds(30));
            return Ok(new { ciphertext });
        }

        [HttpGet("decrypt-to-time-limited")]
        public ActionResult DecryptToTimeLimited(string ciphertext)
        {
            string plainText = protectorToTimeLimit.Unprotect(ciphertext);
            return Ok(new { plainText });
        }

        [HttpGet("encrypt")]
        public ActionResult Encrypt(string plainText)
        {
            string ciphertext = protector.Protect(plainText);
            return Ok(new { ciphertext });
        }

        [HttpGet("decrypt")]
        public ActionResult Decrypt(string ciphertext)
        {
            string plainText = protector.Unprotect(ciphertext);
            return Ok(new { plainText });
        }
    }
}
