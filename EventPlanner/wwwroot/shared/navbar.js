// shared/navbar.js

function loadNavbar() {
    const token = localStorage.getItem("token");
    const navLinks = [];
    const uniqueLinks = new Set();

    function addLink(path, label) {
        const key = `${path}|${label}`;
        if (!uniqueLinks.has(key)) {
            navLinks.push(`<li class="nav-item"><a class="nav-link" href="${path}">${label}</a></li>`);
            uniqueLinks.add(key);
        }
    }

    addLink("/home.html", "Home");

    if (token) {
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            const rolesRaw = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
            const roles = Array.isArray(rolesRaw) ? rolesRaw : [rolesRaw];
            const roleList = roles.map(r => r?.toLowerCase());

            if (roleList.includes("admin")) {
                addLink("/html/users/index.html", "Users");
                addLink("/html/events/index.html", "Events");
                addLink("/html/invites/index.html", "Invites");
                addLink("/html/categories/index.html", "Categories");
                addLink("/html/rsvps/index.html", "RSVPs");
            }
            if (roleList.includes("organizer")) {
                addLink("/html/events/index.html", "Events");
                addLink("/html/invites/index.html", "Invites");
            }
            if (roleList.includes("user")) {
                addLink("/html/rsvps/index.html", "RSVPs");
                addLink("/html/profile.html", "Profile");
            }
        } catch (e) {
            localStorage.removeItem("token");
            return location.href = "/auth/login.html";
        }
    } else {
        addLink("/auth/login.html", "Login");
    }

    const navbarHTML = `
        <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
            <div class="container-fluid">
                <a class="navbar-brand" href="/html/home.html">Event Planner</a>
                <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
                    <span class="navbar-toggler-icon"></span>
                </button>
                <div class="collapse navbar-collapse" id="navbarNav">
                    <ul class="navbar-nav me-auto">${navLinks.join('')}</ul>
                    <ul class="navbar-nav">
                        ${token ? `<li class="nav-item"><a class="nav-link" href="#" onclick="logout()">Logout</a></li>` : ""}
                    </ul>
                </div>
            </div>
        </nav>
    `;

    document.getElementById("navbar").innerHTML = navbarHTML;
}

function logout() {
    localStorage.removeItem("token");
    window.location.href = "/auth/login.html";
}

document.addEventListener("DOMContentLoaded", loadNavbar);
