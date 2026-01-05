# Code review output requirements (call-tree coverage footer)

When performing a code review, always include a **Call-tree coverage footer** at the end of the review.

When reviewing, prefer a **Clean Code** lens (readability, naming, and complexity) **without** requesting changes that conflict with the repository’s `.editorconfig` or established local conventions. Avoid broad refactors unless explicitly requested.

## Call-tree coverage footer format

Include the following items:

- **Traversal completeness:** `Complete` or `Incomplete`.
- **Coverage depth:** the number of expanded call steps (e.g., `A -> B -> C` = depth 3).
- **Expanded symbols:** list the expanded call chain(s) (method names).
- **Stopped at (when incomplete):** the last method reviewed including `file path` and `line range`.
- **Unexpanded callees / reasons:** list any calls not expanded and why (e.g., definition not provided, reflection, dynamic dispatch, delegate/event, too many branches).

## Definition of "Complete"

“Complete” means complete **with respect to the code that is available to the assistant in the current prompt/context**.  
If additional files are required to increase coverage, the review must explicitly request them.