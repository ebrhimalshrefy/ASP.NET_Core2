using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Week5Api.DTOs;
using Week5Api.Models;
using Week5Api.Services;

namespace Week5Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Validation failed.",
                    Errors = validationErrors
                });
            }

            // Check if email is already taken
            var existingByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingByEmail != null)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "An account with this email address already exists."
                });
            }

            // Check if username is already taken
            var existingByUsername = await _userManager.FindByNameAsync(model.Username);
            if (existingByUsername != null)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Username is already in use. Please choose another username."
                });
            }

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FullName = model.Username,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Failed to create user account.",
                    Errors = result.Errors.Select(e => e.Description)
                });
            }

            _logger.LogInformation("New user registered successfully: {Email}", user.Email);

            return StatusCode(StatusCodes.Status201Created, new AuthResponseDto
            {
                Success = true,
                Message = "User registered successfully! You can now log in.",
                UserId = user.Id,
                Username = user.UserName,
                Email = user.Email
            });
        }

        /// <summary>
        /// Authenticates user and returns JWT Bearer token.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Validation failed.",
                    Errors = validationErrors
                });
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password."
                });
            }

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);
            if (!signInResult.Succeeded)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password."
                });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.CreateToken(user, roles);
            var expiration = _tokenService.GetTokenExpiration();

            _logger.LogInformation("User logged in successfully: {Email}", user.Email);

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Authentication successful.",
                Token = token,
                UserId = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Expiration = expiration
            });
        }
    }
}
