const token = localStorage.getItem("token");
if (!token) location.href = "/login.html";

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
    const listEl = document.getElementById("linksList");
    const searchInput = document.querySelector('input[name="search"]');
    const btn = document.querySelector(".main__form__submit");

    async function load() {
      const q = (searchInput?.value || "").trim();
      const res = await authJson(`/links?page=1&pageSize=50${q ? `&q=${encodeURIComponent(q)}` : ""}`);
      const items = res?.items || [];

      listEl.innerHTML = items.length
        ? items.map(x => `
          <div class="link-card">
            <div><b>${x.shortCode}</b> → <a href="${x.shortUrl}" target="_blank">${x.shortUrl}</a></div>
            <div style="opacity:.8; margin-top:4px;">${x.originalUrl}</div>
            <div style="margin-top:6px;">
              clicks: <b>${x.clicks}</b>
              · <a class="link" href="/stats.html?code=${encodeURIComponent(x.shortCode)}">stats</a>
            </div>
          </div>
        `).join("")
        : `<div style="opacity:.7;">Нет ссылок</div>`;
    }

    btn.addEventListener("click", (e) => { e.preventDefault(); load(); });
    await load();
  } catch (e) {
    alert(e?.message || String(e));
  }
})();
