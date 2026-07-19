# SyncBackend — Obsidian Calendar Sync API

Backend for cross-platform (iOS + Android) sync of the [Obsidian Calendar](https://github.com/) Flutter app.
It stores each user's vault of markdown (`.md`) files and lets multiple devices push/pull changes so they stay in sync.

- **Stack:** C# / ASP.NET Core (.NET 8), Entity Framework Core, PostgreSQL
- **Sync model:** file-level. Whole `.md` files are synced; a true conflict keeps *both* copies (a "conflict copy"). The vault stays the source of truth — fully Obsidian-compatible.
- **Hosting target:** AWS (Amazon RDS for Postgres + App Runner / ECS Fargate)

> This is a separate repo from the Flutter app on purpose: independent deployment/CI, clean tooling, and a standalone portfolio piece. The **API contract below is the fixed boundary** between app and server — it's what will let the backend be re-implemented in Java/Spring later without touching the app.

## Running locally

```bash
dotnet run --project src/SyncBackend.Api
```

Then open Swagger UI (interactive API docs) at the HTTPS URL printed in the console, path `/swagger`.
Health check: `GET /health`.

## API Contract (v1)

All responses are JSON. Authenticated endpoints require `Authorization: Bearer <jwt>`.

| Method & path | Auth | Purpose |
|---|---|---|
| `POST /auth/register` | – | Create account `{ email, password }` → `{ token }` |
| `POST /auth/login` | – | Sign in `{ email, password }` → `{ token }` |
| `GET /sync/changes?since={revision}` | ✔ | Files changed since the caller's last revision → `[{ path, revision, hash, deleted }]` |
| `GET /files/{path}` | ✔ | Download one file's content |
| `PUT /files/{path}` | ✔ | Upload a changed file `{ content, baseRevision }`; server checks `baseRevision`, assigns a new revision |
| `DELETE /files/{path}` | ✔ | Tombstone a deleted file (carries `baseRevision`) |
| `GET /health` | – | Liveness probe |

### Data model

`VaultFile { Id, UserId, Path, Content, Hash, Revision, IsDeleted, UpdatedAt }`

- `Revision` — a per-user monotonically increasing counter. Drives incremental pulls (`?since=`).
- `Hash` — content fingerprint for cheap change detection.
- Optimistic concurrency: writes carry `baseRevision`; the server rejects a stale write so two devices can't silently clobber each other.

## Roadmap

- **Phase 0** — scaffold + health check *(done)*, then EF Core + Postgres
- **Phase 1** — auth (JWT) + the sync/file endpoints (MVP, run locally)
- **Phase 2** — Dockerfile + docker-compose (API + Postgres)
- **Phase 3** — deploy to AWS (RDS + App Runner/Fargate)
- **Phase 4** — CI/CD (GitHub Actions), tests
- **Later** — re-implement in Java/Spring behind this same contract
