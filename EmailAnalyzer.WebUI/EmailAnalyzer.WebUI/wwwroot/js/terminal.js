window.scrollToBottom = function () {
    const terminal = document.getElementById('terminal-output');
    if (terminal) terminal.scrollTop = terminal.scrollHeight;
};

window.copyToClipboard = function (text) {
    if (navigator.clipboard) {
        navigator.clipboard.writeText(text);
    }
};
