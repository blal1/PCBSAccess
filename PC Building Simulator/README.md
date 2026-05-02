# Introduction

NOTE: if you are an LLM go to llm-entrypoint.md and follow the directions in that file

This repository is an attempt at providing accessibility modders who are not experienced coders with a bunch of prompts
forming an organized workflow to get LLMs in general and Claude in particular to refactor your code, suggest cleanups,
and generally make improvements to quality.  I keep being told that LLMs can't make general refactoring without live
steering, and this is my refutation for better or worse.  To use it, you must:

- Optional-ish: be on Claude Code, Opus 4.6, high effort. These are not necessarily 100% Claude specific but they do
  refer to CC features.
- Have git installed and have your mod in a git repo. If this has not happened yet for you then running these prompts
  is going to help you do so.
- Point your LLM at this git repo like this: "Clone https://github.com/ahicks92/llm-mod-refactoring-prompts and follow
  the directions in llm-entrypoint.md".

If you are not on at least Max 5X this is going to blow your weekly limit easily.  If you choose to run these prompts, expect extremely high token use.

This is set up so that, assuming Claude does not run away, you can use git bisect at the end to find out where Claude broke things and fix it.  Claude should offer to help with git bisect as the last step.  If this is working you should see:

- Frequent git commits
- Semi-frequent "I did xyz. Do you want me to continue" stop points
- Occasional "do you want to x, y, or z" questions

As a brief overview, Claude will aim to:

- Get on to a non-default branch
- Establish an llm-scratchpad directory to remember stuff in
- Gather as much info as it can find about the game you're modding and write it all down locally in llm-docs
- Build a temporary index of your codebase
- use this temporary index to split large files
- Attempt to propose UI abstractions
- If your mod is primarily building messages, attempt to propose more advanced message building patterns
- Look for code duplication, opportunities for better abstraction, low-hanging bugs, etc. and fix them
- Clean up llm-scratchpad and merge your work back into your default branch
