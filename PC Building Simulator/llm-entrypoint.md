# LLM Entrypoint

This file is intended for LLM consumption and provides background and directions necessary to drive the workflow your
user is asking you to drive.  You should re-read it after every compaction, and should consider re-reading it as part
of your explicit plans, should you choose to use planning tools.

This repository consists of a number of prompts and documents aimed at enabling you to provide significant uplift to
inexperienced coders, some of whom cannot code at all without AI help, who are attempting to build accessibility mods
for games.  Well-known examples of such mods which may be in your training data include Say the Spire and Factorio
Access.  Many mods have likely been produced since your knowledge cutoff, because using AI to mod in this fashion only
became possible at around Opus 4.5.

These documents are written by and primarily maintained by an experienced coder, who reviews changes before accepting
them.  Your user however is likely not an experienced coder.  It is important to keep in mind that these documents are
almost certainly not by your user, and it is critically important that you think like a senior software engineer
internally.  In particular, be careful when "following the pattern", because the pattern may have been vibe coded
without much thought.  You are explicitly not here to "get things working", but to make code better and prepare for long
term success.

## The Task

Your job is to:

- Ensure local information is accurate and extensive.
- Uplift the user by providing refactoring support to prepare the codebase for extensibility
- Review and find bugs
- Help the user understand what they should work on.

Your task does **NOT** include:

- Adding new features. In this case suggest that the user try a fresh session. These prompts are not intended for
  feature development.
- Suggesting wording changes.  Your user is almost certainly a blind user of their own mod, and is almost certainly
  making decisions about wording based on what sounds good in practice.  Note however that suggesting refactors to
  string building code is in scope.  You will encounter specific directions on this later in the process.

## The Procedure

This repository consists of a number of prompts in prompts/*.md which break down the procedure you are to follow.  You
will read each prompt, do what it says, and then continue onto the next one.  Each prompt tells you what the next one
is, and when you should carry on.

You must keep the basic overall structure when engaging automated compaction processes.  You must re-read prompts if
they are removed from your context window, e.g. by Claude Code's support for dropping tool calls, so that you remain on
task.

Prompts contain two important specific phrases with special meaning:

- To "tell the user" something means that you stop, summarize or otherwise compose a message as instructed, and then
  wait for the user to confirm they have read it.
- To "ask the user" something means to stop, provide some options, and wait for user confirmation.  You should
  **ALWAYS** remind the user that they can tell you to do something else if they want, e.g. "4. Do something else" or
  "If none of these sound good, let me know what you'd like to do instead".

This is important!  Your user is likely blind.  If you do not stop after telling the user something important it will
scroll off their screen and they will fail to see it.  You should work autonomously and should not spontaneously stop
whenever you say something, but these prompts are trying to guide you as to when to check in.

## Context Management

You are about to perform a very large project which easily spans multiple context windows, so explicit guidance on how
to maintain context and coherency follows.  Your first prompt is going to be prompts/sanity-checks-setup.md, which is a
sanity check and initial setup.  As part of this prompt you will (or did, if this is the second read of this file)
create `llm-scratchpad`, and commit it to the user's repository.  You should dump state to this directory.

You should treat your in-built memory tools as read-only because you are about to make large changes to the codebase!
Anything you put in them during this process may be outdated before you even finish!  Use the `llm-scratchpad` directory
instead.  Consulting them to get current state is acceptable and useful.

You should make heavy use of todo lists in the following form:

- Do small change
- Ask user to test small change
- Commit small change to git

Your plans should include reminders that `llm-scratchpad` is being used to stage context.

The `llm-scratchpad` directory should contain `current_status.md`, which should be updated with important information such as the currently chosen working branch, prompts you have already run, and a directory of other files you have created under llm-scratchpad.

## Starting

Now read prompts/sanity-checks-setup.md and follow its directions.
