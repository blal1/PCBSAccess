# Overview and Motivation

This prompt ensures that the environment is clean and safe for you to proceed, and that you have all of the information
you need to get started.

**Compaction note:** If you are re-reading this prompt after context compaction, check `llm-scratchpad/current_status.md`
for where you left off.  Do not repeat steps you have already completed.

# Procedure

## 1. Sanity Checks

You should explicitly check the following:

- Git is installed
- This is a git repository currently checked out to master or main
- The git state is clean
- If you are Claude, A CLAUDE.md exists
- The `llm-scratchpad` directory does **NOT** exist.
- You know or can find out what game this mod is for


## 2. Sanity Stop Points

If any of these are true, stop, tell the user, and **DO NOT CONTINUE**:

- There is no CLAUDE.md: stop and tell the user to run `/init`.  
- There is already an `llm-scratchpad` directory: Stop and ask the user if they really want to proceed, explaining to
  them that this directory indicates that something may have already been ongoing. Remind them they can resume previous
  sessions and, if you know without having to look it up, how to do so.
- Git is not installed, or the mod is not in a git repository yet: Stop and tell the user that it is not safe to run
  this procedure without git, and offer to assist them in setting it up
- Git is not in a clean state on master or main: ask the user to get it onto master or main and commit outstanding work,
  and offer to help them if you feel it is appropriate

## 3. Setup

- Pick a branch name, for example `claude-mod-cleanup`.  Check that it is unique, create it, and check it out.  You will
  do your work on this branch.  IMPORTANT: after this step, the sanity checks in this prompt will **NOT** pass because
  the repository is now on a non-default branch, as you are beginning your work.
- Create `llm-scratchpad` and `llm-scratchpad/current_status.md`, and commit these files

Now pause and communicate the following to the user:

- The name of the branch
- That they will want to merge work done on this branch to their default branch if they stop you in the middle of the process
- That if you continue, you will eventually merge it for them.
- If you are Claude, remind them that they can rewind after asking you for explanations as to not pollute context with questions and tangents.

Wait for the user to confirm that they are ready.

# Up Next

Once you have finished the above work, read prompts/information-gathering-and-checking.md
