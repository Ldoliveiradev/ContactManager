# GenAI Usage Log

This folder is the evidence for the exercise's **Generative AI tools** deliverable and the
presentation's *"fluency with GenAI tools and prompt engineering, and critical thinking when
evaluating AI-generated code"* criterion.

It documents, honestly and concretely:

1. **The prompts** used to drive development (prompt engineering).
2. **What the AI produced** (representative samples).
3. **How the output was validated** (tests, manual review, spec check).
4. **What was corrected or improved, and why** (critical thinking — the important part).
5. **How edge cases, auth, and validation were handled.**

## How to read this

- [`01-task-management-api.md`](01-task-management-api.md) — the brief's explicit GenAI
  exercise: "generate a RESTful API for a task management system." Contains the exact prompt,
  the generated output, and a critique of what was wrong/risky and how it was fixed.
- Subsequent files (`02-...`, `03-...`) log the real prompts used to build *this* Contact
  Manager, each paired with the corrections made.

## Guiding principle

AI accelerated the work; it did not make the decisions. Every AI suggestion was checked
against the architecture constraints (no EF/Dapper/MediatR, Clean Architecture boundaries),
covered by a test, and corrected where it was wrong. The corrections are documented because
*the corrections are the skill being demonstrated.*
