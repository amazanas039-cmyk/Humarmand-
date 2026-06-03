# 04 — GUI Design Brief
**Project:** SkillBridge v2 (Hunarmand)
**Design Theme:** Aurora Dark — Deep Violet × Electric Purple × Glass Morphism

---

## 1. Design Philosophy

The interface must feel **premium, modern, and trustworthy** — like a fintech or SaaS product, not a government portal. Every screen should communicate professionalism to both blue-collar labourers using it on mobile and customers hiring from a desktop browser.

**Core aesthetic pillars:**
1. **Dark-first with light mode support** — deep navy/dark-violet background, luminous accent colours.
2. **Glass Morphism** — frosted-glass cards (`backdrop-filter: blur(20px)`), subtle translucent panels, layered depth.
3. **Aurora gradients** — radial purple-to-violet glows as background accents (not flat colours).
4. **Micro-animation** — smooth transitions, hover lifts, loading skeletons, number counters.
5. **Spatial hierarchy** — cards float over backgrounds; shadows use colour not grey.

---

## 2. Colour System (CSS Variables)

```css
:root {
  /* === BRAND COLOURS === */
  --clr-primary:        #7C3AED;   /* Electric Violet — main CTA, active states */
  --clr-primary-light:  #A78BFA;   /* Soft Violet — hover, secondary text accents */
  --clr-primary-dark:   #5B21B6;   /* Deep Violet — pressed states, borders */
  --clr-accent:         #8B5CF6;   /* Mid Violet — gradients, charts */

  /* === BACKGROUND LAYERS (dark mode default) === */
  --clr-bg-base:        #0D0A1E;   /* Deepest Navy/Black-Violet — page background */
  --clr-bg-surface:     #13102A;   /* Cards, panels */
  --clr-bg-elevated:    #1C1740;   /* Modals, dropdowns, hover states */
  --clr-bg-glass:       rgba(28, 23, 64, 0.55);  /* Glass morphism fills */

  /* === TEXT === */
  --clr-text-primary:   #F1F0FF;   /* Near-white, main body */
  --clr-text-secondary: #A09CB8;   /* Muted purple-grey */
  --clr-text-muted:     #6B6880;   /* Placeholder, disabled */

  /* === SEMANTIC === */
  --clr-success:        #10B981;   /* Green */
  --clr-warning:        #F59E0B;   /* Amber */
  --clr-danger:         #EF4444;   /* Red */
  --clr-info:           #3B82F6;   /* Blue */

  /* === GRADIENTS === */
  --grad-primary:       linear-gradient(135deg, #7C3AED 0%, #A855F7 100%);
  --grad-card:          linear-gradient(145deg, rgba(124,58,237,0.15) 0%, rgba(168,85,247,0.05) 100%);
  --grad-aurora:        radial-gradient(ellipse 80% 60% at 50% 0%, rgba(124,58,237,0.35) 0%, transparent 70%);

  /* === BORDERS === */
  --border-glass:       1px solid rgba(255,255,255,0.08);
  --border-primary:     1px solid rgba(124,58,237,0.4);

  /* === SHADOWS === */
  --shadow-card:        0 4px 24px rgba(124,58,237,0.15), 0 1px 4px rgba(0,0,0,0.4);
  --shadow-glow:        0 0 40px rgba(124,58,237,0.35);
  --shadow-btn:         0 4px 16px rgba(124,58,237,0.45);

  /* === SPACING === */
  --space-xs:   4px;
  --space-sm:   8px;
  --space-md:   16px;
  --space-lg:   24px;
  --space-xl:   40px;
  --space-2xl:  64px;

  /* === RADIUS === */
  --radius-sm:  8px;
  --radius-md:  12px;
  --radius-lg:  20px;
  --radius-xl:  32px;
  --radius-pill: 999px;

  /* === BLUR === */
  --blur-glass: blur(20px) saturate(180%);
}
```

---

## 3. Typography

| Role | Font | Weight | Size |
|------|------|--------|------|
| Display / Hero | Plus Jakarta Sans | 800 | 52–72px |
| Heading H1 | Plus Jakarta Sans | 700 | 36px |
| Heading H2 | Plus Jakarta Sans | 700 | 28px |
| Heading H3 | Plus Jakarta Sans | 600 | 22px |
| Body | Inter | 400 | 16px |
| Body Small | Inter | 400 | 14px |
| Caption / Label | Inter | 500 | 12px |
| Button | Inter | 600 | 15px |

