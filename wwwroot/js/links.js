const token = localStorage.getItem("token");
if (!token) location.href = "/login.html";

const searchInput = document.querySelector('input[name="search"]');
const btn = document.querySelector(".main__form__submit");
const tableBody = document.querySelector("[data-links-body]");

const formatDateTime = (value) => {
    if (!value) return "-";
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return "-";
    return new Intl.DateTimeFormat("ru-RU", {
        dateStyle: "medium",
        timeStyle: "short"
    }).format(date);
};

const renderLinks = (links) => {
    tableBody.innerHTML = "";

    if (!links.length) {
        const row = document.createElement("tr");
        row.innerHTML = '<td colspan="5">Нет ссылок по заданному фильтру.</td>';
        tableBody.appendChild(row);
        return;
    }

    links.forEach((link) => {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td><a href="${link.shortUrl}" target="_blank" rel="noopener">${link.shortUrl}</a></td>
            <td><a href="${link.originalUrl}" target="_blank" rel="noopener">${link.originalUrl}</a></td>
            <td>${link.totalClicks}</td>
            <td>${formatDateTime(link.lastClickAt)}</td>
            <td><a href="/stats.html?id=${link.id}">View</a></td>
        `;
        tableBody.appendChild(row);
    });
};

const loadLinks = async () => {
    const search = searchInput.value.trim();
    const query = search ? `?search=${encodeURIComponent(search)}` : "";
    const r = await fetch(`/links${query}`, {
        headers: {
            "Authorization": `Bearer ${token}`
        }
    });

    if (r.status === 401) {
        localStorage.removeItem("token");
        location.href = "/login.html";
        return;
    }

    if (!r.ok) {
        alert("Не удалось загрузить список ссылок");
        return;
    }

    const data = await r.json();
    renderLinks(data);
};

btn.addEventListener("click", async (e) => {
    e.preventDefault();
    await loadLinks();
});

loadLinks();
