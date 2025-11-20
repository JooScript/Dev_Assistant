# Dev Assistant

Dev Assistant is a C# utility designed to validate database rules, generate boilerplate code, and automate essential maintenance operations. It supports validating GUID-based IDs, generating SQL Server backups, and producing C# class models from database tables.  
The project uses the **CS_Utilities** library for logging, guard clauses, helpers, and clean code principles.  
Library: https://github.com/JooScript/CS_Utilities

---

## Overview

Dev Assistant provides three main capabilities:

### 1. Database Validation  
Ensures that key fields (such as `Id`) follow expected formats (e.g., GUID validation).

### 2. Database Backup  
Generates timestamped `.bak` files using SQL Server’s built-in backup mechanism.

### 3. Code Generation  
Automatically generates strongly typed C# classes based on database tables.  
This is helpful when modeling domain entities, DTOs, or repository-layer objects.
