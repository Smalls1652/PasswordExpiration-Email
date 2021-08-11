# Password Expiration Email Notifier Service

This project is for automating the delivery of password expiration reminder emails to users.

## ⚠ Warning ⚠

This branch does not include any connectivity to on-premise resources and is designed purely with Azure AD/Microsoft 365 in mind. I may add on-premise AD integration later on, but the scope of this branch is to migrate all of the code over to C# and utilize cloud-only API endpoints. While this project started off as being only PowerShell based and, eventually, became a hybrid of C#/PowerShell, I've seen a lot of performance benefits going down this route of being C# only.

## 🔨 Planned changes

- C# only code-base.
- An Azure Functions service.
- Add a binary PowerShell module for PowerShell automation if Azure Functions isn't what someone wants to use.
- Potentially add on-premise AD back.