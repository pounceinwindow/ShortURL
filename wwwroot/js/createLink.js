// Require auth for this page
const token = localStorage.getItem("token");
if (!token) location.href = "/login.html";

// Logout (clear token)
document.querySelector("[data-logout]")?.addEventListener("click", () => {
    localStorage.removeItem("token");
});

const btn = document.querySelector(".main__form__submit");
const longInput = document.querySelector('input[name="long_link"]');
const slugInput = document.querySelector('input[name="slug"]');
const expiresInput = document.querySelector('input[name="expires"]');

const linkEl = document.querySelector(".main__qr__link");
const qrBox = document.querySelector(".main__qr__image");

btn.addEventListener("click", async (e) => {
    e.preventDefault();

    const originalUrl = longInput.value.trim();
    const shortCode = slugInput.value.trim();
    const expiresAt = (expiresInput?.value || "").trim();

    if (!originalUrl) {
        alert("Введите ссылку");
        return;
    }

    const r = await fetch("/links", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`
        },
        // expiresAt пока не используется на бэке — оставлено на будущее
        body: JSON.stringify({ originalUrl, shortCode, expiresAt })
    });

    if (r.status === 401) {
        localStorage.removeItem("token");
        location.href = "/login.html";
        return;
    }
    if (r.status === 409) {
        alert("Такой slug уже занят");
        return;
    }
    if (!r.ok) {
        alert("Ошибка создания ссылки");
        return;
    }

    const data = await r.json();

    linkEl.textContent = data.shortUrl;
    linkEl.href = data.shortUrl;
    linkEl.target = "_blank"

    // Keep QR inside the card (see CSS .main__qr__image)
    qrBox.innerHTML = `<img alt="QR" src="/${data.shortCode}/qr?size=200">`;
});

document.querySelector(".main__qr__button").addEventListener("click", async () => {
    const text = linkEl?.href;
    if (text) await navigator.clipboard.writeText(text);
});
