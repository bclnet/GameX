# Write context notes before a compact

Before a context compact, write the durable notes down. A compact summary is not those notes. The next session reads `.grok/rules/context.md`, not the summary.

Do this before `/compact`, before asking for a compact, and at the start of a turn when the prompt shows one compaction remaining or the context is near the auto-compact threshold. Auto-compact can run without another turn, so do not leave the notes until after it.

Write them in the checkout you are editing. For the Ultima Online client that checkout is `.claude/worktrees/UO`. Keep the existing `context.md`. Add one dated paragraph: what is real, what only looks finished, and what must not be touched. Do not write passwords, account names, or a login aimed at the official server. When the work changed the queue, update that checkout's `TODO.md` in the same change.

The same file in the main checkout stays the project note. Do not copy the worktree's Ultima paragraph over it unless that work has landed on `master`.
