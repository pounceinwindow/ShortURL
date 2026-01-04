document.querySelector(".button").addEventListener("click", async (e) => {
    e.preventDefault();

    const email = document.querySelector('input[name="email"]').value.trim();
    const password = document.querySelector('input[name="password"]').value;
    const confirm = document.querySelector('input[name="confirm-password"]').value;

    if (password !== confirm) {
        alert("Пароли не совпадают");
        return;
    }

    const r = await fetch("/auth/create_user", {
        method: "POST",
        headers: {"Content-Type": "application/json"},
        body: JSON.stringify({email, password})
    });

    if (r.status === 409) {
        alert("Пользователь уже существует");
        return;
    }
    if (!r.ok) {
        alert("Ошибка регистрации");
        return;
    }

    location.href = "/login.html";
});
