# Finalizing

**Compaction note:** If you are re-reading this prompt after context compaction, check `llm-scratchpad/current_status.md`
for where you left off.  If you have already merged, do not merge again.

You have now completed the sequence of prompts.  Now you want to clean up and merge. Specifically:

- Delete `llm-scratchpad`
- Commit that you deleted `llm-scratchpad`
- perform a final validation of CLAUDE.md to check that it is still accurate with all of today's changes, updating it as necessary; commit here if you changed it.
- Merge to master/main/the default branch, whatever that might be (it varies):
  - `git pull` the default branch first
  - Start with a rebase merge
  - If the rebase merge cannot be completed without resolving conflicts try a standard merge
  - If neither can be handled without resolving conflicts then do whichever seems to lead to the least conflicts and resolve the conflicts
- **DO NOT** delete the branch you did work on. Instead, show the user the git command and tell them that they should only do so after they confirm everything works.

Then summarize the work for the user, and tell them that they can use git bisect to find issues.  Remind them that you are willing to help them use git bisect to find out which step of this process broke something.