Load via Google Fonts CDN:
```html
<link href="https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;600;700;800&family=Inter:wght@400;500;600&display=swap" rel="stylesheet">
```

---

## 4. Component Library

### 4.1 Glass Card
```css
.glass-card {
  background: var(--clr-bg-glass);
  backdrop-filter: var(--blur-glass);
  -webkit-backdrop-filter: var(--blur-glass);
  border: var(--border-glass);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-card);
  transition: transform 0.25s ease, box-shadow 0.25s ease;
}
.glass-card:hover {
  transform: translateY(-4px);
  box-shadow: var(--shadow-card), var(--shadow-glow);
}
```

### 4.2 Primary Button
```css
.btn-primary {
  background: var(--grad-primary);
  color: white;
  border: none;
  border-radius: var(--radius-pill);
  padding: 12px 28px;
  font-weight: 600;
  font-size: 15px;
  cursor: pointer;
  box-shadow: var(--shadow-btn);
  transition: all 0.2s ease;
}
.btn-primary:hover {
  filter: brightness(1.1);
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(124,58,237,0.6);
}
```

### 4.3 Outline / Ghost Button
```css
.btn-outline {
  background: transparent;
  color: var(--clr-primary-light);
  border: var(--border-primary);
  border-radius: var(--radius-pill);
  padding: 12px 28px;
  font-weight: 600;
  transition: all 0.2s ease;
}
.btn-outline:hover {
  background: rgba(124,58,237,0.12);
  border-color: var(--clr-primary);
}
```

### 4.4 Input Fields
```css
.input-field {
  background: rgba(255,255,255,0.04);
  border: 1px solid rgba(255,255,255,0.1);
  border-radius: var(--radius-md);
  color: var(--clr-text-primary);
  padding: 14px 18px;
  font-size: 15px;
  transition: border-color 0.2s, box-shadow 0.2s;
  outline: none;
}
.input-field:focus {
  border-color: var(--clr-primary);
  box-shadow: 0 0 0 3px rgba(124,58,237,0.2);
}
```

### 4.5 Status Badge
```css
.badge { border-radius: var(--radius-pill); padding: 4px 12px; font-size: 12px; font-weight: 600; }
.badge-success  { background: rgba(16,185,129,0.15); color: #10B981; }
.badge-warning  { background: rgba(245,158,11,0.15);  color: #F59E0B; }
.badge-danger   { background: rgba(239,68,68,0.15);   color: #EF4444; }
.badge-info     { background: rgba(59,130,246,0.15);  color: #3B82F6; }
.badge-primary  { background: rgba(124,58,237,0.2);   color: #A78BFA; }
```

### 4.6 Stat Card (Dashboard)
```css
.stat-card {
  /* extends glass-card */
  padding: var(--space-lg);
  display: flex;
  flex-direction: column;
  gap: 8px;
  position: relative;
  overflow: hidden;
}
.stat-card::before {
  content: '';
  position: absolute;
  top: -20px; right: -20px;
  width: 100px; height: 100px;
  background: var(--grad-primary);
  opacity: 0.12;
  border-radius: 50%;
  filter: blur(30px);
}
.stat-card .value {
  font-size: 32px;
  font-weight: 800;
  font-family: 'Plus Jakarta Sans', sans-serif;
  background: var(--grad-primary);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}
.stat-card .delta-up   { color: var(--clr-success); font-size: 13px; font-weight: 600; }
.stat-card .delta-down { color: var(--clr-danger);  font-size: 13px; font-weight: 600; }
```

### 4.7 Sidebar Navigation
```css
.sidebar {
  width: 260px;
  background: var(--clr-bg-surface);
  border-right: var(--border-glass);
  height: 100vh;
  position: fixed;
  padding: var(--space-lg) 0;
  display: flex;
  flex-direction: column;
}
.sidebar .nav-item {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 13px 24px;
  color: var(--clr-text-secondary);
  text-decoration: none;
  font-weight: 500;
  transition: all 0.2s;
  border-radius: 0 var(--radius-md) var(--radius-md) 0;
  margin-right: 12px;
}
.sidebar .nav-item:hover,
.sidebar .nav-item.active {
  background: rgba(124,58,237,0.15);
  color: var(--clr-primary-light);
  border-right: 3px solid var(--clr-primary);
}
.sidebar .nav-item i { width: 20px; text-align: center; }
```

