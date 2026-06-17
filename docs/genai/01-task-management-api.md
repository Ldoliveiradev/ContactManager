# GenAI Exercise: Task Management RESTful API

> This is the explicit GenAI deliverable from the brief:
> *"generate a RESTful API for a simple task management system... write the prompt you would
> use, show the output, describe how you validated, corrected, and handled edge cases."*

It is a **worked, self-contained example** separate from the Contact Manager app itself.

---

## 1. The prompt I used

A good prompt for code generation is **specific about constraints**, not just the happy path.
A vague prompt ("make a task API") produces generic, insecure scaffolding. Here is the prompt
I used, with the reasoning behind each part:

```text
Generate a RESTful API for a task management system in C# / ASP.NET Core (.NET 10).

Domain:
- Task: Id (Guid), Title (required, max 200), Description (optional, max 2000),
  Status (enum: Todo | InProgress | Done), DueDate (optional, must be today or future),
  UserId (Guid, owner).
- Assume a User model already exists; tasks are owned by a user and a user may only
  see/modify their own tasks.

Constraints (important):
- Clean Architecture: Domain / Application / Infrastructure / API layers. Business rules
  and validation live in Application, behind interfaces. No business logic in controllers.
- Persistence is an interface (ITaskRepository) in Application; do NOT use Entity Framework,
  Dapper, or MediatR. Assume a hand-written ADO.NET implementation exists.
- Endpoints: full CRUD. Use correct HTTP verbs and status codes
  (201 + Location on create, 204 on delete, 404 when not found, 400 on validation error,
  401 unauthenticated, 403 when accessing another user's task).
- Authentication: JWT bearer; the owning UserId comes from the token claims, never from
  the request body.
- Validation: reject blank title, title > 200 chars, past DueDate. Return RFC 7807
  ProblemDetails.

Output: the Task entity, the ITaskService + ITaskRepository interfaces, the TaskService
with validation, and the TasksController. Include xUnit tests for the service validation
rules. Do not write the ADO.NET repository body — leave it as the interface.
```

**Why this prompt works (prompt-engineering rationale):**

| Technique | Why |
|---|---|
| Explicit domain with field constraints | Stops the model inventing fields / missing validation. |
| Named the banned libraries | The model's default for "C# API + persistence" is EF Core. Naming the ban prevents the single most likely wrong answer. |
| Listed exact status codes | Models often return `200` for everything. Spelling out `201/204/404/403` forces correct REST semantics. |
| "UserId from token claims, never the body" | Closes a real, common security hole (IDOR / privilege escalation). |
| Asked for tests | Forces testable design and gives me a validation harness for free. |
| Scoped the output | "leave the repository as an interface" keeps the answer focused and avoids a fabricated SQL impl I'd have to throw away. |

---

## 2. Representative generated output

A representative sample of what the model produced (trimmed, **as generated** — the issues
called out in §4 are still present here, e.g. the `TaskStatus` name clash and public setters):

```csharp
// Domain
public enum TaskStatus { Todo, InProgress, Done }   // clashes with System.Threading.Tasks.TaskStatus

public class Task                                    // clashes with System.Threading.Tasks.Task
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
    public DateOnly? DueDate { get; set; }
    public Guid UserId { get; set; }
}

// Application — note the awkward System.Threading.Tasks.Task<Task> return types,
// a direct symptom of naming the entity `Task`.
public interface ITaskRepository
{
    Task<Task?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Task>> GetByUserAsync(Guid userId);
    Task AddAsync(Task task);
    Task UpdateAsync(Task task);
    Task DeleteAsync(Guid id);
}

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repo;
    public TaskService(ITaskRepository repo) => _repo = repo;

    public async Task<Task> CreateAsync(CreateTaskRequest req, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(req.Title))
            throw new ValidationException("Title is required.");
        if (req.Title.Length > 200)
            throw new ValidationException("Title must be 200 characters or fewer.");
        // ...creates and persists
    }
}

// API
[ApiController, Route("api/tasks"), Authorize]
public class TasksController : ControllerBase
{
    // POST returns CreatedAtAction(...); GET 404 when null; DELETE 204; etc.
}
```

---

## 3. How I validated the AI's suggestions

1. **Ran the generated xUnit tests** — confirmed validation rules actually failed/passed as
   claimed (didn't trust the model's word).
2. **Checked against the brief's constraints** — verified no EF/Dapper/MediatR crept in, and
   that business logic sat in `TaskService`, not the controller.
3. **Reviewed the HTTP semantics** endpoint-by-endpoint against the status codes I specified.
4. **Read the auth path** specifically for the IDOR risk (does a user see others' tasks?).

## 4. What I corrected or improved (the critical-thinking part)

The generated code was a reasonable scaffold but had **real problems** I had to fix:

| Issue found | Why it's wrong | Fix |
|---|---|---|
| Entity named `Task` and enum `TaskStatus` both shadow BCL types (`System.Threading.Tasks.Task` / `TaskStatus`). | Compiles, but `Task<Task>` return types are a genuine readability trap and easy to misuse. | Renamed the entity to `TaskItem` and the enum to `WorkItemStatus`. |
| `DueDate` past-date rule was in the prompt but **missing** from the generated validation. | The model silently dropped an edge case. Classic "looks complete, isn't." | Added explicit `DueDate < today` check + a failing test first. |
| Public setters on the entity (`set;`) let any layer mutate state freely. | Breaks domain invariants; anyone can set an invalid Status. | Made setters private; mutate via intention-revealing methods. |
| `GetByIdAsync` returned the task **without checking ownership**. | **Security bug:** user A could read user B's task by guessing the Guid (IDOR). | Added ownership check → `403 Forbidden` when `task.UserId != callerId`. |
| `ValidationException` thrown but no mapping to `400 ProblemDetails`. | Would surface as a `500`. | Added exception-handling middleware mapping to RFC 7807. |

## 5. Edge cases, authentication, and validation — how they were handled

- **Auth:** `[Authorize]` on the controller; `UserId` read from the JWT `sub`/`NameIdentifier`
  claim, never trusted from the request body. Prevents a client from creating tasks "as"
  another user.
- **Ownership (authorization, not just authentication):** every read/update/delete re-checks
  that the task belongs to the caller → `403` otherwise. This is the bug the AI missed and
  the most important correction.
- **Validation edge cases:** blank/whitespace title, title length boundary (exactly 200 vs
  201), past due date, unknown status value, missing/not-found id (`404`).
- **Concurrency / not-found on update:** update of a non-existent id returns `404`, not a
  silent no-op.

## Takeaway

The AI got me ~70% of the way in minutes, but the remaining 30% — the **security
(IDOR), a dropped validation rule, and domain encapsulation** — is exactly where human
review matters. The value isn't "AI writes code"; it's "AI drafts, engineer verifies
against spec and threat model, tests prove it."
