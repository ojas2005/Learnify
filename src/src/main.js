import './style.css';

// --- CONFIG & STATE ---
const API_BASE = 'http://localhost:5010'; // Gateway URL
const state = {
  user: JSON.parse(localStorage.getItem('user')) || null,
  token: localStorage.getItem('token') || null,
  currentPage: window.location.hash.slice(1) || 'home',
  courses: [],
  myCourses: [],
  stats: null,
  adminAnalytics: null,
  adminUsers: [],
  adminPendingCourses: [],
  adminPendingReviews: [],
  notifications: []
};

// --- API HELPERS ---
async function apiFetch(endpoint, options = {}) {
  const headers = {
    'Content-Type': 'application/json',
    ...(state.token ? { 'Authorization': `Bearer ${state.token}` } : {}),
    ...options.headers
  };
  try {
    const response = await fetch(`${API_BASE}${endpoint}`, { ...options, headers });
    if (response.status === 401) { logout(); return null; }
    if (response.status === 204) return true;
    const data = await response.json();
    if (!response.ok) throw new Error(data.message || 'Something went wrong');
    return data;
  } catch (err) {
    showToast(err.message, 'error');
    return null;
  }
}

// --- AUTH ---
async function login(email, password) {
  const data = await apiFetch('/api/identity/api/accounts/login', {
    method: 'POST',
    body: JSON.stringify({ email, password })
  });
  if (data && data.accessToken) {
    state.token = data.accessToken;
    localStorage.setItem('token', state.token);
    const profile = await apiFetch('/api/identity/api/accounts/me');
    if (profile) {
      state.user = profile;
      localStorage.setItem('user', JSON.stringify(profile));
      showToast(`Welcome back, ${profile.displayName}!`, 'success');
      navigate('dashboard');
    }
  }
}

async function register(name, email, password, role) {
  const data = await apiFetch('/api/identity/api/accounts/register', {
    method: 'POST',
    body: JSON.stringify({ displayName: name, email, password, role })
  });
  if (data) {
    showToast('Account created! Please login.', 'success');
    navigate('login');
  }
}

function logout() {
  state.user = null; state.token = null; state.stats = null;
  state.myCourses = []; state.adminAnalytics = null; state.adminUsers = [];
  localStorage.removeItem('user'); localStorage.removeItem('token');
  navigate('home');
}

