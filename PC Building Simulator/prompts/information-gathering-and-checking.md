# Overview and Motivation

In this stage of the process you will verify that the information you are working from is accurate and gather more
information as necessary.  As an LLM you benefit from accurate, complete information, and so this is your opportunity to
find it.

**Compaction note:** If you are re-reading this prompt after context compaction, check `llm-scratchpad/current_status.md`
for where you left off.  Do not repeat steps you have already completed.

# Procedure

## Documentation Subagent Stage

You will do this by spawning a number of sonnet subagents and then combining their info:

### Subagent 1: validate CLAUDE.md

CLAUDE.md consists of a number of usually discrete factoids such as "x is here", or "we do y like z".  You should spawn
a subagent which will check it for accuracy and make updates as necessary. The subagent should:

- Check and remove any "directories" of files.  For example, a common anti-pattern is to place a snapshot of every file
  without comment in CLAUDE.md, which is only helpful until a new file is added or an old file is removed.
- Attempt to remove any machine specific paths in favor of explanations of where to find the information, e.g.
  `%appdata%` instead of `c:/users/myuser`.
- Build a list of every verifyable fact in CLAUDE.md, then run down that list checking them off and:
  - No longer valid knowledge gets removed;
  - Mostly valid knowledge gets corrected;
  - Anything that is still true gets left alone

In particular CLAUDE.md needs to contain:

- Build directions. You may need to get this from the user. If so, this subagent should report up that you need to open
  a discussion.
- If this mod is using a particular mod loader such as TModLoader or MelonLoader, what that loader is
- The name of the game being modded
- Basic UI paradigms and a conceptual overview of the game

### Subagent 2...n: Documentation Gatherers

You will use the remaining parallel subagents to gather information by sending them after various specific pieces of
information as follows. They should write intermediate results somewhere under `llm-scratchpad`.  You should then
synthesize this to a new or existing `llm-docs` directory at the top level after they finish executing, and notate it in
CLAUDE.md.

Not all of the following information categories are useful.  What is beneficial varies per game, as well as on your
understanding in your training data.  You need to use your best judgement to figure out what to spawn and how many.
Information sources you may wish to review are as follows:

- Files on the player's machine, for example some games have all of their game logic in Lua
- Game walkthroughs
- Official or player-maintained game wikis
- If the game supports modding explicitly, official API documentation or resources
- If you cannot find enough information, game reviews or screenshots

Your goal is to synthesize this into a model of the game which will allow you to conceptualize it.  This includes for
example:

- Control schemes
- "space" info: is it a hex grid? A tile grid? Free space?  A card game without space at all?
- Basic mechanics: does the game have HP? How does aiming work ?
- An enumeration of game screens and what is on them
- AN official or unofficial API reference


When these subagents finish you should divide the information into three categories:

- Things you already knew/don't need.  Throw this out.
- Things which require synthesizing across multiple sources or are hard to figure out: build documents in llm-docs
- Things which are easy to find, e.g. files on the local machine: work references to them into llm-docs or CLAUDE.md as
  appropriate

One thing requires special note: API references.  It is sometimes possible to get an official or unofficial API
reference.  For example Factorio has an official JSON-readable API which can be converted to markdown with some
scripting, and Terraria has player-maintained documentation for TModLoader.  One extremely valuable output of this
process is a directory of markdown files, one per class/module, which you can search in future to find game APIs.  You
should not go as far as trying to make one from scratch, but you should spend significant time figuring out if you can
either find an API reference, or if you can sketch one out by reading source code from other mods.

## Finalization Stage

In this stage, you will:

- Work the docs into the root CLAUDE.md
- Write llm-docs/CLAUDE.md containing an overview of what is in llm-docs, in line with Anthropic's progressive disclosure
- Clean up the intermediate artifacts from the subagents in `llm-scratchpad` if they do not seem useful.
- Update `llm-scratchpad/current_status.md` to remember that you now have docs.
- Make three commits:
  - One with the CLAUDE.md updates
  - One with `llm-docs`
  - One with `llm-scratchpad`
- Stop and consider if you need to ask the user for clarification on any points. If you do:
  - Ask them
  - Work it into the docs where appropriate
  - Make a commit "Clarified xyz from the user and..."
- Summarize everything that was done for the user, stop, and await user confirmation

# Up Next

Once finished, read `code-directory-construction.md`.
