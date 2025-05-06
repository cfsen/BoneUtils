# Test Organization Guidelines

This project uses a structured, type-centric approach to organizing MSTest unit tests for clarity, scalability, and ease of debugging.

## Project & Namespace

- All tests reside in the `Tests` project, which uses the `Tests` root namespace.

## Directory & File Structure

Tests are organized by the type they cover, using the following conventions:

- `{Type}_Tests/` – A directory for all tests related to a specific class or struct.
- `{Type}_Tests.cs` – A general-purpose file for smaller or early-stage tests that don’t yet warrant their own dedicated file.
- `{Type}_{Functionality}Tests.cs` – Focused test files for specific behaviors, components, or features of the type.

This approachs aims to keep related tests grouped together, while allowing for flexible growth as coverage increases.

## Test Classes & Methods

- Test classes mirror their filenames and are placed in namespaces like `Tests.{Type}_Tests`.
- Base classes like `MockDataBuilder` may be used for test setup.
- Test methods follow a `{Type}_{Action}` naming convention (e.g., `BoneNode_CanConstruct`) to make the subject under test obvious.

## Categorization

- `[TestCategory]` attributes are used to group related tests (e.g., `"BoneNode construction"`).
- Additional categories (e.g., "Unit", "Integration") can be added as needed.

## Philosophy

This structure prioritizes:
- Discoverability: Search by type name to find all related tests.
- Readability: Clear mapping between tests and production code.
- Maintainability: Easy to refactor and scale as coverage increases.