function navigate(page) {
  if (!page) return;
  state.currentPage = page;
  // Ensure we don't have multiple hashes or invalid chars
  const cleanPage = page.replace(/^#/, '');
  window.history.pushState({}, '', `#${cleanPage}`);
  render();
}

window.addEventListener('popstate', () => {
  state.currentPage = window.location.hash.slice(1) || 'home';
  render();
});
window.navigate = navigate;
window.logout = logout;

function showToast(message, type = 'info') {
  document.querySelector('.toast-notification')?.remove();
  const colors = { success: 'var(--accent-color)', error: 'var(--error-color)', info: 'var(--primary-color)', warning: 'var(--warning-color)' };
  const toast = document.createElement('div');
  toast.className = 'toast-notification glass';
  toast.style.cssText = `position:fixed;bottom:2rem;right:2rem;padding:1rem 2rem;border-radius:var(--radius-md);z-index:9999;max-width:360px;border-left:4px solid ${colors[type] || colors.info};animation:fadeIn 0.3s ease-out;`;
  toast.innerText = message;
  document.body.appendChild(toast);
  setTimeout(() => toast.remove(), 4000);
}

// --- COMPONENTS ---
const Nav = () => `
  <nav class="nav main-container">
    <div class="logo" onclick="navigate('home')" style="cursor:pointer;"><span style="font-size:2rem;">⚛️</span> EduLearn</div>
    <div style="display:flex;gap:2rem;align-items:center;">
      ${state.user ? `
        <button onclick="navigate('dashboard')" class="btn btn-ghost">Dashboard</button>
        <button onclick="navigate('courses')" class="btn btn-ghost">Browse</button>
        <div style="display:flex;align-items:center;gap:1rem;">
          <div style="text-align:right;"><div style="font-size:0.875rem;font-weight:600;">${state.user.displayName}</div><div style="font-size:0.75rem;color:var(--text-secondary);">${state.user.role}</div></div>
          <button onclick="logout()" class="btn btn-secondary btn-sm">Logout</button>
        </div>
      ` : `
        <button onclick="navigate('login')" class="btn btn-ghost">Login</button>
        <button onclick="navigate('register')" class="btn btn-primary">Get Started</button>
      `}
    </div>
  </nav>`;

const Sidebar = () => {
  const isAdmin = state.user?.role === 'Administrator';
  const isInstructor = state.user?.role === 'Instructor';
  const p = state.currentPage;
  const btn = (page, icon, label) => `<button class="btn ${p === page ? 'btn-primary' : 'btn-ghost'} w-full" onclick="navigate('${page}')">${icon} <span class="btn-text">${label}</span></button>`;
  return `
    <aside class="sidebar glass">
      <button class="menu-toggle" onclick="toggleSidebar()">
        <span></span>
        <span></span>
        <span></span>
      </button>
      ${btn('dashboard', '📊', 'Overview')}
      ${!isAdmin ? btn('my-courses', '📚', 'My Courses') : ''}
      ${isInstructor ? btn('create-course', '➕', 'Create Course') : ''}
      ${btn('courses', '🔍', 'Browse')}
      ${btn('profile', '👤', 'Profile')}
      ${isAdmin ? `
        <div style="margin-top:1rem;border-top:1px solid var(--border);padding-top:1rem;">
          <p style="font-size:0.75rem;color:var(--text-muted);padding:0 0.5rem;margin-bottom:0.5rem;">ADMIN</p>
          ${btn('admin-analytics', '📈', 'Analytics')}
          ${btn('admin-users', '👥', 'Users')}
          ${btn('admin-courses', '📚', 'All Courses')}
          ${btn('admin-approvals', '✅', 'Approvals')}
          ${btn('admin-reviews', '⭐', 'Reviews')}
        </div>` : ''}
      <button onclick="logout()" class="btn btn-ghost w-full" style="margin-top:auto;color:var(--error-color);">🚪 <span class="btn-text">Logout</span></button>
    </aside>`;
};

window.toggleSidebar = () => {
  const sidebar = document.querySelector('.sidebar');
  if (sidebar) {
    sidebar.classList.toggle('expanded');
  }
};

window.toggleTheme = () => {
  const html = document.documentElement;
  const isDark = html.getAttribute('data-theme') === 'dark';
  if (isDark) {
    html.removeAttribute('data-theme');
    localStorage.setItem('theme', 'light');
  } else {
    html.setAttribute('data-theme', 'dark');
    localStorage.setItem('theme', 'dark');
  }
  renderDashboardContent();
};

// --- PAGE RENDERERS ---
function pgOverview() {
  const s = state.stats;
  const isInstructor = state.user?.role === 'Instructor';
  const isAdmin = state.user?.role === 'Administrator';
  return `
    <h2 style="margin-bottom:0.5rem;">Welcome, ${state.user?.displayName} 👋</h2>
    <p style="color:var(--text-secondary);margin-bottom:2rem;">here's your learning overview</p>
    <div style="display:grid;grid-template-columns:repeat(auto-fit,minmax(180px,1fr));gap:1.5rem;margin-bottom:2rem;">
      <div class="card glass" style="text-align:center;"><p style="color:var(--text-secondary);font-size:0.85rem;">Enrolled</p><h2 style="color:var(--primary-color);">${s?.totalCoursesEnrolled ?? 0}</h2></div>
      <div class="card glass" style="text-align:center;"><p style="color:var(--text-secondary);font-size:0.85rem;">Completed</p><h2 style="color:var(--accent-color);">${s?.totalCoursesCompleted ?? 0}</h2></div>
      <div class="card glass" style="text-align:center;"><p style="color:var(--text-secondary);font-size:0.85rem;">In Progress</p><h2 style="color:var(--warning-color);">${s?.coursesInProgress ?? 0}</h2></div>
      <div class="card glass" style="text-align:center;"><p style="color:var(--text-secondary);font-size:0.85rem;">Certificates</p><h2 style="color:var(--secondary-color);">${s?.certificatesEarned ?? 0}</h2></div>
    </div>
    ${!isAdmin ? pgMyCoursesMini() : ''}
    ${isInstructor ? `<div id="instructorSection" style="margin-top:2rem;"><p style="color:var(--text-secondary);">Loading your courses...</p></div>` : ''}
    ${isAdmin ? `<p style="color:var(--text-secondary);">Use the admin panel on the left to manage the platform.</p>` : ''}`;
}

function pgMyCoursesMini() {
  if (!state.myCourses?.length) return `
    <div class="card glass" style="text-align:center;padding:3rem;">
      <h3 style="margin-bottom:1rem;">No courses yet!</h3>
      <p style="color:var(--text-secondary);margin-bottom:1.5rem;">Start learning by enrolling in a course.</p>
      <button class="btn btn-primary" onclick="navigate('courses')">Browse Courses</button>
    </div>`;
  const active = state.myCourses.filter(r => r.status !== 'Withdrawn').slice(0, 4);
  return `
    <h3 style="margin-bottom:1rem;">Continue Learning</h3>
    <div style="display:grid;gap:1rem;">
      ${active.map(reg => `
        <div class="card glass" style="display:flex;justify-content:space-between;align-items:center;flex-wrap:wrap;gap:1rem;">
          <div style="flex:1;">
            <h4>${reg.courseTitle || 'Course #' + reg.courseId}</h4>
            <div class="progress-container"><div class="progress-bar" style="width:${reg.completionPercent}%;"></div></div>
            <p style="font-size:0.75rem;color:var(--text-muted);">${reg.completionPercent}% • <span style="color:${reg.status === 'Completed' ? 'var(--accent-color)' : 'var(--warning-color)'};">${reg.status}</span>${reg.credentialIssued ? ' • 🏆 Certified' : ''}</p>
          </div>
          <div style="display:flex;gap:0.5rem;flex-shrink:0;">
            ${reg.status === 'Active' ? `<button class="btn btn-primary btn-sm" onclick="window.showProgressModal(${reg.id},${reg.completionPercent})">Update Progress</button>` : ''}
          </div>
        </div>`).join('')}
    </div>
    ${state.myCourses.length > 4 ? `<div style="text-align:center;margin-top:1rem;"><button class="btn btn-ghost" onclick="navigate('my-courses')">View All ${state.myCourses.length} Courses →</button></div>` : ''}`;
}

function pgMyCourses() {
  if (!state.myCourses?.length) return `
    <h2 style="margin-bottom:2rem;">My Courses</h2>
    <div class="card glass" style="text-align:center;padding:3rem;">
      <h3 style="margin-bottom:1rem;">No courses yet!</h3>
      <p style="color:var(--text-secondary);margin-bottom:1.5rem;">Start learning today.</p>
      <button class="btn btn-primary" onclick="navigate('courses')">Browse Courses</button>
    </div>`;
  return `
    <h2 style="margin-bottom:2rem;">My Courses (${state.myCourses.length})</h2>
    <div style="display:grid;gap:1.5rem;">
      ${state.myCourses.map(reg => `
        <div class="card glass" style="cursor:pointer;" onclick="navigate('course-detail/${reg.courseId}')">
          <div style="display:flex;justify-content:space-between;align-items:flex-start;flex-wrap:wrap;gap:1rem;">
            <div style="flex:1;">
              <h3 style="margin-bottom:0.25rem;">${reg.courseTitle || 'Course #' + reg.courseId}</h3>
              <p style="font-size:0.85rem;color:var(--text-secondary);margin-bottom:0.5rem;">Enrolled: ${new Date(reg.registeredOn).toLocaleDateString()}${reg.finishedOn ? ' • Completed: ' + new Date(reg.finishedOn).toLocaleDateString() : ''}</p>
              <span style="background:${reg.status === 'Completed' ? 'rgba(16,185,129,0.2)' : reg.status === 'Withdrawn' ? 'rgba(239,68,68,0.2)' : 'rgba(245,158,11,0.2)'};color:${reg.status === 'Completed' ? 'var(--accent-color)' : reg.status === 'Withdrawn' ? 'var(--error-color)' : 'var(--warning-color)'};padding:0.2rem 0.6rem;border-radius:999px;font-size:0.75rem;font-weight:600;">${reg.status}</span>
              ${reg.credentialIssued ? `<span style="margin-left:0.5rem;background:rgba(99,102,241,0.2);color:var(--primary-color);padding:0.2rem 0.6rem;border-radius:999px;font-size:0.75rem;font-weight:600;">🏆 Certified</span>` : ''}
            </div>
            <div style="display:flex;flex-direction:column;gap:0.5rem;" onclick="event.stopPropagation()">
              ${reg.status === 'Active' ? `
                <button class="btn btn-primary btn-sm" onclick="window.showProgressModal(${reg.id},${reg.completionPercent})">Update Progress</button>
                <button class="btn btn-secondary btn-sm" onclick="window.withdrawCourse(${reg.id})">Withdraw</button>` : ''}
            </div>
          </div>
          <div class="progress-container" style="margin-top:1rem;"><div class="progress-bar" style="width:${reg.completionPercent}%;"></div></div>
          <p style="font-size:0.75rem;color:var(--text-muted);margin-top:0.25rem;">${reg.completionPercent}% complete</p>
        </div>`).join('')}
    </div>`;
}

function pgCreateCourse() {
  return `
    <h2 style="margin-bottom:2rem;">Create New Course</h2>
    <div class="card glass" style="max-width:700px;">
      <form id="createCourseForm">
        <div class="input-group"><label class="input-label">Title *</label><input type="text" class="input-field" id="cc-title" required minlength="5" maxlength="200" placeholder="e.g. Complete Web Development Bootcamp"></div>
        <div class="input-group"><label class="input-label">Synopsis</label><textarea class="input-field" id="cc-synopsis" rows="3" maxlength="1000" placeholder="Brief course description..."></textarea></div>
        <div style="display:grid;grid-template-columns:1fr 1fr;gap:1rem;">
          <div class="input-group"><label class="input-label">Topic *</label><input type="text" class="input-field" id="cc-topic" required placeholder="e.g. Web Development"></div>
          <div class="input-group"><label class="input-label">Language</label><input type="text" class="input-field" id="cc-language" value="English"></div>
        </div>
        <div style="display:grid;grid-template-columns:1fr 1fr;gap:1rem;">
          <div class="input-group"><label class="input-label">Difficulty</label><select class="input-field" id="cc-difficulty"><option value="0">Beginner</option><option value="1">Intermediate</option><option value="2">Advanced</option></select></div>
          <div class="input-group"><label class="input-label">Price ($)</label><input type="number" class="input-field" id="cc-price" min="0" step="0.01" value="0"></div>
        </div>
        <div class="input-group"><label class="input-label">Cover Image URL</label><input type="url" class="input-field" id="cc-cover" placeholder="https://..."></div>
        <button type="submit" class="btn btn-primary w-full">Create Course</button>
      </form>
    </div>`;
}

function pgEditCourse() {
  return `
    <h2 style="margin-bottom:2rem;">Edit Course</h2>
    <div class="card glass" style="max-width:700px;">
      <form id="editCourseForm">
        <div class="input-group"><label class="input-label">Title *</label><input type="text" class="input-field" id="ec-title" required minlength="5" maxlength="200" placeholder="e.g. Complete Web Development Bootcamp"></div>
        <div class="input-group"><label class="input-label">Synopsis</label><textarea class="input-field" id="ec-synopsis" rows="3" maxlength="1000" placeholder="Brief course description..."></textarea></div>
        <div style="display:grid;grid-template-columns:1fr 1fr;gap:1rem;">
          <div class="input-group"><label class="input-label">Topic *</label><input type="text" class="input-field" id="ec-topic" required placeholder="e.g. Web Development"></div>
          <div class="input-group"><label class="input-label">Language</label><input type="text" class="input-field" id="ec-language" value="English"></div>
        </div>
        <div style="display:grid;grid-template-columns:1fr 1fr;gap:1rem;">
          <div class="input-group"><label class="input-label">Difficulty</label><select class="input-field" id="ec-difficulty"><option value="0">Beginner</option><option value="1">Intermediate</option><option value="2">Advanced</option></select></div>
          <div class="input-group"><label class="input-label">Price ($)</label><input type="number" class="input-field" id="ec-price" min="0" step="0.01" value="0"></div>
        </div>
        <div class="input-group"><label class="input-label">Cover Image URL *</label><input type="url" class="input-field" id="ec-cover" required placeholder="https://..."></div>
        <button type="submit" class="btn btn-primary w-full">Update Course</button>
        <button type="button" class="btn btn-secondary w-full" style="margin-top:0.5rem;" onclick="navigate('dashboard')">Cancel</button>
      </form>
    </div>
    <div style="margin-top:2rem;">
      <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:1rem;">
        <h3>Lessons</h3>
        <button class="btn btn-primary btn-sm" onclick="window.showAddLessonModalForEdit()">+ Add Lesson</button>
      </div>
      <div id="editCourseLessons"><p style="color:var(--text-secondary);">Loading lessons...</p></div>
    </div>`;
}

function pgProfile() {
  const u = state.user;
  const hasProfilePic = u.profilePictureUrl || u.picture;
  return `
    <h2 style="margin-bottom:2rem;">My Profile</h2>
    <div class="card glass" style="max-width:600px;">
      <div style="display:flex;align-items:center;gap:1.5rem;margin-bottom:2rem;">
        <div style="width:80px;height:80px;border-radius:50%;background:linear-gradient(135deg,var(--primary-color),var(--secondary-color));display:flex;align-items:center;justify-content:center;font-size:2rem;font-weight:700;overflow:hidden;position:relative;">
          ${hasProfilePic
      ? `<img src="${u.profilePictureUrl || u.picture}" style="width:100%;height:100%;object-fit:cover;" onerror="this.style.display='none';this.parentElement.innerHTML='${u.displayName?.[0]?.toUpperCase() || '?'}'">`
      : `${u.displayName?.[0]?.toUpperCase() || '?'}`
    }
        </div>
        <div><h3>${u.displayName}</h3><p style="color:var(--text-secondary);">${u.email}</p><span style="background:rgba(99,102,241,0.2);color:var(--primary-color);padding:0.2rem 0.6rem;border-radius:999px;font-size:0.75rem;font-weight:600;">${u.role}</span></div>
      </div>
      <form id="profileForm">
        <div class="input-group"><label class="input-label">Display Name</label><input type="text" class="input-field" id="prof-name" value="${u.displayName}"></div>
        <div class="input-group">
          <label class="input-label">Profile Picture</label>
          <div style="display:flex;gap:1rem;align-items:center;margin-bottom:0.5rem;">
            <input type="file" class="input-field" id="prof-pic-file" accept="image/*" style="flex:1;">
            <button type="button" class="btn btn-secondary btn-sm" onclick="document.getElementById('prof-pic-file').click()">Upload</button>
          </div>
          <p style="font-size:0.75rem;color:var(--text-muted);margin-bottom:0.5rem;">Or enter URL:</p>
          <input type="url" class="input-field" id="prof-pic" value="${u.profilePictureUrl || ''}" placeholder="https://...">
        </div>
        <button type="submit" class="btn btn-primary">Save Changes</button>
      </form>
      <hr style="border-color:var(--glass-border);margin:2rem 0;">
      <h4 style="margin-bottom:1rem;">Change Password</h4>
      <form id="passwordForm">
        <div class="input-group"><label class="input-label">Current Password</label><input type="password" class="input-field" id="pw-current"></div>
        <div class="input-group"><label class="input-label">New Password (min 8 chars)</label><input type="password" class="input-field" id="pw-new" minlength="8"></div>
        <button type="submit" class="btn btn-secondary">Change Password</button>
      </form>
    </div>`;
}

function pgAdminAnalytics() {
  const a = state.adminAnalytics;
  if (!a) return `<h2>Analytics</h2><p style="color:var(--text-secondary);margin-top:1rem;">Loading...</p>`;
  return `
    <h2 style="margin-bottom:2rem;">Platform Analytics</h2>
    <div style="display:grid;grid-template-columns:repeat(auto-fit,minmax(180px,1fr));gap:1.5rem;margin-bottom:2rem;">
      <div class="card glass" style="text-align:center;"><p style="color:var(--text-secondary);font-size:0.85rem;">Total Users</p><h2 style="color:var(--primary-color);">${a.users?.totalUsers ?? 0}</h2><p style="font-size:0.75rem;color:var(--text-muted);">+${a.users?.newUsersThisMonth ?? 0} this month</p></div>
      <div class="card glass" style="text-align:center;"><p style="color:var(--text-secondary);font-size:0.85rem;">Active Users</p><h2 style="color:var(--accent-color);">${a.users?.activeUsers ?? 0}</h2><p style="font-size:0.75rem;color:var(--text-muted);">${a.users?.suspendedUsers ?? 0} suspended</p></div>
      <div class="card glass" style="text-align:center;"><p style="color:var(--text-secondary);font-size:0.85rem;">Total Courses</p><h2 style="color:var(--warning-color);">${a.courses?.totalCourses ?? 0}</h2><p style="font-size:0.75rem;color:var(--text-muted);">${a.courses?.pendingApproval ?? 0} pending</p></div>
      <div class="card glass" style="text-align:center;"><p style="color:var(--text-secondary);font-size:0.85rem;">Enrollments</p><h2 style="color:var(--secondary-color);">${a.enrollments?.totalEnrollments ?? 0}</h2><p style="font-size:0.75rem;color:var(--text-muted);">${(a.enrollments?.completionRate ?? 0).toFixed(1)}% completion</p></div>
      <div class="card glass" style="text-align:center;"><p style="color:var(--text-secondary);font-size:0.85rem;">Revenue</p><h2 style="color:var(--accent-color);">$${(a.revenue?.totalRevenue ?? 0).toFixed(0)}</h2><p style="font-size:0.75rem;color:var(--text-muted);">$${(a.revenue?.revenueThisMonth ?? 0).toFixed(0)} this month</p></div>
    </div>
    <h3 style="margin-bottom:1rem;">User Breakdown</h3>
    <div style="display:grid;grid-template-columns:repeat(3,1fr);gap:1rem;margin-bottom:2rem;">
      <div class="card glass" style="text-align:center;"><p style="color:var(--text-secondary);">Learners</p><h3>${a.users?.learners ?? 0}</h3></div>
      <div class="card glass" style="text-align:center;"><p style="color:var(--text-secondary);">Instructors</p><h3>${a.users?.instructors ?? 0}</h3></div>
      <div class="card glass" style="text-align:center;"><p style="color:var(--text-secondary);">Admins</p><h3>${a.users?.administrators ?? 0}</h3></div>
    </div>
    ${a.popularCourses?.length ? `
      <h3 style="margin-bottom:1rem;">Popular Courses</h3>
      <div class="card glass" style="overflow:auto;">
        <table style="width:100%;border-collapse:collapse;">
          <thead><tr style="border-bottom:1px solid var(--glass-border);">
            <th style="text-align:left;padding:0.75rem;color:var(--text-secondary);">Course</th>
            <th style="text-align:right;padding:0.75rem;color:var(--text-secondary);">Enrollments</th>
            <th style="text-align:right;padding:0.75rem;color:var(--text-secondary);">Avg Rating</th>
            <th style="text-align:right;padding:0.75rem;color:var(--text-secondary);">Revenue</th>
          </tr></thead>
          <tbody>${a.popularCourses.map(c => `
            <tr style="border-bottom:1px solid var(--glass-border);">
              <td style="padding:0.75rem;">${c.title}</td>
              <td style="text-align:right;padding:0.75rem;">${c.enrollmentCount}</td>
              <td style="text-align:right;padding:0.75rem;">${c.averageRating.toFixed(1)} ⭐</td>
              <td style="text-align:right;padding:0.75rem;">$${Number(c.revenue).toFixed(0)}</td>
            </tr>`).join('')}</tbody>
        </table>
      </div>` : ''}`;
}

function renderUsersTable(users) {
  if (!users?.length) return `<p style="color:var(--text-secondary);">No users found.</p>`;
  return `
    <div class="card glass" style="overflow:auto;">
      <table style="width:100%;border-collapse:collapse;">
        <thead><tr style="border-bottom:1px solid var(--glass-border);">
          <th style="text-align:left;padding:0.75rem;color:var(--text-secondary);">Name</th>
          <th style="text-align:left;padding:0.75rem;color:var(--text-secondary);">Email</th>
          <th style="text-align:left;padding:0.75rem;color:var(--text-secondary);">Role</th>
          <th style="text-align:left;padding:0.75rem;color:var(--text-secondary);">Status</th>
          <th style="text-align:right;padding:0.75rem;color:var(--text-secondary);">Actions</th>
        </tr></thead>
        <tbody>${users.map(u => `
          <tr style="border-bottom:1px solid var(--glass-border);">
            <td style="padding:0.75rem;">${u.displayName}</td>
            <td style="padding:0.75rem;color:var(--text-secondary);font-size:0.85rem;">${u.email}</td>
            <td style="padding:0.75rem;"><span style="background:rgba(99,102,241,0.15);color:var(--primary-color);padding:0.2rem 0.6rem;border-radius:999px;font-size:0.75rem;">${u.role}</span></td>
            <td style="padding:0.75rem;"><span style="color:${u.isActive ? 'var(--accent-color)' : 'var(--error-color)'};">${u.isActive ? '✓ Active' : '⊘ Suspended'}</span></td>
            <td style="padding:0.75rem;text-align:right;">
              ${u.id !== state.user?.id ? `
                ${u.isActive
        ? `<button class="btn btn-secondary btn-sm" onclick="window.suspendUser(${u.id})">Suspend</button>`
        : `<button class="btn btn-primary btn-sm" onclick="window.reactivateUser(${u.id})">Reactivate</button>`}
                <button class="btn btn-secondary btn-sm" style="color:var(--error-color);margin-left:0.5rem;" onclick="window.deleteUser(${u.id},'${u.displayName.replace(/'/g, "\\'")}')">Delete</button>
              `: '<span style="color:var(--text-muted);font-size:0.85rem;">You</span>'}
            </td>
          </tr>`).join('')}</tbody>
      </table>
    </div>`;
}

function pgAdminUsers() {
  return `
    <h2 style="margin-bottom:1rem;">User Management</h2>
    <div style="display:flex;gap:1rem;margin-bottom:2rem;flex-wrap:wrap;">
      <input type="text" class="input-field" id="userSearchInput" placeholder="Search by name or email..." style="max-width:360px;" oninput="window.searchUsers(this.value)">
      <select class="input-field" id="roleFilter" style="max-width:180px;" onchange="window.filterUsers(this.value)">
        <option value="">All Roles</option>
        <option value="Learner">Learners</option>
        <option value="Instructor">Instructors</option>
        <option value="Administrator">Admins</option>
      </select>
    </div>
    <div id="usersTable">${renderUsersTable(state.adminUsers)}</div>`;
}

function pgAdminApprovals() {
  const courses = state.adminPendingCourses;
  return `
    <h2 style="margin-bottom:2rem;">Course Approvals</h2>
    ${!courses?.length
      ? `<div class="card glass" style="text-align:center;padding:3rem;"><p style="color:var(--text-secondary);">No courses pending approval 🎉</p></div>`
      : `<div style="display:grid;gap:1rem;">${courses.map(c => `
          <div class="card glass">
            <div style="display:flex;justify-content:space-between;align-items:flex-start;flex-wrap:wrap;gap:1rem;">
              <div style="flex:1;">
                <h3 style="margin-bottom:0.25rem;">${c.title}</h3>
                <p style="color:var(--text-secondary);font-size:0.85rem;margin-bottom:0.5rem;">${c.synopsis || 'No description'}</p>
                <div style="display:flex;gap:1rem;flex-wrap:wrap;font-size:0.8rem;color:var(--text-muted);">
                  <span>👤 ${c.authorName || 'Unknown'}</span><span>📚 ${c.topic}</span><span>💰 $${c.listPrice}</span>
                </div>
              </div>
              <div style="display:flex;gap:0.5rem;">
                <button class="btn btn-primary btn-sm" onclick="window.approveCourse(${c.id})">✓ Approve</button>
                <button class="btn btn-secondary btn-sm" style="color:var(--error-color);" onclick="window.rejectCourse(${c.id})">✗ Reject</button>
              </div>
            </div>
          </div>`).join('')}</div>`}`;
}

function pgAdminReviews() {
  const reviews = state.adminPendingReviews;
  return `
    <h2 style="margin-bottom:2rem;">Review Moderation</h2>
    ${!reviews?.length
      ? `<div class="card glass" style="text-align:center;padding:3rem;"><p style="color:var(--text-secondary);">No pending reviews 🎉</p></div>`
      : `<div style="display:grid;gap:1rem;">${reviews.map(r => `
          <div class="card glass">
            <div style="display:flex;justify-content:space-between;align-items:flex-start;flex-wrap:wrap;gap:1rem;">
              <div style="flex:1;">
                <div style="display:flex;gap:1rem;align-items:center;margin-bottom:0.5rem;">
                  <strong>${r.learnerName || 'User'}</strong>
                  <span>${'⭐'.repeat(r.starRating)}</span>
                  <span style="color:var(--text-muted);font-size:0.8rem;">${r.courseTitle}</span>
                </div>
                <p style="color:var(--text-secondary);">${r.reviewText || 'No comment'}</p>
                <p style="font-size:0.75rem;color:var(--text-muted);margin-top:0.5rem;">${new Date(r.submittedOn).toLocaleDateString()}</p>
              </div>
              <div style="display:flex;gap:0.5rem;">
                <button class="btn btn-primary btn-sm" onclick="window.approveReview(${r.id})">✓ Approve</button>
                <button class="btn btn-secondary btn-sm" style="color:var(--error-color);" onclick="window.rejectReview(${r.id})">✗ Reject</button>
                <button class="btn btn-secondary btn-sm" onclick="window.deleteReview(${r.id})">🗑</button>
              </div>
            </div>
          </div>`).join('')}</div>`}`;
}

function pgAdminCourses() {
  return `
    <h2 style="margin-bottom:1rem;">All Courses Management</h2>
    <div style="display:flex;gap:1rem;margin-bottom:2rem;">
      <input type="text" class="input-field" id="adminCourseSearch" placeholder="Search courses..." style="max-width:360px;" oninput="window.filterAdminCourses(this.value)">
    </div>
    <div id="adminCoursesList"><p style="color:var(--text-secondary);">Loading courses...</p></div>
  `;
}

async function loadAdminCourses() {
  const courses = await apiFetch('/api/courses/api/courses');
  if (courses) {
    state.adminAllCourses = courses;
    renderAdminCoursesList(courses);
  }
}

function renderAdminCoursesList(courses) {
  const container = document.getElementById('adminCoursesList');
  if (!container) return;

  if (!courses?.length) {
    container.innerHTML = `<p style="color:var(--text-secondary);">No courses found.</p>`;
    return;
  }

  container.innerHTML = `
    <div class="card glass" style="overflow:auto;">
      <table style="width:100%;border-collapse:collapse;">
        <thead><tr style="border-bottom:1px solid var(--glass-border);">
          <th style="text-align:left;padding:0.75rem;color:var(--text-secondary);">Course</th>
          <th style="text-align:left;padding:0.75rem;color:var(--text-secondary);">Author</th>
          <th style="text-align:left;padding:0.75rem;color:var(--text-secondary);">Status</th>
          <th style="text-align:right;padding:0.75rem;color:var(--text-secondary);">Actions</th>
        </tr></thead>
        <tbody>${courses.map(c => `
          <tr style="border-bottom:1px solid var(--glass-border);">
            <td style="padding:0.75rem;">
              <div style="font-weight:600;">${c.title}</div>
              <div style="font-size:0.75rem;color:var(--text-muted);">${c.topic}</div>
            </td>
            <td style="padding:0.75rem;color:var(--text-secondary);">${c.authorName || 'Unknown'}</td>
            <td style="padding:0.75rem;">
              <span style="background:${c.isApproved ? 'rgba(16,185,129,0.2)' : c.isApproved === false ? 'rgba(239,68,68,0.2)' : 'rgba(245,158,11,0.2)'};color:${c.isApproved ? 'var(--accent-color)' : c.isApproved === false ? 'var(--error-color)' : 'var(--warning-color)'};padding:0.2rem 0.6rem;border-radius:999px;font-size:0.75rem;font-weight:600;">
                ${c.isApproved ? 'Approved' : c.isApproved === false ? 'Rejected' : 'Pending'}
              </span>
            </td>
            <td style="padding:0.75rem;text-align:right;">
              <button class="btn btn-primary btn-sm" onclick="navigate('edit-course/${c.id}')">Edit</button>
              ${!c.isApproved ? `<button class="btn btn-secondary btn-sm" style="margin-left:0.5rem;" onclick="window.adminApproveCourse(${c.id})">Approve</button>` : ''}
            </td>
          </tr>`).join('')}</tbody>
      </table>
    </div>`;
}

window.filterAdminCourses = (term) => {
  const filtered = state.adminAllCourses?.filter(c =>
    c.title?.toLowerCase().includes(term.toLowerCase()) ||
    c.topic?.toLowerCase().includes(term.toLowerCase()) ||
    c.authorName?.toLowerCase().includes(term.toLowerCase())
  ) || [];
  renderAdminCoursesList(filtered);
};

window.adminApproveCourse = async (courseId) => {
  const res = await apiFetch(`/api/courses/api/courses/${courseId}/approve`, { method: 'POST' });
  if (res) {
    showToast('Course approved!', 'success');
    loadAdminCourses();
  }
};

// --- RENDER DASHBOARD CONTENT ---
function renderDashboardContent() {
  const content = document.getElementById('dashboardContent');
  if (!content) return;
  const page = state.currentPage;
  const map = {
    'dashboard': pgOverview,
    'my-courses': pgMyCourses,
    'create-course': pgCreateCourse,
    'profile': pgProfile,
    'admin-analytics': pgAdminAnalytics,
    'admin-users': pgAdminUsers,
    'admin-approvals': pgAdminApprovals,
    'admin-reviews': pgAdminReviews,
    'admin-courses': pgAdminCourses
  };
  content.innerHTML = `<div class="dashboard-logo">
    <button class="theme-toggle" onclick="toggleTheme()" title="Toggle theme">
      <svg class="moon-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"></path>
      </svg>
      <svg class="sun-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <circle cx="12" cy="12" r="5"></circle>
        <line x1="12" y1="1" x2="12" y2="3"></line>
        <line x1="12" y1="21" x2="12" y2="23"></line>
        <line x1="4.22" y1="4.22" x2="5.64" y2="5.64"></line>
        <line x1="18.36" y1="18.36" x2="19.78" y2="19.78"></line>
        <line x1="1" y1="12" x2="3" y2="12"></line>
        <line x1="21" y1="12" x2="23" y2="12"></line>
        <line x1="4.22" y1="19.78" x2="5.64" y2="18.36"></line>
        <line x1="18.36" y1="5.64" x2="19.78" y2="4.22"></line>
      </svg>
    </button>
    <span onclick="navigate('home')">⚛️ EduLearn</span>
  </div>` + (map[page] || pgOverview)();
  attachListeners(page);
}

function attachListeners(page) {
  if (page === 'create-course') {
    document.getElementById('createCourseForm')?.addEventListener('submit', async e => {
      e.preventDefault();
      const btn = e.target.querySelector('button[type=submit]');
      btn.disabled = true; btn.textContent = 'Creating...';
      await createCourse();
      btn.disabled = false; btn.textContent = 'Create Course';
    });
  }
  if (page === 'admin-courses') {
    loadAdminCourses();
  }
  if (page === 'admin-approvals') {
    loadAdminPendingCourses();
  }
  if (page.startsWith('edit-course')) {
    const courseId = page.split('/')[1];
    loadEditCourse(courseId);

    document.getElementById('editCourseForm')?.addEventListener('submit', async e => {
      e.preventDefault();
      const btn = e.target.querySelector('button[type=submit]');
      btn.disabled = true; btn.textContent = 'Updating...';
      await updateCourse(courseId);
      btn.disabled = false; btn.textContent = 'Update Course';
    });
  }
  if (page === 'profile') {
    document.getElementById('profileForm')?.addEventListener('submit', async e => {
      e.preventDefault();
      const fileInput = document.getElementById('prof-pic-file');
      const urlInput = document.getElementById('prof-pic');
      let profilePictureUrl = urlInput.value;

      // Handle file upload with FormData
      if (fileInput.files && fileInput.files[0]) {
        const file = fileInput.files[0];
        console.log('Uploading file:', file.name, file.type, file.size);

        const formData = new FormData();
        formData.append('file', file);

        try {
          const uploadRes = await fetch(`${API_BASE}/api/identity/api/accounts/me/profile-picture`, {
            method: 'POST',
            headers: {
              ...(state.token ? { 'Authorization': `Bearer ${state.token}` } : {})
            },
            body: formData
          });

          if (uploadRes.ok) {
            const uploadData = await uploadRes.json();
            profilePictureUrl = uploadData.url || uploadData.profilePictureUrl;
            console.log('Upload successful, URL:', profilePictureUrl);
          } else {
            const error = await uploadRes.json();
            console.error('Upload failed:', error);
            showToast('Failed to upload image. Using URL instead.', 'warning');
            // Fall back to URL input
            profilePictureUrl = urlInput.value;
          }
        } catch (err) {
          console.error('Upload error:', err);
          showToast('Upload failed. Please use a URL instead.', 'warning');
          profilePictureUrl = urlInput.value;
        }
      }

      await updateProfile(profilePictureUrl);
    });

    async function updateProfile(profilePictureUrl) {
      const displayName = document.getElementById('prof-name').value;
      console.log('Updating profile:', { displayName, profilePictureUrl: profilePictureUrl?.substring(0, 50) + '...' });

      const res = await apiFetch('/api/identity/api/accounts/me/profile', {
        method: 'PATCH',
        body: JSON.stringify({
          displayName: displayName,
          profilePictureUrl: profilePictureUrl
        })
      });

      console.log('Profile update response:', res);

      if (res) {
        state.user = { ...state.user, displayName: res.displayName, profilePictureUrl: res.profilePictureUrl };
        localStorage.setItem('user', JSON.stringify(state.user));
        showToast('Profile updated!', 'success');
        render();
      } else {
        showToast('Failed to update profile. Please check the console for details.', 'error');
      }
    }

    document.getElementById('passwordForm')?.addEventListener('submit', async e => {
      e.preventDefault();
      const res = await apiFetch('/api/identity/api/accounts/me/password', {
        method: 'PATCH',
        body: JSON.stringify({ currentPassword: document.getElementById('pw-current').value, newPassword: document.getElementById('pw-new').value })
      });
      if (res) { showToast('Password changed!', 'success'); e.target.reset(); }
    });
  }
  if (page === 'dashboard' && state.user?.role === 'Instructor') {
    setTimeout(loadInstructorCourses, 50);
  }
}

// --- MAIN RENDER ---
async function render() {
  const app = document.getElementById('app');

  // Check for OAuth callback token in query string (?token=...) — set by backend redirect
  const urlParams = new URLSearchParams(window.location.search);
  const queryToken = urlParams.get('token');
  if (queryToken) {
    state.token = queryToken;
    localStorage.setItem('token', queryToken);
    // Clean up URL so token doesn't stay in address bar
    window.history.replaceState({}, document.title, '/#dashboard');
    state.currentPage = 'dashboard';
    const profile = await apiFetch('/api/identity/api/accounts/me');
    if (profile) {
      state.user = profile;
      localStorage.setItem('user', JSON.stringify(profile));
      showToast(`Welcome, ${profile.displayName}! 🎉`, 'success');
    }
    // Fall through to render dashboard
  }

  const page = state.currentPage;

  if (page.startsWith('login-success')) {
    // Legacy hash-based token (#login-success?token=...)
    const token = new URLSearchParams(window.location.hash.split('?')[1]).get('token');
    if (token) {
      state.token = token;
      localStorage.setItem('token', token);
      const profile = await apiFetch('/api/identity/api/accounts/me');
      if (profile) {
        state.user = profile;
        localStorage.setItem('user', JSON.stringify(profile));
        showToast(`Welcome back, ${profile.displayName}!`, 'success');
        navigate('dashboard'); return;
      }
    }
    navigate('login'); return;
  }

  if (page === 'login' || page === 'register') {
    const tpl = page === 'login' ? loginTpl() : registerTpl();
    app.innerHTML = tpl;
    setupAuthListeners(page); return;
  }

  if (!page || page === 'home') {
    app.innerHTML = Nav() + homeTpl(); return;
  }

  if (page === 'courses') {
    app.innerHTML = Nav() + coursesTpl();
    loadCourses(); return;
  }

  if (page.startsWith('course-detail')) {
    const courseId = page.split('/')[1];
    app.innerHTML = Nav() + courseDetailTpl();
    loadCourseDetail(courseId); return;
  }

  if (page.startsWith('edit-course')) {
    const courseId = page.split('/')[1];
    app.innerHTML = `<div style="display:flex;min-height:100vh;">${Sidebar()}<main class="main-content" id="dashboardContent">${pgEditCourse()}</main></div>`;
    attachListeners(page); return;
  }

  const dashPages = ['dashboard', 'my-courses', 'create-course', 'profile', 'admin-analytics', 'admin-users', 'admin-approvals', 'admin-reviews', 'course-detail'];
  if (dashPages.includes(page)) {
    if (!state.user) { navigate('login'); return; }
    app.innerHTML = `<div style="display:flex;min-height:100vh;">${Sidebar()}<main class="main-content" id="dashboardContent"><p style="color:var(--text-secondary);">Loading...</p></main></div>`;
    await loadDashboardPage(page); return;
  }

  app.innerHTML = Nav() + `<div class="main-container" style="text-align:center;padding:5rem 0;"><h1>404</h1><p style="color:var(--text-secondary);margin:1rem 0;">Page not found</p><button class="btn btn-primary" onclick="navigate('home')">Go Home</button></div>`;
}

function loginTpl() {
  return `<div class="auth-page"><div class="card auth-card glass animate-fade">
    <h2 style="margin-bottom:2rem;">Welcome Back</h2>
    <form id="loginForm">
      <div class="input-group"><label class="input-label">Email</label><input type="email" class="input-field" id="email" required></div>
      <div class="input-group"><label class="input-label">Password</label><input type="password" class="input-field" id="password" required></div>
      <button type="submit" class="btn btn-primary w-full">Sign In</button>
    </form>
    <div style="margin:1.5rem 0;display:flex;align-items:center;gap:1rem;"><div style="flex:1;height:1px;background:var(--glass-border);"></div><span style="font-size:0.8rem;color:var(--text-muted);">OR</span><div style="flex:1;height:1px;background:var(--glass-border);"></div></div>
    <button onclick="window.loginWithGoogle()" class="btn btn-secondary w-full" style="gap:0.75rem;"><img src="https://www.google.com/favicon.ico" style="width:18px;height:18px;"> Continue with Google</button>
    <p style="margin-top:1.5rem;text-align:center;color:var(--text-secondary);">New here? <a href="#register" onclick="navigate('register')" style="color:var(--primary-color);">Register</a></p>
  </div></div>`;
}

function registerTpl() {
  return `<div class="auth-page"><div class="card auth-card glass animate-fade">
    <h2 style="margin-bottom:2rem;">Create Account</h2>
    <button onclick="window.registerWithGoogle()" class="btn btn-secondary w-full" style="gap:0.75rem;margin-bottom:1.5rem;"><img src="https://www.google.com/favicon.ico" style="width:18px;height:18px;"> Continue with Google</button>
    <div style="display:flex;align-items:center;margin:1.5rem 0;"><div style="flex:1;height:1px;background:var(--border);"></div><span style="padding:0 1rem;color:var(--text-muted);font-size:0.875rem;">or</span><div style="flex:1;height:1px;background:var(--border);"></div></div>
    <form id="registerForm">
      <div class="input-group"><label class="input-label">Name</label><input type="text" class="input-field" id="reg-name" required></div>
      <div class="input-group"><label class="input-label">Email</label><input type="email" class="input-field" id="reg-email" required></div>
      <div class="input-group"><label class="input-label">Password</label><input type="password" class="input-field" id="reg-password" required></div>
      <div class="input-group"><label class="input-label">Role</label><select class="input-field" id="reg-role"><option value="Learner">Student</option><option value="Instructor">Instructor</option></select></div>
      <button type="submit" class="btn btn-primary w-full">Join Now</button>
    </form>
    <p style="margin-top:1.5rem;text-align:center;color:var(--text-secondary);">Have account? <a href="#login" onclick="navigate('login')" style="color:var(--primary-color);">Sign In</a></p>
  </div></div>`;
}

function homeTpl() {
  return `<div class="main-container" style="padding-top:5rem;display:grid;grid-template-columns:1.2fr 1fr;gap:4rem;align-items:center;">
    <div>
      <h1 style="font-size:4.5rem;margin-bottom:1.5rem;line-height:1.1;">Master Any Skill<br/><span style="color:var(--primary-color)">At Your Own Pace.</span></h1>
      <p style="font-size:1.25rem;color:var(--text-secondary);max-width:600px;margin-bottom:3rem;">EduLearn provides a premium learning experience with world-class instructors, structured paths, and verifiable certifications.</p>
      <div style="display:flex;gap:1.5rem;">
        <button class="btn btn-primary btn-lg" onclick="navigate('courses')">Explore Courses</button>
        <button class="btn btn-secondary btn-lg" onclick="navigate('register')">Start Teaching</button>
      </div>
    </div>
    <div class="animate-fade"><img src="/lms_landing_hero.png" style="width:100%;border-radius:var(--radius-lg);box-shadow:var(--shadow-lg);" alt="EduLearn Hero" onerror="this.style.display='none'"></div>
  </div>`;
}

function coursesTpl() {
  return `<div class="main-container" style="padding:5rem 0;">
    <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:2rem;flex-wrap:wrap;gap:1rem;">
      <h2>Course Catalog</h2>
      <input type="text" class="input-field" id="courseSearch" placeholder="Search courses..." style="width:280px;" oninput="window.filterCourses(this.value)">
    </div>
    <div id="courseCatalogGrid" style="display:grid;grid-template-columns:repeat(auto-fill,minmax(300px,1fr));gap:2rem;"><p>Loading catalog...</p></div>
  </div>`;
}

function courseDetailTpl() {
  return `<div class="main-container" style="padding:5rem 0;">
    <div id="courseDetailContent"><p>Loading course details...</p></div>
  </div>`;
}

// --- AUTH LISTENERS ---
function setupAuthListeners(page) {
  const form = document.getElementById(page === 'login' ? 'loginForm' : 'registerForm');
  if (!form) return;
  form.addEventListener('submit', async e => {
    e.preventDefault();
    const btn = form.querySelector('button[type=submit]');
    btn.disabled = true; btn.textContent = 'Please wait...';
    if (page === 'login') {
      await login(document.getElementById('email').value, document.getElementById('password').value);
    } else {
      await register(document.getElementById('reg-name').value, document.getElementById('reg-email').value, document.getElementById('reg-password').value, document.getElementById('reg-role').value);
    }
    btn.disabled = false; btn.textContent = page === 'login' ? 'Sign In' : 'Join Now';
  });
}

// --- DATA LOADERS ---
async function loadMyCourses() {
  console.log('loadMyCourses called');
  console.log('state.user:', state.user);
  if (!state.user) {
    console.log('No user found in loadMyCourses');
    return;
  }
  console.log('Fetching courses for user ID:', state.user.id);
  const regs = await apiFetch(`/api/registration/api/registrations/learner/${state.user.id}`);
  console.log('Received registrations:', regs);
  if (regs) {
    state.myCourses = regs;
    console.log('Updated state.myCourses:', state.myCourses);
    // Also refresh stats
    const stats = await apiFetch(`/api/analytics/api/analytics/student/${state.user.id}/stats`);
    if (stats) state.stats = stats;
    console.log('Updated stats:', state.stats);
    renderDashboardContent();
  } else {
    console.log('Failed to fetch registrations');
  }
}

async function loadDashboardPage(page) {
  console.log('loadDashboardPage called with:', page);
  console.log('state.user:', state.user);
  if (page === 'dashboard') {
    console.log('Loading dashboard data...');
    const [stats, regs] = await Promise.all([
      apiFetch(`/api/analytics/api/analytics/student/${state.user.id}/stats`),
      apiFetch(`/api/registration/api/registrations/learner/${state.user.id}`)
    ]);
    console.log('Dashboard data received - stats:', stats, 'regs:', regs);
    if (stats) state.stats = stats;
    if (regs) state.myCourses = regs;
    console.log('Updated state - myCourses:', state.myCourses);
    renderDashboardContent();
    if (state.user.role === 'Instructor') loadInstructorCourses();
  } else if (page === 'my-courses') {
    await loadMyCourses();
  } else if (page === 'admin-analytics') {
    renderDashboardContent();
    const a = await apiFetch('/api/analytics/api/admin/analytics/dashboard');
    if (a) { state.adminAnalytics = a; renderDashboardContent(); }
  } else if (page === 'admin-users') {
    renderDashboardContent();
    const [learners, instructors] = await Promise.all([
      apiFetch('/api/identity/api/accounts/by-role/Learner'),
      apiFetch('/api/identity/api/accounts/by-role/Instructor')
    ]);
    state.adminUsers = [...(learners || []), ...(instructors || [])];
    renderDashboardContent();
  } else if (page === 'admin-approvals') {
    renderDashboardContent();
    // Fetch all courses and filter pending (published but not approved)
    // The standard browse endpoint only shows approved courses.
    // We need a workaround: get from analytics pending count.
    // Since there's no direct admin list-all-courses endpoint in the gateway,
    // we show a helpful message and what we can.
    state.adminPendingCourses = [];
    const analytics = await apiFetch('/api/analytics/api/admin/analytics/dashboard');
    if (analytics) {
      state.adminAnalytics = analytics;
      // Show pending count from analytics but note limitation
    }
    renderDashboardContent();
    // Try to get pending via a search that returns all - use empty search which falls back to browse
    // Actually: browse only returns approved. We'll show a notice.
    const infoEl = document.getElementById('dashboardContent');
    if (infoEl && state.adminAnalytics?.courses?.pendingApproval > 0) {
      infoEl.innerHTML += `<div class="card glass" style="margin-top:1rem;border-left:4px solid var(--warning-color);padding:1rem;">
        <p>ℹ️ ${state.adminAnalytics.courses.pendingApproval} course(s) are pending approval. The courses API requires a dedicated admin endpoint to list unapproved courses. Ask your backend team to expose <code>GET /api/courses/api/courses/pending</code> for full functionality.</p>
      </div>`;
    }
  } else if (page === 'admin-reviews') {
    renderDashboardContent();
    const reviews = await apiFetch('/api/reviews/api/admin/reviews/pending');
    if (reviews) { state.adminPendingReviews = reviews; renderDashboardContent(); }
  } else {
    renderDashboardContent();
  }
}

async function loadInstructorCourses() {
  const courses = await apiFetch('/api/courses/api/courses/my-courses');
  const section = document.getElementById('instructorSection');
  if (!section) return;
  if (!courses?.length) {
    section.innerHTML = `<div style="margin-top:2rem;padding-top:2rem;border-top:1px solid var(--glass-border);"><div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:1rem;"><h3>My Courses (Instructor)</h3><button class="btn btn-primary btn-sm" onclick="navigate('create-course')">+ New Course</button></div><p style="color:var(--text-secondary);">You haven't created any courses yet.</p></div>`;
    return;
  }
  section.innerHTML = `
    <div style="margin-top:2rem;padding-top:2rem;border-top:1px solid var(--glass-border);">
      <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:1rem;">
        <h3>My Courses (${courses.length})</h3>
        <button class="btn btn-primary btn-sm" onclick="navigate('create-course')">+ New Course</button>
      </div>
      <div style="display:grid;gap:1rem;">
        ${courses.map(c => `
          <div class="card glass" style="display:flex;justify-content:space-between;align-items:center;flex-wrap:wrap;gap:1rem;">
            <div><h4>${c.title}</h4>
              <div style="display:flex;gap:0.75rem;font-size:0.8rem;color:var(--text-muted);margin-top:0.25rem;">
                <span>${c.totalRegistrations} enrolled</span><span>$${c.listPrice}</span>
                <span style="color:${c.isApproved ? 'var(--accent-color)' : c.isPublished ? 'var(--warning-color)' : 'var(--text-muted)'};">${c.isApproved ? '✓ Live' : c.isPublished ? '⏳ In Review' : '📝 Draft'}</span>
              </div>
            </div>
            <div style="display:flex;gap:0.5rem;flex-shrink:0;">
              <button class="btn btn-secondary btn-sm" onclick="navigate('edit-course/${c.id}')">Edit</button>
              ${!c.isPublished ? `<button class="btn btn-primary btn-sm" onclick="window.submitCourseForReview(${c.id})">Submit for Review</button>` : ''}
              <button class="btn btn-secondary btn-sm" style="color:var(--error-color);" onclick="window.deleteCourse(${c.id})">Delete</button>
            </div>
          </div>`).join('')}
      </div>
    </div>`;
}

async function loadCourses() {
  const courses = await apiFetch('/api/courses/api/courses');
  if (state.user) {
    const regs = await apiFetch(`/api/registration/api/registrations/learner/${state.user.id}`);
    if (regs) state.myCourses = regs;
  }
  if (courses) {
    state.courses = courses;
    renderCourseGrid(courses);
  }
}

async function loadCourseDetail(courseId) {
  console.log('Loading course detail for courseId:', courseId);
  const course = await apiFetch(`/api/courses/api/courses/${courseId}`);
  console.log('Course data:', course);

  // Check if lessons are embedded in the course object under various possible names
  let lessons = course?.lessons || course?.modules || course?.courseModules || course?.lessonModules || [];
  console.log('Lessons from course object:', lessons);

  // Try various possible lesson API endpoints
  let lessonsEndpointFound = false;
  if (!lessons || lessons.length === 0) {
    const endpoints = [
      `/api/curriculum/api/curriculum/course/${courseId}`,
      `/api/courses/api/courses/${courseId}/lessons`,
      `/api/courses/api/courses/${courseId}/modules`,
      `/api/lessons/api/lessons?courseId=${courseId}`,
      `/api/modules/api/modules?courseId=${courseId}`,
      `/api/courses/api/lessons?courseId=${courseId}`,
      `/api/lessons/api/lessons/course/${courseId}`
    ];

    for (const endpoint of endpoints) {
      console.log('Trying endpoint:', endpoint);
      lessons = await apiFetch(endpoint);
      console.log('Result:', lessons);
      if (lessons && lessons.length > 0) {
        console.log('Found lessons at:', endpoint);
        lessonsEndpointFound = true;
        break;
      }
    }
  }

  // Store lessons in state for startLesson to access
  state.currentCourseLessons = lessons;

  const content = document.getElementById('courseDetailContent');
  if (!content) return;

  if (!course) {
    content.innerHTML = `<p style="color:var(--text-secondary);">Failed to load course details.</p>`;
    return;
  }

  const diff = ['Beginner', 'Intermediate', 'Advanced'];
  const isEnrolled = state.myCourses?.some(r => r.courseId === course.id && r.status !== 'Withdrawn');
  const isInstructor = state.user?.role === 'Instructor';
  const isCourseInstructor = course.authorId === state.user?.id;

  content.innerHTML = `
    <button class="btn btn-ghost btn-sm" onclick="navigate('courses')" style="margin-bottom:1.5rem;">← Back to Courses</button>
    <div style="display:grid;grid-template-columns:2fr 1fr;gap:2rem;">
      <div>
        ${course.coverImageUrl ? `<img src="${course.coverImageUrl}" style="width:100%;height:300px;object-fit:cover;border-radius:var(--radius-lg);margin-bottom:2rem;" onerror="this.style.display='none'">` : ''}
        <h1 style="margin-bottom:1rem;">${course.title}</h1>
        <div style="display:flex;gap:0.75rem;margin-bottom:1.5rem;flex-wrap:wrap;">
          <span style="background:rgba(99,102,241,0.15);color:var(--primary-color);padding:0.3rem 0.8rem;border-radius:999px;font-size:0.85rem;">${course.topic}</span>
          <span style="background:rgba(245,158,11,0.15);color:var(--warning-color);padding:0.3rem 0.8rem;border-radius:999px;font-size:0.85rem;">${diff[course.difficulty] || 'Unknown'}</span>
          <span style="background:rgba(16,185,129,0.15);color:var(--accent-color);padding:0.3rem 0.8rem;border-radius:999px;font-size:0.85rem;">${course.totalRegistrations} enrolled</span>
        </div>
        <p style="color:var(--text-secondary);line-height:1.7;margin-bottom:2rem;">${course.synopsis || 'No description available.'}</p>

        <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:1rem;">
          <h3 style="margin:0;">Lessons</h3>
          ${isInstructor && isCourseInstructor ? `<button class="btn btn-primary btn-sm" onclick="window.showAddLessonModal(${courseId})">+ Add Lesson</button>` : ''}
        </div>
        ${!lessons || !lessons.length
      ? `<div class="card glass" style="padding:2rem;text-align:center;">
              <p style="color:var(--text-secondary);">No lessons available yet.</p>
            </div>`
      : `<div style="display:grid;gap:1rem;">
              ${lessons.map((lesson, index) => `
                <div class="card glass" style="padding:1.5rem;display:flex;justify-content:space-between;align-items:center;flex-wrap:wrap;gap:1rem;">
                  <div style="flex:1;">
                    <div style="display:flex;align-items:center;gap:1rem;margin-bottom:0.5rem;">
                      <span style="background:var(--primary-color);color:white;width:32px;height:32px;border-radius:50%;display:flex;align-items:center;justify-content:center;font-weight:600;font-size:0.85rem;">${index + 1}</span>
                      <h4 style="margin:0;">${lesson.title || 'Lesson ' + (index + 1)}</h4>
                    </div>
                    <p style="font-size:0.85rem;color:var(--text-secondary);margin-left:3.25rem;">${lesson.videoUrl || 'No link set'}</p>
                  </div>
                  ${isEnrolled
          ? `<button class="btn btn-primary btn-sm" onclick="window.startLesson(${lesson.id})">Start Lesson</button>`
          : `<button class="btn btn-secondary btn-sm" disabled style="opacity:0.6;">Enroll to Access</button>`
        }
                </div>
              `).join('')}
            </div>`
    }
      </div>
      
      <div>
        <div class="card glass" style="position:sticky;top:2rem;">
          <h3 style="margin-bottom:1rem;">Course Info</h3>
          <div style="display:grid;gap:1rem;">
            <div>
              <p style="font-size:0.75rem;color:var(--text-muted);margin-bottom:0.25rem;">Instructor</p>
              <p style="font-weight:500;">${course.authorName || 'Unknown'}</p>
            </div>
            <div>
              <p style="font-size:0.75rem;color:var(--text-muted);margin-bottom:0.25rem;">Price</p>
              <p style="font-weight:700;font-size:1.25rem;color:var(--primary-color);">${course.listPrice > 0 ? '$' + course.listPrice : 'Free'}</p>
            </div>
            <div>
              <p style="font-size:0.75rem;color:var(--text-muted);margin-bottom:0.25rem;">Rating</p>
              <p style="font-weight:500;">${course.averageRating > 0 ? '⭐'.repeat(Math.min(Math.round(course.averageRating), 5)) + ' ' + course.averageRating.toFixed(1) : 'No ratings yet'}</p>
            </div>
            <div>
              <p style="font-size:0.75rem;color:var(--text-muted);margin-bottom:0.25rem;">Lessons</p>
              <p style="font-weight:500;">${lessons?.length || 0} lessons</p>
            </div>
          </div>
          <hr style="border-color:var(--glass-border);margin:1.5rem 0;">
          ${isEnrolled
      ? `<button class="btn btn-secondary w-full" disabled style="opacity:0.6;">✓ Enrolled</button>`
      : `<button class="btn btn-primary w-full" onclick="window.enrollCourse(${course.id})">Enroll Now</button>`
    }
        </div>
      </div>
    </div>
  `;
}

function renderCourseGrid(courses) {
  console.log('renderCourseGrid called with courses:', courses);
  console.log('state.myCourses:', state.myCourses);
  const grid = document.getElementById('courseCatalogGrid');
  if (!grid) return;
  if (!courses?.length) { grid.innerHTML = `<p style="color:var(--text-secondary);">No courses available yet.</p>`; return; }
  const diff = ['Beginner', 'Intermediate', 'Advanced'];
  grid.innerHTML = courses.map(c => {
    const isEnrolled = state.myCourses?.some(r => r.courseId === c.id && r.status !== 'Withdrawn');
    console.log(`Course ${c.id} - isEnrolled:`, isEnrolled, 'checking against:', state.myCourses);
    return `
      <div class="card glass animate-fade" style="cursor:pointer;" onclick="navigate('course-detail/${c.id}')">
        ${c.coverImageUrl ? `<img src="${c.coverImageUrl}" style="width:100%;height:160px;object-fit:cover;border-radius:var(--radius-md);margin-bottom:1rem;" onerror="this.style.display='none'">` : ''}
        <div style="display:flex;gap:0.5rem;margin-bottom:0.75rem;flex-wrap:wrap;">
          <span style="background:rgba(99,102,241,0.15);color:var(--primary-color);padding:0.2rem 0.6rem;border-radius:999px;font-size:0.7rem;">${c.topic}</span>
          <span style="background:rgba(245,158,11,0.15);color:var(--warning-color);padding:0.2rem 0.6rem;border-radius:999px;font-size:0.7rem;">${diff[c.difficulty] || 'Unknown'}</span>
        </div>
        <h4 style="margin-bottom:0.5rem;">${c.title}</h4>
        <p style="font-size:0.85rem;color:var(--text-secondary);margin-bottom:0.5rem;line-height:1.5;">${(c.synopsis || '').substring(0, 100)}${(c.synopsis || '').length > 100 ? '...' : ''}</p>
        <p style="font-size:0.75rem;color:var(--text-muted);margin-bottom:0.75rem;">By ${c.authorName || 'Unknown'} • ${c.totalRegistrations} enrolled</p>
        ${c.averageRating > 0 ? `<p style="font-size:0.75rem;color:var(--warning-color);margin-bottom:0.75rem;">${'⭐'.repeat(Math.min(Math.round(c.averageRating), 5))} ${c.averageRating.toFixed(1)} (${c.approvedReviewCount} reviews)</p>` : ''}
        <div style="display:flex;justify-content:space-between;align-items:center;" onclick="event.stopPropagation()">
          <span style="font-weight:700;font-size:1.1rem;">${c.listPrice > 0 ? '$' + c.listPrice : 'Free'}</span>
          ${(() => {
        console.log(`Rendering button for course ${c.id}:`, {
          hasUser: !!state.user,
          isEnrolled,
          userId: state.user?.id,
          myCoursesCount: state.myCourses?.length || 0
        });
        return state.user
          ? isEnrolled
            ? `<button class="btn btn-secondary btn-sm" disabled style="opacity:0.6;">  Enrolled</button>`
            : `<button class="btn btn-primary btn-sm" onclick="window.enrollCourse(${c.id})">Enroll Now</button>`
          : `<button class="btn btn-primary btn-sm" onclick="navigate('login')">Login to Enroll</button>`;
      })()
      }
        </div>
        ${state.user && isEnrolled ? `
          <div style="margin-top:1rem;border-top:1px solid var(--glass-border);padding-top:0.75rem;">
            <button class="btn btn-ghost btn-sm" onclick="window.openReviewModal(${c.id},'${c.title.replace(/'/g, "\\'")}')">✍️ Write a Review</button>
          </div>` : ''}
      </div>`;
  }).join('');
}

