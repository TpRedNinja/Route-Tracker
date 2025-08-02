# Rules you copilot must follow when generating code

1. **NEVER REMOVE EXISTING FEATURES** unless I specifically ask you to.
2. **GENERATE ALL CODE.**
   - Ignore this rule only for files longer than 300 lines unless I specify otherwise.
   - For large files, generate one function at a time.
   - If multiple functions are changed, generate them one by one.
   - For single-line or small multi-line changes outside functions, generate each one individually.
3. **NO TLDR-style placeholders** like `// rest of code goes here`. Complete all code blocks.
4. Generate the most **optimized** code possible,
   - UNLESS optimization requires altering core logic or removing features.
5. **Ask questions** after I give you instructions. If you’re unsure about anything, ask.
   - DO NOT assume anything—ever.
6. **DO NOT trigger IDE suggestions** like “collection can be simplified.” Make your output match what the IDE would recommend.
7. When given a `#solution`, always read and evaluate **all** the provided code first, before filtering or discarding files.
   - You often miss important logic if you skip this step.
8. **Use existing formatting, naming, and comment styles.**
   - Match what’s in the codebase. Don’t bring in a different structure or tone.
9. **If forced to assume due to lack of info,** mark the assumption in code with:
   ```csharp
   // WARNING: Assumed [explanation]
	```
10. **Always specify where generated code belongs within `#region` blocks.**
    - If inserting into an existing region, clearly state **where**:
      - After a specific function
      - Before a specific function
      - At the start or end of the region
    - Be specific to avoid confusion.
11. **If the code should go into a *new* region, say so explicitly.**
    - Give the new region a meaningful name.
    - Make sure it logically fits with surrounding code and doesn't duplicate existing regions.
    - generate the code with the region included.
    - generate the full region, not just the code inside it.
12. if a function isnt in a region and you think u can add it to a new region then do so and say so
13. NEVER generate a single if statement without braces.
    - Always use braces `{}` for clarity and maintainability.