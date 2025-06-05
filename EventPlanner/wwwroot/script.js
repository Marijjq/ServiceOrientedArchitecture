const API_BASE = 'http://localhost:5000/api';

// Helper: API fetch with auth
async function apiFetch(url, method = 'GET', body = null) {
    const token = localStorage.getItem('token');
    const headers = { 'Authorization': `Bearer ${token}` };

    if (body) {
        headers['Content-Type'] = 'application/json';
        body = JSON.stringify(body);
    }

    const response = await fetch(`${API_BASE}${url}`, { method, headers, body });

    if (response.status === 401) {
        logout();
        return;
    }

    if (!response.ok) throw new Error(await response.text());
    return response.json();
}

// Helper: Render table
function renderTable(containerId, data, columns, actions = []) {
    const container = document.querySelector(containerId);
    const table = document.createElement('table');
    table.className = 'table';

    // Header
    const thead = document.createElement('thead');
    const headerRow = document.createElement('tr');
    columns.forEach(col => {
        const th = document.createElement('th');
        th.textContent = col.charAt(0).toUpperCase() + col.slice(1);
        headerRow.appendChild(th);
    });
    if (actions.length) {
        const th = document.createElement('th');
        th.textContent = 'Actions';
        headerRow.appendChild(th);
    }
    thead.appendChild(headerRow);

    // Body
    const tbody = document.createElement('tbody');
    data.forEach(item => {
        const row = document.createElement('tr');
        columns.forEach(col => {
            const td = document.createElement('td');
            td.textContent = item[col] || '';
            row.appendChild(td);
        });

        if (actions.length) {
            const actionTd = document.createElement('td');
            actions.forEach(action => {
                const btn = document.createElement('button');
                btn.className = action.className || 'btn-small';
                btn.textContent = action.label;
                btn.onclick = () => action.handler(item.id);
                actionTd.appendChild(btn);
            });
            row.appendChild(actionTd);
        }

        tbody.appendChild(row);
    });

    table.appendChild(thead);
    table.appendChild(tbody);
    container.innerHTML = '';
    container.appendChild(table);
}

// Fetch and display users (Admin only)
async function loadUsers() {
    const users = await apiFetch('/users');
    renderTable('#usersTable', users, ['id', 'email', 'roles'], [
        { label: 'Delete', className: 'btn-danger', handler: deleteUser }
    ]);
}

async function deleteUser(id) {
    if (!confirm('Are you sure?')) return;
    await apiFetch(`/users/${id}`, 'DELETE');
    loadUsers();
}

// Fetch and display events (Organizer only)
async function loadEvents() {
    const events = await apiFetch('/events?myEvents=true');
    renderTable('#eventsTable', events, ['id', 'name', 'date'], [
        { label: 'Edit', handler: editEvent },
        { label: 'Delete', className: 'btn-danger', handler: deleteEvent }
    ]);
}

async function editEvent(id) {
    // Implement edit functionality
    alert('Edit event: ' + id);
}

async function deleteEvent(id) {
    if (!confirm('Are you sure?')) return;
    await apiFetch(`/events/${id}`, 'DELETE');
    loadEvents();
}

// Fetch and display invites (Organizer only)
async function loadInvites() {
    const invites = await apiFetch('/invites?myInvites=true');
    renderTable('#invitesTable', invites, ['id', 'eventName', 'inviteeEmail', 'status']);
}

// Fetch and display user events (User only)
async function loadUserEvents() {
    const events = await apiFetch('/events/my');
    renderTable('#userEventsTable', events, ['id', 'name', 'date', 'status']);
}

// Fetch and display user invites (User only)
async function loadUserInvites() {
    const invites = await apiFetch('/invites/my');
    renderTable('#userInvitesTable', invites, ['id', 'eventName', 'status'], [
        { label: 'Accept', handler: acceptInvite },
        { label: 'Decline', className: 'btn-danger', handler: declineInvite }
    ]);
}

async function acceptInvite(id) {
    await apiFetch(`/invites/${id}/respond?status=Accepted`, 'POST');
    loadUserInvites();
}

async function declineInvite(id) {
    await apiFetch(`/invites/${id}/respond?status=Declined`, 'POST');
    loadUserInvites();
}

// Logout
function logout() {
    localStorage.removeItem('token');
    window.location.href = 'index.html';
}

// On page load: check auth, load user, show relevant sections
document.addEventListener('DOMContentLoaded', async () => {
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = 'index.html';
        return;
    }

    try {
        const user = await apiFetch('/account/me');
        document.getElementById('userName').textContent = user.email;
        document.getElementById('userRole').textContent = user.roles[0] || 'User';

        // Show relevant section based on role
        if (user.roles.includes('Admin')) {
            document.getElementById('adminSection').style.display = 'block';
            loadUsers();
        }
        if (user.roles.includes('Organizer')) {
            document.getElementById('organizerSection').style.display = 'block';
            loadEvents();
            loadInvites();
        }
        if (user.roles.includes('User')) {
            document.getElementById('userSection').style.display = 'block';
            loadUserEvents();
            loadUserInvites();
        }
    } catch (error) {
        logout();
    }
});
