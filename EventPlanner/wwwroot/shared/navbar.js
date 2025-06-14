// shared/navbar.js

function loadNavbar() {
    const token = localStorage.getItem("token");
    const navLinks = [];

    // Always show Home link
    navLinks.push(`<li class="nav-item"><a class="nav-link" href="/home.html">Home</a></li>`);

    if (token) {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const roles = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || [];
        const roleList = Array.isArray(roles) ? roles : [roles];

        if (roleList.includes("Admin")) {
            navLinks.push(`<li class="nav-item"><a class="nav-link" href="/html/users/index.html">Users</a></li>`);
            navLinks.push(`<li class="nav-item"><a class="nav-link" href="/html/events/index.html">Events</a></li>`);
            navLinks.push(`<li class="nav-item"><a class="nav-link" href="/html/invites/index.html">Invites</a></li>`);
            navLinks.push(`<li class="nav-item"><a class="nav-link" href="/html/categories/index.html">Categories</a></li>`);
            navLinks.push(`<li class="nav-item"><a class="nav-link" href="/html/rsvps/index.html">RSVPs</a></li>`);
        }
        if (roleList.includes("Organizer")) {
            navLinks.push(`<li class="nav-item"><a class="nav-link" href="/html/events/index.html">Events</a></li>`);
            navLinks.push(`<li class="nav-item"><a class="nav-link" href="/html/invites/index.html">Invites</a></li>`);
        }
        if (roleList.includes("User")) {
            navLinks.push(`<li class="nav-item"><a class="nav-link" href="/html/rsvps/index.html">RSVPs</a></li>`);
            navLinks.push(`<li class="nav-item"><a class="nav-link" href="/html/profile.html">Profile</a></li>`);
        }
    } else {
        // If not logged in, show login
        navLinks.push(`<li class="nav-item"><a class="nav-link" href="/auth/login.html">Login</a></li>`);
    }

    const navbarHTML = `
        <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
            <div class="container-fluid">
                <a class="navbar-brand" href="/home.html">Event Planner</a>
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

// Auto-load navbar on page load
document.addEventListener("DOMContentLoaded", loadNavbar);
