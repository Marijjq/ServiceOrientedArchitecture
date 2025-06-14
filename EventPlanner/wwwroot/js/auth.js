document.addEventListener("DOMContentLoaded", () => {
    const loginForm = document.getElementById("loginForm");
    const registerForm = document.getElementById("registerForm");

    if (loginForm) {
        loginForm.addEventListener("submit", async (e) => {
            e.preventDefault();
            const emailOrUsername = document.getElementById("emailOrUsername").value;
            const password = document.getElementById("password").value;

            const res = await fetch("/api/account/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ emailOrUsername, password })
            });

            const data = await res.json();

            if (res.ok && data.token) {
                localStorage.setItem("token", data.token);
                window.location.href = "/html/home.html";
            } else {
                document.getElementById("loginError").innerText = data.message || "Login failed.";
            }
        });
    }

    if (registerForm) {
        registerForm.addEventListener("submit", async (e) => {
            e.preventDefault();
            const user = {
                username: document.getElementById("username").value,
                email: document.getElementById("email").value,
                password: document.getElementById("password").value,
                firstName: document.getElementById("firstName").value,
                lastName: document.getElementById("lastName").value,
                phoneNumber: document.getElementById("phoneNumber").value
            };

            const res = await fetch("/api/account/register", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(user)
            });

            const data = await res.json();

            if (res.ok) {
                window.location.href = "/html/login.html";
            } else {
                document.getElementById("registerError").innerText = data.message || "Registration failed.";
            }
        });
    }
});
