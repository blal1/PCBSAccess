# Overview and Motivation

**Compaction note:** If you are re-reading this prompt after context compaction, check `llm-scratchpad/current_status.md`
for where you left off.  If you have already determined this is not a string builder mod, skip to the "Up Next" section.

This is not about simply writing a string builder and using it.  This is about a particular kind of mod, and what that means.  In this stage, you should identify whether you are looking at a string builder mod or not.

A "string builder" mod, a term invented for this procedure which is not widely accepted so far, is a mod whose job is to build strings.  For example in Factorio Access, 75%+ of the code is "extract this piece of information about something, and append it to the big message we are building".

String builders often become string builders without realizing it.  A lot of people go in thinking the big problem is menus, but really the big problem is ending up with pages and pages and pages of:

```
let value = get_the_value_somehow();
message = message + " " + value;
// repeat 20 times
```

You see this in things like city builders, where even the UI labels are "combine 5 pieces of dynamic info into a screen reader friendly string".

This has a couple problems.  The first is getting spaces right. The second is repetition and composability. For example how do you split this into functions?  Should the caller or the callee add the space?  What about other separators such as "," or "."?

# Guidance

There are two steps here: identifying if this is a string builder, then proposing a resolution.

To identify if this is a string builder, I suggest two passes:

- In pass 1: use Haiku subagents on every file to find out how much of the file is building strings, as a percent and total line count
- In pass 2: if the total percent is over 25% or so, or if there are thousands of lines of string building, evaluate the code yourself to see if the mod falls into the category of string builder mod.

To clean up a string builder, you want to end up with something like this:

```
let msg = builder()
msg.fragment("hello").fragment("myname").list_item("bob").list_item("jim");
```

Which might turn into "hello myname bob, jim".  Or:

```
let builder = builder();
builder.push_comma() // no-ops, nothing else yet and push_comma is smart
.push_fragment("hello") // fine, push a fragment
.push_comma() // actually pushes ", "
.push_comma() // Does nothing, we guard against double comma and no-op it so these can be passed to functions
.push_fragment("there")
```

Which might become "hello, there".

In essence you are deciding between 4 strategies:

- The first example above, call it a semantic builder: the builder doesn't expose comma, it exposes a concept of lists and fragments. Tradeoff: more LLM friendly in some ways, but harder for a human reader to follow, and sometimes without as much control as one might like.
- The second example above, the low-level builder: the user pushes punctuation and it guards against duplicates, space is implicitly put after every call
- Just forgoing all this and writing a variadic `smart_join(...)` function that joins with some cleanup
- Or deciding that even though it is a string builder, it's not fixable

To make this fit you may need to bend the "don't propose wording changes" rules laid out in llm-entrypoint.md.  This is the one place where it is acceptable to break those rules, but you should let the user know how much that might change and give them a chance to interrupt.

# Procedure

Using the above guidance:

- Identify whether this is a string builder mod. If it isn't, go to the next prompt.
- Determine if a string/message builder abstraction is applicable and would make the code cleaner
- Design the abstraction
- Enumerate all places you want to roll the abstraction out
- Turn this into a plan, committing frequently.  The plan should include when to commit; "after every file" is a good choice, "after specific important functions" is a good choice; etc.
- Confirm the plan with the user and run it
- Ask the user to use the mod for a while to see what breaks
- Commit any final changes or fixes based on user testing, then move to the next prompt

# Up Next

Next, read `prompts/low-level-cleanup.md`