// --- ACTIONS ---
window.filterCourses = (q) => {
  if (!state.courses) return;
  const f = q ? state.courses.filter(c => (c.title + c.synopsis + c.topic).toLowerCase().includes(q.toLowerCase())) : state.courses;
  renderCourseGrid(f);
};

// Test function to check authentication state
window.checkAuth = () => {
  console.log('=== AUTHENTICATION CHECK ===');
  console.log('state.user:', state.user);
  console.log('state.token:', state.token ? 'exists' : 'missing');
  console.log('localStorage user:', localStorage.getItem('user'));
  console.log('localStorage token:', localStorage.getItem('token'));
  console.log('state.myCourses:', state.myCourses);
  console.log('state.courses:', state.courses);
  return {
    hasUser: !!state.user,
    hasToken: !!state.token,
    myCoursesCount: state.myCourses?.length || 0,
    coursesCount: state.courses?.length || 0
  };
};

window.enrollCourse = async (courseId) => {
  if (!state.user) {
    showToast('Please login to enroll', 'warning');
    return navigate('login');
  }

  // Use the shared helper for consistency
  const res = await apiFetch('/api/registration/api/registrations', {
    method: 'POST',
    body: JSON.stringify({ courseId })
  });

  if (res) {
    showToast('Enrolled successfully! 🚀', 'success');
    // Refresh the local state
    await loadMyCourses();
    // Refresh the course catalog to update the button status
    if (state.courses.length) renderCourseGrid(state.courses);
    navigate('my-courses');
  }
};

