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
