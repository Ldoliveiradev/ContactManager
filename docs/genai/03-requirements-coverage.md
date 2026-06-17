# GenAI Deliverable Coverage

This file maps the **Generative AI tools** section of the exercise brief to the
documentation included in this repository.

## Brief requirement: write the prompt you would use

- Status: Done
- Evidence:
  - [`01-task-management-api.md`](./01-task-management-api.md), section
    **"1. The prompt I used"**
  - [`02-clean-arch-layer-boundary.md`](./02-clean-arch-layer-boundary.md), section
    **"1. The problem"** includes the prompt used to evaluate a real architecture issue

## Brief requirement: show the output code or a representative sample

- Status: Done
- Evidence:
  - [`01-task-management-api.md`](./01-task-management-api.md), section
    **"2. Representative generated output"**
  - [`02-clean-arch-layer-boundary.md`](./02-clean-arch-layer-boundary.md), section
    **"2. AI-generated answer (representative output)"**

## Brief requirement: describe how the AI's suggestions were validated

- Status: Done
- Evidence:
  - [`01-task-management-api.md`](./01-task-management-api.md), section
    **"3. How I validated the AI's suggestions"**
  - [`02-clean-arch-layer-boundary.md`](./02-clean-arch-layer-boundary.md), section
    **"3. How I validated the output"**

## Brief requirement: describe how the output was corrected or improved

- Status: Done
- Evidence:
  - [`01-task-management-api.md`](./01-task-management-api.md), section
    **"4. What I corrected or improved"**
  - [`02-clean-arch-layer-boundary.md`](./02-clean-arch-layer-boundary.md), section
    **"4. Corrections and improvements"**

## Brief requirement: describe how edge cases, authentication, or validations were handled

- Status: Done
- Evidence:
  - [`01-task-management-api.md`](./01-task-management-api.md), section
    **"5. Edge cases, authentication, and validation"**
  - [`02-clean-arch-layer-boundary.md`](./02-clean-arch-layer-boundary.md), section
    **"5. Edge cases and architectural notes"**
  - In the main project itself, related fixes are visible in the API ownership checks,
    duplicate registration handling, and password-change authorization rules

## What this demonstrates to the interview panel

- Prompt engineering:
  the prompts are explicit about constraints, architecture, and security
- Critical evaluation:
  the documentation shows where AI output was incomplete, risky, or overengineered
- Validation discipline:
  the output was checked with tests, manual review, and comparison against the brief
- Security awareness:
  ownership and auth decisions were not trusted to the generated scaffold

## Recommended presentation flow for this section

1. Open [`01-task-management-api.md`](./01-task-management-api.md).
2. Show the prompt first.
3. Show the representative output sample.
4. Spend most of the time on the corrections table and the missed ownership bug.
5. Close with [`02-clean-arch-layer-boundary.md`](./02-clean-arch-layer-boundary.md) to show
   AI used as a reviewer on a real decision in this codebase, not just a code generator.
