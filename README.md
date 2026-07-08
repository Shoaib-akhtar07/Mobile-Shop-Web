# MobileShop

Your trusted destination for the latest smartphones, accessories, and mobile gadgets. Built with ASP.NET Core MVC, this e-commerce platform offers quality products from top brands at competitive prices.

## Features

- Product catalog with search, filtering, and categories
- Shopping cart and checkout with Stripe payment integration
- User authentication (Email/Password, Facebook, Google OAuth)
- Admin dashboard for product, order, and user management
- Email notifications for orders
- Role-based authorization (Admin, Customer, Manager)

## Tech Stack

- **Framework:** ASP.NET Core 10.0 MVC
- **Database:** SQL Server (Entity Framework Core)
- **Authentication:** ASP.NET Core Identity + OAuth (Facebook, Google)
- **Payments:** Stripe
- **Email:** SMTP
- **Mapping:** AutoMapper

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (LocalDB or full instance)
- Stripe account (for payment processing)
- SMTP email service (for notifications)

## Setup Instructions

### 1. Clone the repository

```bash
git clone https://github.com/Shoaib-akhtar07/MobileShops.git
cd MobileShops
```

### 2. Configure secrets

Copy `appsettings.json` and fill in your credentials. **NEVER commit real credentials to source control.**

### 3. Configure via User Secrets (Recommended)

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=MobileShopDb;..."
dotnet user-secrets set "AdminSettings:Email" "your-admin@email.com"
dotnet user-secrets set "AdminSettings:Password" "YourSecurePassword!"
dotnet user-secrets set "Stripe:PublishableKey" "pk_test_..."
dotnet user-secrets set "Stripe:SecretKey" "sk_test_..."
dotnet user-secrets set "EmailSettings:SmtpServer" "smtp.example.com"
dotnet user-secrets set "EmailSettings:SenderEmail" "noreply@example.com"
dotnet user-secrets set "EmailSettings:SenderPassword" "your-smtp-password"
```

### 4. Run the application

```bash
dotnet run
```

The app will be available at `https://localhost:7154` or `http://localhost:5026`.

## Required Secrets & Credentials

The following secrets must be configured before running the application. They are **NOT** included in the repository for security reasons.

| Secret | Description | How to Obtain |
|--------|-------------|---------------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | SQL Server configuration |
| `AdminSettings:Email` | Admin account email | Choose any valid email |
| `AdminSettings:Password` | Admin account password | Choose a strong password |
| `Stripe:PublishableKey` | Stripe public API key | [Stripe Dashboard](https://dashboard.stripe.com/apikeys) |
| `Stripe:SecretKey` | Stripe secret API key | [Stripe Dashboard](https://dashboard.stripe.com/apikeys) |
| `EmailSettings:SmtpServer` | SMTP server hostname | Your email provider |
| `EmailSettings:SmtpPort` | SMTP server port (default: 587) | Your email provider |
| `EmailSettings:SenderEmail` | Sender email address | Your email provider |
| `EmailSettings:SenderPassword` | SMTP password / app password | Your email provider |
| `Facebook:AppId` | Facebook OAuth App ID | [Facebook Developers](https://developers.facebook.com) |
| `Facebook:AppSecret` | Facebook OAuth App Secret | [Facebook Developers](https://developers.facebook.com) |
| `Google:ClientId` | Google OAuth Client ID | [Google Cloud Console](https://console.cloud.google.com) |
| `Google:ClientSecret` | Google OAuth Client Secret | [Google Cloud Console](https://console.cloud.google.com) |

## Security Notes

> **WARNING:** This repository is public. Never commit real passwords, API keys, connection strings, or any other secrets to source control.

- Use **.NET User Secrets** for local development
- Use **environment variables** or a **secrets manager** for production
- The `.gitignore` is configured to exclude sensitive files, but always verify before pushing

## Default Admin Account

The admin account is seeded from configuration (`AdminSettings:Email` and `AdminSettings:Password`). If not configured, default values are used. **Change the default password immediately in production.**

## License

This project is for educational purposes.
