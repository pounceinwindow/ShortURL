function getToken() {
    return localStorage.getItem("token");
}

async function api(path, { method="GET", body, auth=true } = {}) {
    const headers = { "Content-Type": "application/json" };
    if (auth) {
        const token = getToken();
        if (!token) throw new Error("NO_TOKEN");
        headers["Authorization"] = `Bearer ${token}`;
    }

    const r = await fetch(path, {
        method,
        headers,
        body: body ? JSON.stringify(body) : undefined
    });

    if (r.status === 401) {
        localStorage.removeItem("token");
        location.href = "/login.html";
        return;
    }

    const text = await r.text();
    const data = text ? JSON.parse(text) : null;

    if (!r.ok) {
        const msg = data?.message || data?.errors?.join(", ") || "API_ERROR";
        throw new Error(msg);
    }

    return data;
}
