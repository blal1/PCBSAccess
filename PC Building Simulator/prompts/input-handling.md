# Motivation and Overview

**Compaction note:** If you are re-reading this prompt after context compaction, check `llm-scratchpad/current_status.md`
for where you left off.  If you have already completed input/UI refactoring, skip to the "Up Next" section.

This stage deals with a particular antipattern that arises in most game code/mod code: someone starts with an input
system and then needs it to do more and before you know it there are thousands of lines of spaghetti.  This stage of the
cleanup is about attempting to identify this, and if it can easily be fixed.  This prompt does not offer a fixed
procedure but instead guidance on what does and does not work.  You should read this prompt, explore the codebase,
summarize your findings to the user, and offer them suggestions on changes.  If the user indicates they wish to
proceed, you should take the chosen alternative and turn it into a plan using plan mode, then execute on that plan
before returning to the prompt chain.

It is critically important to get user sign-off, as these are going to be a large set of changes.

# Background: The Input Problem in Accessibility Mods

Here is a summary of the problem, as faced by accessibility mods:

- The game may or may not support the keyboard
- The game may or may not itself have a unified GUI system, and even if it does there are usually exceptions
- The mod is going to need to deal with having various keys that are enabled/disabled on conditions
- The mod may or may not yet have some sort of abstraction for any of this

So, in the easiest case a mod's input problem is "the game already supports keyboard and has a concept of focus, so we
just read when focus changes".  This never happens in practice.  In the hardest case the problem can be "we
have no access to the input or GUI systems at all, and have to write our own GUI framework, and also we had to add
keyboard support".

For accessibility mods there is also little difference between "input system" and "ui system", as there is generally no
drawn UI, and a menu is simply responding to keys by changing the index of the menu item or similar.

# Real-World Examples

Here are a few concrete examples of how this has played out in practice:

- Say the Spire.  Say the Spire had the "no keyboard support" problem.  It solved it by faking out keyboard actions as
  gamepad buttons.
- Factorio Access.  Factorio Access runs Lua internally in a way where no access to the game GUI is possible, and
  additionally adds various mod rules that resulted in needing a full React-inspired talking GUI framework.
- Against the Storm.  Against the Storm started by trying to integrate with the game's GUIs, and this worked for a
  while.  But bugs, coupled with those GUIs only really being friendly to sighted players, eventually resulted in a
  number of bespoke menus, and eventually the mod decided to simply write its own.  Because it is in C# and does not
  have weird rules like Factorio, however, it was able to do so using traditional OOP paradigms.

# Design Considerations

In practice almost any good system is going to look like the standard input handler stack in any of its various
manifestations, or just having simple event handler functions.  Your job here is to decide how far to go and propose it
to the user, if necessary.  If the user already has something you can again shortcut past a lot of this and move on.

## Key Questions to Answer

- Is keyboard support present in the game already and if not how is the mod adding it?  If the mod isn't adding it what
  did the mod do instead? (usually this is "we only support gamepad for now and want to add it later", but clarification
  with the user is encouraged)
- How much complexity is needed? For example a game like Slay the Spire where one is simply selecting cards only really
  needs a couple levels, and the levels don't interact, but a city builder can very easily turn into "I have this menu
  open but still need those 5 info keys to be able to announce stuff".

## Event Bubbling

In most non-mod input systems, defaulting unhandled events to not bubble is the right answer, but in mods that question
is not so simple. If the game has a lot of info keys, locking the user out of using them while various menus or modes
are enabled is not necessarily the good option.

## Search and Navigation

Note one important detail: most complex UIs will eventually want to offer searching in some sort of universal fashion.
If this is going to happen in this case, something to ask the user about, you should consider that in your design.  In
particular:

- It will be necessary to get the label of, and jump the focus to, gui elements/menu items
- It will be necessary to make some sort of talking textbox or typeahead overlay

# Procedure

- Explore the codebase taking the above info into account. Find out if a good UI/input abstraction exists already.
- Decide if a new one is needed and if so how advanced it needs to be.  Ask the user questions as necessary to find out
  what the future holds.
- Design it and propose a plan.  Make sure to remove the old one as part of this plan.
- Iteratively ask the user to test the result and fix the bugs that they find.
- Commit it and continue to the next prompt in the chain.

As always, when done, summarize to the user and confirm they want to proceed.

# Up Next

Next, read `prompts/string-builder.md`
