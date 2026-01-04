const token = localStorage.getItem("token");
if (!token) location.href = "/login.html";

const params = new URLSearchParams(location.search);
const linkId = params.get("id");

const totalClicksEl = document.querySelector("[data-total-clicks]");
const last24hEl = document.querySelector("[data-last-24h]");
const lastClickEl = document.querySelector("[data-last-click]");
const recentClicksEl = document.querySelector("[data-recent-clicks]");

const formatDateTime = (value) => {
    if (!value) return "-";
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return "-";
    return new Intl.DateTimeFormat("ru-RU", {
        dateStyle: "medium",
        timeStyle: "short"
    }).format(date);
};

const renderRecentClicks = (clicks) => {
    recentClicksEl.innerHTML = "";

    if (!clicks.length) {
        const row = document.createElement("tr");
        row.innerHTML = '<td colspan="4">Пока нет кликов по ссылке.</td>';
        recentClicksEl.appendChild(row);
        return;
    }

    clicks.forEach((click) => {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td>${formatDateTime(click.timestamp)}</td>
            <td>${click.browser}</td>
            <td>${click.referer}</td>
            <td>IP hash: ${click.ipHash}</td>
        `;
        recentClicksEl.appendChild(row);
    });
};

const loadStats = async () => {
    if (!linkId) {
        alert("Не передан id ссылки");
        return;
    }

    const r = await fetch(`/links/${linkId}/stats`, {
        headers: {
            "Authorization": `Bearer ${token}`
        }
    });

    if (r.status === 401) {
        localStorage.removeItem("token");
        location.href = "/login.html";
        return;
    }

    if (r.status === 404) {
        alert("Ссылка не найдена");
        return;
    }

    if (!r.ok) {
        alert("Не удалось загрузить статистику");
        return;
    }

    const data = await r.json();
    totalClicksEl.textContent = data.totalClicks;
    last24hEl.textContent = data.last24Hours;
    lastClickEl.textContent = formatDateTime(data.lastClickAt);
    renderRecentClicks(data.recentClicks || []);
};

loadStats();
