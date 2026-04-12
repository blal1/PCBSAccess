# Unity Accessibility Modding Book Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create a comprehensive, 14-chapter educational book on Unity accessibility modding in English and German.

**Architecture:** A progressive learning path divided into six parts, from foundation to community distribution, centered on a modular framework.

**Tech Stack:** Markdown, C#, Unity, BepInEx, MelonLoader.

---

### Task 1: Infrastructure & Table of Contents

**Files:**
- Create: `docs/BOOK_OF_ACCESSIBILITY.md`
- Create: `docs-de/BUCH_DER_BARRIEREFREIHEIT.md`

- [ ] **Step 1: Create English ToC**
Write the main index with links to all 14 chapters.

- [ ] **Step 2: Create German ToC**
Translate the index and link to the German chapters.

- [ ] **Step 3: Commit**
```bash
git add docs/BOOK_OF_ACCESSIBILITY.md docs-de/BUCH_DER_BARRIEREFREIHEIT.md
git commit -m "book: initialize table of contents"
```

### Task 2: Part I - The Foundation (Chapters 1-2)

**Files:**
- Create: `docs/book/chapter-1.md` (Modding Ecosystem)
- Create: `docs/book/chapter-2.md` (Setting Up Your Laboratory)

- [ ] **Step 1: Write Chapter 1**
Detail Unity, BepInEx, MelonLoader, and Tolk.

- [ ] **Step 2: Write Chapter 2**
Convert `setup-guide.md` content into a narrative chapter with explicit CLI steps.

- [ ] **Step 3: Commit**
```bash
git add docs/book/chapter-1.md docs/book/chapter-2.md
git commit -m "book: implement chapters 1 and 2"
```

### Task 3: Part II - The Architecture (Chapters 3-5)

**Files:**
- Create: `docs/book/chapter-3.md` (ScreenReader)
- Create: `docs/book/chapter-4.md` (State Management)
- Create: `docs/book/chapter-5.md` (Extraction & Reflection)

- [ ] **Step 1: Write Chapter 3**
Incorporate `ScreenReader.cs.template` logic and the "Say vs. SayQueued" philosophy.

- [ ] **Step 2: Write Chapter 4**
Detail the `AccessStateManager` and context-aware input.

- [ ] **Step 3: Write Chapter 5**
Document `UITextExtractor` and `ReflectionHelper` with full code examples.

- [ ] **Step 4: Commit**
```bash
git add docs/book/chapter-3.md docs/book/chapter-4.md docs/book/chapter-5.md
git commit -m "book: implement chapters 3, 4, and 5"
```

### Task 4: Part III - The Art of Discovery (Chapters 6-7)

**Files:**
- Create: `docs/book/chapter-6.md` (Reading the Matrix)
- Create: `docs/book/chapter-7.md` (Identifying Game Logic)

- [ ] **Step 1: Write Chapter 6**
Techniques for non-visual code navigation in dnSpy.

- [ ] **Step 2: Write Chapter 7**
How to find the "Source of Truth" in a decompile.

- [ ] **Step 3: Commit**
```bash
git add docs/book/chapter-6.md docs/book/chapter-7.md
git commit -m "book: implement chapters 6 and 7"
```

### Task 5: Part IV - Implementation & Patterns (Chapters 8-10)

**Files:**
- Create: `docs/book/chapter-8.md` (Mastering Menus)
- Create: `docs/book/chapter-9.md` (Living in the World)
- Create: `docs/book/chapter-10.md` (Advanced Interception)

- [ ] **Step 1: Write Chapter 8**
Navigation patterns and the accessibility checklist.

- [ ] **Step 2: Write Chapter 9**
Spatialized audio and world exploration.

- [ ] **Step 3: Write Chapter 10**
Deep dive into Harmony patches.

- [ ] **Step 4: Commit**
```bash
git add docs/book/chapter-8.md docs/book/chapter-9.md docs/book/chapter-10.md
git commit -m "book: implement chapters 8, 9, and 10"
```

### Task 6: Part V - Going Global (Chapters 11-12)

**Files:**
- Create: `docs/book/chapter-11.md` (The Polyglot Mod)
- Create: `docs/book/chapter-12.md` (Hooking Native Localization)

- [ ] **Step 1: Write Chapter 11**
Building the `Loc` system.

- [ ] **Step 2: Write Chapter 12**
Syncing with the game's `LocalizationManager`.

- [ ] **Step 3: Commit**
```bash
git add docs/book/chapter-11.md docs/book/chapter-12.md
git commit -m "book: implement chapters 11 and 12"
```

### Task 7: Part VI - Deployment & Beyond (Chapters 13-14)

**Files:**
- Create: `docs/book/chapter-13.md` (The Finishing Touches)
- Create: `docs/book/chapter-14.md` (Distribution)

- [ ] **Step 1: Write Chapter 13**
Optimization and avoiding silent degradation.

- [ ] **Step 2: Write Chapter 14**
Packaging and community.

- [ ] **Step 3: Commit**
```bash
git add docs/book/chapter-13.md docs/book/chapter-14.md
git commit -m "book: implement chapters 13 and 14"
```

### Task 8: German Translation

**Files:**
- Create: `docs-de/book/kapitel-1.md` through `kapitel-14.md`

- [ ] **Step 1: Translate Chapters 1-14**
Produce the full German version of the book.

- [ ] **Step 2: Commit**
```bash
git add docs-de/book/
git commit -m "book: translate all chapters to German"
```

### Task 9: Final Verification

- [ ] **Step 1: Verify all internal links**
Ensure the ToC and cross-chapter links function.

- [ ] **Step 2: Consistency Check**
Verify that code snippets in the book match the `templates/` folder exactly.

- [ ] **Step 3: Final Commit**
```bash
git add .
git commit -m "chore: final book verification and link repair"
```
