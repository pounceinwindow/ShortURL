// Require auth for this page
const token = localStorage.getItem("token");
if (!token) location.href = "/login.html";

// Logout (clear token)
document.querySelector("[data-logout]")?.addEventListener("click", () => {
  localStorage.removeItem("token");
});

async function authJson(path, { method = "GET", body } = {}) {
  const r = await fetch(path, {
    method,
    headers: {
      "Content-Type": "application/json",
      "Authorization": `Bearer ${token}`
    },
    body: body ? JSON.stringify(body) : undefined
  });

  if (r.status === 401) {
    localStorage.removeItem("token");
    location.href = "/login.html";
    return;
  }

  const text = await r.text();
  let data = null;
  try {
    data = text ? JSON.parse(text) : null;
  } catch {
    // When the server returns an HTML error page (e.g., developer exception page),
    // JSON.parse will throw. Keep the raw text for debugging.
    data = { raw: text };
  }

  if (!r.ok) {
    const msg = data?.message || (Array.isArray(data?.errors) ? data.errors.join(", ") : null) || "API_ERROR";
    throw new Error(msg);
  }

  return data;
}

(async () => {
  try {
    const params = new URLSearchParams(location.search);
    const code = params.get("code");
    if (!code) { alert("Нет code в query (?code=...)"); location.href = "/links.html"; return; }

    const totalEl = document.querySelector("[data-total-clicks]");
    const last24El = document.querySelector("[data-last-24h]");
    const lastClickEl = document.querySelector("[data-last-click]");
    const tbody = document.querySelector(".stats__table tbody");

    const s = await authJson(`/links/${encodeURIComponent(code)}/stats`);

    totalEl.textContent = s.totalClicks;
    last24El.textContent = s.last24h;
    lastClickEl.textContent = s.lastClick ? new Date(s.lastClick).toLocaleString() : "—";


      tbody.innerHTML = (s.recent || []).map(c => {
          const url = safeHttpUrl(c.referer);
          const target = url
              ? `<a class="link stats__url" href="${escapeAttr(url)}" target="_blank" rel="noreferrer">${escapeHtml(url)}</a>`
              : "—";

          return `
  <tr>
    <td>${new Date(c.timestamp).toLocaleString()}</td>
    <td>${escapeHtml(`${c.browser} · ${c.deviceType}`)}</td>
    <td>${escapeHtml(c.country || "—")}</td>
    <td>${escapeHtml(c.city || "—")}</td>
    <td>${escapeHtml(c.ipAddress || "—")}</td>
    <td>${target}</td>
  </tr>
  `;
      }).join("");

  } catch (e) {
    const msg = e?.message || String(e);
    alert(msg);
  }
})();

function escapeHtml(str) {
  return String(str)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;");
}

function escapeAttr(str) {
  return escapeHtml(str).replaceAll('"', "&quot;");
}

function safeHttpUrl(u) {
  const s = (u || "").trim();
  if (!s) return null;
  if (s.startsWith("http://") || s.startsWith("https://")) return s;
  return null;
}


