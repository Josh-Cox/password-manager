# PasswordManager

PasswordManager is a secure cross-platform password manager built with .NET.

The solution is structured into multiple projects to separate UI, business logic, API access, and command-line tooling.

The application uses modern security practices including:
- AES-256-GCM authenticated encryption
- PBKDF2-SHA256 key derivation
- Azure Blob Storage for encrypted vault storage
- Microsoft Entra ID authentication
- Zero-knowledge style vault encryption

---

# Security Architecture

PasswordManager encrypts vault data locally before it is uploaded or stored remotely.

## Encryption

Vaults are encrypted using AES-256-GCM authenticated encryption.

### Security Features
- AES-GCM authenticated encryption
- Random nonce generation
- PBKDF2 key derivation with SHA-256
- Per-user cryptographic salt
- Vault integrity validation using authenticated tags and vault headers

The encryption key is derived from the user's master password and is never stored directly.

Encrypted vault structure:
```text
[nonce][tag][ciphertext]
```

Vaults are decrypted locally on-device using the user's master password.

---

# Cloud Storage

Encrypted vault files are stored using Azure Blob Storage.

The API uploads and retrieves encrypted binary vault files from a dedicated Azure Blob container.

### Storage Flow
1. Vault encrypted locally
2. Encrypted bytes uploaded to API
3. API stores encrypted `.dat` file in Azure Blob Storage
4. Vault downloaded and decrypted locally on-device

The server never stores decrypted vault contents.

---

# Authentication

User authentication is handled using Microsoft Entra ID.

This provides:
- Secure Microsoft account authentication
- Token-based authentication flows
- Identity management
- Secure API access

Authentication is separated from encryption:
- Entra ID authenticates the user
- Master password decrypts the vault

This ensures vault encryption remains independent from cloud authentication.

---

# Solution Structure

## PasswordManager.Core

Core business logic and application services.

### Responsibilities
- Vault encryption/decryption
- Master password validation
- Command/query handling
- Password entry management
- Authentication/session logic
- Shared models and interfaces
- Cryptography services

This project contains the main application logic and is referenced by other projects.

---

## PasswordManager.UI

.NET MAUI frontend application.

### Responsibilities
- Mobile/Desktop user interface
- Navigation and page flow
- User interactions
- Activity/loading states
- Session handling
- Displaying vault data

References `PasswordManager.Core`.

---

## PasswordManager.API

ASP.NET Core backend API.

### Responsibilities
- Azure Blob Storage integration
- Vault upload/download endpoints
- Authentication handling
- Secure encrypted vault transport
- Remote storage services

Vaults are stored as encrypted binary blobs.

---

## PasswordManager.CLI

Command-line interface for interacting with the password manager.

### Responsibilities
- Vault management from terminal
- Entry creation/editing
- Import/export tools
- Automation and scripting support
- Debugging/testing utilities

References `PasswordManager.Core`.

---

# Technologies

- .NET
- .NET MAUI
- ASP.NET Core
- C#
- Azure Blob Storage
- Microsoft Entra ID
- AES-256-GCM
- PBKDF2-SHA256
- Dependency Injection
- Command/Query architecture

---

# Goals

- Secure local-first password storage
- Modern authenticated encryption
- Cross-platform support
- Cloud synchronization
- Responsive mobile experience
- Clean layered architecture
- Maintainable and testable codebase
