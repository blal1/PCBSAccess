# Motivation and Overview

A common problem with mods which are made by non-coders is the "one file of death" problem: a single source file or
small set of files containing almost all the logic, potentially 10000+ lines in length.  In this stage of the process,
you will prepare to fix this if necessary, and lay the groundwork for later refactors.

Mods are a funny thing.  Because they run in odd environments, you are also likely functioning without accurate LSP.
This step also fixes that, by preparing a database you can query.

**Compaction note:** If you are re-reading this prompt after context compaction, check `llm-scratchpad/current_status.md`
for where you left off.  If the code index already exists under `llm-scratchpad/code-index`, you have completed this
step and should skip to the "Up Next" section.

# Procedure

Make `llm-scratchpad/code-index`

Find all code files in the mod.  For each file, spawn a Sonnet subagent which creates a matching file under
`llm-scratchpad/code-index`. For example, `a/b.cs` goes to `llm-scratchpad/code-index/a/b.cs.md`.  Try to do them in
parallel, several at a time.  The subagent should include:

- Any comments
- All classes and methods but not their bodies, in declaration order.
- For complicated methods or methods with misleading names, a brief note on what the code does and that the name may be
  misleading
- Line numbers

So for example, as Pythonic pseudocode, you could do something like:

```
class UiScreen:
  def get_focus() -> UiElement (line 31)

  # Moves to the next focusable element. Wraps at the last one.
  def next_element() -> bool: (line 52)
```

the specific format is not important as long as it is consistent across all generated files and is easily searchable
with whatever search tools are available to you.  You will note that this is under `llm-scratchpad`, which is a
scratchpad.  These are not long-term files.  They are for this run of the refactor/cleanup loop.

When done, you should commit this index.  This may seem counterintuitive, but it is useful to have it as you move forward, because later steps of this process may require bisecting.  The index's existence should also be recorded in `llm-scratchpad/current_status.md`

# Up Next

If you found a file longer than 2000 lines continue to `prompts/large-file-handling.md`.

Otherwise continue to `prompts/input-handling.md`.

