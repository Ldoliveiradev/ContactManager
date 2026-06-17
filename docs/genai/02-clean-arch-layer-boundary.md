# GenAI Log: Clean Architecture Layer Boundary — Auth Service Placement

> This file documents a real architectural decision that arose during the Contact Manager
> implementation, where AI was used as a **collaborator and reviewer**, not just a code
> generator. The decision involved where to place `AuthService` and how to resolve the
> dependency inversion problem it created.

---

## 1. The problem

The first AI-generated scaffold placed `AuthService` (register/login) inside
`Infrastructure.Identity`. At first glance that seems natural: auth lives with JWT and
password hashing. But it broke the Clean Architecture dependency rule:

```
Application  ──depends on──▶  Infrastructure   ✗  (rule violation)
```

`Application` defines the use-case contracts. If `AuthService` sits in `Infrastructure`,
any Application service that needs to call it would have to reference an Infrastructure
type — or we'd have to duplicate the interface.

**Prompt used to surface this issue:**

```text
We have AuthService in Infrastructure.Identity. The Application layer's IAccountService
and IContactService are also there. Clean Architecture says Application depends on Domain
only — Infrastructure implements Application interfaces. Does placing AuthService in
Infrastructure violate that rule? If yes, where should it live and what dependency
problem does that create?
```

---

## 2. AI-generated answer (representative output)

```
Yes, it violates the rule. AuthService is a use-case orchestrator: it reads from the
repository (IAccountRepository) and calls password hashing and token generation.
Use-case orchestration belongs in Application.

Moving it to Application.Auth.Services creates a new problem: AuthService needs to
generate a JWT token, but IJwtTokenGenerator is in Infrastructure.Identity — so
Application would depend on Infrastructure again.

Resolution options:
1. Put `ITokenGenerator` / `IPasswordHasher` in the Application auth interfaces package.
   Infrastructure implements them and Application consumes them without depending on
   Infrastructure.
2. Put `ITokenGenerator` in Domain.Interfaces.
   Technically valid, but it pushes authentication contracts into Domain.
3. Keep AuthService in Infrastructure but expose only IAuthService from Application.
   Works but AuthService can never be unit-tested without pulling Infrastructure deps.
```

---

## 3. How I validated the output

**Checked the dependency graph manually** before writing any code:

| Layer | May depend on | May NOT depend on |
|---|---|---|
| Domain | (nothing) | Application, Infrastructure, API |
| Application | Domain | Infrastructure, API |
| Infrastructure | Application, Domain | API |
| API | Application, Infrastructure (DI root only) | — |

The final implementation uses **Application-owned auth interfaces**:
- `AuthService` (Application) depends on `ITokenGenerator` / `IPasswordHasher`
  in `ContactManager.Application.Auth.Interfaces` ✓
- `JwtTokenGenerator` / `Pbkdf2PasswordHasher` (Infrastructure) implement those
  contracts ✓
- No Application → Infrastructure edge ✓

**Ran the build** after restructuring to confirm no circular references.

**Ran the tests** — the Application test suite covers `AuthService` with mocked
`IUserRepository`, `IAccountRepository`, `IPasswordHasher`, and `ITokenGenerator`,
confirming `AuthService` is fully testable without any Infrastructure dependency.

---

## 4. Corrections and improvements

| AI suggestion | Problem found | What was changed |
|---|---|---|
| "Put ITokenGenerator in Domain.Interfaces" | It solves the dependency problem, but still places authentication contracts in the Domain layer, which is broader than necessary. | Placed `ITokenGenerator` and `IPasswordHasher` in `ContactManager.Application.Auth.Interfaces` instead. |
| `AuthService` threw `UsernameAlreadyExistsException` for duplicate registration | Exception-based flow forces callers to use try/catch for expected business outcomes — not idiomatic for service-layer APIs; also complicates the middleware. | Replaced with response-object pattern: `RegisterResponse.Failure("username already taken")`. Exceptions are only for truly unexpected failures (DB down, etc.). |
| AI generated an `IdentityService` / `IIdentityService` facade over the hasher + token generator | It duplicated what `AuthService`/`AccountService` already do directly via `IPasswordHasher`/`ITokenGenerator`, was registered in DI but **injected by nothing**, and its `IsCurrentUser` helper was an unused alternative to the inline ownership guard. Classic AI "best-practice" scaffolding with no consumer — the kind of thing the brief's *critical-evaluation* criterion rewards catching. | **Deleted** `IdentityService`, `IIdentityService`, and `IJwtTokenGenerator`. `JwtTokenGenerator` now implements only the `ITokenGenerator` Application auth port. Ownership is enforced by the `contact.AccountId != accountId` guard in `ContactService`, with the user id read from the JWT via `ClaimsPrincipalExtensions.GetUserId()`. |

---

## 5. Edge cases and architectural notes

**Why not MediatR?** The brief explicitly forbids it. `AuthService` is a plain C# class
implementing `IAuthService`; the controller depends only on that interface. No mediator
needed.

**Single registration for JwtTokenGenerator:** It implements only the `ITokenGenerator`
Application auth port. The DI root registers it once as a singleton so all callers share the same
key material:

```csharp
services.AddSingleton<ITokenGenerator>(sp =>
    new JwtTokenGenerator(jwtOptions, sp.GetRequiredService<TimeProvider>()));
```

(`TimeProvider` is injected so token expiry is deterministic and unit-testable.)

**Ownership enforcement:** The user id is read from the JWT in the API layer via
`ClaimsPrincipalExtensions.GetUserId()` (reading `ClaimsPrincipal` is a web concern, so it
stays in the controller layer) and passed into the Application service. `ContactService`
then guards every mutation with `contact.AccountId != accountId` and returns `403` for a
non-owned contact. No separate `IsCurrentUser` service is needed.

**User vs. Account boundary:** A later refactor split authentication from the business
domain — `UserModel` (username + password hash) is an Application-layer auth concern, while
`AccountDomain` (name, email) is the business entity in the Domain layer. They share one id.
This keeps password/credential data out of the domain entirely; the domain only ever sees an
opaque id.

**Test coverage of the boundary:** Because `AuthService` depends only on Domain interfaces
(plus the Application-owned `IUserRepository`), `AuthServiceTests` has zero Infrastructure
imports. That's the acid test: if a test file for an Application service needs to import an
Infrastructure namespace, the boundary is wrong.
