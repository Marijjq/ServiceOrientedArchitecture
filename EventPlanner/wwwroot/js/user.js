let isAdmin = false;
let currentUserId = '';

// Utility functions
const showError = (message) => {
    const errorDiv = document.getElementById('error');
    if (errorDiv) {
        errorDiv.textContent = message;
        errorDiv.style.color = '#dc3545';
    }
};

const clearError = () => {
    const errorDiv = document.getElementById('error');
    if (errorDiv) errorDiv.textContent = '';
};

// Load users for admin
const loadUsers = async () => {
    try {
        const token = localStorage.getItem('token');
        const res = await fetch('/api/user', {
            headers: { Authorization: `Bearer ${token}` }
        });

        if (!res.ok) throw new Error(await res.text());
        const users = await res.json();

        const tbody = document.querySelector('#userTable tbody');
        tbody.innerHTML = users.map(user => `
            <tr>
                <td>${user.username}</td>
                <td>${user.email}</td>
                <td>${user.firstName} ${user.lastName}</td>
                <td>${user.role}</td>
                <td>${user.phoneNumber || ''}</td>
                <td>
                    <button onclick="editUser('${user.id}')">Edit</button>
                    <button class="danger" onclick="deleteUser('${user.id}')">Delete</button>
                </td>
            </tr>
        `).join('');

    } catch (error) {
        showError(error.message);
    }
};

// Load user profile
const loadProfile = async () => {
    try {
        const token = localStorage.getItem('token');
        const res = await fetch(`/api/user/${currentUserId}`, {
            headers: { Authorization: `Bearer ${token}` }
        });

        if (!res.ok) throw new Error(await res.text());
        const user = await res.json();

        document.getElementById('profileContent').innerHTML = `
            <p><strong>Username:</strong> ${user.username}</p>
            <p><strong>Email:</strong> ${user.email}</p>
            <p><strong>Name:</strong> ${user.firstName} ${user.lastName}</p>
            <p><strong>Phone:</strong> ${user.phoneNumber || 'N/A'}</p>
        `;

    } catch (error) {
        showError(error.message);
    }
};

// Form handler
const handleFormSubmit = async (e) => {
    e.preventDefault();
    clearError();

    const isEdit = !!localStorage.getItem('editUserId');
    const token = localStorage.getItem('token');

    const userData = {
        username: document.getElementById('username')?.value,
        email: document.getElementById('email')?.value,
        password: document.getElementById('password')?.value,
        firstName: document.getElementById('firstName').value,
        lastName: document.getElementById('lastName').value,
        phoneNumber: document.getElementById('phone').value
    };

    try {
        let response;
        if (isEdit) {
            response = await fetch(`/api/user/${localStorage.getItem('editUserId')}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({
                    firstName: userData.firstName,
                    lastName: userData.lastName,
                    phoneNumber: userData.phoneNumber
                })
            });
        } else {
            response = await fetch('/api/user', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(userData)
            });
        }

        if (!response.ok) throw new Error(await response.text());
        localStorage.removeItem('editUserId');
        window.location.href = '/html/user/user.html';
    } catch (error) {
        showError(error.message);
    }
};

// Initialization
window.addEventListener('DOMContentLoaded', async () => {
    const user = JSON.parse(localStorage.getItem('user'));
    currentUserId = user?.id;
    isAdmin = user?.roles?.includes('Admin');

    if (window.location.pathname.endsWith('/html/user/user.html')) {
        if (isAdmin) {
            document.getElementById('adminSection').style.display = 'block';
            loadUsers();
        } else {
            document.getElementById('userProfile').style.display = 'block';
            loadProfile();
        }
    }

    // Form initialization
    const form = document.getElementById('userForm');
    if (form) {
        form.addEventListener('submit', handleFormSubmit);
        const editId = localStorage.getItem('editUserId');

        if (editId) {
            document.getElementById('formTitle').textContent = 'Edit User';
            document.getElementById('createFields').style.display = 'none';

            try {
                const token = localStorage.getItem('token');
                const res = await fetch(`/api/user/${editId}`, {
                    headers: { Authorization: `Bearer ${token}` }
                });
                const user = await res.json();

                document.getElementById('firstName').value = user.firstName;
                document.getElementById('lastName').value = user.lastName;
                document.getElementById('phone').value = user.phoneNumber || '';
            } catch (error) {
                showError(error.message);
            }
        }
    }
});

window.editUser = (id) => {
    localStorage.setItem('editUserId', id);
    window.location.href = '/html/user/user-form.html';
};

window.editProfile = () => {
    localStorage.setItem('editUserId', currentUserId);
    window.location.href = '/html/user/user-form.html';
};

window.deleteUser = async (id) => {
    if (!confirm('Are you sure you want to delete this user?')) return;

    try {
        const token = localStorage.getItem('token');
        const response = await fetch(`/api/user/${id}`, {
            method: 'DELETE',
            headers: { Authorization: `Bearer ${token}` }
        });
        if (!response.ok) throw new Error('Delete failed');
        window.location.reload();
    } catch (error) {
        showError(error.message);
    }
};
