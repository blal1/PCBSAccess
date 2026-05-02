# Motivation and Overview

**Compaction note:** If you are re-reading this prompt after context compaction, check `llm-scratchpad/current_status.md`
and your task list for where you left off.  Do not re-propose cleanups you have already completed.

In this stage you will put on your senior developer hat, evaluate the project at the per-file level, and look for things that can be cleaned up with low risk.

# Guidance

The procedure here is simple: you are to build a list of possible cleanups, as long as possible, prune that list to the reasonable subset, present the user with that list, and do the ones they suggest.  In this stage you are considering the codebase at the individual file level.  So, spawn a subagent per file and have it return:

- A list of things in the file that could be cleaned up
- An estimate of how many lines this might be removed
- A list of intra-file duplicate code that could be factored out

once you have, each subagent should return a large list.  You should take these lists, decide on the top 10 or 20 most impactful from the set, and then further validate them.  After that propose this list to the user.

A good cleanup:

- Removes significant numbers of lines (not all cleanups remove lines! But it is always a good win)
- Eliminates significant duplication
- Makes code more flexible and easier to read 6 months from now
- Allows adding more features, especially features we don't know we need today
- Removes and makes bugs less likely

"It's a lot of work", or "This is a finished-seeming mod, so risky refactors don't seem worth it" are not good reasons
to drop from the list.  The user is running these prompts because they want cleaner code and because you are an LLM
coding at superhuman speeds, not because they are trying to avoid risk.

# Procedure

using the above guidance:

- Identify possible cleanups
- Compose the list and ask the user which ones they want
- Turn this into a plan which commits after every individual cleanup
- Ask the user to test and confirm that everything works before continuing
- Summarize what was done, wait on the user to confirm, then move to the next prompt


# Up Next

Next, read `prompts/high-level-cleanup.md`