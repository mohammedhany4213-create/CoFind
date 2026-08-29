# coFind

> Find the co-founder your startup actually needs.

[![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-TypeScript-61DAFB?logo=react&logoColor=white)](https://react.dev/)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-blue)](#architecture)
[![License](https://img.shields.io/badge/License-Proprietary-red)](#license)

---

## Overview

Most co-founder matching happens informally — scattered across Facebook groups, Twitter threads, and word of mouth. **coFind** replaces that with a focused, public marketplace: founders post what they're building and who they need, and interested co-founders reach out directly. No noise, no algorithm, no gatekeeping.

A founder creates an account and publishes an **offer** — a description of their startup, the role or skillset they're looking for, and what's expected from a co-founder. Offers are visible to everyone on the home page, account or not, with a one-tap WhatsApp button for instant contact.

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Roadmap](#roadmap)
- [License](#license)

## Features

- **Public offer feed** — Every offer is visible on the home page with no sign-in required, maximizing reach for founders.
- **Structured founder profiles** — Founders describe their project, the skills and role they need, and availability expectations in one place.
- **Direct contact, no friction** — Every offer includes a WhatsApp button, skipping in-app messaging entirely in favor of the channel people actually use.
- **Account-gated posting** — Anyone can browse, but posting an offer requires an account, keeping the feed accountable.

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core (.NET 10) |
| Frontend | React + TypeScript + Vite |
| Database | SQL Server + Entity Framework Core |
| Architecture | Clean Architecture (Domain / Application / Infrastructure / Api) |

## Architecture

coFind's backend follows **Clean Architecture**, keeping business rules independent of frameworks, databases, and UI:

```
┌─────────────────────────────────────────┐
│                  Api                     │  ← Controllers, HTTP, DI wiring
├─────────────────────────────────────────┤
│              Infrastructure              │  ← EF Core, DbContext, repositories
├─────────────────────────────────────────┤
│               Application                │  ← Use cases, DTOs, service interfaces
├─────────────────────────────────────────┤
│                 Domain                   │  ← Entities, core business rules
└─────────────────────────────────────────┘
```

Dependencies flow strictly inward — `Domain` has no dependency on anything else, and every outer layer depends only on the layers beneath it. This keeps the core business logic (what an "offer" is, what makes a profile valid) fully decoupled from *how* it's persisted or exposed.

## Project Structure

```
coFind/
│
├── backend/
│   ├── src/
│   │   ├── coFind.Api/              # Controllers, Program.cs, DI setup
│   │   ├── coFind.Application/      # Use cases, DTOs, service interfaces
│   │   ├── coFind.Domain/           # Entities, core business rules
│   │   └── coFind.Infrastructure/   # EF Core, repositories, external services
│   │
│   └── tests/
│       ├── coFind.UnitTests/
│       └── coFind.IntegrationTests/
│
├── frontend/
│   └── coFind.Client/               # React + TypeScript + Vite
│
├── docs/
│   ├── database-schema.drawio
│   ├── database-schema.png
│   └── architecture.md
│
└── README.md
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (LTS)
- SQL Server (LocalDB or full instance)

### Backend

```bash
cd backend
dotnet restore
dotnet build
dotnet run --project src/coFind.Api
```

### Frontend

```bash
cd frontend/coFind.Client
npm install
npm run dev
```

## Roadmap

- [ ] Core domain: `User` and `Offer` entities
- [ ] Offer creation and public listing endpoints
- [ ] Authentication for posting offers
- [ ] Public home page with offer feed
- [ ] Offer detail view with WhatsApp contact
- [ ] Search and filtering by required skills

## License

Copyright (c) 2026 Mohammed Hany. All Rights Reserved.

This project is proprietary and confidential. No part of this codebase may be copied, modified, distributed, or used without explicit written permission from the owner.