window.withdrawCourse = async (id) => {
  if (!confirm('Withdraw from this course?')) return;
  const res = await apiFetch(`/api/registration/api/registrations/${id}/withdraw`, { method: 'POST' });
  if (res !== null) {
    showToast('Withdrawn from course.', 'info');
    state.myCourses = state.myCourses.map(r => r.id === id ? { ...r, status: 'Withdrawn' } : r);
    renderDashboardContent();
  }
};

window.showProgressModal = (id, current) => {
  const modal = document.createElement('div');
  modal.style.cssText = 'position:fixed;inset:0;background:rgba(0,0,0,0.7);z-index:9999;display:flex;align-items:center;justify-content:center;';
  modal.id = 'progressModal';
  modal.innerHTML = `
    <div class="card glass" style="max-width:400px;width:90%;padding:2rem;">
      <h3 style="margin-bottom:1.5rem;">Update Progress</h3>
      <label class="input-label">Completion: <span id="progressVal">${current}</span>%</label>
      <input type="range" id="progressSlider" min="0" max="100" value="${current}" style="width:100%;margin:1rem 0;accent-color:var(--primary-color);" oninput="document.getElementById('progressVal').textContent=this.value">
      <div style="display:flex;gap:1rem;margin-top:1rem;">
        <button class="btn btn-primary" style="flex:1;" onclick="window.saveProgress(${id})">Save</button>
        <button class="btn btn-secondary" style="flex:1;" onclick="document.getElementById('progressModal').remove()">Cancel</button>
      </div>
    </div>`;
  document.body.appendChild(modal);
};

