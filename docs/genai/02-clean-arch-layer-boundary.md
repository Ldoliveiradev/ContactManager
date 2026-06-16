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
1. Add ITokenGenerator to Domain.Interfaces. JwtTokenGenerator implements it.
   Application depends on Domain (allowed).
2. Put ITokenGenerator in Application.Common.Interfaces.
   Same effect — Application owns the contract, Infrastructure implements it.
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

Option 1 (ITokenGenerator in Domain) satisfies all rows:
- `AuthService` (Application) depends on `ITokenGenerator` (Domain) ✓
- `JwtTokenGenerator` (Infrastructure) implements `ITokenGenerator` (Domain) ✓
- No Application → Infrastructure edge ✓

**Ran the build** after restructuring to confirm no circular references.

**Ran the tests** — 47 Application tests cover `AuthService` with mocked
`IAccountRepository`, `IPasswordHasher`, and `ITokenGenerator` (all Domain interfaces),
confirming `AuthService` is fully testable without any Infrastructure dependency.

---

## 4. Corrections and improvements

| AI suggestion | Problem found | What was changed |
|---|---|---|
| "Put ITokenGenerator in Application.Common.Interfaces" | Creates an Application-level interface that wraps a Domain concept (token generation from a Domain entity). Domain is a better home since it defines `AccountDomain`. | Placed `ITokenGenerator` in `Domain.Interfaces` instead. |
| `AuthService` threw `UsernameAlreadyExistsException` for duplicate registration | Exception-based flow forces callers to use try/catch for expected business outcomes — not idiomatic for service-layer APIs; also complicates the middleware. | Replaced with response-object pattern: `RegisterResponse.Failure("username already taken")`. Exceptions are only for truly unexpected failures (DB down, etc.). |
| `IdentityService` in Infrastructure merged DB access with crypto | Would make `IdentityService` untestable in isolation and give it two reasons to change. | Split: `AuthService` (Application) owns DB orchestration; `IdentityService` (Infrastructure.Identity) is pure stateless crypto and JWT wrapping with no DB access. |
| `IIdentityService` placed in Application layer | Application can't own an interface that describes Infrastructure concerns (password hashing, JWT signing). | Moved `IIdentityService` to `Infrastructure.Identity.Interfaces` — it's an internal contract within the Identity project, not a cross-layer port. |

---

## 5. Edge cases and architectural notes

**Why not MediatR?** The brief explicitly forbids it. `AuthService` is a plain C# class
implementing `IAuthService`; the controller depends only on that interface. No mediator
needed.

**Single registration for JwtTokenGenerator:** It implements both `IJwtTokenGenerator`
(Infrastructure internal) and `ITokenGenerator` (Domain port). The DI root registers the
concrete once, then maps both interfaces to the same singleton — avoiding two instances
with separate key material:

```csharp
services.AddSingleton<JwtTokenGenerator>(_ => new JwtTokenGenerator(jwtOptions));
services.AddSingleton<IJwtTokenGenerator>(sp => sp.GetRequiredService<JwtTokenGenerator>());
services.AddSingleton<ITokenGenerator>(sp => sp.GetRequiredService<JwtTokenGenerator>());
```

**IsCurrentUser ownership:** The `IsCurrentUser(Guid, ClaimsPrincipal)` helper belongs in
`IdentityService` (Infrastructure.Identity) — it reads JWT claims, which is an
infrastructure concern. It does NOT belong in `AuthService` (Application), which has no
knowledge of `ClaimsPrincipal`. Controllers call it via `IIdentityService` to guard
account-scoped endpoints.

**Test coverage of the boundary:** Because `AuthService` depends only on Domain interfaces,
`AuthServiceTests` has zero Infrastructure imports. That's the acid test: if a test file
for an Application service needs to import an Infrastructure namespace, the boundary is
wrong.
