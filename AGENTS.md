OUTPUT FORMAT I WANT
- Organize by file headers.
- Use filename + 1-based line references like [L123].
- Findings tagged: [ERROR]/[BUG]/[SECURITY]/[PERF]/[DESIGN]/[API]/[DOCS]/[TEST]/[STYLE]/[RISK].
- After findings: “Coverage / Call-tree traversal depth” (which paths you traced, and where you had to stop due to missing context).

DEFINITION OF DONE
- I should be able to trust that:
  1) Indexer parameters normalize to a stable identity across retrieval paths
  2) Parameter cache keys are consistent and won’t split/duplicate entries depending on acquisition path
  3) TypeData caching makes the hot path reflection-free after warmup
  4) Explicit interface accessors are handled correctly (no name-heuristic pitfalls)
