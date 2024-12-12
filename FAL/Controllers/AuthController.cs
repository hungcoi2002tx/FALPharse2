using FAL.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using System.ComponentModel.DataAnnotations;
using FAL.Utils;
using Share.Model;
using System;
using System.Linq.Expressions;
using Amazon.DynamoDBv2.DocumentModel;
using FAL.Services.IServices;

namespace FAL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IDynamoDBContext _dbContext;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly ICollectionService _collectionService;

        public AuthController(IConfiguration configuration, IDynamoDBContext dbContext, ICollectionService collectionService)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _jwtTokenGenerator = new JwtTokenGenerator(configuration);
            _collectionService = collectionService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            // Validate login information
            if (!IsValidLoginModel(loginModel))
            {
                return BadRequest(new { status = false, message = "Invalid login information." });
            }

            // Retrieve user information from the database
            var user = await _dbContext.LoadAsync<Account>(loginModel.Username);
            if (user == null)
            {
                return Unauthorized(new { status = false, message = "Username does not exist." });
            }
            if (!IsActive(user))
            {
                return Unauthorized(new { status = false, message = $"Your account has not been approved. Status: {user.Status}." });
            }

            // Check the password
            if (!IsPasswordValid(loginModel.Password, user.Password))
            {
                return Unauthorized(new { status = false, message = "Incorrect password." });
            }

            // Generate JWT token
            var (tokenString, expiresIn) = GenerateToken(user);

            // Return user information and token
            return Ok(new
            {
                status = true,
                token = tokenString,
                userRole = user.RoleId,
                systemName = user.SystemName,
                expiresIn
            });
        }

        private static bool IsActive(Account account)
        {
            return string.Equals(account.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsValidLoginModel(LoginModel loginModel)
        {
            return loginModel != null &&
                   !string.IsNullOrWhiteSpace(loginModel.Username) &&
                   !string.IsNullOrWhiteSpace(loginModel.Password);
        }

        // Password validation method
        private bool IsPasswordValid(string inputPassword, string storedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(inputPassword, storedPassword);
        }

        // Token generation method
        private (string Token, int ExpiresIn) GenerateToken(Account user)
        {
            var jwtToken = _jwtTokenGenerator.GenerateJwtToken(user.Username, user.RoleId.ToString(), user.SystemName);
            var tokenString = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            var expiresIn = (int)(jwtToken.ValidTo - DateTime.UtcNow).TotalSeconds;
            return (tokenString, expiresIn);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AccountRegisterDTO userDto)
        {
            if (userDto == null)
            {
                return BadRequest("Invalid registration information.");
            }

            // Validate the input fields
            if (!ValidateRegisterDto(userDto, out string validationMessage))
            {
                return BadRequest(validationMessage);
            }

            // Check if the user already exists by username
            var existingUser = await _dbContext.LoadAsync<Account>(userDto.Username);
            if (existingUser != null)
            {
                return Conflict("Username already exists.");
            }

            // Check if the SystemName already exists (using ScanAsync)
            var scanResult = await _dbContext.ScanAsync<Account>(
                           [
                               new("SystemName", ScanOperator.Equal, userDto.SystemName)
                           ]).GetRemainingAsync();

            if (scanResult.Count != 0)
            {
                return Conflict("SystemName already exists.");
            }

            // Check if collection already exists with the same SystemName
            var collectionExists = await _collectionService.IsCollectionExistAsync(userDto.SystemName);
            if (collectionExists)
            {
                return Conflict("Collection already exists for this SystemName.");
            }

            // Create a new collection for the user
            bool collectionCreated = await _collectionService.CreateCollectionAsync(userDto.SystemName);
            if (!collectionCreated)
            {
                return StatusCode(500, "Failed to create collection.");
            }

            try
            {
                // Hash the password
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

                // Create a new User object
                var user = new Account
                {
                    Username = userDto.Username,
                    Password = hashedPassword,
                    Email = userDto.Email,
                    RoleId = 2, // Default RoleId is 2 for staff
                    SystemName = userDto.SystemName,
                    WebhookUrl = userDto.WebhookUrl,
                    WebhookSecretKey = userDto.WebhookSecretKey,
                    Status = "Deactive" // Default status
                };

                // Save the user to DynamoDB
                await _dbContext.SaveAsync(user);

                return Ok("Registration and collection creation successful!");
            }
            catch (Exception ex)
            {
                // Rollback the collection creation in case of user creation failure
                bool collectionDeleted = await _collectionService.DeleteCollectionAsync(userDto.SystemName);
                if (!collectionDeleted)
                {
                    return StatusCode(500, "Collection creation failed, and we could not rollback.");
                }

                return StatusCode(500, $"User registration failed: {ex.Message}");
            }
        }



        // Validation method for UserRegisterDTO
        private bool ValidateRegisterDto(AccountRegisterDTO userDto, out string errorMessage)
        {
            if (string.IsNullOrEmpty(userDto.Username) || userDto.Username.Length < 3)
            {
                errorMessage = "Username must have at least 3 characters.";
                return false;
            }

            if (string.IsNullOrEmpty(userDto.Password) || userDto.Password.Length < 6)
            {
                errorMessage = "Password must have at least 6 characters.";
                return false;
            }

            if (!new EmailAddressAttribute().IsValid(userDto.Email))
            {
                errorMessage = "Invalid email address.";
                return false;
            }

            if (string.IsNullOrEmpty(userDto.SystemName))
            {
                errorMessage = "System Name cannot be empty.";
                return false;
            }

            //if (string.IsNullOrEmpty(userDto.WebhookUrl))
            //{
            //    errorMessage = "Webhook URL cannot be empty.";
            //    return false;
            //}

            //if (string.IsNullOrEmpty(userDto.WebhookSecretKey))
            //{
            //    errorMessage = "Webhook Secret Key cannot be empty.";
            //    return false;
            //}

            errorMessage = string.Empty;
            return true;
        }
    }
}