### 4.8 Top Navbar (Dashboard)
```css
.topbar {
  height: 64px;
  background: var(--clr-bg-glass);
  backdrop-filter: var(--blur-glass);
  border-bottom: var(--border-glass);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 var(--space-xl);
  position: sticky;
  top: 0;
  z-index: 100;
}
```

### 4.9 Rating Stars
Use filled/half/empty SVG stars rendered from a 0–5 float score. Colour: `#FBBF24`.

### 4.10 Notification Toast
```css
.toast {
  position: fixed; bottom: 24px; right: 24px;
  min-width: 320px;
  background: var(--clr-bg-elevated);
  border: var(--border-glass);
  border-left: 4px solid var(--clr-primary);
  border-radius: var(--radius-md);
  padding: 16px 20px;
  box-shadow: var(--shadow-card);
  backdrop-filter: var(--blur-glass);
  animation: slideInRight 0.3s ease;
}
@keyframes slideInRight {
  from { transform: translateX(120%); opacity: 0; }
  to   { transform: translateX(0);    opacity: 1; }
}
```

---

## 5. Page-by-Page Design Specification

### 5.1 Public Landing / Home Page (`/`)

**Goal:** Instant credibility. Convert visitor to sign-up in under 10 seconds.

**Layout — full-page scroll, 5 sections:**

#### Section 1 — Hero
- Full-viewport. Background: `--clr-bg-base` with `--grad-aurora` overlay + animated floating blobs (radial gradients, slow movement via CSS keyframes).
- **Sticky transparent navbar** that becomes glass on scroll: logo (purple icon + "Hunarmand" wordmark), links (Home, How It Works, Browse Skills, Login), CTA button "Get Started →".
- Left column (55%): `New` pill badge + headline "Find Skilled Workers, Instantly." + sub-headline + two CTA buttons ("Hire a Professional →" purple, "Register as Labourer" outline) + social proof row (avatar stack + "2,000+ Verified Workers").
- Right column (45%): Floating dashboard mockup card (glass card with mini stats — total workers, ratings, live requests). Rotated 10deg, drop shadow glow.

#### Section 2 — How It Works (3-step)
- Background: slightly lighter (`--clr-bg-surface`).
- Three glass cards in a row: Step 1 "Post Your Job", Step 2 "Browse Verified Workers", Step 3 "Hire & Pay Safely".
- Each card: numbered gradient circle, icon, short copy. Connecting dotted line between cards.

#### Section 3 — Skill Categories
- Grid of 8+ pill/card buttons: Electrician, Plumber, Carpenter, Painter, Mason, Welder, AC Technician, Cleaner, Tiler, etc.
- Each has a Font Awesome icon + label. On hover: purple glow border.
- CTA: "Browse All Categories →"

#### Section 4 — Featured Labourers (Top 4 by rating)
- Pulled from DB. Each card: avatar placeholder + name, skills badge, star rating, "Rs. X/hr", distance (if geo available), "View Profile" button.
- Cards use glass morphism.

#### Section 5 — Footer
- Three-column: branding + tagline, Quick Links, Contact.
- Bottom bar: © year, Privacy, Terms.

---

### 5.2 Login Pages — Three Separate Screens

**One page per role:** `/Auth/CustomerLogin`, `/Auth/LabourerLogin`, `/Auth/AdminLogin`

**Design pattern (identical layout, different accent copy):**
- Full-screen background: dark base + large blurred aurora blob (unique hue per role: violet for customer, teal-ish for labourer, deep blue for admin).
- Centered glass card (max-width 460px, border-radius 24px, `backdrop-filter: blur(40px)`).
- Logo at top, role label ("Customer Portal", "Labourer Portal", "Admin Console"), form fields, primary CTA button, link to register.
- Animated gradient border on card (CSS `@keyframes` rotating conic-gradient border trick).
- Below card: small links to switch portals.

**Fields:**
- Customer: Email, Password, [Login] — "Don't have an account? Register"
- Labourer: Email, Password, [Login]
- Admin: Username, Password, [Admin Login] — no registration link

---

### 5.3 Registration Pages

**Customer** (`/Auth/CustomerRegister`): Single page — Full Name, Email, Phone, Password, Confirm Password.

