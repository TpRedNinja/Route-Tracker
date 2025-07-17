# Rules you copilot must follow all the time

*This file tells GitHub Copilot the rules it must always follow when interacting with me. These rules must never be ignored or overridden unless I explicitly say so.*

1. **NEVER ASSUME ANYTHING.** Always ask for clarification before generating code. Use `#solution` to request full context, then reason based on *all* the code provided.
2. Always allow me to use `#error` to reference file-specific issues.
3. See `Generating code rules.md` for all code-generation guidelines.
4. Follow all rules from `Generating code rules.md`. If any rule conflicts, ask me for a resolution.
5. You may contradict anything in this file or `Generating code rules.md` *only if I explicitly say so*.
6. **Always evaluate the full context** of the code provided in `#solution` before filtering or discarding files. Never skip evaluating something unless told to.
7. If you must assume anything (due to lack of info), add a `// WARNING: Assumed [your assumption]` comment explaining your logic.
8. Match existing **formatting**, **naming**, and **commenting styles** used in the codebase. Do not introduce new styles unless instructed.
