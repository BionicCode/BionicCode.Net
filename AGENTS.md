# GENERAL ENGINEERING RULES
- Favor correctness, readability, testability, and explicit domain boundaries over minimizing type count.
- Do not let external SDK/runtime types leak into domain models unless explicitly requested.
- Prefer replacing structurally wrong draft designs over patching them in place.
- When a task is architectural, propose a plan first before writing code.

If the user prompt is exactly "<review>", treat it as a placeholder that expands to the task defined in the [REVIEW MISSION (default)](#review-mission-default) section below.

User prompt instructions always override this file.

# REVIEW MISSION (default)
Goal: Perform a deep, call-tree-based code review focused on correctness of reflection identity and caching behavior.

Default entry points:
- Start at the public API surface of:
  - `SymbolReflectionInfoCache`
  - `SymbolReflectionInfoCacheKey`
- Trace the full reachable call tree from those entry points as far as repository context allows.

If the user specifies different entry points, files, methods, or paths, follow the user prompt instead.

# PRIMARY INVARIANTS (Definition of Done)
A correct implementation should satisfy all of the following:

1. Indexer parameters normalize to a stable identity across all retrieval paths.
2. Parameter cache keys are consistent and do not split or duplicate entries depending on acquisition path.
3. `TypeData` caching makes the hot path reflection-free after warmup.
4. Explicit interface accessors are handled correctly without brittle name-based heuristics.

# REVIEW PRIORITIES
Prioritize findings in this order:
1. Correctness
2. Cache identity / consistency
3. Hidden duplication or split-cache risks
4. API contract mismatches
5. Performance on hot paths
6. Maintainability / design clarity

# REQUIRED REVIEW METHOD
- Review by tracing actual call paths, not by isolated file scanning only.
- Start from the selected entry point(s) and follow calls downward until:
  - the full relevant path is understood, or
  - you hit a boundary caused by missing files, generated code, external dependencies, or insufficient context.
- Prefer evidence-based findings over speculative concerns.
- Do not infer behavior from naming alone when call tracing can verify it.

# OUTPUT FORMAT
Organize the review by file.

For each file:
- Use a file header with the filename.
- Use 1-based line references in the format `[L123]`.
- Tag each finding with one primary category:
  - `[ERROR]`
  - `[BUG]`
  - `[SECURITY]`
  - `[PERF]`
  - `[DESIGN]`
  - `[API]`
  - `[DOCS]`
  - `[TEST]`
  - `[STYLE]`
  - `[RISK]`

When useful, mention secondary impacts in the explanation, but keep one primary tag per finding.

# REQUIRED SECTIONS IN THE REVIEW
Include these sections in this order:

1. `Scope / Entry Points`
   - What you reviewed
   - Which entry points were used

2. `Call-tree`
   - Show the traced chain(s), for example:
     - `MethodA -> MethodB -> MethodC`
   - Include enough detail to show how conclusions were reached

3. `Findings`
   - Organized by file
   - Include line references and category tags

4. `Coverage / Call-tree traversal depth`
   - State which paths were fully traced
   - State where tracing stopped
   - State why tracing stopped (missing file, external dependency, unclear dynamic dispatch, generated code, etc.)

# STOP / UNCERTAINTY RULES
- If you cannot fully verify a path, stop and explicitly say where verification stopped.
- Do not present an assumption as a confirmed defect.
- Distinguish clearly between:
  - confirmed issue,
  - likely risk,
  - unverified suspicion due to missing context.

# OPTIONAL COMMANDS / EXECUTION
- Do not run builds, tests, benchmarks, or other commands unless the user explicitly requests it.
- Prefer static reasoning and code inspection by default.

# REVIEW STYLE
- Be concise but not shallow.
- Focus on actionable findings.
- Prefer robust fixes over minimal patches when the design is structurally unsafe.
- Call out invariant violations explicitly.
