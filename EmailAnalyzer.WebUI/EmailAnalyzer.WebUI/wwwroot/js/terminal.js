// ── Scroll ──────────────────────────────────────────────────────
window.scrollToBottom = function () {
    const el = document.getElementById('terminal-output');
    if (el) el.scrollTop = el.scrollHeight;
};

// ── Clipboard ───────────────────────────────────────────────────
window.copyToClipboard = function (text) {
    if (navigator.clipboard) navigator.clipboard.writeText(text);
};

// ── Theme (dark / light) ────────────────────────────────────────
window.getTheme = function () {
    return localStorage.getItem('theme') || 'dark';
};

window.toggleTheme = function () {
    const html = document.documentElement;
    const isNowLight = html.classList.toggle('light-mode');
    localStorage.setItem('theme', isNowLight ? 'light' : 'dark');
    return isNowLight ? 'light' : 'dark';
};

// Uruchamiany natychmiast — zapobiega flashowi przy przeładowaniu strony
(function () {
    if (localStorage.getItem('theme') === 'light')
        document.documentElement.classList.add('light-mode');
})();
