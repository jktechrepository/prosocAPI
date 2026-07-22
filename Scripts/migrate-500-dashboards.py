#!/usr/bin/env python3
"""Migre les StatusCode(500) anonymes vers this.TechnicalErrorResponse."""
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent / "Controllers"

SIMPLE_PATTERNS = [
    (
        re.compile(r'return StatusCode\(500, new \{ error = "([^"]+)" \}\);'),
        r'return this.TechnicalErrorResponse("\1", ex);',
    ),
    (
        re.compile(r'return StatusCode\(500, new \{ message = "([^"]+)" \}\);'),
        r'return this.TechnicalErrorResponse("\1", ex);',
    ),
    (
        re.compile(
            r'return StatusCode\(500, new \{ Message = "([^"]+)", Error = ex\.Message \}\);'
        ),
        r'return this.TechnicalErrorResponse("\1", ex);',
    ),
    (
        re.compile(r'return StatusCode\(500, new \{ Message = "([^"]+)" \}\);'),
        r'return this.TechnicalErrorResponse("\1", ex);',
    ),
]

SPECIAL_REPLACEMENTS = [
    (
        'return StatusCode(500, ex.Message);',
        'return this.TechnicalErrorResponse("Erreur interne du serveur", ex);',
    ),
    (
        'return StatusCode(500, $"Erreur lors de la récupération des affiliés: {ex.Message}");',
        'return this.TechnicalErrorResponse("Erreur lors de la récupération des affiliés", ex);',
    ),
]

SKIP_SUFFIXES = ('.bak',)

changed_files = 0
total_replacements = 0

for path in sorted(ROOT.glob('*.cs')):
    if path.name.endswith(SKIP_SUFFIXES):
        continue

    text = path.read_text(encoding='utf-8')
    original = text

    for pattern, repl in SIMPLE_PATTERNS:
        text, n = pattern.subn(repl, text)
        total_replacements += n

    for old, new in SPECIAL_REPLACEMENTS:
        count = text.count(old)
        if count:
            text = text.replace(old, new)
            total_replacements += count

    if text != original:
        path.write_text(text, encoding='utf-8')
        changed_files += 1
        print(f"  {path.name}")

print(f"\n{total_replacements} remplacements dans {changed_files} fichiers")
