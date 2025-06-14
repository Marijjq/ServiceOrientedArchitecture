document.addEventListener('DOMContentLoaded', async () => {
    const token = localStorage.getItem('token');
    if (!token) window.location.href = 'login.html';

    // Verify admin role
    const user = JSON.parse(localStorage.getItem('user'));
    if (!user.roles.includes('Admin')) window.location.href = 'category.html';

    // Load users and roles
    try {
        const [usersRes, rolesRes] = await Promise.all([
            fetch('/api/user', { headers: { Authorization: `Bearer ${token}` } }),
            fetch('/api/account/roles', { headers: { Authorization: `Bearer ${token}` } })
        ]);

        const users = await usersRes.json();
        const roles = await rolesRes.json();

        const userSelect = document.getElementById('userSelect');
        users.forEach(user => {
            const option = document.createElement('option');
            option.value = user.id;
            option.textContent = user.username;
            userSelect.appendChild(option);
        });

        const roleSelect = document.getElementById('roleSelect');
        roles.forEach(role => {
            const option = document.createElement('option');
            option.value = role.name;
            option.textContent = role.name;
            roleSelect.appendChild(option);
        });

    } catch (error) {
        showError(error.message);
    }
});

// Handle role creation
document.getElementById('createRoleForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const roleName = document.getElementById('roleName').value;

    try {
        const response = await fetch('/api/account/role', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            },
            body: JSON.stringify(roleName)
        });

        if (!response.ok) throw new Error(await response.text());
        window.location.reload();
    } catch (error) {
        showError(error.message);
    }
});

// Handle role assignment
document.getElementById('assignRoleForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const data = {
        userId: document.getElementById('userSelect').value,
        roleName: document.getElementById('roleSelect').value
    };

    try {
        const response = await fetch('/api/account/assign-role', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            },
            body: JSON.stringify(data)
        });

        if (!response.ok) throw new Error(await response.text());
        alert('Role assigned successfully!');
    } catch (error) {
        showError(error.message);
    }
});
