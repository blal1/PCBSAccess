# Motivation and Overview

**Compaction note:** If you are re-reading this prompt after context compaction, check `llm-scratchpad/current_status.md`
and your task list for where you left off.  Do not re-propose changes you have already completed.

This stage of the process is a traditional PR-style code review but you are both the reviewer and the fixer.

# Guidance

In this stage, put your senior developer hat on and perform a traditional code review of the whole codebase.  The procedure with this prompt is slim because the best strategies depend on the codebase.  Here are the general things to consider:

- Is this code good next year?
- Is this code fragile and likely to break with game updates in a way that's not intentional?
  - Specifically, sometimes mods are fragile because that is the only way
- Is this code underdocumented? Overdocumented?
- Is there a lot of cross-file duplication?
- Could some sort of general abstractions being added somewhere (for example OOP) significantly simplify things?
- can anything be replaced with calls to the standard library? (IMPORTANT! Mods do not generally have access to third party packages, and may not have the full standard library either. You should validate what is really available, even if this means asking the user to test)
- Can specific algorithms be merged into a single more general algorithm?

As with the previous step "It's risky" or "it's a lot of work" are not reasons to avoid a change.  The user is running these prompts to increase quality and long-term success.  It's about next year and the surprises the future holds, not being conservative.


As a reminder your user is not an experienced coder. So at this point, you should:

- Propose options with clear explanations of the trade-offs and risks that can be understood by the average person
- Aim for code organization which does not require being a genius to understand (e.g. don't get super clever with closures)
- Remind the user that you are happy to answer questions they might have

# Procedure

- Do the code review with the above guidance
- Prepare a plan to address the changes the user wants addressed and only the changes the user wants addressed
- Commit after each individual change
- After each individual change, ask the user to test the related functionality with a clear description of what they should specifically check
- When done, summarize the work and ask the user to do a more thorough test by playing the game
- Move onto the next prompt

# Up Next

Next, read `prompts/finalization.md`