**Labourer** (`/Auth/LabourerRegister`): **Multi-step wizard** (5 steps shown as progress bar):
1. Personal Info — Name, Email, Phone, CNIC, Password
2. Skill Category — dropdown + multi-select sub-skills
3. Experience & Rate — years of experience, hourly rate (PKR), short bio
4. Availability — weekly schedule grid (Mon–Sun × Morning/Afternoon/Evening checkboxes)
5. Location — Leaflet map with draggable pin + "Use My Location" button

Progress bar: coloured pill segments, current step highlighted violet.

---

### 5.4 Customer Dashboard (`/Customer/Dashboard`)

**Sidebar nav links:** Dashboard, Browse Workers, My Requests, Disputes, Profile, Logout.

**Main content — 4 sections:**
1. **Welcome banner** — "Good morning, [Name]!" + quick stats row (Active Requests, Completed, Pending Review).
2. **Quick Hire** — category pill selector to jump to search.
3. **Active Requests** — table/card list of in-progress jobs with live status badges.
4. **Recent Reviews Given** — compact list.

---

### 5.5 Browse & Search (`/Customer/Browse`)

**Left panel (30%):** Filter sidebar
- Search input (name/skill keyword)
- Category dropdown
- Min Rating slider (0–5 stars)
- Min Experience slider (0–20 years)
- **Proximity Radius Slider** (1 km – 50 km) with live numeric readout
- Availability filter (day of week)
- "Apply Filters" button

**Right panel (70%):** Split view — toggle between:
- **List view:** Labour cards (avatar, name, skill, rating stars, price, distance, "View Profile" + "Hire" buttons)
- **Map view:** Leaflet.js map full height, circle overlay showing radius, labour markers as custom purple pins; clicking pin shows popup card.

---

### 5.6 Labourer Dashboard (`/Labourer/Dashboard`)

**Sidebar nav links:** Dashboard, My Jobs, Earnings, Profile, Disputes, Logout.

**Dashboard layout (inspired by Aurora sample):**
- **Top stats row (4 cards):**
  - Total Jobs Completed (with % delta vs last month)
  - Average Rating (star + number)
  - Total Earnings (PKR, this month)
  - Active Jobs
- **Main chart (left 65%):** Line chart — "Earnings Over Time" (last 6 months, Chart.js), with gradient fill under line.
- **Right panel (35%):** "Upcoming Jobs" list (job title, customer name, date, status badge) + "Recent Reviews" (avatar, stars, short text).
- **Bottom row:** "Jobs Funnel" bar chart (Requested → Accepted → InProgress → Completed → Disputed) + "Rating Distribution" doughnut chart (1★ to 5★ breakdown).

---

### 5.7 Admin Dashboard (`/Admin/Dashboard`)

**Top nav + sidebar.**

**Dashboard overview — 6 stat cards in 3×2 grid:**
- Total Registered Users (delta)
- Total Labourers (Verified / Pending)
- Total Jobs Completed (this month)
- Active Disputes
- Revenue Estimate (PKR)
- New Registrations Today

**Charts row 1 (2 charts side-by-side):**
- **User Growth Line Chart** — Customers & Labourers over last 12 months (two lines, Chart.js).
- **Daily Transactions Bar Chart** — jobs accepted per day for last 30 days.

