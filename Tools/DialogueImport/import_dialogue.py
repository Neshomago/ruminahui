#!/usr/bin/env python3
"""Convert docs/04-dialogue-script.md into Assets/Resources/Dialogue/<mission>.json.

The markdown doc stays the source of truth: edit the doc, re-run this script, commit both.
Usage:  python3 Tools/DialogueImport/import_dialogue.py            (write files)
        python3 Tools/DialogueImport/import_dialogue.py --check    (fail if JSON is out of date with the doc)

Beat kinds:
  line      a spoken line            (speaker, direction, text)
  action    stage direction / scene description / "(Beat.)"
  gameplay  a [Gameplay: ...] note    (prompts = quoted ALL-CAPS prompt labels, e.g. STAY SILENT)
  qte       a [QTE: ...] note         (prompts as above)
  textcard  closing text-card line
"""
import json
import os
import re
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
DOC = os.path.join(ROOT, "docs", "04-dialogue-script.md")
OUT_DIR = os.path.join(ROOT, "Assets", "Resources", "Dialogue")

MISSION_RE = re.compile(r'^# (M\d\.\d) — "([^"]+)"\s*(.*)$')
HEADING_RE = re.compile(r'^\*\*(.+?)\*\*\s*(.*)$')
LINE_RE = re.compile(r'^> \*\*([^*:]+):\*\*\s*(?:\*\(([^)]*)\)\*)?\s*(.*)$')
# "[Gameplay: text]" / "[QTE: text]" strip the label; "[QTE-style choice, ...]" keeps its whole prose.
NOTE_RE = re.compile(r'^\*\[(Gameplay|QTE)(:?)\s*(.*?)\]\*?$', re.I)
# Prompt labels are quoted ALL-CAPS, sometimes with sentence punctuation inside the quotes: "RUN."
PROMPT_RE = re.compile(r'"([A-Z][A-Z ]*[A-Z])[.,!?]?"')

SPEAKER_NAMES = {"RUMIÑAHUI": "Rumiñahui", "PILLAHUASO": "Pillahuaso", "CAMP BOY 2": "Camp Boy 2"}


def file_key(mission_id):
    # "M0.1" -> "M0_1": avoids dotted asset names in Resources.Load (LESSONS_LEARNED L-019).
    return mission_id.replace(".", "_")


def clean(text):
    text = text.replace("**", "").replace("*", "")
    return re.sub(r"\s+", " ", text).strip()


def speaker_display(raw):
    raw = raw.strip()
    return SPEAKER_NAMES.get(raw, raw.title())


def note_beat(match):
    label, colon, body = match.group(1), match.group(2), match.group(3)
    # No colon → the label is part of the prose ("[Gameplay ends here …]", "[QTE-style choice …]"): keep it verbatim.
    raw = match.group(0)
    text = body if colon else raw[raw.index("[") + 1: raw.rindex("]")]
    prompts = list(dict.fromkeys(PROMPT_RE.findall(text)))  # unique, in order
    return {"kind": label.lower(), "speaker": "", "direction": "", "text": clean(text), "prompts": prompts}


def parse(doc_text):
    scripts = []
    script = scene = None

    def new_scene(heading):
        nonlocal scene
        scene = {"heading": clean(heading), "beats": []}
        script["scenes"].append(scene)

    def add(beat):
        if scene is None:
            new_scene("")
        beat.setdefault("prompts", [])
        scene["beats"].append(beat)

    for raw in doc_text.splitlines():
        line = raw.strip()
        if line.startswith("## "):            # PRODUCTION NOTES etc. — not dialogue data
            break
        m = MISSION_RE.match(line)
        if m:
            script = {"missionId": m.group(1), "docTitle": m.group(2).strip(),
                      "subtitle": clean(m.group(3)).strip("()"), "scenes": []}
            scripts.append(script)
            scene = None
            continue
        if script is None or not line or line == "---" or line == ">":
            continue
        if line == "**[END]**":
            script = None
            continue

        m = HEADING_RE.match(line)
        if m and not line.startswith("> "):
            new_scene(m.group(1).rstrip(":"))
            rest = m.group(2).strip()
            if rest:
                n = NOTE_RE.match(rest)
                add(note_beat(n) if n else {"kind": "action", "speaker": "", "direction": "", "text": clean(rest)})
            continue

        m = LINE_RE.match(line)
        if m:
            add({"kind": "line", "speaker": speaker_display(m.group(1)), "direction": clean(m.group(2) or ""),
                 "text": clean(m.group(3))})
            continue

        if line.startswith("> "):             # blockquote without a speaker = text card
            add({"kind": "textcard", "speaker": "", "direction": "", "text": clean(line[2:])})
            continue

        n = NOTE_RE.match(line)
        if n:
            add(note_beat(n))
            continue

        add({"kind": "action", "speaker": "", "direction": "", "text": clean(line)})

    # Stable ids: <mission>.<scene>.<beat>, e.g. "M5.6.2.03"
    for s in scripts:
        for si, sc in enumerate(s["scenes"], 1):
            for bi, b in enumerate(sc["beats"], 1):
                b["id"] = f'{s["missionId"]}.{si}.{bi:02d}'
                # field order for readable JSON
                sc["beats"][bi - 1] = {k: b[k] for k in ("id", "kind", "speaker", "direction", "text", "prompts")}
    return scripts


def render(script):
    return json.dumps(script, ensure_ascii=False, indent=2) + "\n"


def main():
    check = "--check" in sys.argv
    with open(DOC, encoding="utf-8") as f:
        scripts = parse(f.read())
    os.makedirs(OUT_DIR, exist_ok=True)
    stale = []
    for s in scripts:
        path = os.path.join(OUT_DIR, file_key(s["missionId"]) + ".json")
        content = render(s)
        existing = open(path, encoding="utf-8").read() if os.path.exists(path) else None
        if existing == content:
            continue
        if check:
            stale.append(path)
        else:
            with open(path, "w", encoding="utf-8") as f:
                f.write(content)
        lines = sum(1 for sc in s["scenes"] for b in sc["beats"] if b["kind"] == "line")
        print(f'{"STALE" if check else "wrote"} {os.path.relpath(path, ROOT)}  ({len(s["scenes"])} scenes, {lines} lines)')
    if check and stale:
        print("Dialogue JSON is out of date with docs/04-dialogue-script.md — run the importer.")
        sys.exit(1)
    if not stale and check:
        print("Dialogue JSON up to date.")


if __name__ == "__main__":
    main()
