If user prompt only contains "<review>", consider it an instructions placeholder for the [REVIEW MISSION (default)](#review-mission-default) section which serves as the real task input:

# REVIEW MISSION (default)
Goal: Perform a deep, call-tree-based review focused on correctness of reflection identity/caching.
Default entry point(s):
- Start at: public API of SymbolReflectionInfoCache and SymbolReflectionInfoCacheKey
- Trace: Full call tree.

If the user prompt specifies a different entry point or path, follow the user prompt (user prompt overrides AGENTS.md).

# PRIMARY INVARIANTS (Definition of Done)
I should be able to trust that:
1) Indexer parameters normalize to a stable identity across retrieval paths
2) Parameter cache keys are consistent and won’t split/duplicate entries depending on acquisition path
3) TypeData caching makes the hot path reflection-free after warmup
4) Explicit interface accessors are handled correctly (no name-heuristic pitfalls)

# OUTPUT FORMAT I WANT
- Organize by file headers.
- Use filename + 1-based line references like [L123].
- Findings tagged: [ERROR]/[BUG]/[SECURITY]/[PERF]/[DESIGN]/[API]/[DOCS]/[TEST]/[STYLE]/[RISK].
- After findings: “Coverage / Call-tree traversal depth” (which paths you traced, and where you had to stop due to missing context).

# REVIEW DEPTH RULES
- Always include a short “Call-tree” section showing the traced chain (at least: method A -> method B -> method C).
- If you cannot fully trace due to missing files/context, stop and explicitly say where you stopped.

# OPTIONAL: COMMANDS / EXECUTION
- Do not run build/tests unless explicitly requested by the user.
- Prefer static reasoning + code inspection.