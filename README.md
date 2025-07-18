# Simple Casino Game System

## Description
A lightweight, real-time casino game platform built with SignalR, enabling players to create and join tables, play various games loaded dynamically from a JSON file, and receive payouts. Designed for developers exploring real-time communication and microservices architecture.

## Tech Stack
- SignalR (Real-time communication)
- ASP.NET Core (Backend services)
- JSON (Configuration for games)
- JWT (Authentication, mocked users)
- React (Frontend)

## Requirements
- .NET 6.0 SDK or later for backend services
- Node.js and npm for frontend
- Basic familiarity with SignalR and WebSocket concepts

## Installation & Usage
### Backend
1. Clone the repository.
2. Restore dependencies: `dotnet restore`
3. Run services in order:
   - Auth Service (if implemented)
   - Wallet Service
   - Lobby Service
   - Game Services

### Frontend
1. Navigate to the front-end directory.
2. Install dependencies: `npm install`
3. Start development server: `npm start`

### Usage
- Connect with a mocked JWT-authenticated user.
- Join or create tables via the SignalR hub.
- Play available games with dynamic rules loaded from `games.json`.
- Winnings are paid out automatically after games conclude.

## Notes
- All real-time player interactions occur via SignalR.
- Only the Lobby Service can invoke the Wallet Service for payouts.
- Adjust configurations as needed for development environment.

When you are done, submit the project from your profile: [https://bitasmbl.com/home/profile](https://bitasmbl.com/home/profile)