window.saveProgress = async (id) => {
  const pct = parseInt(document.getElementById('progressSlider')?.value || '0');
  document.getElementById('progressModal')?.remove();
  const res = await apiFetch(`/api/registration/api/registrations/${id}/progress`, {
    method: 'PATCH',
    body: JSON.stringify({ completionPercent: pct })
  });
  if (res !== null) {
    showToast('Progress updated!', 'success');
    state.myCourses = state.myCourses.map(r => r.id === id ? { ...r, completionPercent: pct, status: pct >= 100 ? 'Completed' : r.status } : r);
    const stats = await apiFetch(`/api/analytics/api/analytics/student/${state.user.id}/stats`);
    if (stats) state.stats = stats;
    renderDashboardContent();
  }
};

window.updateProgress = window.saveProgress; // alias

async function createCourse() {
  const res = await apiFetch('/api/courses/api/courses', {
    method: 'POST',
    body: JSON.stringify({
      title: document.getElementById('cc-title')?.value,
      synopsis: document.getElementById('cc-synopsis')?.value,
      topic: document.getElementById('cc-topic')?.value,
      language: document.getElementById('cc-language')?.value || 'English',
      difficulty: parseInt(document.getElementById('cc-difficulty')?.value || '0'),
      listPrice: parseFloat(document.getElementById('cc-price')?.value || '0'),
      coverImageUrl: document.getElementById('cc-cover')?.value || undefined
    })
  });
  if (res) { showToast('Course created! Submit it for review when ready.', 'success'); navigate('dashboard'); }
}

