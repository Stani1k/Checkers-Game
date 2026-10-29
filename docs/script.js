(() => {
  const root = document.documentElement;
  document.getElementById("theme").addEventListener("click", () => {
    const next = root.dataset.theme === "light" ? "dark" : "light";
    root.dataset.theme = next;
    localStorage.setItem("theme", next);
  });

  document.querySelector("[data-copy]").addEventListener("click", async (e) => {
    const text = e.target.parentElement.querySelector("code").innerText;
    try { await navigator.clipboard.writeText(text); e.target.textContent = "Copied!"; }
    catch { e.target.textContent = "Press Ctrl+C"; }
    setTimeout(() => (e.target.textContent = "Copy"), 1500);
  });

  const N = 8;
  const initial = () => {
    const g = Array.from({ length: N }, () => Array(N).fill(null));
    for (let r = 0; r < N; r++)
      for (let c = 0; c < N; c++) {
        if ((r + c) % 2 === 0) continue;
        if (r < 3) g[r][c] = { s: "b", k: false };
        else if (r > 4) g[r][c] = { s: "r", k: false };
      }
    return g;
  };
  const inside = (r, c) => r >= 0 && r < N && c >= 0 && c < N;

  function jumps(g, r, c) {
    const p = g[r][c], out = [];
    for (const dr of p.k ? [-1, 1] : [p.s === "r" ? -1 : 1])
      for (const dc of [-1, 1]) {
        const or = r + dr, oc = c + dc, tr = r + 2 * dr, tc = c + 2 * dc;
        if (inside(tr, tc) && g[or][oc] && g[or][oc].s !== p.s && !g[tr][tc])
          out.push({ r: tr, c: tc, over: [or, oc] });
      }
    return out;
  }
  function steps(g, r, c) {
    const p = g[r][c], out = [];
    for (const dr of p.k ? [-1, 1] : [p.s === "r" ? -1 : 1])
      for (const dc of [-1, 1])
        if (inside(r + dr, c + dc) && !g[r + dr][c + dc]) out.push({ r: r + dr, c: c + dc });
    return out;
  }
  function allMoves(g, side) {
    const caps = [], plain = [];
    for (let r = 0; r < N; r++)
      for (let c = 0; c < N; c++) {
        if (g[r][c]?.s !== side) continue;
        jumps(g, r, c).forEach((m) => caps.push({ from: [r, c], ...m }));
        steps(g, r, c).forEach((m) => plain.push({ from: [r, c], ...m }));
      }
    return caps.length ? caps : plain;
  }

  function render(el, g, state = {}) {
    el.replaceChildren();
    for (let r = 0; r < N; r++)
      for (let c = 0; c < N; c++) {
        const sq = document.createElement("div");
        sq.className = "sq" + ((r + c) % 2 ? " dark" : "");
        sq.dataset.r = r; sq.dataset.c = c;
        if (state.sel && state.sel[0] === r && state.sel[1] === c) sq.classList.add("sel");
        if (state.hints?.some((m) => m.r === r && m.c === c)) sq.classList.add("hint");
        const p = g[r][c];
        if (p) {
          const d = document.createElement("div");
          d.className = "piece " + p.s;
          if (p.k) d.textContent = "♛";
          sq.append(d);
        }
        el.append(sq);
      }
  }

  render(document.getElementById("heroBoard"), initial());

  const boardEl = document.getElementById("board");
  const statusEl = document.getElementById("status");
  let g, turn, sel, locked, over;

  function reset() {
    g = initial(); turn = "r"; sel = null; locked = false; over = false;
    statusEl.textContent = "Your move.";
    draw();
  }
  function movesFor(r, c) {
    const all = allMoves(g, turn).filter((m) => m.from[0] === r && m.from[1] === c);
    return locked && !(sel && sel[0] === r && sel[1] === c) ? [] : all;
  }
  function draw() {
    render(boardEl, g, { sel, hints: sel ? movesFor(...sel) : [] });
  }
  function apply(m) {
    const [fr, fc] = m.from, p = g[fr][fc];
    g[fr][fc] = null; g[m.r][m.c] = p;
    let captured = false;
    if (m.over) { g[m.over[0]][m.over[1]] = null; captured = true; }
    const crowned = !p.k && (p.s === "r" ? m.r === 0 : m.r === N - 1);
    if (crowned) p.k = true;
    return captured && !crowned && jumps(g, m.r, m.c).length > 0;
  }
  function checkEnd() {
    if (allMoves(g, turn).length) return false;
    over = true;
    statusEl.textContent = turn === "r" ? "No moves left — the computer wins." : "You win! 🎉";
    return true;
  }
  function endTurn() {
    turn = turn === "r" ? "b" : "r"; sel = null; locked = false;
    draw();
    if (checkEnd()) return;
    if (turn === "b") { statusEl.textContent = "Computer is thinking…"; setTimeout(aiMove, 600); }
    else statusEl.textContent = allMoves(g, "r")[0]?.over ? "Your move — you must capture." : "Your move.";
  }
  function aiMove() {
    const pick = () => {
      const ms = allMoves(g, "b");
      return ms[Math.floor(Math.random() * ms.length)];
    };
    let m = pick();
    if (!m) return;
    while (m) {
      const more = apply(m);
      if (!more) break;
      const next = jumps(g, m.r, m.c);
      m = { from: [m.r, m.c], ...next[Math.floor(Math.random() * next.length)] };
    }
    endTurn();
  }

  boardEl.addEventListener("click", (e) => {
    if (over || turn !== "r") return;
    const sq = e.target.closest(".sq");
    if (!sq) return;
    const r = +sq.dataset.r, c = +sq.dataset.c;
    if (sel) {
      const m = movesFor(...sel).find((x) => x.r === r && x.c === c);
      if (m) {
        if (apply(m)) { sel = [r, c]; locked = true; statusEl.textContent = "Keep capturing!"; draw(); }
        else endTurn();
        return;
      }
    }
    if (locked) return;
    if (g[r][c]?.s === "r" && movesFor(r, c).length) sel = [r, c];
    else {
      sel = null;
      if (g[r][c]?.s === "r") statusEl.textContent = "That piece can't move — a capture is mandatory elsewhere.";
    }
    draw();
  });

  document.getElementById("reset").addEventListener("click", reset);
  reset();
})();
