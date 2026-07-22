#!/usr/bin/env bash
# Audit des réponses HTTP 500 dans les contrôleurs ProsocAPI.
# Génère un rapport CSV et affiche un résumé (non bloquant en CI).
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CONTROLLERS_DIR="${ROOT}/Controllers"
REPORT_DIR="${ROOT}/audit-reports"
CSV_FILE="${REPORT_DIR}/500-errors.csv"
SUMMARY_FILE="${REPORT_DIR}/500-errors-summary.txt"

mkdir -p "${REPORT_DIR}"

if ! command -v rg >/dev/null 2>&1; then
  echo "Erreur: ripgrep (rg) est requis." >&2
  exit 1
fi

classify_line() {
  local rest="$1"
  if echo "${rest}" | rg -q 'Erreur interne du serveur'; then
    echo "generic_string"
  elif echo "${rest}" | rg -q 'CreateTechnicalError|errorResponse'; then
    echo "structured"
  elif echo "${rest}" | rg -q 'new \{ error'; then
    echo "anonymous_error"
  elif echo "${rest}" | rg -q 'new \{ message'; then
    echo "anonymous_message"
  elif echo "${rest}" | rg -q 'new \{ Message'; then
    echo "anonymous_message_capitalized"
  else
    echo "other"
  fi
}

echo "file;line;pattern" > "${CSV_FILE}"

generic_count=0
structured_count=0
anonymous_error_count=0
anonymous_message_count=0
other_count=0

while IFS= read -r row; do
  file="${row%%:*}"
  remainder="${row#*:}"
  line="${remainder%%:*}"
  rest="${remainder#*:}"
  pattern="$(classify_line "${rest}")"
  rel_file="${file#${ROOT}/}"
  echo "${rel_file};${line};${pattern}" >> "${CSV_FILE}"

  case "${pattern}" in
    generic_string) generic_count=$((generic_count + 1)) ;;
    structured) structured_count=$((structured_count + 1)) ;;
    anonymous_error) anonymous_error_count=$((anonymous_error_count + 1)) ;;
    anonymous_message|anonymous_message_capitalized) anonymous_message_count=$((anonymous_message_count + 1)) ;;
    other) other_count=$((other_count + 1)) ;;
  esac
done < <(rg 'StatusCode\(500' "${CONTROLLERS_DIR}" -n --no-heading --glob '*.cs')

total=$((generic_count + structured_count + anonymous_error_count + anonymous_message_count + other_count))

catch_unused_ex_count="$(rg -U 'catch \(Exception ex\)[\s\S]{0,300}StatusCode\(500,\s*"Erreur interne du serveur"\)' "${CONTROLLERS_DIR}" --glob '*.cs' --count-matches 2>/dev/null | awk -F: '{s+=$2} END {print s+0}' || true)"
technical_error_count="$(rg 'TechnicalErrorResponse\(' "${CONTROLLERS_DIR}" --glob '*.cs' --count-matches 2>/dev/null | awk -F: '{s+=$2} END {print s+0}' || true)"

{
  echo "=== Audit erreurs HTTP 500 — $(date -u +"%Y-%m-%dT%H:%M:%SZ") ==="
  echo ""
  echo "Total StatusCode(500): ${total}"
  echo "  - generic_string (\"Erreur interne du serveur\"): ${generic_count}"
  echo "  - anonymous_error ({ error: ... }): ${anonymous_error_count}"
  echo "  - anonymous_message ({ message/Message: ... }): ${anonymous_message_count}"
  echo "  - structured (CreateTechnicalError / errorResponse): ${structured_count}"
  echo "  - other: ${other_count}"
  echo ""
  echo "Catch (Exception ex) avec réponse générique (ex ignoré): ${catch_unused_ex_count}"
  echo "TechnicalErrorResponse (format structuré): ${technical_error_count}"
  echo ""
  echo "Top 10 contrôleurs par nombre de StatusCode(500):"
  rg 'StatusCode\(500' "${CONTROLLERS_DIR}" --glob '*.cs' --count-matches 2>/dev/null \
    | sort -t: -k2 -nr \
    | head -10 \
    | awk -v root="${ROOT}/" -F: '{sub(root, "", $1); printf "  %3s  %s\n", $2, $1}'
  echo ""
  echo "Contrôleurs héritant de BaseApiController (pagination générique):"
  rg ': BaseApiController' "${CONTROLLERS_DIR}" -l 2>/dev/null | wc -l | awk '{print "  " $1 " fichiers"}'
  echo ""
  echo "Usages CreatePaginatedResponseAsync / CreateExtendedPaginatedResponseAsync:"
  rg 'CreatePaginatedResponseAsync|CreateExtendedPaginatedResponseAsync' "${CONTROLLERS_DIR}" --count-matches 2>/dev/null \
    | awk -F: '{s+=$2} END {print "  " s+0 " appels dans " NR " fichiers"}'
  echo ""
  echo "Rapport détaillé: ${CSV_FILE}"
} | tee "${SUMMARY_FILE}"

echo ""
echo "⚠️  ${generic_count} réponses 500 génériques détectées (${catch_unused_ex_count} avec catch ex non exposé)."
echo "    Voir ${SUMMARY_FILE} pour le détail."

exit 0
