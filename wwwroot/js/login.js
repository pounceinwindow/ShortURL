const btn = document.querySelector(".main__form__submit");
btn.addEventListener("click", async (e) => {
    e.preventDefault();

    const email = document.querySelector('input[name="email"]').value.trim();
    const password = document.querySelector('input[name="password"]').value;

    const r = await fetch("/auth/login", {
        method: "POST",
        headers: {"Content-Type": "application/json"},
        body: JSON.stringify({email, password})
    });

    if (!r.ok) {
        alert("Неверный логин/пароль");
        return;
    }

    const data = await r.json();
    localStorage.setItem("token", data.accessToken);
    location.href = "/create_link.html";
});