// Utility functions
function showError(message) {
    const errorDiv = document.getElementById('error');
    if (errorDiv) {
        errorDiv.textContent = message;
        errorDiv.style.color = 'red';
    }
}

function clearError() {
    const errorDiv = document.getElementById('error');
    if (errorDiv) errorDiv.textContent = '';
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleString();
}

// Main page logic
window.onload = async () => {
    const token = localStorage.getItem("token");
    if (!token) window.location.href = "/html/login.html";

    const user = JSON.parse(localStorage.getItem("user"));
    const isAdmin = user?.roles?.includes("Admin");
    const isOrganizer = user?.roles?.includes("Organizer");

    // Show/hide sections based on role
    document.getElementById('newInviteBtn').style.display =
        (isAdmin || isOrganizer) ? 'inline-block' : 'none';
    document.getElementById('allInvitesSection').style.display =
        (isAdmin || isOrganizer) ? 'block' : 'none';

    try {
        // Load all invites (for admins/organizers)
        if (isAdmin || isOrganizer) {
            const res = await fetch("/api/invite", {
                headers: { Authorization: `Bearer ${token}` }
            });
            if (!res.ok) throw new Error(await res.text());
            const invites = await res.json();

            const tbody = document.querySelector("#inviteTable tbody");
            tbody.innerHTML = invites.map(invite => `
                <tr>
                    <td>${invite.id}</td>
                    <td>${invite.eventTitle}</td>
                    <td>${invite.inviterName}</td>
                    <td>${invite.inviteeName}</td>
                    <td>${invite.status}</td>
                    <td>${formatDate(invite.sentAt)}</td>
                    <td>
                        <button onclick="editInvite(${invite.id})">Edit</button>
                        <button onclick="deleteInvite(${invite.id})">Delete</button>
                    </td>
                </tr>
            `).join('');
        }

        // Load pending invites for current user
        const pendingRes = await fetch(`/api/invite/user/${user.id}/pending`, {
            headers: { Authorization: `Bearer ${token}` }
        });
        if (!pendingRes.ok) throw new Error(await pendingRes.text());
        const pendingInvites = await pendingRes.json();

        const pendingTbody = document.querySelector("#pendingInviteTable tbody");
        pendingTbody.innerHTML = pendingInvites.map(invite => `
            <tr>
                <td>${invite.eventTitle}</td>
                <td>${invite.inviterName}</td>
                <td>${invite.message || ''}</td>
                <td>${formatDate(invite.sentAt)}</td>
                <td>
                    ${invite.status === 'Pending' ? `
                    <button onclick="respondToInvite(${invite.id}, 'Accepted')">Accept</button>
                    <button onclick="respondToInvite(${invite.id}, 'Declined')">Decline</button>
                    ` : invite.status}
                </td>
            </tr>
        `).join('');

        document.getElementById('loading').style.display = 'none';
    } catch (error) {
        showError(error.message);
        document.getElementById('loading').style.display = 'none';
    }

    window.editInvite = (id) => {
        localStorage.setItem("editInviteId", id);
        window.location.href = "/html/invite/invite-form.html";
    };

    window.deleteInvite = async (id) => {
        if (!confirm("Are you sure?")) return;
        try {
            const response = await fetch(`/api/invite/${id}`, {
                method: "DELETE",
                headers: { Authorization: `Bearer ${token}` }
            });
            if (!response.ok) throw new Error(await response.text());
            window.location.reload();
        } catch (error) {
            showError(error.message);
        }
    };

    window.respondToInvite = async (id, status) => {
        try {
            const response = await fetch(`/api/invite/${id}`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                    Authorization: `Bearer ${token}`
                },
                body: JSON.stringify({
                    status: status,
                    respondedAt: new Date().toISOString()
                })
            });
            if (!response.ok) throw new Error(await response.text());
            window.location.reload();
        } catch (error) {
            showError(error.message);
        }
    };
};

// Form logic
document.getElementById('inviteForm')?.addEventListener('submit', handleInviteSubmit);

async function handleInviteSubmit(e) {
    e.preventDefault();
    clearError();
    const token = localStorage.getItem("token");
    const user = JSON.parse(localStorage.getItem("user"));

    const inviteData = {
        inviterId: user.id,
        inviteeId: document.getElementById('inviteeId').value,
        eventId: document.getElementById('eventId').value,
        message: document.getElementById('message').value,
        expiresAt: document.getElementById('expiresAt').value
    };

    try {
        const response = await fetch("/api/invite", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                Authorization: `Bearer ${token}`
            },
            body: JSON.stringify(inviteData)
        });
        if (!response.ok) throw new Error(await response.text());
        window.location.href = 'invite.html';
    } catch (error) {
        showError(error.message);
    }
}

// Form initialization
window.addEventListener('DOMContentLoaded', async () => {
    const form = document.getElementById('inviteForm');
    if (!form) return;

    const token = localStorage.getItem("token");

    // Load events
    try {
        const eventsRes = await fetch("/api/event", {
            headers: { Authorization: `Bearer ${token}` }
        });
        const events = await eventsRes.json();
        const eventSelect = document.getElementById('eventId');
        eventSelect.innerHTML = events.map(e =>
            `<option value="${e.id}">${e.title}</option>`
        ).join('');
    } catch (error) {
        showError("Failed to load events");
    }

    // Load users (assuming /api/user endpoint exists)
    try {
        const usersRes = await fetch("/api/user", {
            headers: { Authorization: `Bearer ${token}` }
        });
        const users = await usersRes.json();
        const userSelect = document.getElementById('inviteeId');
        userSelect.innerHTML = users.map(u =>
            `<option value="${u.id}">${u.name}</option>`
        ).join('');
    } catch (error) {
        showError("Failed to load users");
    }

    // Load existing invite for editing
    const editId = localStorage.getItem("editInviteId");
    if (editId) {
        document.getElementById('formTitle').textContent = 'Edit Invite';
        try {
            const response = await fetch(`/api/invite/${editId}`, {
