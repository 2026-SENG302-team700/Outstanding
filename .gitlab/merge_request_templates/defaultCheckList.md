## Dev Checklist

- [ ] Unit tests are present and pass
- [ ] Integration tests are present and pass
- [ ] Manual tests are created and tested
- [ ] No code Smells
  - Readable and understandable
- [ ] Relevant Docstrings
- [ ] Affected ACs are included in the MR description.

## Reviewer Checklist

- [ ] No code Smells
  - Readable and understandable
- [ ] Unit Tests
  - Is there enough Unit tests and do they all Pass?
- [ ] Integration Tests
  - Is there enough Integration tests and do they all Pass.
- [ ] Relevant Docstrings
  - Do all methods have Docstring
- [ ] Passes Acceptance Criteria
  - Do all of the AC touched in this branch pass the specification from the sprint backlog
- [ ] Protected against sneaky inputs or malicious activity
  - Is the app protected against SQL injections, HTML injections and edge case inputs (e.g. whitespaces are trimmed)

### NFR Checklist (Reviewer)

- [ ] **NFR 2: Accessible and consistent UI**
  - Colours and fonts remain consistent across pages.
  - The application is responsive across screen sizes (mobile → desktop).
  - Menus, buttons, links, and input fields behave consistently across pages (placement, interaction, and labelling).

- [ ] **NFR 3: User-friendly and fool-proof interaction**
  - Required fields and input formats are clearly indicated.
  - Invalid inputs or errors are visually highlighted to help users correct them.
  - All form errors are shown at once during submission.
  - Destructive actions (e.g., deletion) require explicit confirmation.
  - User input is preserved after errors (except for passwords).

- [ ] **NFR 4: Character support**
  - The product accepts all valid characters, including accented letters (e.g., Māori) and emojis in open text fields.

- [ ] **NFR 5: Keyboard navigation and accessibility**
  - Pressing **Tab** moves users sequentially through interactive elements to support keyboard navigation and screen readers.

- [ ] **NFR 6: Locale-aware formatting**
  - The system displays formatted data (e.g., dates and times) according to the user’s locale.