window.submitCourseForReview = async (id) => {
  const res = await apiFetch(`/api/courses/api/courses/${id}/submit-for-review`, { method: 'POST' });
  if (res) { showToast('Submitted for admin review!', 'success'); loadInstructorCourses(); }
};

window.deleteCourse = async (id) => {
  if (!confirm('Delete this course?')) return;
  const res = await apiFetch(`/api/courses/api/courses/${id}`, { method: 'DELETE' });
  if (res !== null) { showToast('Course deleted.', 'success'); loadInstructorCourses(); }
};

async function loadEditCourse(courseId) {
  const course = await apiFetch(`/api/courses/api/courses/${courseId}`);
  if (!course) {
    showToast('Failed to load course.', 'error');
    navigate('dashboard');
    return;
  }

  document.getElementById('ec-title').value = course.title || '';
  document.getElementById('ec-synopsis').value = course.synopsis || '';
  document.getElementById('ec-topic').value = course.topic || '';
  document.getElementById('ec-language').value = course.language || 'English';
  document.getElementById('ec-difficulty').value = course.difficulty || 0;
  document.getElementById('ec-price').value = course.listPrice || 0;
  document.getElementById('ec-cover').value = course.coverImageUrl || '';

  // Load lessons for this course
  state.editingCourseId = courseId;
  loadLessonsForEdit(courseId);
}

