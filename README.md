# Fireclicks Server

.NET 8.0 Web API server for request counting functionality.

## Prerequisites

- **.NET 8.0 SDK** or later
- **Visual Studio** or **Rider** (optional, for development)

## Quick Start

### 1. Navigate to Server Directory

```bash
cd Server
```

### 2. Start the Server

```bash
dotnet run
```

The server will start on `http://localhost:5000`

## Server Details

### Technology Stack
- **.NET 8.0** Web API
- **ASP.NET Core** Minimal API

### Configuration
- **Port**: `5000`
- **Base URL**: `http://localhost:5000`
- Configuration file: `Server/appsettings.json`

### Building the Server

```bash
cd Server
dotnet build
```

### Running in Production Mode

```bash
cd Server
dotnet run --configuration Release
```

## Server Endpoints

- `GET /` - API information
- `POST /api/request-count` - Increment request counter

## API Documentation

### POST /api/request-count

Increments the request counter for a given token.

**Request:**
```json
{
  "token": "encrypted_token_string"
}
```
## Development

### Server Development
1. Navigate to `Server/` directory
2. Make changes to server code
3. Run `dotnet run` to test
4. Server will auto-reload on code changes (in Development mode)

## Important Files

- `Program.cs` - Server entry point and API routes
- `RequestCounterService.cs` - Request counting logic
- `TokenCryptoService.cs` - Token encryption/decryption

