"""Shared report helpers for Vortex Blender tools."""

from __future__ import annotations

from dataclasses import dataclass, field
from datetime import datetime, timezone
from pathlib import Path
from typing import Iterable


@dataclass
class CheckResult:
    name: str
    ok: bool
    detail: str = ""
    level: str = "error"  # error | warn | info

    def line(self) -> str:
        if self.ok:
            mark = "PASS"
        elif self.level == "warn":
            mark = "WARN"
        else:
            mark = "FAIL"
        extra = f" — {self.detail}" if self.detail else ""
        return f"[{mark}] {self.name}{extra}"


@dataclass
class Report:
    title: str
    checks: list[CheckResult] = field(default_factory=list)
    notes: list[str] = field(default_factory=list)

    def add(self, name: str, ok: bool, detail: str = "", level: str = "error") -> CheckResult:
        c = CheckResult(name, ok, detail, level if not ok else "info")
        self.checks.append(c)
        return c

    def extend(self, items: Iterable[CheckResult]) -> None:
        self.checks.extend(items)

    @property
    def passed(self) -> int:
        return sum(1 for c in self.checks if c.ok)

    @property
    def failed(self) -> int:
        return sum(1 for c in self.checks if not c.ok and c.level == "error")

    @property
    def warnings(self) -> int:
        return sum(1 for c in self.checks if not c.ok and c.level == "warn")

    def to_text(self) -> str:
        lines = [
            f"# {self.title}",
            f"Generated: {datetime.now(timezone.utc).strftime('%Y-%m-%d %H:%M:%S UTC')}",
            "",
            f"Summary: PASS={self.passed} FAIL={self.failed} WARN={self.warnings}",
            "",
        ]
        for c in self.checks:
            lines.append(c.line())
        if self.notes:
            lines.append("")
            lines.append("## Notes")
            for n in self.notes:
                lines.append(f"- {n}")
        lines.append("")
        return "\n".join(lines)

    def write(self, path: Path) -> Path:
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(self.to_text(), encoding="utf-8")
        return path
