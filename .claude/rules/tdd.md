# TDD

> **Always in effect.** This rule is loaded for every task in this repository and applies to
> every change that touches source code — no exceptions without asking first.

**No production code is written while there is no failing test that requires it.** Every task
that touches source code follows Test-Driven Development, one small behaviour at a time.

## The cycle

1. **RED** — write the test first and run it. It must fail, and it must fail for the expected
   reason (the missing behaviour), not because it does not compile by accident or the fixture
   is wrong. Never skip running the test at this step: a test that was never seen failing
   proves nothing.
2. **GREEN** — write the **minimum** production code that makes the test pass. No extra fields,
   branches, overloads or "we will need it later" abstractions. Run the test again and confirm
   it passes, along with the rest of the suite.
3. **BLUE** — refactor both the test and the production code just written: remove duplication,
   improve names, extract fixtures/helpers, apply @.claude/rules/folder-structure.md. Run the
   suite again after refactoring; behaviour must not change and every test must stay green.

Then repeat from RED for the next behaviour. Never run several behaviours through the cycle at
once — one failing test, one piece of production code, one refactor.

## Running the tests

```bash
# RED / GREEN on a single test while iterating:
dotnet test --filter "FullyQualifiedName~<Verb><Aggregate>Test"

# BLUE, and before declaring anything done:
dotnet test
```

The end-to-end suite needs MySQL up (`docker compose up -d`, port 33060).

## Rules that follow from this

- Write the test in the project that matches the layer — `Codeflix.Catalog.UnitTests`,
  `.IntegrationTests` or `.EndToEndTests` — in the folder given by
  @.claude/rules/folder-structure.md, using the `add-test-suite` skill for the fixture,
  collection and trait protocol.
- A new use case starts with its unit test, not with the handler, the input or the interface.
- Bug fixes start with a test that reproduces the bug and fails; fix only after seeing it red.
- Do not write a batch of production code and then add tests for it afterwards — that is not
  TDD and is not accepted here.
- Do not weaken or delete a test to make the suite green. If a test is wrong, fix the test in
  its own RED step and say why.
- Report the cycle honestly: if a test was not seen failing, or the full suite was not run,
  say so instead of implying it was.
- If a change genuinely cannot be driven by a test (EF migration, generated code, pure config,
  a `.csproj` or Docker change), say so explicitly and ask before writing it.
