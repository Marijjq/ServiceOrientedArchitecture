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

const formatDate = (dateString) => 
    new Date(dateString).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    });

// RSVP Operations
const loadRSVPs = async () => {
    const token = localStorage.getItem('token');
    if (!token) window.location.href = '/html/login.html';

    try {
        const res = await fetch('/api/rsvp', {
            headers: { Authorization: `Bearer ${token}` }
        });
        if (!res.ok) throw new Error(await res.text());
        
        const rsvps = await res.json();
        const tbody = document.querySelector('#rsvpTable tbody');
        tbody.innerHTML = rsvps.map(rsvp => `
            <tr>
                <td>${rsvp.eventTitle}</td>
                <td>${rsvp.userName}</td>
                <td>
                    <select class="status-select" data-id="${rsvp.id}">
                        <option ${rsvp.status === 'Going' ? 'selected' : ''}>Going</option>
                        <option ${rsvp.status === 'Maybe' ? 'selected' : ''}>Maybe</option>
                        <option ${rsvp.status === 'NotGoing' ? 'selected' : ''}>Not Going</option>
                    </select>
                </td>
                <td>${formatDate(rsvp.createdAt)}</td>
                <td>
                    <button onclick="editRSVP(${rsvp.id})">Edit</button>
                    <button class="danger" onclick="deleteRSVP(${rsvp.id})">Delete</button>
                </td>
            </tr>
        `).join('');

        document.querySelectorAll('.status-select').forEach(select => {
            select.addEventListener('change', handleStatusChange);
        });

    } catch (error) {
        showError(error.message);
    }
};

const handleStatusChange = async (e) => {
    const rsvpId = e.target.dataset.id;
    const newStatus = e.target.value;
    
    try {
        const response = await fetch(`/api/rsvp/${rsvpId}/status?status=${newStatus}`, {
            method: 'PATCH',
            headers: { 
                'Authorization': `Bearer ${localStorage.getItem('token')}`,
                'Content-Type': 'application/json'
            }
        });
        if (!response.ok) throw new Error('Status update failed');
    } catch (error) {
        showError(error.message);
        e.target.value = e.target.oldValue;
    }
};

const handleFormSubmit = async (e) => {
    e.preventDefault();
    clearError();
    
    const rsvpData = {
        eventId: parseInt(document.getElementById('eventId').value),
        status: document.getElementById('status').value
    };

    try {
        const response = await fetch('/api/rsvp', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            },
            body: JSON.stringify(rsvpData)
        });
        
        if (!response.ok) throw new Error(await response.text());
        window.location.href = '/html/rsvp/rsvp.html';
    } catch (error) {
        showError(error.message);
    }
};

// Event Listeners
document.addEventListener('DOMContentLoaded', () => {
    if (window.location.pathname.endsWith('rsvp.html')) {
        loadRSVPs();
    }
    
    document.getElementById('rsvpForm')?.addEventListener('submit', handleFormSubmit);
});

window.editRSVP = (id) => {
    localStorage.setItem('editRSVPId', id);
    window.location.href = '/html/rsvp/rsvp-form.html';
};

window.deleteRSVP = async (id) => {
    if (!confirm('Are you sure you want to delete this RSVP?')) return;
    
    try {
        const response = await fetch(`/api/rsvp/${id}`, {
            method: 'DELETE',
            headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
        });
        if (!response.ok) throw new Error('Delete failed');
        loadRSVPs();
    } catch (error) {
        showError(error.message);
    }
};
