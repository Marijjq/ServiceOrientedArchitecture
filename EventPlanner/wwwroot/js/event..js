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

// Format date for display
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleString();
}

// Main page logic
window.onload = async () => {
    const token = localStorage.getItem("token");
    if (!token && window.location.pathname.includes("event-form.html")) {
        // Allow anonymous access if needed, or redirect if not
        // window.location.href = "login.html";
    }

    // Handle event table
    const tableBody = document.querySelector("#eventTable tbody");
    if (tableBody) {
        try {
            const res = await fetch("/api/event", {
                headers: { Authorization: token ? `Bearer ${token}` : undefined }
            });
            if (!res.ok) throw new Error(await res.text());
            const events = await res.json();
            tableBody.innerHTML = "";
            document.getElementById('loading').style.display = 'none';
            events.forEach(evt => {
                const row = `<tr>
                    <td>${evt.id}</td>
                    <td>${evt.title}</td>
                    <td>${evt.description || ''}</td>
                    <td>${evt.location}</td>
                    <td>${formatDate(evt.startDate)}</td>
                    <td>${formatDate(evt.endDate)}</td>
                    <td>${evt.categoryName || 'N/A'}</td>
                    <td>${evt.status}</td>
                    <td>
                        <button onclick="editEvent(${evt.id})">Edit</button>
                        <button onclick="deleteEvent(${evt.id})">Delete</button>
                    </td>
                </tr>`;
                tableBody.innerHTML += row;
            });
        } catch (error) {
            showError(error.message);
            document.getElementById('loading').style.display = 'none';
        }
    }

    // Handle edit/delete
    window.editEvent = (id) => {
        localStorage.setItem("editEventId", id);
        window.location.href = "/html/event/event-form.html";
    };

    window.deleteEvent = async (id) => {
        if (!confirm("Are you sure?")) return;
        const token = localStorage.getItem("token");
        try {
            const response = await fetch(`/api/event/${id}`, {
                method: "DELETE",
                headers: { Authorization: `Bearer ${token}` }
            });
            if (!response.ok) throw new Error(await response.text());
            window.location.reload();
        } catch (error) {
            showError(error.message);
        }
    };
};

// Form logic
document.getElementById('eventForm')?.addEventListener('submit', handleEventFormSubmit);

async function handleEventFormSubmit(e) {
    e.preventDefault();
    clearError();
    const token = localStorage.getItem("token");
    const isEdit = localStorage.getItem("editEventId");
    const title = document.getElementById('title').value;
    const description = document.getElementById('description').value;
    const location = document.getElementById('location').value;
    const startDate = document.getElementById('startDate').value;
    const endDate = document.getElementById('endDate').value;
    const categoryId = document.getElementById('categoryId').value;
    const maxParticipants = document.getElementById('maxParticipants').value;
    const status = document.getElementById('status').value;

    if (!title || !location || !startDate || !endDate || !categoryId) {
        showError("Required fields are missing!");
        return;
    }

    const eventData = {
        title,
        description,
        location,
        startDate,
        endDate,
        categoryId: parseInt(categoryId),
        maxParticipants: parseInt(maxParticipants) || 0,
        status
    };

    try {
        let response;
        if (isEdit) {
            response = await fetch(`/api/event/${isEdit}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': token ? `Bearer ${token}` : undefined
                },
                body: JSON.stringify(eventData)
            });
        } else {
            response = await fetch('/api/event', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': token ? `Bearer ${token}` : undefined
                },
                body: JSON.stringify(eventData)
            });
        }
        if (!response.ok) throw new Error(await response.text());
        localStorage.removeItem("editEventId");
        window.location.href = '/html/event/event.html';
    } catch (error) {
        showError(error.message);
    }
}

// Form initialization for edits and category dropdown
window.addEventListener('DOMContentLoaded', async () => {
    const editId = localStorage.getItem("editEventId");
    const formTitle = document.getElementById('formTitle');
    const categorySelect = document.getElementById('categoryId');

    if (formTitle && editId) formTitle.textContent = 'Edit Event';

    // Load categories
    try {
        const catRes = await fetch("/api/category", {
            headers: { Authorization: localStorage.getItem("token") ? `Bearer ${localStorage.getItem("token")}` : undefined }
        });
        if (!catRes.ok) throw new Error('Failed to load categories');
        const categories = await catRes.json();
        categorySelect.innerHTML = '';
        categories.forEach(cat => {
            const option = document.createElement('option');
            option.value = cat.id;
            option.textContent = cat.name;
            categorySelect.appendChild(option);
        });
    } catch (error) {
        showError(error.message);
    }

    // Load event data for edit
    if (editId) {
        const token = localStorage.getItem("token");
        try {
            const response = await fetch(`/api/event/${editId}`, {
                headers: { Authorization: token ? `Bearer ${token}` : undefined }
            });
            if (!response.ok) throw new Error('Event not found');
            const event = await response.json();
            document.getElementById('title').value = event.title;
            document.getElementById('description').value = event.description || '';
            document.getElementById('location').value = event.location;
            document.getElementById('startDate').value = new Date(event.startDate).toISOString().slice(0, 16);
            document.getElementById('endDate').value = new Date(event.endDate).toISOString().slice(0, 16);
            document.getElementById('categoryId').value = event.categoryId;
            document.getElementById('maxParticipants').value = event.maxParticipants;
            document.getElementById('status').value = event.status;
        } catch (error) {
            showError(error.message);
            localStorage.removeItem("editEventId");
        }
    }
});
