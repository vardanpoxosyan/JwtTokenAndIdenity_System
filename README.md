# ASP.NET Core JWT Authentication API

A secure authentication and authorization API built with **ASP.NET Core 10**, **Entity Framework Core**, and **ASP.NET Core Identity**.

The project implements JWT-based authentication with **Access Tokens**, **Refresh Tokens**, **Role-based Authorization**, password validation, account lockout, token revocation, and refresh token rotation.

## 🚀 Features

* User registration
* User login
* Password hashing with ASP.NET Core Identity
* JWT Access Token authentication
* Refresh Token authentication
* Refresh Token rotation
* Refresh Token hashing before database storage
* Refresh Token expiration
* Refresh Token revocation
* Logout
* Role-based authorization
* User role assignment
* Account lockout after failed login attempts
* ASP.NET Core Identity `UserManager`
* ASP.NET Core Identity `RoleManager`
* ASP.NET Core Identity `SignInManager`
* Entity Framework Core
* SQL Server
* Global exception handling
* OpenAPI / Scalar API documentation

## 🛠️ Technologies

* C#
* .NET 10
* ASP.NET Core Web API
* ASP.NET Core Identity
* Entity Framework Core
* SQL Server
* JWT Bearer Authentication
* Scalar
* OpenAPI

## 🔐 Authentication Flow

The application uses two types of tokens.

### Access Token

The Access Token is a short-lived JWT used to access protected API endpoints.

Example:

```text
Client
   ↓
Access Token
   ↓
Protected API
   ↓
JWT validation
   ↓
Request allowed
```

The Access Token has a short expiration time configured in `appsettings.json`.

### Refresh Token

The Refresh Token is a long-lived random token used to obtain a new Access Token after the Access Token expires.

```text
Access Token
    ↓
Expires
    ↓
Refresh Token
    ↓
New Access Token
    ↓
Continue using the API
```

The user does not need to log in again while the Refresh Token is valid.

## 🔄 Refresh Token Rotation

Every time a Refresh Token is used, it is revoked and a new Refresh Token is generated.

```text
Refresh Token A
       ↓
     /refresh
       ↓
Revoked Token A
       +
New Token B
```

This prevents the same Refresh Token from being reused after it has already been consumed.

## 🔒 Refresh Token Security

Refresh Tokens are generated using a cryptographically secure random number generator.

```csharp
RandomNumberGenerator.GetBytes(64)
```

The raw Refresh Token is returned to the client, but only its SHA-256 hash is stored in the database.

```text
Client
   ↓
Raw Refresh Token

Database
   ↓
SHA-256 Hash
```

Therefore, the actual Refresh Token value is not stored directly in the database.

## 👤 ASP.NET Core Identity

ASP.NET Core Identity is used for user and role management.

The application uses:

```text
UserManager
    ↓
User management

RoleManager
    ↓
Role management

SignInManager
    ↓
Password verification and sign-in operations
```

### User Registration

During registration:

1. The application checks whether the email already exists.
2. A new `User` is created.
3. `UserManager.CreateAsync()` validates and hashes the password.
4. The default `User` role is created if it does not exist.
5. The user is assigned the `User` role.

Example:

```csharp
var result = await userManager.CreateAsync(
    user,
    dto.Password);
```

ASP.NET Core Identity handles password hashing and password policy validation.

## 👑 Roles

The application supports role-based authorization.

Example roles:

```text
User
Admin
```

Roles are included in the JWT as claims.

Example:

```text
User
   ↓
Role = Admin
   ↓
JWT
   ↓
ClaimTypes.Role
```

This allows protected endpoints to use role-based authorization.

Example:

```csharp
[Authorize(Roles = "Admin")]
```

## 🔑 Login Flow

The login process works as follows:

```text
Email + Password
       ↓
Find User
       ↓
Check Password
       ↓
Check Lockout
       ↓
Get User Roles
       ↓
Generate JWT
       ↓
Generate Refresh Token
       ↓
Hash Refresh Token
       ↓
Save Hash to Database
       ↓
Return Access + Refresh Token
```

## 🔄 Refresh Flow

When the Access Token expires:

```text
Refresh Token
       ↓
Hash Refresh Token
       ↓
Find Token in Database
       ↓
Check Exists
       ↓
Check Revoked
       ↓
Check Expiration
       ↓
Generate New Access Token
       ↓
Generate New Refresh Token
       ↓
Revoke Old Refresh Token
       ↓
Save New Refresh Token
       ↓
Return New Tokens
```

## 🚪 Logout

Logout revokes the provided Refresh Token.

```text
Client
   ↓
Refresh Token
   ↓
Hash Token
   ↓
Find Token
   ↓
IsRevoked = true
```

After revocation, the Refresh Token cannot be used again.

