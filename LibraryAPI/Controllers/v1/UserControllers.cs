using AutoMapper;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Abstractions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryAPI.Controllers.v1
{
    [ApiController]
    [Route("api/v1/users")]
    //[Authorize] // we gonna use specific authorization
    public class UserControllers: ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<User> signInManager;
        private readonly IServicesUser servicesUser;
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public UserControllers(UserManager<User> userManager, IConfiguration configuration,
            SignInManager<User> signInManager, IServicesUser servicesUser, 
            ApplicationDbContext context, IMapper mapper)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.servicesUser = servicesUser;
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet(Name = "GetUsersV1")]
        [Authorize(Policy = "esadmin")]
        public async Task<IEnumerable<UserDTO>> Get()
        {
            var users = await context.Users.ToListAsync();
            var userDTO = mapper.Map<IEnumerable<UserDTO>>(users);
            return userDTO;
        }

        [HttpPost("register", Name = "CreateUserV1")]
        public async Task<ActionResult<AuthenticationResponseDTO>> Register(
            UserCredentialsDTO userCredentialsDTO)
        {
            var user = new User
            {
                UserName = userCredentialsDTO.Email,
                Email = userCredentialsDTO.Email
            };

            var result = await userManager.CreateAsync(user, userCredentialsDTO.Password!);

            if(result.Succeeded)
            {
                var authenticationResponse = await BuildToken(userCredentialsDTO);
                return authenticationResponse;
            }
            else
            {
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return ValidationProblem();
            }
        }

        [HttpPost("login", Name = "LoginUserV1")]
        public async Task<ActionResult<AuthenticationResponseDTO>> Login(
            UserCredentialsDTO userCredentialsDTO)
        {
            var user = await userManager.FindByEmailAsync(userCredentialsDTO.Email);

            if (user is null)
            {
                return IncorrectLogin();
            }

            var result = await signInManager.CheckPasswordSignInAsync(user,
                userCredentialsDTO.Password!, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return await BuildToken(userCredentialsDTO);
            }
            else
            {
                return IncorrectLogin();
            }
        }

        [HttpPut(Name = "UpdateUserV1")]
        [Authorize]
        public async Task<ActionResult> Put(UserUpdatedDTO userUpdateDTO)
        {
            var user = await servicesUser.GetUser();

            if (user is null)
            {
                return NotFound();
            }

            user.DateOfBirth = userUpdateDTO.DateOfBirth;

            await userManager.UpdateAsync(user);
            return NoContent();
        }

        [HttpGet("renew-token", Name = "RenewTokenV1")]
        [Authorize]
        public async Task<ActionResult<AuthenticationResponseDTO>> RenewToken()
        {
            var user = await servicesUser.GetUser();

            if (user is null)
            {
                return NotFound();
            }

            var userCredentialsDTO = new UserCredentialsDTO { Email = user.Email! };
            var authenticationResponse =  await BuildToken(userCredentialsDTO);

            return authenticationResponse;
        }

        [HttpPost("add-admin", Name = "AddAdminV1")]
        [Authorize(Policy = "esadmin")]
        public async Task<ActionResult> AddAdmin(EditClaimDTO editClaimDTO)
        {
            var user = await userManager.FindByEmailAsync(editClaimDTO.Email);

            if (user is null)
            {
                return NotFound();
            }

            await userManager.AddClaimAsync(user, new Claim("esadmin", "true"));
            return NoContent();
        }

        [HttpPost("remove-admin", Name = "RemoveAdminV1")]
        [Authorize(Policy = "esadmin")]
        public async Task<ActionResult> RemoveAdmin(EditClaimDTO editClaimDTO)
        {
            var user = await userManager.FindByEmailAsync(editClaimDTO.Email);

            if (user is null)
            {
                return NotFound();
            }

            await userManager.RemoveClaimAsync(user, new Claim("esadmin", "true"));
            return NoContent();
        }

        private ActionResult IncorrectLogin()
        {
            ModelState.AddModelError(string.Empty, "Incorrect login");
            return ValidationProblem();
        }

        private async Task<AuthenticationResponseDTO> BuildToken(
            UserCredentialsDTO userCredentialsDTO)
        {
            var claims = new List<Claim>
            {
                new Claim("email", userCredentialsDTO.Email),
                new Claim("Whaterever i want", "Any value")
            };

            var user = await userManager.FindByEmailAsync(userCredentialsDTO.Email);
            var claimsDB = await userManager.GetClaimsAsync(user!);

            claims.AddRange(claimsDB);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["keyjwt"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddYears(1);

            var securityToken = new JwtSecurityToken(issuer: null, audience: null,
                claims: claims, expires: expiration, signingCredentials: credentials);

            var token = new JwtSecurityTokenHandler().WriteToken(securityToken);

            return new AuthenticationResponseDTO
            {
                Token = token,
                Expiration = expiration
            };
        }
    }
}
