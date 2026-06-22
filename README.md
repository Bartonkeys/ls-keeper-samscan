# ls-keeper-samscan

Batch scan microservice that enriches SAM CPH holdings data via the APHA Integration Bridge, producing a CSV export to S3.

* [Overview](#overview)
* [How It Works](#how-it-works)
* [API](#api)
* [Configuration](#configuration)
* [Docker Compose](#docker-compose)
* [Testing](#testing)
* [Running](#running)

## Overview

`ls-keeper-samscan` is a spike/PoC service that:

1. Fetches all SAM CPH holdings from the **LS Keeper Data Bridge** (paginated).
2. For each holding, calls the **APHA Integration Bridge** to retrieve location and customer details.
3. Flattens the combined data into a CSV file.
4. Uploads the CSV to S3 and logs a pre-signed download URL (TTL = 7 days).

## How It Works

- `POST /api/scan` triggers a batch job asynchronously (returns `202 Accepted`).
- Only one scan can run at a time (in-process `SemaphoreSlim`). A second request returns `409 Conflict`.
- APHA API response times are logged per-request and included in the CSV.
- Individual holding enrichment failures are logged as warnings and recorded in the CSV — they do not abort the scan.

## API

| Endpoint | Method | Description |
|---|---|---|
| `/api/scan` | POST | Trigger a new scan (async) |
| `/api/scan/status` | GET | Get current scan status |
| `/health` | GET | CDP health check |

## Configuration

Configuration is loaded from `appsettings.json` / environment variables:

| Section | Key | Description |
|---|---|---|
| `Apha:BaseUrl` | Base URL for APHA Integration Bridge | |
| `Apha:TokenUrl` | OAuth2 token endpoint (Cognito) | |
| `Apha:ClientId` | OAuth2 client ID | Via CDP secrets |
| `Apha:ClientSecret` | OAuth2 client secret | Via CDP secrets |
| `DataBridge:BaseUrl` | Base URL for LS Keeper Data Bridge | |
| `DataBridge:PageSize` | Number of holdings per page (default: 100) | |
| `S3:BucketName` | S3 bucket for CSV output | |
| `S3:Region` | AWS region (default: eu-west-2) | |
| `S3:Prefix` | S3 key prefix (default: samscan) | |
| `S3:PreSignedUrlTtlDays` | Pre-signed URL expiry (default: 7) | |

### Docker Compose

A Docker Compose template is in [compose.yml](compose.yml).

Local environment includes:

- Floci (LocalStack replacement) for AWS services (S3)
- Redis
- MongoDB
- This service

```bash
docker compose up --build -d
```

### Testing

```bash
dotnet test
```

### Running

```bash
dotnet run --project LsKeeperSamscan --launch-profile LsKeeperSamscan
```

### About the licence

The Open Government Licence (OGL) was developed by the Controller of Her Majesty's Stationery Office (HMSO) to enable
information providers in the public sector to license the use and re-use of their information under a common open
licence.

It is designed to encourage use and re-use of information freely and flexibly, with only a few conditions.