## 📁 Project Structure

```text
ExceptionHandling/
│
├── Controllers/
│   └── AuthController.cs
│
├── Data/
│   └── ApplicationDbContext.cs
│
├── DTO/
│   ├── RegisterDto.cs
│   ├── LoginDto.cs
│   ├── LoginResponseDto.cs
│   └── RefreshTokenDto.cs
│
├── Exceptions/
│   ├── UnauthorizedException.cs
│   ├── ConflictException.cs
│   ├── BadRequestException.cs
│   └── ExceptionMiddleWare.cs
│
├── Models/
│   ├── User.cs
│   └── RefreshToken.cs
│
├── Service/
│   ├── IAuthService.cs
│   └── AuthService.cs
│
├── DependencyInjection/
│   └── DependencyInjection.cs
│
├── Program.cs
└── appsettings.json
```

## ⚙️ Configuration

Configure JWT settings in `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "YOUR_SECRET_KEY",
    "Issuer": "JwtAuthApi",
    "Audience": "JwtAuthClient",
    "ExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

For production environments, the JWT secret should not be committed to source control.

Use environment variables, User Secrets, Azure Key Vault, or another secure secret-management solution.

## 🗄️ Database

The project uses Entity Framework Core with SQL Server.

The database stores ASP.NET Core Identity tables and Refresh Tokens.

Conceptually:

```text
Users
Roles
UserRoles
UserClaims
...
RefreshTokens
```

A Refresh Token is associated with a specific user:

```text
User
 │
 └── RefreshTokens
        ├── Token
        ├── UserId
        ├── ExpiresAt
        └── IsRevoked
```

## 📡 Main Authentication Endpoints

### Register

```http
POST /api/Auth/register
```

Creates a new user.

### Login

```http
POST /api/Auth/login
```

Returns:

```json
{
  "accessToken": "...",
  "refreshToken": "..."
}
```

### Refresh Token

```http
POST /api/Auth/refresh
```

Generates a new Access Token and rotates the Refresh Token.

### Logout

```http
POST /api/Auth/logout
```

Revokes the current Refresh Token.

## 🧪 API Testing

The project uses **Scalar** for API documentation and testing.

After starting the application, open:

```text
https://localhost:<PORT>/scalar
```

From Scalar you can test:

```text
POST /api/Auth/register
POST /api/Auth/login
POST /api/Auth/refresh
POST /api/Auth/logout
```

For protected endpoints, provide the JWT Access Token using:

```text
Authorization: Bearer <access_token>
```

## 🛡️ Security Considerations

The project follows several security practices:

* Passwords are never stored as plain text.
* Passwords are hashed using ASP.NET Core Identity.
* Access Tokens have a short lifetime.
* Refresh Tokens have a longer lifetime.
* Refresh Tokens are generated using a cryptographically secure random generator.
* Refresh Token hashes are stored instead of raw tokens.
* Expired Refresh Tokens are rejected.
* Revoked Refresh Tokens are rejected.
* Refresh Token rotation is implemented.
* Failed login attempts can trigger account lockout.
* JWT roles are included as claims.
* JWT signature validation is enabled.

## 📚 What This Project Demonstrates

This project demonstrates practical knowledge of:

```text
ASP.NET Core Web API
        ↓
ASP.NET Core Identity
        ↓
UserManager
        ↓
RoleManager
        ↓
SignInManager
        ↓
JWT Authentication
        ↓
Role-based Authorization
        ↓
Access Tokens
        ↓
Refresh Tokens
        ↓
Token Rotation
        ↓
Token Revocation
        ↓
Entity Framework Core
        ↓
SQL Server
        ↓
Exception Handling
```

## ▶️ Getting Started

### 1. Clone the repository

```bash
git clone <your-repository-url>
```

### 2. Navigate to the project

```bash
cd ExceptionHandling
```

### 3. Configure the database

Update the connection string in `appsettings.json` or use User Secrets.

### 4. Apply migrations

```bash
dotnet ef database update
```

### 5. Run the application

```bash
dotnet run
```

### 6. Open Scalar

```text
https://localhost:<PORT>/scalar
```

## 🎯 Future Improvements

Possible improvements for the project:

* Email confirmation
* Forgot password / Reset password
* Two-factor authentication
* Email verification
* Multiple device/session management
* Refresh token cleanup
* Admin management endpoints
* Permission-based authorization
* Audit logging
* Rate limiting
* Secure cookie-based Refresh Tokens
* Integration and unit tests

## 👨‍💻 Author

**Vardan Poghosyan**

C# / .NET Developer

Focused on:

* C#
* .NET
* ASP.NET Core
* Entity Framework Core
* Web API
* Authentication & Authorization
* SQL Server