async function loadLessonsForEdit(courseId) {
  const lessons = await apiFetch(`/api/curriculum/api/curriculum/course/${courseId}`);
  state.editingCourseLessons = lessons || [];
  renderEditLessons();
}

function renderEditLessons() {
  const container = document.getElementById('editCourseLessons');
  if (!container) return;

  if (!state.editingCourseLessons?.length) {
    container.innerHTML = `<p style="color:var(--text-secondary);">No lessons yet. Click "+ Add Lesson" to create one.</p>`;
    return;
  }

  container.innerHTML = `
    <div style="display:grid;gap:1rem;">
      ${state.editingCourseLessons.map((lesson, index) => `
        <div class="card glass" style="padding:1rem;display:flex;justify-content:space-between;align-items:center;flex-wrap:wrap;gap:1rem;">
          <div style="flex:1;">
            <div style="display:flex;align-items:center;gap:0.5rem;margin-bottom:0.25rem;">
              <span style="background:var(--primary-color);color:white;width:24px;height:24px;border-radius:50%;display:flex;align-items:center;justify-content:center;font-weight:600;font-size:0.75rem;">${index + 1}</span>
              <strong>${lesson.title || 'Untitled'}</strong>
            </div>
            <p style="font-size:0.75rem;color:var(--text-secondary);margin-left:2rem;">${lesson.videoUrl || 'No link set'}</p>
          </div>
          <div style="display:flex;gap:0.5rem;">
            <button class="btn btn-secondary btn-sm" onclick="window.editLesson(${lesson.id})">Edit</button>
            <button class="btn btn-secondary btn-sm" style="color:var(--error-color);" onclick="window.deleteLesson(${lesson.id})">Delete</button>
          </div>
        </div>
      `).join('')}
    </div>`;
}

async function updateCourse(courseId) {
  const res = await apiFetch(`/api/courses/api/courses/${courseId}`, {
    method: 'PUT',
    body: JSON.stringify({
      title: document.getElementById('ec-title').value,
      synopsis: document.getElementById('ec-synopsis').value,
      topic: document.getElementById('ec-topic').value,
      language: document.getElementById('ec-language').value,
      difficulty: parseInt(document.getElementById('ec-difficulty').value),
      listPrice: parseFloat(document.getElementById('ec-price').value),
      coverImageUrl: document.getElementById('ec-cover').value
    })
  });
  if (res) {
    showToast('Course updated successfully!', 'success');
    navigate('dashboard');
  } else {
    showToast('Failed to update course.', 'error');
  }
}

// Admin actions
window.approveCourse = async (id) => {
  const res = await apiFetch(`/api/courses/api/courses/${id}/approve`, { method: 'POST' });
  if (res) { showToast('Course approved!', 'success'); state.adminPendingCourses = state.adminPendingCourses.filter(c => c.id !== id); renderDashboardContent(); }
};
window.rejectCourse = async (id) => {
  const res = await apiFetch(`/api/courses/api/courses/${id}/reject`, { method: 'POST' });
  if (res) { showToast('Course rejected.', 'info'); state.adminPendingCourses = state.adminPendingCourses.filter(c => c.id !== id); renderDashboardContent(); }
};
window.suspendUser = async (id) => {
  if (!confirm('Suspend this user?')) return;
  const res = await apiFetch(`/api/identity/api/accounts/${id}/suspend`, { method: 'POST' });
  if (res !== null) { showToast('User suspended.', 'info'); state.adminUsers = state.adminUsers.map(u => u.id === id ? { ...u, isActive: false } : u); renderDashboardContent(); }
};
window.reactivateUser = async (id) => {
  const res = await apiFetch(`/api/identity/api/accounts/${id}/reactivate`, { method: 'POST' });
  if (res !== null) { showToast('User reactivated!', 'success'); state.adminUsers = state.adminUsers.map(u => u.id === id ? { ...u, isActive: true } : u); renderDashboardContent(); }
};
window.deleteUser = async (id, name) => {
  if (!confirm(`Permanently delete "${name}"?`)) return;
  const res = await apiFetch(`/api/identity/api/accounts/${id}`, { method: 'DELETE' });
  if (res !== null) { showToast('User deleted.', 'info'); state.adminUsers = state.adminUsers.filter(u => u.id !== id); renderDashboardContent(); }
};
window.searchUsers = async (q) => {
  if (q.length < 2) { window.filterUsers(document.getElementById('roleFilter')?.value || ''); return; }
  const res = await apiFetch(`/api/identity/api/accounts/search?term=${encodeURIComponent(q)}`);
  if (res) { const t = document.getElementById('usersTable'); if (t) t.innerHTML = renderUsersTable(res); }
};
window.filterUsers = (role) => {
  const t = document.getElementById('usersTable');
  if (!t) return;
  t.innerHTML = renderUsersTable(role ? state.adminUsers.filter(u => u.role === role) : state.adminUsers);
};