**Charts row 2 (2 charts side-by-side):**
- **Job Status Distribution** — Doughnut chart: Pending / InProgress / Completed / Disputed / Cancelled.
- **Top Skill Categories** — Horizontal bar chart by demand (# requests per category).

**Heatmap section:** Recent activity log table (timestamp, user, action, IP).

**Quick-action cards:**
- "Pending Verifications" with count badge → link to verification queue.
- "Open Disputes" with count badge → link to disputes panel.

---

### 5.8 Admin — Verification Queue (`/Admin/LabourerVerification`)

Table with sortable columns: Name, Category, Experience, CNIC, Registered At, Status.
Row expand → profile detail panel (all labourer data, documents).
Action buttons: "Approve ✓" (green), "Reject ✗" (red, requires modal reason input).

---

### 5.9 Admin — Dispute Resolution (`/Admin/Disputes`)

**Dispute list:** Table of open disputes — ID, Customer, Labourer, Job, Date Opened, Status.

**Clicking a dispute → Dispute Detail Page:**
- Top: dispute metadata (job, reason, raised by).
- Centre: **Live Chat Panel** (SignalR-powered, 3 columns):
  - Left: Customer messages
  - Centre: Chat thread (all 3 parties with labelled bubbles)
  - Right: Labourer messages
  - Bottom: Admin input + Send button
- Admin action buttons: "Mark Resolved → (Customer favoured)", "Mark Resolved → (Labourer favoured)", "Escalate".
- On resolution: chat panel becomes read-only, status updates to Resolved.

---

### 5.10 Labourer — Job Detail (`/Labourer/JobDetail/{id}`)

Glass card layout: Customer info, job description, date/time, location (mini Leaflet map), current status.
Action buttons per status:
- Pending: "Accept ✓" / "Reject ✗" (reject opens reason modal)
- Accepted: "Mark In Progress"
- InProgress: "Mark Completed"

---

## 6. Animation Specification

| Element | Animation | Duration |
|---------|-----------|----------|
| Page load | Fade-in + translateY(20px → 0) | 0.4s ease |
| Card hover | translateY(-4px) + shadow glow | 0.25s |
| Button press | scale(0.97) | 0.1s |
| Sidebar active | Background slide-in from left | 0.2s |
| Stat numbers | Animated count-up (JS) | 1.2s |
| Chart render | Chart.js default animation (easeInOutQuart) | 0.8s |
| Toast slide | translateX(120% → 0) | 0.3s |
| Dispute chat | New message fade+slide from bottom | 0.2s |
| Modal | Scale(0.95 → 1) + fade | 0.2s |
| Aurora blobs | slow float loop (translateX/Y ±30px) | 12s infinite ease-in-out |

---

## 7. Responsive Breakpoints

```css
/* Mobile first */
/* sm */ @media (min-width: 640px) { ... }
/* md */ @media (min-width: 768px) { ... }
/* lg */ @media (min-width: 1024px) { ... }
/* xl */ @media (min-width: 1280px) { ... }
```

- **Mobile (<768px):** Single column, sidebar collapses to hamburger + bottom drawer, map fills full width, labourer cards stack.
- **Tablet (768–1024px):** Two-column cards, sidebar hidden by default.
- **Desktop (>1024px):** Full layout as described above.

---

## 8. Icon System

Use **Font Awesome 6 Free** (CDN). Key icons:

| Concept | FA Class |
|---------|----------|
| Dashboard | `fa-gauge-high` |
| Jobs | `fa-briefcase` |
| Workers/Labourers | `fa-hard-hat` |
| Customer | `fa-user` |
| Admin | `fa-shield-halved` |
| Earnings | `fa-coins` |
| Rating | `fa-star` |
| Location/Map | `fa-map-location-dot` |
| Dispute | `fa-gavel` |
| Chat | `fa-comments` |
| Verify | `fa-badge-check` |
| Notification | `fa-bell` |
| Logout | `fa-right-from-bracket` |
| Calendar | `fa-calendar-days` |
| Settings | `fa-sliders` |

---

## 9. Map UI Specification (Leaflet.js)

- **Tile source:** `https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png` (free, no key)
- **Dark map skin:** Use `https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png` for dark-mode consistency.
- **Customer location marker:** Blue pulsing dot (custom DivIcon, CSS animation).
- **Labour markers:** Custom purple pin SVG icons. Clustered with `Leaflet.markercluster` when >20 results.
- **Radius circle:** `L.circle()` with `color: '#7C3AED'`, `fillOpacity: 0.08`.
- **Radius slider:** HTML range input linked to circle radius via JS event listener, updates in real-time without map reload.
- **Popup:** Glass-morphism styled HTML popup — name, skill, rating, distance, "View Profile" link.
- **Geocoding:** Nominatim reverse geocode on location pin drop to fill address field.

---

## 10. Light Mode (Optional Toggle)

```css
[data-theme="light"] {
  --clr-bg-base:      #F5F3FF;
  --clr-bg-surface:   #FFFFFF;
  --clr-bg-elevated:  #EDE9FE;
  --clr-bg-glass:     rgba(255,255,255,0.7);
  --clr-text-primary: #1E1340;
  --clr-text-secondary: #6B5CA5;
  --clr-text-muted:   #9B89C4;
  --border-glass:     1px solid rgba(124,58,237,0.15);
  --shadow-card:      0 4px 24px rgba(124,58,237,0.1), 0 1px 4px rgba(0,0,0,0.06);
}
```

Toggle button in navbar sets `document.documentElement.setAttribute('data-theme', 'light'/'dark')` and saves to localStorage.
