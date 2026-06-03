/* SkillBridge v2 (Hunarmand) — Theme Management */

(function () {
  const storedTheme = localStorage.getItem('theme');
  const systemPrefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
  const initialTheme = storedTheme || (systemPrefersDark ? 'dark' : 'light');
  
  document.documentElement.setAttribute('data-theme', initialTheme);
})();

document.addEventListener('DOMContentLoaded', () => {
  const themeToggleBtn = document.getElementById('theme-toggle');
  
  if (themeToggleBtn) {
    // Initial icon state
    updateThemeIcon();

    themeToggleBtn.addEventListener('click', () => {
      const currentTheme = document.documentElement.getAttribute('data-theme');
      const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
      
      document.documentElement.setAttribute('data-theme', newTheme);
      localStorage.setItem('theme', newTheme);
      
      updateThemeIcon();
    });
  }

  function updateThemeIcon() {
    const currentTheme = document.documentElement.getAttribute('data-theme');
    const icon = themeToggleBtn.querySelector('i');
    if (icon) {
      if (currentTheme === 'dark') {
        icon.className = 'fa-solid fa-sun';
      } else {
        icon.className = 'fa-solid fa-moon';
      }
    }
  }
});