window.startLesson = (lessonId) => {
  const lesson = state.currentCourseLessons?.find(l => l.id === lessonId);
  if (lesson?.videoUrl) {
    window.open(lesson.videoUrl, '_blank');
  } else {
    showToast('No link available for this lesson.', 'error');
  }
};

window.showAddLessonModal = (courseId) => {
  const modal = document.createElement('div');
  modal.style.cssText = 'position:fixed;inset:0;background:rgba(0,0,0,0.7);z-index:9999;display:flex;align-items:center;justify-content:center;';
  modal.id = 'addLessonModal';
  modal.innerHTML = `
    <div class="card glass" style="max-width:500px;width:90%;padding:2rem;">
      <h3 style="margin-bottom:1.5rem;">Add New Lesson</h3>
      <form id="addLessonForm">
        <div class="input-group"><label class="input-label">Lesson Title *</label><input type="text" class="input-field" id="lesson-title" required placeholder="e.g. Introduction to the topic"></div>
        <div class="input-group"><label class="input-label">Video/Content Link *</label><input type="url" class="input-field" id="lesson-video" required placeholder="https://youtube.com/... or https://docs.google.com/..."></div>
        <div style="display:flex;gap:1rem;margin-top:1.5rem;">
          <button type="button" class="btn btn-secondary" onclick="document.getElementById('addLessonModal').remove()">Cancel</button>
          <button type="submit" class="btn btn-primary" style="flex:1;">Add Lesson</button>
        </div>
      </form>
    </div>
  `;
  document.body.appendChild(modal);

  document.getElementById('addLessonForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const title = document.getElementById('lesson-title').value;
    const videoUrl = document.getElementById('lesson-video').value;

    const res = await apiFetch(`/api/curriculum/api/curriculum/course/${courseId}/lessons`, {
      method: 'POST',
      body: JSON.stringify({ title, videoUrl })
    });

    if (res) {
      showToast('Lesson added successfully!', 'success');
      modal.remove();
      loadCourseDetail(courseId); // Refresh the course detail to show the new lesson
    }
  });
};

window.showAddLessonModalForEdit = () => {
  const courseId = state.editingCourseId;
  if (!courseId) return;

  const modal = document.createElement('div');
  modal.style.cssText = 'position:fixed;inset:0;background:rgba(0,0,0,0.7);z-index:9999;display:flex;align-items:center;justify-content:center;';
  modal.id = 'addLessonModal';
  modal.innerHTML = `
    <div class="card glass" style="max-width:500px;width:90%;padding:2rem;">
      <h3 style="margin-bottom:1.5rem;">Add New Lesson</h3>
      <form id="addLessonForm">
        <div class="input-group"><label class="input-label">Lesson Title *</label><input type="text" class="input-field" id="lesson-title" required placeholder="e.g. Introduction to the topic"></div>
        <div class="input-group"><label class="input-label">Video/Content Link *</label><input type="url" class="input-field" id="lesson-video" required placeholder="https://youtube.com/... or https://docs.google.com/..."></div>
        <div style="display:flex;gap:1rem;margin-top:1.5rem;">
          <button type="button" class="btn btn-secondary" onclick="document.getElementById('addLessonModal').remove()">Cancel</button>
          <button type="submit" class="btn btn-primary" style="flex:1;">Add Lesson</button>
        </div>
      </form>
    </div>
  `;
  document.body.appendChild(modal);

  document.getElementById('addLessonForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const title = document.getElementById('lesson-title').value;
    const videoUrl = document.getElementById('lesson-video').value;

    const res = await apiFetch(`/api/curriculum/api/curriculum/course/${courseId}/lessons`, {
      method: 'POST',
      body: JSON.stringify({ title, videoUrl })
    });

    if (res) {
      showToast('Lesson added successfully!', 'success');
      modal.remove();
      loadLessonsForEdit(courseId); // Refresh the lessons list
    }
  });
};

window.editLesson = (lessonId) => {
  const lesson = state.editingCourseLessons?.find(l => l.id === lessonId);
  if (!lesson) return;

  const modal = document.createElement('div');
  modal.style.cssText = 'position:fixed;inset:0;background:rgba(0,0,0,0.7);z-index:9999;display:flex;align-items:center;justify-content:center;';
  modal.id = 'editLessonModal';
  modal.innerHTML = `
    <div class="card glass" style="max-width:500px;width:90%;padding:2rem;">
      <h3 style="margin-bottom:1.5rem;">Edit Lesson</h3>
      <form id="editLessonForm">
        <div class="input-group"><label class="input-label">Lesson Title *</label><input type="text" class="input-field" id="edit-lesson-title" required value="${lesson.title || ''}"></div>
        <div class="input-group"><label class="input-label">Video/Content Link *</label><input type="url" class="input-field" id="edit-lesson-video" required value="${lesson.videoUrl || ''}" placeholder="https://youtube.com/... or https://docs.google.com/..."></div>
        <div style="display:flex;gap:1rem;margin-top:1.5rem;">
          <button type="button" class="btn btn-secondary" onclick="document.getElementById('editLessonModal').remove()">Cancel</button>
          <button type="submit" class="btn btn-primary" style="flex:1;">Update Lesson</button>
        </div>
      </form>
    </div>
  `;
  document.body.appendChild(modal);

  document.getElementById('editLessonForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const title = document.getElementById('edit-lesson-title').value;
    const videoUrl = document.getElementById('edit-lesson-video').value;

    const res = await apiFetch(`/api/curriculum/api/curriculum/lessons/${lessonId}`, {
      method: 'PUT',
      body: JSON.stringify({ title, videoUrl })
    });

    if (res) {
      showToast('Lesson updated successfully!', 'success');
      modal.remove();
      loadLessonsForEdit(state.editingCourseId); // Refresh the lessons list
    }
  });
};

window.deleteLesson = async (lessonId) => {
  if (!confirm('Delete this lesson?')) return;

  const res = await apiFetch(`/api/curriculum/api/curriculum/lessons/${lessonId}`, { method: 'DELETE' });
  if (res) {
    showToast('Lesson deleted successfully!', 'success');
    loadLessonsForEdit(state.editingCourseId); // Refresh the lessons list
  }
};
window.approveReview = async (id) => {
  const res = await apiFetch(`/api/reviews/api/admin/reviews/${id}/approve`, { method: 'POST' });
  if (res) { showToast('Review approved!', 'success'); state.adminPendingReviews = state.adminPendingReviews.filter(r => r.id !== id); renderDashboardContent(); }
};
window.rejectReview = async (id) => {
  const res = await apiFetch(`/api/reviews/api/admin/reviews/${id}/reject`, { method: 'POST' });
  if (res) { showToast('Review rejected.', 'info'); state.adminPendingReviews = state.adminPendingReviews.filter(r => r.id !== id); renderDashboardContent(); }
};
window.deleteReview = async (id) => {
  if (!confirm('Delete this review?')) return;
  const res = await apiFetch(`/api/reviews/api/admin/reviews/${id}`, { method: 'DELETE' });
  if (res !== null) { showToast('Review deleted.', 'info'); state.adminPendingReviews = state.adminPendingReviews.filter(r => r.id !== id); renderDashboardContent(); }
};

window.openReviewModal = (courseId, courseTitle) => {
  document.getElementById('reviewModal')?.remove();
  const m = document.createElement('div');
  m.id = 'reviewModal';
  m.style.cssText = 'position:fixed;inset:0;background:rgba(0,0,0,0.7);z-index:9999;display:flex;align-items:center;justify-content:center;';
  m.innerHTML = `
    <div class="card glass" style="max-width:450px;width:90%;padding:2rem;">
      <h3 style="margin-bottom:0.5rem;">Review: ${courseTitle}</h3>
      <p style="color:var(--text-secondary);font-size:0.85rem;margin-bottom:1.5rem;">Your review helps other learners.</p>
      <div class="input-group"><label class="input-label">Rating</label>
        <select class="input-field" id="reviewRating">
          <option value="5">⭐⭐⭐⭐⭐ Excellent</option>
          <option value="4">⭐⭐⭐⭐ Great</option>
          <option value="3">⭐⭐⭐ Good</option>
          <option value="2">⭐⭐ Fair</option>
          <option value="1">⭐ Poor</option>
        </select>
      </div>
      <div class="input-group"><label class="input-label">Comment</label><textarea class="input-field" id="reviewComment" rows="4" placeholder="Share your experience..."></textarea></div>
      <div style="display:flex;gap:1rem;">
        <button class="btn btn-primary" style="flex:1;" onclick="window.postReview(${courseId})">Submit</button>
        <button class="btn btn-secondary" style="flex:1;" onclick="document.getElementById('reviewModal').remove()">Cancel</button>
      </div>
    </div>`;
  document.body.appendChild(m);
};

window.postReview = async (courseId) => {
  const rating = parseInt(document.getElementById('reviewRating')?.value || '5');
  const comment = document.getElementById('reviewComment')?.value || '';
  document.getElementById('reviewModal')?.remove();
  const res = await apiFetch('/api/reviews/api/reviews', {
    method: 'POST',
    body: JSON.stringify({ courseId, starRating: rating, comment })
  });
  if (res) showToast('Review submitted! It appears after moderation.', 'success');
};

// Keep old alias
window.submitReview = window.openReviewModal;

window.loginWithGoogle = () => {
  // Go directly to Identity service (port 5001) — NOT through gateway (5010).
  // OAuth correlation cookies must be set and read by the same host:port.
  // Google Console must have http://localhost:5001/signin-google as an authorized redirect URI.
  window.location.href = `http://localhost:5001/api/accounts/external-login?provider=Google`;
};

window.registerWithGoogle = () => {
  const role = document.getElementById('reg-role').value;
  if (!role) {
    showToast('Please select a role (Student or Instructor)', 'error');
    return;
  }
  // Go directly to Identity service (port 5001) — NOT through gateway (5010).
  window.location.href = `http://localhost:5001/api/accounts/external-login?provider=Google&role=${role}`;
};

// --- BOOT ---
// Load saved theme preference
const savedTheme = localStorage.getItem('theme');
if (savedTheme === 'dark') {
  document.documentElement.setAttribute('data-theme', 'dark');
}
render();