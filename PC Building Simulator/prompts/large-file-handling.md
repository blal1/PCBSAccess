# Overview and Motivation

In this step you will attempt to fix large files, then update the code index to match whatever you changed.  The goal is
to reduce the amount of context a human reader or LLM must consume at one time.

**Compaction note:** If you are re-reading this prompt after context compaction, check `llm-scratchpad/current_status.md`
and your task list for where you left off.  Do not re-split files you have already split.

# Procedure

The specific antipattern to evaluate for here is large files that can be trivially split.  You are not refactoring the
code, you are just getting `mybigfile.cs` that happens to be 5 modules shoved together separated.  This can include mild
refactors such as new method parameters, but larger rethinking comes later.

Identify all files which are over 500 lines and which have more than one concern. For each file (that is, one file at a time), run the following procedure:

- Decide on candidate files to which it will be split. This can include entirely new files (removing the original), or pulling code out and importing and (re)exporting it.
- Decide what functions and methods go in each file, and if new ones are needed.
- Consider the implications on possible bugs. List to yourself what bugs the split may cause.
- Explain the split to the user, ask them to confirm they want to do it, and if they say no continue to the next file instead
- Perform the split, using the mod's build commands to validate where possible.
- if there are automated tests, run them.
- Stop and summarize the split to the user.  Ask them to test that:
  - Things still load/run/etc (a smoke test, but the user may not know the term)
  - To check specific things if you identified potential bugs.
- Make a commit with this split

You will spend significant time on each file.  You have access to a task system.  Use it!  Break this down into todo items to at least the file granularity.  There is no way you will do this without hitting the compaction boundary!

# Up Next

Once all large files have been split, continue to `prompts/input-handling.md`.
